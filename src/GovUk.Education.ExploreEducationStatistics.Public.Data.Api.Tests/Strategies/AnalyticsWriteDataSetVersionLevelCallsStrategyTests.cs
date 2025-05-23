using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Strategies;

public abstract class AnalyticsWriteDataSetVersionCallsStrategyTests
{
    public class ReportTests : AnalyticsWriteDataSetVersionCallsStrategyTests
    {
        private const string SnapshotPrefix = $"{nameof(AnalyticsWriteDataSetVersionCallsStrategyTests)}.{nameof(ReportTests)}";
        
        [Fact]
        public async Task CallWrittenSuccessfully_WithCoreDataSetVersionDetails()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            
            var strategy = BuildStrategy(
                pathResolver: pathResolver,
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2025-03-16T12:01:02Z")));
            
            await strategy.Report(new CaptureDataSetVersionCallRequest(
                DataSetId: new Guid("01d29401-7274-a871-a8db-d4bc4e98c324"),
                DataSetVersionId: new Guid("01d29401-7974-1276-a06b-b28a6a5385c6"),
                DataSetVersion: "1.2.0",
                DataSetTitle: "Data Set 1",
                StartTime: DateTime.Parse("2025-02-28T03:07:44.850Z"),
                Parameters: null,
                PreviewToken: null,
                RequestedDataSetVersion: null,
                Type: DataSetVersionCallType.GetSummary), default);
            
            var files = Directory
                .GetFiles(pathResolver.PublicApiDataSetVersionCallsDirectoryPath());
            
            var filePath = Assert.Single(files);

            var filename = filePath
                .Split($"{pathResolver.PublicApiDataSetVersionCallsDirectoryPath()}{Path.DirectorySeparatorChar}")[1];
            Assert.StartsWith("20250316-120102_", filename);

            Snapshot.Match(
                currentResult: await File.ReadAllTextAsync(filePath),
                snapshotName: $"{SnapshotPrefix}.{nameof(CallWrittenSuccessfully_WithCoreDataSetVersionDetails)}");
        }
        
        [Fact]
        public async Task CallWrittenSuccessfully_WithPreviewTokenAndRequestedDataSetVersion()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            
            var strategy = BuildStrategy(
                pathResolver: pathResolver,
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2025-03-16T12:01:02Z")));
            
            await strategy.Report(new CaptureDataSetVersionCallRequest(
                DataSetId: new Guid("01d29401-7274-a871-a8db-d4bc4e98c324"),
                DataSetVersionId: new Guid("01d29401-7974-1276-a06b-b28a6a5385c6"),
                DataSetVersion: "1.2.0",
                DataSetTitle: "Data Set 1",
                StartTime: DateTime.Parse("2025-02-26T03:07:44.850Z"),
                Parameters: null,
                PreviewToken: new PreviewTokenRequest(
                    Label: "Preview token content",
                    DataSetVersionId: new Guid("01d29401-7974-1276-a06b-b28a6a5385c6"),
                    Created: DateTime.Parse("2025-02-23T11:02:44.850Z"),
                    Expiry: DateTime.Parse("2025-02-24T11:02:44.850Z")),
                RequestedDataSetVersion: "1.*",
                Type: DataSetVersionCallType.DownloadCsv), default);
            
            var files = Directory
                .GetFiles(pathResolver.PublicApiDataSetVersionCallsDirectoryPath());
            
            var filePath = Assert.Single(files);

            var filename = filePath
                .Split($"{pathResolver.PublicApiDataSetVersionCallsDirectoryPath()}{Path.DirectorySeparatorChar}")[1];
            Assert.StartsWith("20250316-120102_", filename);

            Snapshot.Match(
                currentResult: await File.ReadAllTextAsync(filePath),
                snapshotName: $"{SnapshotPrefix}.{nameof(CallWrittenSuccessfully_WithPreviewTokenAndRequestedDataSetVersion)}");
        }
        
        [Fact]
        public async Task CallWrittenSuccessfully_WithParameters()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            
            var strategy = BuildStrategy(pathResolver);

            await strategy.Report(new CaptureDataSetVersionCallRequest(
                DataSetId: new Guid("01d29401-7274-a871-a8db-d4bc4e98c324"),
                DataSetVersionId: new Guid("01d29401-7974-1276-a06b-b28a6a5385c6"),
                DataSetVersion: "1.2.0",
                DataSetTitle: "Data Set 1",
                StartTime: DateTime.Parse("2025-02-24T03:07:44.850Z"),
                Parameters: new GetMetadataAnalyticsParameters(
                    Types: [
                        DataSetMetaType.Filters,
                        DataSetMetaType.Indicators,
                        DataSetMetaType.Locations,
                        DataSetMetaType.TimePeriods
                    ]),
                PreviewToken: null,
                RequestedDataSetVersion: null,
                Type: DataSetVersionCallType.GetMetadata), default);

            var filePath = Assert.Single(Directory
                .GetFiles(pathResolver.PublicApiDataSetVersionCallsDirectoryPath()));
            
            Snapshot.Match(
                currentResult: await File.ReadAllTextAsync(filePath),
                snapshotName: $"{SnapshotPrefix}.{nameof(CallWrittenSuccessfully_WithParameters)}");
        }

        private static AnalyticsWriteDataSetVersionCallsStrategy BuildStrategy(
            IAnalyticsPathResolver pathResolver,
            DateTimeProvider? dateTimeProvider = null)
        {
            return new AnalyticsWriteDataSetVersionCallsStrategy(
                pathResolver,
                dateTimeProvider ?? new DateTimeProvider(),
                Mock.Of<ILogger<AnalyticsWriteDataSetVersionCallsStrategy>>());
        }
    }
}
