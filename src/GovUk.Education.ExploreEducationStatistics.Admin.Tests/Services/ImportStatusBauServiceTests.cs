using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;
using Publication = GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.Publication;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.Release;
using Theme = GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.Theme;
using Topic = GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.Topic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ImportStatusBauServiceTests
    {
        [Fact]
        public async Task GetIncompleteImports_NoResults()
        {
            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);
            tableStorageService
                .Setup(storageService =>
                    storageService.ExecuteQueryAsync(DatafileImportsTableName,
                        It.IsAny<TableQuery<DatafileImport>>()))
                .ReturnsAsync(new List<DatafileImport>());
            var importStatusBauService = BuildImportStatusBauService(tableStorageService.Object);
            var result = await importStatusBauService.GetAllIncompleteImports();
            Assert.True(result.IsRight);
            Assert.Empty(result.Right);
        }

        [Fact]
        public async Task GetIncompleteImports_TwoResults()
        {
            var testTheme = new Theme
            {
                Id = Guid.NewGuid(),
                Title = "Test theme",
                Slug = "test-theme"
            };

            var testTopic = new Topic
            {
                Id = Guid.NewGuid(),
                Title = "Test topic",
                Slug = "test-topic",
                Theme = testTheme
            };

            var testPublication = new Publication
            {
                Id = Guid.NewGuid(),
                Title = "Test Publication",
                Slug = "test-publication",
                Topic = testTopic
            };

            var testRelease = new Release
            {
                Id = Guid.NewGuid(),
                Slug = "test-release",
                Publication = testPublication,
                TimeIdentifier = TimeIdentifier.CalendarYear,
                Year = 2000
            };

            var testImportMessage1 = new ImportMessage
            {
                SubjectId = Guid.NewGuid(),
                DataFileName = "one.csv",
                MetaFileName = "one.meta.csv",
                Release = testRelease,
                NumBatches = 0,
                BatchNo = 0,
                RowsPerBatch = 0,
                Seeding = false,
                TotalRows = 0,
                ArchiveFileName = "",
            };

            var testImportMessage2 = new ImportMessage
            {
                SubjectId = Guid.NewGuid(),
                DataFileName = "two.csv",
                MetaFileName = "two.meta.csv",
                Release = testRelease,
                NumBatches = 0,
                BatchNo = 0,
                RowsPerBatch = 0,
                Seeding = false,
                TotalRows = 0,
                ArchiveFileName = "",
            };

            var testDatafileImport1 = new DatafileImport
            {
                PartitionKey = testRelease.Id.ToString(),
                RowKey = testImportMessage1.DataFileName,
                NumberOfRows = testImportMessage1.TotalRows,
                Message = JsonConvert.SerializeObject(testImportMessage1),
                Status = IStatus.FAILED,
                Errors = "",
                PercentageComplete = 99,
            };

            var testDatafileImport2 = new DatafileImport
            {
                PartitionKey = testRelease.Id.ToString(),
                RowKey = testImportMessage2.DataFileName,
                NumberOfRows = testImportMessage2.TotalRows,
                Message = JsonConvert.SerializeObject(testImportMessage2),
                Status = IStatus.STAGE_1,
                Errors = "Test error!",
                PercentageComplete = 54,
            };

            var queryResults = new List<DatafileImport>()
            {
                testDatafileImport1,
                testDatafileImport2
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = testRelease.Id,
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    PublicationId = testPublication.Id,
                    Publication = new Content.Model.Publication
                    {
                        Id = testPublication.Id,
                        Title = testPublication.Title,
                    },
                    ReleaseName = testRelease.Year.ToString(),
                });
                await contentDbContext.AddAsync(new File
                {
                    ReleaseId = testRelease.Id,
                    SubjectId = testImportMessage1.SubjectId,
                    Filename = testDatafileImport1.RowKey,
                    Type = FileType.Data
                });
                await contentDbContext.AddAsync(new File
                {
                    ReleaseId = testRelease.Id,
                    SubjectId = testImportMessage2.SubjectId,
                    Filename = testDatafileImport2.RowKey,
                    Type = FileType.Data,
                });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);
                tableStorageService
                    .Setup(storageService =>
                        storageService.ExecuteQueryAsync(DatafileImportsTableName,
                            It.IsAny<TableQuery<DatafileImport>>()))
                    .ReturnsAsync(queryResults);
                var importStatusBauService =
                    BuildImportStatusBauService(tableStorageService.Object, contentDbContext: contentDbContext);

                var result = await importStatusBauService.GetAllIncompleteImports();
                Assert.True(result.IsRight);
                Assert.Equal(2, result.Right.Count);

                var result1 = result.Right.Find(r => r.SubjectId == testImportMessage1.SubjectId);
                Assert.NotNull(result1);
                Assert.Equal(testPublication.Id, result1.PublicationId);
                Assert.Equal(testPublication.Title, result1.PublicationTitle);
                Assert.Equal(testRelease.Id, result1.ReleaseId);
                Assert.Equal("Calendar Year 2000", result1.ReleaseTitle);
                Assert.Equal(testDatafileImport1.RowKey, result1.DataFileName);
                Assert.Equal(testDatafileImport1.NumberOfRows, result1.NumberOfRows);
                Assert.Equal(testDatafileImport1.Status, result1.Status);
                Assert.Equal(testDatafileImport1.PercentageComplete, result1.StagePercentageComplete);

                var result2 = result.Right.Find(r => r.SubjectId == testImportMessage2.SubjectId);
                Assert.NotNull(result2);
                Assert.Equal(testPublication.Id, result2.PublicationId);
                Assert.Equal(testPublication.Title, result2.PublicationTitle);
                Assert.Equal(testRelease.Id, result2.ReleaseId);
                Assert.Equal($"Calendar Year 2000", result2.ReleaseTitle);
                Assert.Equal(testDatafileImport2.RowKey, result2.DataFileName);
                Assert.Equal(testDatafileImport2.NumberOfRows, result2.NumberOfRows);
                Assert.Equal(testDatafileImport2.Status, result2.Status);
                Assert.Equal(testDatafileImport2.PercentageComplete, result2.StagePercentageComplete);
            }
        }

        [Fact]
        public async Task GetIncompleteImports_NoReleaseFileReferencesTableRow()
        {
            var testTheme = new Theme
            {
                Id = Guid.NewGuid(),
                Title = "Test theme",
                Slug = "test-theme"
            };

            var testTopic = new Topic
            {
                Id = Guid.NewGuid(),
                Title = "Test topic",
                Slug = "test-topic",
                Theme = testTheme
            };

            var testPublication = new Publication
            {
                Id = Guid.NewGuid(),
                Title = "Test Publication",
                Slug = "test-publication",
                Topic = testTopic
            };

            var testRelease = new Release
            {
                Id = Guid.NewGuid(),
                Slug = "test-release",
                Publication = testPublication,
                TimeIdentifier = TimeIdentifier.CalendarYear,
                Year = 2000
            };

            var testImportMessage1 = new ImportMessage
            {
                SubjectId = Guid.NewGuid(),
                DataFileName = "one.csv",
                MetaFileName = "one.meta.csv",
                Release = testRelease,
                NumBatches = 0,
                BatchNo = 0,
                RowsPerBatch = 0,
                Seeding = false,
                TotalRows = 0,
                ArchiveFileName = "",
            };

            var testDatafileImport1 = new DatafileImport
            {
                PartitionKey = testRelease.Id.ToString(),
                RowKey = testImportMessage1.DataFileName,
                NumberOfRows = testImportMessage1.TotalRows,
                Message = JsonConvert.SerializeObject(testImportMessage1),
                Status = IStatus.FAILED,
                Errors = "",
                PercentageComplete = 99,
            };

            var queryResults = new List<DatafileImport>()
            {
                testDatafileImport1,
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = testRelease.Id,
                    TimePeriodCoverage = TimeIdentifier.CalendarYear,
                    PublicationId = testPublication.Id,
                    Publication = new Content.Model.Publication
                    {
                        Id = testPublication.Id,
                        Title = testPublication.Title,
                    },
                    ReleaseName = testRelease.Year.ToString(),
                });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);
                tableStorageService
                    .Setup(storageService =>
                        storageService.ExecuteQueryAsync(DatafileImportsTableName,
                            It.IsAny<TableQuery<DatafileImport>>()))
                    .ReturnsAsync(queryResults);
                var importStatusBauService =
                    BuildImportStatusBauService(tableStorageService.Object, contentDbContext: contentDbContext);

                var result = await importStatusBauService.GetAllIncompleteImports();
                Assert.True(result.IsRight);
                Assert.Empty(result.Right);
            }
        }

        internal static ImportStatusBauService BuildImportStatusBauService(
            ITableStorageService tableStorageService,
            IUserService userService = null,
            ContentDbContext contentDbContext = null,
            ILogger<ImportStatusBauService> logger = null)
        {
            return new ImportStatusBauService(
                tableStorageService,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                logger ?? new Mock<ILogger<ImportStatusBauService>>().Object
            );
        }
    }
}
