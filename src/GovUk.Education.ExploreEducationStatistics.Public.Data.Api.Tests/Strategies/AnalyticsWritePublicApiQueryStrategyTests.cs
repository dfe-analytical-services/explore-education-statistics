using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Strategies;

public abstract class AnalyticsWritePublicApiQueryStrategyTests
{
    public class ReportTests : AnalyticsWritePublicApiQueryStrategyTests
    {
        private const string SnapshotPrefix = $"{nameof(AnalyticsWritePublicApiQueryStrategyTests)}.{nameof(ReportTests)}";
        
        [Fact]
        public async Task Success()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            var service = BuildStrategy(pathResolver);

            await service.Report(new CaptureDataSetVersionQueryRequest(
                DataSetId: new Guid("acb97c97-89c9-4b74-88e7-39c27f6bab63"),
                DataSetVersionId: new Guid("0da7c640-80a8-44e2-8028-fc529bcedcb1"),
                DataSetVersion: "2.3.1",
                DataSetTitle: "Data Set Title",
                Query: DataSetQueryRequestTestData.NestedQuery1,
                ResultsCount: 55,
                TotalRowsCount: 5100,
                StartTime: DateTime.Parse("2025-02-20T12:00:00.000Z"),
                EndTime: DateTime.Parse("2025-02-20T12:00:10.234Z")), default);
            
            await service.Report(new CaptureDataSetVersionQueryRequest(
                DataSetId: new Guid("72e16c8c-2dc0-4063-bdf6-ee52bd127ebe"),
                DataSetVersionId: new Guid("bb68fd95-1231-498c-8858-b061d739ae17"),
                DataSetVersion: "3.0.0",
                DataSetTitle: "Data Set Title 2",
                Query: DataSetQueryRequestTestData.NestedQuery2,
                ResultsCount: 120,
                TotalRowsCount: 10000,
                StartTime: DateTime.Parse("2025-02-20T01:00:00.000Z"),
                EndTime: DateTime.Parse("2025-02-20T01:00:10.999Z")), default);

            var files = Directory
                .GetFiles(pathResolver.PublicApiQueriesDirectoryPath())
                .Order()
                .ToList();
            
            Assert.Equal(2, files.Count);
            
            var file1Contents = await File.ReadAllTextAsync(files[0]);
            Snapshot.Match(
                currentResult: file1Contents,
                snapshotName: $"{SnapshotPrefix}.{nameof(Success)}.Query1");
            
            var file2Contents = await File.ReadAllTextAsync(files[1]);
            Snapshot.Match(
                currentResult: file2Contents,
                snapshotName: $"{SnapshotPrefix}.{nameof(Success)}.Query2");
        }

        private static AnalyticsWritePublicApiQueryStrategy BuildStrategy(
            IAnalyticsPathResolver pathResolver,
            DateTimeProvider? dateTimeProvider = null)
        {
            return new AnalyticsWritePublicApiQueryStrategy(
                pathResolver,
                dateTimeProvider ?? new DateTimeProvider(),
                Mock.Of<ILogger<AnalyticsWritePublicApiQueryStrategy>>());
        }
    }
}
