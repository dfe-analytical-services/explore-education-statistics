using GovUk.Education.ExploreEducationStatistics.Analytics.Service.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;

public abstract class AnalyticsServiceTests
{
    public class ReportDataSetVersionQuery : AnalyticsServiceTests
    {
        private const string SnapshotPrefix = $"{nameof(AnalyticsServiceTests)}.{nameof(ReportDataSetVersionQuery)}";
        
        [Fact]
        public async Task Success()
        {
            var pathResolver = new TestAnalyticsPathResolver();
            var service = BuildService(pathResolver);

            await service.ReportDataSetVersionQuery(
                dataSetId: new Guid("acb97c97-89c9-4b74-88e7-39c27f6bab63"),
                dataSetVersionId: new Guid("0da7c640-80a8-44e2-8028-fc529bcedcb1"),
                semVersion: "2.3.1",
                dataSetTitle: "Data Set Title",
                query: DataSetQueryRequestTestData.NestedQuery1,
                resultsCount: 55,
                totalRowsCount: 5100,
                startTime: DateTime.Parse("2025-02-20T12:00:00.000Z"),
                endTime: DateTime.Parse("2025-02-20T12:00:10.234Z"));
            
            await service.ReportDataSetVersionQuery(
                dataSetId: new Guid("72e16c8c-2dc0-4063-bdf6-ee52bd127ebe"),
                dataSetVersionId: new Guid("bb68fd95-1231-498c-8858-b061d739ae17"),
                semVersion: "3.0.0",
                dataSetTitle: "Data Set Title 2",
                query: DataSetQueryRequestTestData.NestedQuery2,
                resultsCount: 120,
                totalRowsCount: 10000,
                startTime: DateTime.Parse("2025-02-20T01:00:00.000Z"),
                endTime: DateTime.Parse("2025-02-20T01:00:10.999Z"));

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

        private static AnalyticsService BuildService(IAnalyticsPathResolver pathResolver)
        {
            return new(pathResolver, Mock.Of<ILogger<AnalyticsService>>());
        }
    }
}
