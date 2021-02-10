using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ValidationTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class MigrateImportsServiceTests
    {
        [Fact]
        public async Task MigrateImports_FailsIfMigratedImportsAlreadyExist()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            // Test the migration won't run if there's already been an attempt to run it,
            // until the migrated data is removed
            var migratedImportSubjectId = Guid.NewGuid();
            var migratedImport = new DataImport
            {
                File = new File
                {
                    Filename = "migrated.csv",
                    Release = release,
                    Type = FileType.Data,
                    SubjectId = migratedImportSubjectId
                },
                MetaFile = new File
                {
                    Filename = "migrated.meta.csv",
                    Release = release,
                    Type = Metadata,
                    SubjectId = migratedImportSubjectId
                },
                Status = DataImportStatus.COMPLETE,
                Migrated = true
            };

            var nonMigratedImportSubjectId = Guid.NewGuid();
            var nonMigratedImport = new DataImport
            {
                File = new File
                {
                    Filename = "non-migrated.csv",
                    Release = release,
                    Type = FileType.Data,
                    SubjectId = nonMigratedImportSubjectId
                },
                MetaFile = new File
                {
                    Filename = "non-migrated.meta.csv",
                    Release = release,
                    Type = Metadata,
                    SubjectId = nonMigratedImportSubjectId
                },
                Status = DataImportStatus.QUEUED,
                Migrated = false
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.DataImports.AddRangeAsync(migratedImport, nonMigratedImport);
                await contentDbContext.SaveChangesAsync();
            }

            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildMigrateImportsService(contentDbContext: contentDbContext,
                    tableStorageService: tableStorageService.Object);

                var result = await service.MigrateImports();
                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, DataFileImportsMigrationAlreadyRun);
            }

            MockUtils.VerifyAllMocks(tableStorageService);
        }

        [Fact]
        public async Task MigrateImports_SuccessfullyMigrateTableImports()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var importedSubjectId = Guid.NewGuid();

            var zipFile = new File
            {
                Filename = "imported1.zip",
                Release = release,
                Type = DataZip
            };

            var dataFile = new File
            {
                Filename = "imported1.csv",
                Release = release,
                Type = FileType.Data,
                Source = zipFile,
                SubjectId = importedSubjectId
            };

            var metaFile = new File
            {
                Filename = "imported1.meta.csv",
                Release = release,
                Type = Metadata,
                SubjectId = importedSubjectId
            };

            var tableImports = new List<TableImport>
            {
                new TableImport
                {
                    PartitionKey = release.Id.ToString(),
                    RowKey = dataFile.Filename,
                    Errors = "",
                    Message = TableImportSampleJson.Message(
                        releaseId: release.Id,
                        subjectId: importedSubjectId,
                        dataFilename: dataFile.Filename,
                        metaFilename: metaFile.Filename,
                        zipFilename: zipFile.Filename
                    ),
                    Status = "COMPLETE",
                    PercentageComplete = 100,
                    NumberOfRows = 10000,
                    Timestamp = DateTimeOffset.Now
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.Files.AddRangeAsync(zipFile, dataFile, metaFile);
                await contentDbContext.SaveChangesAsync();
            }

            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            tableStorageService.Setup(service =>
                    service.ExecuteQueryAsync("imports", It.IsAny<TableQuery<TableImport>>()))
                .ReturnsAsync(tableImports);

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildMigrateImportsService(contentDbContext: contentDbContext,
                    tableStorageService: tableStorageService.Object);

                var result = await service.MigrateImports();
                Assert.True(result.IsRight);
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var imports = await contentDbContext.DataImports
                    .Include(i => i.Errors)
                    .ToListAsync();

                Assert.Single(imports);

                Assert.Equal(tableImports[0].Timestamp.UtcDateTime, imports[0].Created);
                Assert.Equal(DataImportStatus.COMPLETE, imports[0].Status);
                Assert.Equal(100, imports[0].StagePercentageComplete);
                Assert.Equal(importedSubjectId, imports[0].SubjectId);
                Assert.Equal(dataFile.Id, imports[0].FileId);
                Assert.Equal(metaFile.Id, imports[0].MetaFileId);
                Assert.Equal(zipFile.Id, imports[0].ZipFileId);
                Assert.Equal(10000, imports[0].Rows);
                Assert.Equal(10, imports[0].NumBatches);
                Assert.Equal(1000, imports[0].RowsPerBatch);
                Assert.Equal(10000, imports[0].TotalRows);
                Assert.True(imports[0].Migrated);

                Assert.NotNull(imports[0].Errors);
                Assert.Empty(imports[0].Errors);
            }

            MockUtils.VerifyAllMocks(tableStorageService);
        }

        [Fact]
        public async Task MigrateImports_FailedImportsWithNoMessagesAreIgnored()
        {
            var tableImports = new List<TableImport>
            {
                // Failed imports with no message, should be ignored
                new TableImport
                {
                    PartitionKey = Guid.NewGuid().ToString(),
                    RowKey = "null-message.csv",
                    Errors = null,
                    Message = null,
                    Status = "FAILED",
                    PercentageComplete = 0,
                    NumberOfRows = 10000,
                    Timestamp = DateTimeOffset.Now
                },
                new TableImport
                {
                    PartitionKey = Guid.NewGuid().ToString(),
                    RowKey = "empty-message.csv",
                    Errors = null,
                    Message = "",
                    Status = "FAILED",
                    PercentageComplete = 0,
                    NumberOfRows = 10000,
                    Timestamp = DateTimeOffset.Now
                }
            };

            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            tableStorageService.Setup(service =>
                    service.ExecuteQueryAsync("imports", It.IsAny<TableQuery<TableImport>>()))
                .ReturnsAsync(tableImports);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildMigrateImportsService(contentDbContext: contentDbContext,
                    tableStorageService: tableStorageService.Object);

                var result = await service.MigrateImports();
                Assert.True(result.IsRight);
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var imports = await contentDbContext.DataImports
                    .Include(i => i.Errors)
                    .ToListAsync();

                Assert.Empty(imports);
            }

            MockUtils.VerifyAllMocks(tableStorageService);
        }

        [Fact]
        public async Task MigrateImports_ImportsWithoutCorrespondingFilesAreIgnored()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            // Add data file and meta files belonging to different subjects
            // Each subject will be missing a corresponding file type

            var dataFile = new File
            {
                Filename = "data.csv",
                Release = release,
                Type = FileType.Data,
                SubjectId = Guid.NewGuid()
            };

            var metaFile = new File
            {
                Filename = "meta.csv",
                Release = release,
                Type = Metadata,
                SubjectId = Guid.NewGuid()
            };

            var tableImports = new List<TableImport>
            {
                // Using unknown subject id - Neither data file or meta file should be found
                new TableImport
                {
                    PartitionKey = release.Id.ToString(),
                    RowKey = dataFile.Filename,
                    Errors = "",
                    Message = TableImportSampleJson.Message(
                        releaseId: release.Id,
                        subjectId: Guid.NewGuid(),
                        dataFilename: dataFile.Filename,
                        metaFilename: metaFile.Filename,
                        zipFilename: null
                    ),
                    Status = "COMPLETE",
                    PercentageComplete = 100,
                    NumberOfRows = 10000,
                    Timestamp = DateTimeOffset.Now
                },
                // Using data file subject id - Only data file should be found
                new TableImport
                {
                    PartitionKey = release.Id.ToString(),
                    RowKey = dataFile.Filename,
                    Errors = "",
                    Message = TableImportSampleJson.Message(
                        releaseId: release.Id,
                        subjectId: dataFile.SubjectId.Value,
                        dataFilename: dataFile.Filename,
                        metaFilename: metaFile.Filename,
                        zipFilename: null
                    ),
                    Status = "COMPLETE",
                    PercentageComplete = 100,
                    NumberOfRows = 10000,
                    Timestamp = DateTimeOffset.Now
                },
                // Using meta file subject id - Only meta file should be found
                new TableImport
                {
                    PartitionKey = release.Id.ToString(),
                    RowKey = dataFile.Filename,
                    Errors = "",
                    Message = TableImportSampleJson.Message(
                        releaseId: release.Id,
                        subjectId: metaFile.SubjectId.Value,
                        dataFilename: dataFile.Filename,
                        metaFilename: metaFile.Filename,
                        zipFilename: null
                    ),
                    Status = "COMPLETE",
                    PercentageComplete = 100,
                    NumberOfRows = 10000,
                    Timestamp = DateTimeOffset.Now
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.Files.AddRangeAsync(dataFile, metaFile);
                await contentDbContext.SaveChangesAsync();
            }

            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            tableStorageService.Setup(service =>
                    service.ExecuteQueryAsync("imports", It.IsAny<TableQuery<TableImport>>()))
                .ReturnsAsync(tableImports);

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildMigrateImportsService(contentDbContext: contentDbContext,
                    tableStorageService: tableStorageService.Object);

                var result = await service.MigrateImports();
                Assert.True(result.IsRight);
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var imports = await contentDbContext.DataImports
                    .Include(i => i.Errors)
                    .ToListAsync();

                // All three table imports are ignored since they were all missing one or both files
                Assert.Empty(imports);
            }

            MockUtils.VerifyAllMocks(tableStorageService);
        }

        [Fact]
        public async Task MigrateImports_TableImportWithErrorsCreatesDataImportErrors()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var importedSubjectId = Guid.NewGuid();

            var zipFile = new File
            {
                Filename = "imported1.zip",
                Release = release,
                Type = DataZip
            };

            var dataFile = new File
            {
                Filename = "imported1.csv",
                Release = release,
                Type = FileType.Data,
                Source = zipFile,
                SubjectId = importedSubjectId
            };

            var metaFile = new File
            {
                Filename = "imported1.meta.csv",
                Release = release,
                Type = Metadata,
                SubjectId = importedSubjectId
            };

            var tableImports = new List<TableImport>
            {
                // Import with errors
                new TableImport
                {
                    PartitionKey = release.Id.ToString(),
                    RowKey = dataFile.Filename,
                    Errors = @"[{""Message"":""error1""},{""Message"": ""error2""}]",
                    Message = TableImportSampleJson.Message(
                        releaseId: release.Id,
                        subjectId: importedSubjectId,
                        dataFilename: dataFile.Filename,
                        metaFilename: metaFile.Filename,
                        zipFilename: zipFile.Filename
                    ),
                    Status = "COMPLETE",
                    PercentageComplete = 100,
                    NumberOfRows = 10000,
                    Timestamp = DateTimeOffset.Now
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.Files.AddRangeAsync(zipFile, dataFile, metaFile);
                await contentDbContext.SaveChangesAsync();
            }

            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            tableStorageService.Setup(service =>
                    service.ExecuteQueryAsync("imports", It.IsAny<TableQuery<TableImport>>()))
                .ReturnsAsync(tableImports);

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildMigrateImportsService(contentDbContext: contentDbContext,
                    tableStorageService: tableStorageService.Object);

                var result = await service.MigrateImports();
                Assert.True(result.IsRight);
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var imports = await contentDbContext.DataImports
                    .Include(i => i.Errors)
                    .ToListAsync();

                Assert.Single(imports);

                Assert.Equal(tableImports[0].Timestamp.UtcDateTime, imports[0].Created);
                Assert.Equal(DataImportStatus.COMPLETE, imports[0].Status);
                Assert.Equal(100, imports[0].StagePercentageComplete);
                Assert.Equal(importedSubjectId, imports[0].SubjectId);
                Assert.Equal(dataFile.Id, imports[0].FileId);
                Assert.Equal(metaFile.Id, imports[0].MetaFileId);
                Assert.Equal(zipFile.Id, imports[0].ZipFileId);
                Assert.Equal(10000, imports[0].Rows);
                Assert.Equal(10, imports[0].NumBatches);
                Assert.Equal(1000, imports[0].RowsPerBatch);
                Assert.Equal(10000, imports[0].TotalRows);
                Assert.True(imports[0].Migrated);

                Assert.NotNull(imports[0].Errors);
                Assert.Equal(2, imports[0].Errors.Count);

                Assert.Equal(tableImports[0].Timestamp.UtcDateTime, imports[0].Errors[0].Created);
                Assert.Equal("error1", imports[0].Errors[0].Message);

                Assert.Equal(tableImports[0].Timestamp.UtcDateTime, imports[0].Errors[1].Created);
                Assert.Equal("error2", imports[0].Errors[1].Message);
            }

            MockUtils.VerifyAllMocks(tableStorageService);
        }

        [Fact]
        public async Task MigrateImports_NewDataImportsAreNotTouchedByMigration()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var importedSubjectId = Guid.NewGuid();

            var zipFile = new File
            {
                Filename = "imported1.zip",
                Release = release,
                Type = DataZip
            };

            var dataFile = new File
            {
                Filename = "imported1.csv",
                Release = release,
                Type = FileType.Data,
                Source = zipFile,
                SubjectId = importedSubjectId
            };

            var metaFile = new File
            {
                Filename = "imported1.meta.csv",
                Release = release,
                Type = Metadata,
                SubjectId = importedSubjectId
            };

            // Test the migration won't affect non-migrated imports which may exist from new uploads
            var nonMigratedSubjectId = Guid.NewGuid();
            var nonMigratedImport = new DataImport
            {
                Created = DateTime.UtcNow,
                File = new File
                {
                    Filename = "non-migrated.csv",
                    Release = release,
                    Type = FileType.Data,
                    SubjectId = nonMigratedSubjectId
                },
                MetaFile = new File
                {
                    Filename = "non-migrated.meta.csv",
                    Release = release,
                    Type = Metadata,
                    SubjectId = nonMigratedSubjectId
                },
                Status = DataImportStatus.QUEUED,
                Migrated = false
            };

            var tableImports = new List<TableImport>
            {
                new TableImport
                {
                    PartitionKey = release.Id.ToString(),
                    RowKey = dataFile.Filename,
                    Errors = "",
                    Message = TableImportSampleJson.Message(
                        releaseId: release.Id,
                        subjectId: importedSubjectId,
                        dataFilename: dataFile.Filename,
                        metaFilename: metaFile.Filename,
                        zipFilename: zipFile.Filename
                    ),
                    Status = "COMPLETE",
                    PercentageComplete = 100,
                    NumberOfRows = 10000,
                    Timestamp = DateTimeOffset.Now
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.Files.AddRangeAsync(zipFile, dataFile, metaFile);
                await contentDbContext.DataImports.AddAsync(nonMigratedImport);
                await contentDbContext.SaveChangesAsync();
            }

            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);

            tableStorageService.Setup(service =>
                    service.ExecuteQueryAsync("imports", It.IsAny<TableQuery<TableImport>>()))
                .ReturnsAsync(tableImports);

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildMigrateImportsService(contentDbContext: contentDbContext,
                    tableStorageService: tableStorageService.Object);

                var result = await service.MigrateImports();
                Assert.True(result.IsRight);
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var imports = await contentDbContext.DataImports
                    .Include(i => i.Errors)
                    .ToListAsync();

                Assert.Equal(2, imports.Count);

                // Expect the existing non-migrated import to be first and untouched
                Assert.Equal(nonMigratedImport.Id, imports[0].Id);
                Assert.False(imports[0].Migrated);

                // Expect the migrated import to be next
                Assert.Equal(tableImports[0].Timestamp.UtcDateTime, imports[1].Created);
                Assert.Equal(DataImportStatus.COMPLETE, imports[1].Status);
                Assert.Equal(100, imports[1].StagePercentageComplete);
                Assert.Equal(importedSubjectId, imports[1].SubjectId);
                Assert.Equal(dataFile.Id, imports[1].FileId);
                Assert.Equal(metaFile.Id, imports[1].MetaFileId);
                Assert.Equal(zipFile.Id, imports[1].ZipFileId);
                Assert.Equal(10000, imports[1].Rows);
                Assert.Equal(10, imports[1].NumBatches);
                Assert.Equal(1000, imports[1].RowsPerBatch);
                Assert.Equal(10000, imports[1].TotalRows);
                Assert.True(imports[1].Migrated);

                Assert.NotNull(imports[1].Errors);
                Assert.Empty(imports[1].Errors);
            }

            MockUtils.VerifyAllMocks(tableStorageService);
        }

        private static MigrateImportsService BuildMigrateImportsService(
            ContentDbContext contentDbContext,
            ITableStorageService tableStorageService = null,
            IUserService userService = null)
        {
            return new MigrateImportsService(
                contentDbContext,
                tableStorageService ?? new Mock<ITableStorageService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                new Mock<ILogger<MigrateImportsService>>().Object
            );
        }
    }

    public class TableImportTests
    {
        [Fact]
        public void ImportStatus_StatusIsParsedCorrectly()
        {
            var tableImport = new TableImport
            {
                Status = "COMPLETE"
            };

            Assert.Equal(DataImportStatus.COMPLETE, tableImport.DataImportStatus);
        }

        [Fact]
        public void DataFilename_FieldsAreMappedCorrectly()
        {
            var releaseId = Guid.NewGuid();
            var filename = "data.csv";

            var tableImport = new TableImport
            {
                PartitionKey = releaseId.ToString(),
                RowKey = filename
            };

            Assert.Equal(filename, tableImport.DataFilename);
            Assert.Equal(releaseId, tableImport.ReleaseId);
        }

        [Fact]
        public void ImportErrors_NullIsEmptyList()
        {
            var tableImport = new TableImport
            {
                Errors = null
            };

            Assert.NotNull(tableImport.ImportErrors);
            Assert.Empty(tableImport.ImportErrors);
        }

        [Fact]
        public void ImportErrors_EmptyStringIsEmptyList()
        {
            var tableImport = new TableImport
            {
                Errors = string.Empty
            };

            Assert.NotNull(tableImport.ImportErrors);
            Assert.Empty(tableImport.ImportErrors);
        }

        [Fact]
        public void ImportErrors_EmptyErrorsJsonIsDeserialized()
        {
            var tableImport = new TableImport
            {
                Errors = "[]"
            };

            Assert.NotNull(tableImport.ImportErrors);
            Assert.Empty(tableImport.ImportErrors);
        }

        [Fact]
        public void ImportErrors_MalformedErrorsJsonIsSingleTableImportError()
        {
            var tableImport = new TableImport
            {
                Errors = "malformed errors json causing exception"
            };

            var errors = tableImport.ImportErrors.ToList();
            Assert.Equal("malformed errors json causing exception", errors[0].Message);
        }

        [Fact]
        public void ImportErrors_NonEmptyErrorsJsonIsDeserialized()
        {
            var tableImport = new TableImport
            {
                Errors = @"[{""Message"":""error1""},{""Message"": ""error2""}]"
            };

            var errors = tableImport.ImportErrors.ToList();
            Assert.Equal(2, errors.Count);
            Assert.Equal("error1", errors[0].Message);
            Assert.Equal("error2", errors[1].Message);
        }

        [Fact]
        public void ImportMessage_NullIsNullImportMessage()
        {
            var tableImport = new TableImport
            {
                Message = null
            };

            Assert.Null(tableImport.ImportMessage);
        }

        [Fact]
        public void ImportMessage_EmptyStringIsNullImportMessage()
        {
            var tableImport = new TableImport
            {
                Message = string.Empty
            };

            Assert.Null(tableImport.ImportMessage);
        }

        [Fact]
        public void ImportMessage_MessageJsonIsDeserialized()
        {
            var subjectId = Guid.NewGuid();
            var tableImport = new TableImport
            {
                Message = TableImportSampleJson.Message(
                    releaseId: Guid.NewGuid(),
                    subjectId: subjectId,
                    dataFilename: "data.csv",
                    metaFilename: "data.meta.csv",
                    zipFilename: "data.zip"
                )
            };

            Assert.NotNull(tableImport.ImportMessage);
            Assert.Equal(subjectId, tableImport.ImportMessage.SubjectId);
            Assert.Equal("data.csv", tableImport.ImportMessage.DataFileName);
            Assert.Equal("data.meta.csv", tableImport.ImportMessage.MetaFileName);
            Assert.Equal("data.zip", tableImport.ImportMessage.ArchiveFileName);
            Assert.Equal(10, tableImport.ImportMessage.NumBatches);
            Assert.Equal(1000, tableImport.ImportMessage.RowsPerBatch);
            Assert.Equal(10000, tableImport.ImportMessage.TotalRows);
        }
    }

    public static class TableImportSampleJson
    {
        public static string Message(Guid releaseId,
            Guid subjectId,
            string dataFilename,
            string metaFilename,
            string zipFilename)
        {
            return @$"
{{
  ""SubjectId"": ""{subjectId}"",
  ""DataFileName"": ""{dataFilename}"",
  ""MetaFileName"": ""{metaFilename}"",
  ""Release"": {{
    ""Id"": ""{releaseId}"",
    ""Slug"": ""2076-77"",
    ""Publication"": {{
      ""Id"": ""a325a5b9-55c4-4282-17a9-08d8b9416e3d"",
      ""Title"": ""importer-test_20210115-115800"",
      ""Slug"": ""importer-test-20210115-115800"",
      ""Topic"": {{
        ""Id"": ""3cd51f1d-6d9a-4c1e-3bfd-08d8b3c1d3dc"",
        ""Title"": ""importer topic"",
        ""Slug"": ""importer-topic"",
        ""Theme"": {{
          ""Id"": ""af61a0bc-80e7-4893-8ba2-08d8b3c1f892"",
          ""Title"": ""Importer theme"",
          ""Slug"": ""importer-theme""
        }}
      }}
    }},
    ""TimeIdentifier"": 0,
    ""Year"": 2076,
    ""PreviousVersionId"": null
  }},
  ""NumBatches"": 10,
  ""BatchNo"": 1,
  ""RowsPerBatch"": 1000,
  ""Seeding"": false,
  ""TotalRows"": 10000,
  ""ArchiveFileName"": ""{zipFilename}""
}}
";
        }
    }
}
