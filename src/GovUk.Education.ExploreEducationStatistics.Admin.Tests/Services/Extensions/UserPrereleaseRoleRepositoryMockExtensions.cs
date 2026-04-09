using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using MockQueryable;
using Moq;
using Moq.Language.Flow;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;

internal static class UserPrereleaseRoleRepositoryMockExtensions
{
    public static IReturnsResult<IUserPrereleaseRoleRepository> SetupQuery(
        this Mock<IUserPrereleaseRoleRepository> mock,
        ResourceRoleFilter resourceRoleFilterToApply = ResourceRoleFilter.ActiveOnly,
        params UserReleaseRole[] userPrereleaseRolesToReturn
    ) => mock.Setup(m => m.Query(resourceRoleFilterToApply)).Returns(userPrereleaseRolesToReturn.BuildMock());
}
