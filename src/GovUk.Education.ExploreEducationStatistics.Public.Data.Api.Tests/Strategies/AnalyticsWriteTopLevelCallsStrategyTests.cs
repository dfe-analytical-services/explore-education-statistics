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

public abstract class AnalyticsWriteTopLevelCallsStrategyTests
{
    public class ReportTests : AnalyticsWriteTopLevelCallsStrategyTests
    {
        private const string SnapshotPrefix = $"{nameof(AnalyticsWriteTopLevelCallsStrategyTests)}.{nameof(ReportTests)}";
        
        [Fact]
        public async Task CallWrittenSuccessfully_WithCoreTopLevelDetails()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            
            var strategy = BuildStrategy(
                pathResolver: pathResolver,
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2025-03-16T12:01:02Z")));
            
            await strategy.Report(new CaptureTopLevelCallRequest(
                StartTime: DateTime.Parse("2025-02-28T03:07:44.850Z"),
                Type: TopLevelCallType.GetPublications), default);
            
            var files = Directory
                .GetFiles(pathResolver.PublicApiTopLevelCallsDirectoryPath());
            
            var filePath = Assert.Single(files);

            var filename = filePath
                .Split($"{pathResolver.PublicApiTopLevelCallsDirectoryPath()}{Path.DirectorySeparatorChar}")[1];
            Assert.StartsWith("20250316-120102_", filename);

            Snapshot.Match(
                currentResult: await File.ReadAllTextAsync(filePath),
                snapshotName: $"{SnapshotPrefix}.{nameof(CallWrittenSuccessfully_WithCoreTopLevelDetails)}");
        }
        
        [Fact]
        public async Task CallWrittenSuccessfully_WithParameters()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            
            var strategy = BuildStrategy(
                pathResolver: pathResolver,
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2025-03-16T12:01:02Z")));
            
            await strategy.Report(new CaptureTopLevelCallRequest(
                StartTime: DateTime.Parse("2025-02-26T03:07:44.850Z"),
                Parameters: new PaginationParameters(Page: 1, PageSize: 10),
                Type: TopLevelCallType.GetPublications), default);
            
            var files = Directory
                .GetFiles(pathResolver.PublicApiTopLevelCallsDirectoryPath());
            
            var filePath = Assert.Single(files);

            var filename = filePath
                .Split($"{pathResolver.PublicApiTopLevelCallsDirectoryPath()}{Path.DirectorySeparatorChar}")[1];
            Assert.StartsWith("20250316-120102_", filename);
            Assert.StartsWith("20250316-120102_", filename);

            Snapshot.Match(
                currentResult: await File.ReadAllTextAsync(filePath),
                snapshotName: $"{SnapshotPrefix}.{nameof(CallWrittenSuccessfully_WithParameters)}");
        }
        
        private static AnalyticsWriteTopLevelCallsStrategy BuildStrategy(
            IAnalyticsPathResolver pathResolver,
            DateTimeProvider? dateTimeProvider = null)
        {
            return new AnalyticsWriteTopLevelCallsStrategy(
                pathResolver,
                new CommonAnalyticsWriteStrategyWorkflow<CaptureTopLevelCallRequest>(
                    dateTimeProvider: dateTimeProvider ?? new DateTimeProvider(),
                    Mock.Of<ILogger<CommonAnalyticsWriteStrategyWorkflow<CaptureTopLevelCallRequest>>>()));
        }
    }
}
