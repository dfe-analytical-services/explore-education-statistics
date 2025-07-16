using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Strategies;

public abstract class AnalyticsWriteDataSetCallsStrategyTests
{
    public class ReportTests : AnalyticsWriteDataSetCallsStrategyTests
    {
        private const string SnapshotPrefix = $"{nameof(AnalyticsWriteDataSetCallsStrategyTests)}.{nameof(ReportTests)}";
        
        [Fact]
        public async Task CallWrittenSuccessfully_WithCoreDataSetDetails()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            
            var strategy = BuildStrategy(
                pathResolver: pathResolver,
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2025-03-16T12:01:02Z")));
            
            await strategy.Report(new CaptureDataSetCallRequest(
                DataSetId: new Guid("01d29401-7274-a871-a8db-d4bc4e98c324"),
                DataSetTitle: "Data Set 1",
                StartTime: DateTime.Parse("2025-02-28T03:07:44.850Z"),
                PreviewToken: null,
                Type: DataSetCallType.GetSummary), default);
            
            var files = Directory
                .GetFiles(pathResolver.BuildOutputDirectory(AnalyticsWriteDataSetCallsStrategy.OutputSubPaths));
            
            var filePath = Assert.Single(files);

            var filename = filePath
                .Split($"{pathResolver.BuildOutputDirectory(AnalyticsWriteDataSetCallsStrategy.OutputSubPaths)}{Path.DirectorySeparatorChar}")[1];
            Assert.StartsWith("20250316-120102_", filename);

            Snapshot.Match(
                currentResult: await File.ReadAllTextAsync(filePath),
                snapshotName: $"{SnapshotPrefix}.{nameof(CallWrittenSuccessfully_WithCoreDataSetDetails)}");
        }
        
        [Fact]
        public async Task CallWrittenSuccessfully_WithPreviewToken()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            
            var strategy = BuildStrategy(
                pathResolver: pathResolver,
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2025-03-16T12:01:02Z")));
            
            await strategy.Report(new CaptureDataSetCallRequest(
                DataSetId: new Guid("01d29401-7274-a871-a8db-d4bc4e98c324"),
                DataSetTitle: "Data Set 1",
                StartTime: DateTime.Parse("2025-02-26T03:07:44.850Z"),
                PreviewToken: new PreviewTokenRequest(
                    Label: "Preview token content",
                    DataSetVersionId: new Guid("01d29401-7974-1276-a06b-b28a6a5385c6"),
                    Created: DateTime.Parse("2025-02-23T11:02:44.850Z"),
                    Expiry: DateTime.Parse("2025-02-24T11:02:44.850Z")),
                Type: DataSetCallType.GetSummary), default);
            
            var files = Directory
                .GetFiles(pathResolver.BuildOutputDirectory(AnalyticsWriteDataSetCallsStrategy.OutputSubPaths));
            
            var filePath = Assert.Single(files);

            var filename = filePath
                .Split($"{pathResolver.BuildOutputDirectory(AnalyticsWriteDataSetCallsStrategy.OutputSubPaths)}{Path.DirectorySeparatorChar}")[1];
            Assert.StartsWith("20250316-120102_", filename);
            Assert.StartsWith("20250316-120102_", filename);

            Snapshot.Match(
                currentResult: await File.ReadAllTextAsync(filePath),
                snapshotName: $"{SnapshotPrefix}.{nameof(CallWrittenSuccessfully_WithPreviewToken)}");
        }
        
        [Fact]
        public async Task CallWrittenSuccessfully_WithParameters()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            
            var strategy = BuildStrategy(
                pathResolver: pathResolver,
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2025-03-16T12:01:02Z")));
            
            await strategy.Report(new CaptureDataSetCallRequest(
                DataSetId: new Guid("01d29401-7274-a871-a8db-d4bc4e98c324"),
                DataSetTitle: "Data Set 1",
                StartTime: DateTime.Parse("2025-02-26T03:07:44.850Z"),
                Parameters: new PaginationParameters(Page: 1, PageSize: 10),
                Type: DataSetCallType.GetVersions,
                PreviewToken: null), default);
            
            var files = Directory
                .GetFiles(pathResolver.BuildOutputDirectory(AnalyticsWriteDataSetCallsStrategy.OutputSubPaths));
            
            var filePath = Assert.Single(files);

            var filename = filePath
                .Split($"{pathResolver.BuildOutputDirectory(AnalyticsWriteDataSetCallsStrategy.OutputSubPaths)}{Path.DirectorySeparatorChar}")[1];
            Assert.StartsWith("20250316-120102_", filename);
            Assert.StartsWith("20250316-120102_", filename);

            Snapshot.Match(
                currentResult: await File.ReadAllTextAsync(filePath),
                snapshotName: $"{SnapshotPrefix}.{nameof(CallWrittenSuccessfully_WithParameters)}");
        }
        
        private static AnalyticsWriteDataSetCallsStrategy BuildStrategy(
            IAnalyticsPathResolver pathResolver,
            DateTimeProvider? dateTimeProvider = null)
        {
            return new AnalyticsWriteDataSetCallsStrategy(
                pathResolver,
                new CommonAnalyticsWriteStrategyWorkflow<CaptureDataSetCallRequest>(
                    dateTimeProvider: dateTimeProvider ?? new DateTimeProvider(),
                    Mock.Of<ILogger<CommonAnalyticsWriteStrategyWorkflow<CaptureDataSetCallRequest>>>()));
        }
    }
}
