using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Strategies;

public abstract class AnalyticsWritePublicationCallsStrategyTests
{
    public class ReportTests : AnalyticsWritePublicationCallsStrategyTests
    {
        private const string SnapshotPrefix = $"{nameof(AnalyticsWritePublicationCallsStrategyTests)}.{nameof(ReportTests)}";
        
        [Fact]
        public async Task CallWrittenSuccessfully_WithCorePublicationDetails()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            
            var strategy = BuildStrategy(
                pathResolver: pathResolver,
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2025-03-16T12:01:02Z")));
            
            await strategy.Report(new CapturePublicationCallRequest(
                PublicationId: new Guid("01d29401-7274-a871-a8db-d4bc4e98c324"),
                PublicationTitle: "Publication 1",
                StartTime: DateTime.Parse("2025-02-28T03:07:44.850Z"),
                Type: PublicationCallType.GetSummary), default);
            
            var files = Directory
                .GetFiles(pathResolver.PublicApiPublicationCallsDirectoryPath());
            
            var filePath = Assert.Single(files);

            var filename = filePath
                .Split($"{pathResolver.PublicApiPublicationCallsDirectoryPath()}{Path.DirectorySeparatorChar}")[1];
            Assert.StartsWith("20250316-120102_", filename);

            Snapshot.Match(
                currentResult: await File.ReadAllTextAsync(filePath),
                snapshotName: $"{SnapshotPrefix}.{nameof(CallWrittenSuccessfully_WithCorePublicationDetails)}");
        }
        
        [Fact]
        public async Task CallWrittenSuccessfully_WithParameters()
        {
            using var pathResolver = new TestAnalyticsPathResolver();
            
            var strategy = BuildStrategy(
                pathResolver: pathResolver,
                dateTimeProvider: new DateTimeProvider(DateTime.Parse("2025-03-16T12:01:02Z")));
            
            await strategy.Report(new CapturePublicationCallRequest(
                PublicationId: new Guid("01d29401-7274-a871-a8db-d4bc4e98c324"),
                PublicationTitle: "Publication 1",
                StartTime: DateTime.Parse("2025-02-26T03:07:44.850Z"),
                Parameters: new PaginationParameters(Page: 1, PageSize: 10),
                Type: PublicationCallType.GetDataSets), default);
            
            var files = Directory
                .GetFiles(pathResolver.PublicApiPublicationCallsDirectoryPath());
            
            var filePath = Assert.Single(files);

            var filename = filePath
                .Split($"{pathResolver.PublicApiPublicationCallsDirectoryPath()}{Path.DirectorySeparatorChar}")[1];
            Assert.StartsWith("20250316-120102_", filename);
            Assert.StartsWith("20250316-120102_", filename);

            Snapshot.Match(
                currentResult: await File.ReadAllTextAsync(filePath),
                snapshotName: $"{SnapshotPrefix}.{nameof(CallWrittenSuccessfully_WithParameters)}");
        }
        
        private static AnalyticsWritePublicationCallsStrategy BuildStrategy(
            IAnalyticsPathResolver pathResolver,
            DateTimeProvider? dateTimeProvider = null)
        {
            return new AnalyticsWritePublicationCallsStrategy(
                pathResolver,
                new CommonAnalyticsWriteStrategyWorkflow<CapturePublicationCallRequest>(
                    dateTimeProvider: dateTimeProvider ?? new DateTimeProvider(),
                    Mock.Of<ILogger<CommonAnalyticsWriteStrategyWorkflow<CapturePublicationCallRequest>>>()));
        }
    }
}
