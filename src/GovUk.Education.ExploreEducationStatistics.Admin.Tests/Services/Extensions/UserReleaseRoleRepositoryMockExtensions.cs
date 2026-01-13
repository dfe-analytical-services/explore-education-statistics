using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using MockQueryable;
using Moq;
using Moq.Language.Flow;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;

internal static class UserReleaseRoleRepositoryMockExtensions
{
    public static IReturnsResult<IUserReleaseRoleRepository> SetupQuery(
        this Mock<IUserReleaseRoleRepository> mock,
        ResourceRoleFilter resourceRoleFilterToApply = ResourceRoleFilter.ActiveOnly,
        params UserReleaseRole[] userReleaseRolesToReturn
    ) => mock.Setup(m => m.Query(resourceRoleFilterToApply)).Returns(userReleaseRolesToReturn.BuildMock());
}
