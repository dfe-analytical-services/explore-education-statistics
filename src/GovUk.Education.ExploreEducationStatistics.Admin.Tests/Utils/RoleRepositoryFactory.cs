using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils;

public static class RoleRepositoryFactory
{
    public static (UserPublicationRoleRepository, UserPrereleaseRoleRepository) BuildRoleRepositories(
        ContentDbContext contentDbContext
    )
    {
        var userRepository = new UserRepository(contentDbContext);

        var newPermissionsSystemHelper = new NewPermissionsSystemHelper();

        var userReleaseRoleQueryRepository = new UserReleaseRoleQueryRepository(contentDbContext);

        var userPublicationRoleRepository = new UserPublicationRoleRepository(
            contentDbContext: contentDbContext,
            newPermissionsSystemHelper: newPermissionsSystemHelper,
            userReleaseRoleQueryRepository: userReleaseRoleQueryRepository,
            userRepository: userRepository
        );

        var userPrereleaseRoleRepository = new UserPrereleaseRoleRepository(
            contentDbContext: contentDbContext,
            userPublicationRoleRepository: userPublicationRoleRepository,
            newPermissionsSystemHelper: newPermissionsSystemHelper,
            userReleaseRoleQueryRepository: userReleaseRoleQueryRepository,
            userRepository: userRepository
        );

        return (userPublicationRoleRepository, userPrereleaseRoleRepository);
    }
}
