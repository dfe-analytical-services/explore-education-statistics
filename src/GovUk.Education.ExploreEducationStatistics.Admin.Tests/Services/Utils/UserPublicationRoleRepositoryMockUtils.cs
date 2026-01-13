using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using MockQueryable;
using Moq;
using Moq.Language.Flow;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Utils;

internal static class UserPublicationRoleRepositoryMockUtils
{
    public static IReturnsResult<IUserPublicationRoleRepository> SetupQuery(
        this Mock<IUserPublicationRoleRepository> mock,
        ResourceRoleFilter resourceRoleFilterToApply = ResourceRoleFilter.ActiveOnly,
        params UserPublicationRole[] userPublicationRolesToReturn
    ) => mock.Setup(m => m.Query(resourceRoleFilterToApply)).Returns(userPublicationRolesToReturn.BuildMock());
}
