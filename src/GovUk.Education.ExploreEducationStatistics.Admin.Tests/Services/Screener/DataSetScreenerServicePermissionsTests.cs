using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Screener;

public class DataSetScreenerServicePermissionsTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task GetScreenerProgress()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion();

        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseVersions.Add(releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PermissionTestUtils
            .PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(releaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion)
            .AssertForbidden(userService =>
            {
                var service = BuildService(userService: userService.Object, contentDbContext: contentDbContext);
                return service.GetScreenerProgress(releaseVersion.Id, CancellationToken.None);
            });
    }

    private IDataSetScreenerService BuildService(IUserService userService, ContentDbContext contentDbContext)
    {
        return new DataSetScreenerService(
            dataSetScreenerClient: Mock.Of<IDataSetScreenerClient>(MockBehavior.Strict),
            queueServiceClient: Mock.Of<IQueueServiceClient>(MockBehavior.Strict),
            userService: userService,
            contentDbContext: contentDbContext,
            timeProvider: TimeProvider.System,
            mapper: MapperUtils.AdminMapper(),
            options: new DataScreenerOptions().ToOptionsWrapper()
        );
    }
}
