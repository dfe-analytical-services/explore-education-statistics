using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using MockQueryable;
using Moq;
using Moq.Language.Flow;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;

internal static class UserPreReleaseRoleRepositoryMockExtensions
{
    public static IReturnsResult<IUserPreReleaseRoleRepository> SetupQuery(
        this Mock<IUserPreReleaseRoleRepository> mock,
        ResourceRoleFilter resourceRoleFilterToApply = ResourceRoleFilter.ActiveOnly,
        params UserReleaseRole[] userPreReleaseRolesToReturn
    ) => mock.Setup(m => m.Query(resourceRoleFilterToApply)).Returns(userPreReleaseRolesToReturn.BuildMock());
}
