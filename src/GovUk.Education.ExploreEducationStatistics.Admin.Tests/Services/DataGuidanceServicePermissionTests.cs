#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataGuidanceServicePermissionTests
{
    private readonly ReleaseVersion _releaseVersion = new()
    {
        Id = Guid.NewGuid()
    };

    [Fact]
    public async Task GetDataGuidance()
    {
        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseVersions.Add(_releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(contentDbContext: contentDbContext,
                        userService: userService.Object);
                    return service.GetDataGuidance(_releaseVersion.Id);
                }
            );
    }

    [Fact]
    public async Task UpdateDataGuidance()
    {
        await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
        contentDbContext.ReleaseVersions.Add(_releaseVersion);
        await contentDbContext.SaveChangesAsync();

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, SecurityPolicies.CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(contentDbContext: contentDbContext,
                        userService: userService.Object);
                    return service.UpdateDataGuidance(_releaseVersion.Id, new DataGuidanceUpdateRequest());
                }
            );
    }

    private static DataGuidanceService SetupService(
        ContentDbContext? contentDbContext = null,
        IDataGuidanceDataSetService? dataGuidanceDataSetService = null,
        IUserService? userService = null,
        IReleaseDataFileRepository? releaseDataFileRepository = null)
    {
        return new DataGuidanceService(
            contentDbContext ?? new Mock<ContentDbContext>().Object,
            dataGuidanceDataSetService ?? Mock.Of<IDataGuidanceDataSetService>(),
            userService ?? Mock.Of<IUserService>(),
            releaseDataFileRepository ?? Mock.Of<IReleaseDataFileRepository>());
    }
}
