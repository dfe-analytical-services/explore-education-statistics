using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

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
                Id = Guid.Empty,
                Slug = "test-release",
                Publication = testPublication,
                TimeIdentifier = TimeIdentifier.CalendarYear,
                Year = 2000
            };

            var testImportMessage1 = new ImportMessage
            {
                SubjectId = Guid.NewGuid(),
                DataFileName = "one.csv",
                OrigDataFileName = "one.csv",
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
                OrigDataFileName = "two.csv",
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
                PartitionKey = Guid.NewGuid().ToString(),
                RowKey = "one.csv",
                NumberOfRows = 1000000,
                Message = JsonConvert.SerializeObject(testImportMessage1),
                Status = IStatus.FAILED,
                Errors = "",
                PercentageComplete = 99,
            };

            var testDatafileImport2 = new DatafileImport
            {
                PartitionKey = Guid.NewGuid().ToString(),
                RowKey = "two.csv",
                NumberOfRows = 123,
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

            var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);
            tableStorageService
                .Setup(storageService =>
                    storageService.ExecuteQueryAsync(DatafileImportsTableName,
                        It.IsAny<TableQuery<DatafileImport>>()))
                .ReturnsAsync(queryResults);
            var importStatusBauService = BuildImportStatusBauService(tableStorageService.Object);

            var result = await importStatusBauService.GetAllIncompleteImports();
            Assert.True(result.IsRight);
            Assert.Equal(2, result.Right.Count);

            var result1 = result.Right.Find(r => r.SubjectId.ToString() == testDatafileImport1.PartitionKey);
            Assert.NotNull(result1);
            Assert.Equal(testDatafileImport1.RowKey, result1.DataFileName);
            Assert.Equal(testImportMessage1.MetaFileName, result1.MetaFileName);
            Assert.Equal(JsonConvert.SerializeObject(testRelease),
                JsonConvert.SerializeObject(result1.Release));
            Assert.Equal(testDatafileImport1.Errors, result1.Errors);
            Assert.Equal(testDatafileImport1.NumberOfRows, result1.NumberOfRows);
            Assert.Equal(testDatafileImport1.Status, result1.Status);
            Assert.Equal(testDatafileImport1.PercentageComplete, result1.StagePercentageComplete);

            var result2 = result.Right.Find(r => r.SubjectId.ToString() == testDatafileImport2.PartitionKey);
            Assert.NotNull(result2);
            Assert.Equal(testDatafileImport2.RowKey, result2.DataFileName);
            Assert.Equal(testImportMessage2.MetaFileName, result2.MetaFileName);
            Assert.Equal(JsonConvert.SerializeObject(testRelease),
                JsonConvert.SerializeObject(result2.Release));
            Assert.Equal(testDatafileImport2.Errors, result2.Errors);
            Assert.Equal(testDatafileImport2.NumberOfRows, result2.NumberOfRows);
            Assert.Equal(testDatafileImport2.Status, result2.Status);
            Assert.Equal(testDatafileImport2.PercentageComplete, result2.StagePercentageComplete);
        }

        internal static ImportStatusBauService BuildImportStatusBauService(
            ITableStorageService tableStorageService,
            ILogger<ImportStatusBauService> logger = null,
            IUserService userService = null)
        {
            return new ImportStatusBauService(
                tableStorageService,
                logger ?? new Mock<ILogger<ImportStatusBauService>>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}
