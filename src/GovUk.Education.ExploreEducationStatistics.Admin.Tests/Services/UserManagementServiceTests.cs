using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserManagementServiceTests
    {
        [Fact]
        public async Task GetResourceRoles()
        {
            var service = SetupUserManagementService();

            var result = await service.GetResourceRoles();

            Assert.True(result.IsRight);

            Assert.True(result.Right.ContainsKey("Publication"));
            Assert.True(result.Right.ContainsKey("Release"));

            Assert.Single(result.Right["Publication"]);
            Assert.Equal(5, result.Right["Release"].Count);
        }

        private static UserManagementService SetupUserManagementService(
            ContentDbContext contentDbContext = null,
            UsersAndRolesDbContext usersAndRolesDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper = null,
            IEmailService emailService = null,
            IUserPublicationRoleRepository userPublicationRoleRepository = null,
            IUserReleaseRoleRepository userReleaseRoleRepository = null,
            UserManager<ApplicationUser> userManager = null,
            IUserService userService = null,
            IConfiguration configuration = null)
        {
            contentDbContext ??= InMemoryApplicationDbContext();
            usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

            return new UserManagementService(
                usersAndRolesDbContext ?? InMemoryUserAndRolesDbContext(),
                userService ?? AlwaysTrueUserService().Object,
                contentDbContext,
                emailService ?? new Mock<IEmailService>().Object,
                configuration ?? new Mock<IConfiguration>().Object,
                userPublicationRoleRepository ?? new Mock<IUserPublicationRoleRepository>().Object,
                userReleaseRoleRepository ?? new Mock<IUserReleaseRoleRepository>().Object,
                userManager ?? MockUserManager().Object,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext)
            );
        }
    }
}
