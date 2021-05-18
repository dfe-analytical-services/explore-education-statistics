using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils.AdminMockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserRoleServicePermissionTests
    {
        [Fact]
        public async Task AddGlobalRole()
        {
            // TODO EES-2131
        }

        [Fact]
        public async Task AddPublicationRole()
        {
            // TODO EES-2131
        }

        [Fact]
        public async Task AddReleaseRole()
        {
            // TODO EES-2131
        }

        [Fact]
        public async Task GetAllGlobalRoles()
        {
            // TODO EES-2131
        }

        [Fact]
        public async Task GetAllResourceRoles()
        {
            // TODO EES-2131
        }

        [Fact]
        public async Task GetGlobalRoles()
        {
            // TODO EES-2131
        }

        [Fact]
        public async Task GetPublicationRoles()
        {
            // TODO EES-2131
        }

        [Fact]
        public async Task GetReleaseRoles()
        {
            // TODO EES-2131
        }

        [Fact]
        public async Task RemoveGlobalRole()
        {
            // TODO EES-2131
        }

        [Fact]
        public async Task RemoveUserPublicationRole()
        {
            // TODO EES-2131
        }

        [Fact]
        public async Task RemoveUserReleaseRole()
        {
            // TODO EES-2131
        }

        private static UserRoleService SetupUserRoleService(
            ContentDbContext contentDbContext = null,
            UsersAndRolesDbContext usersAndRolesDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper = null,
            IEmailTemplateService emailTemplateService = null,
            IUserPublicationRoleRepository userPublicationRoleRepository = null,
            IUserReleaseRoleRepository userReleaseRoleRepository = null,
            UserManager<ApplicationUser> userManager = null,
            IUserService userService = null)
        {
            contentDbContext ??= InMemoryApplicationDbContext();
            usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

            return new UserRoleService(
                usersAndRolesDbContext ?? InMemoryUserAndRolesDbContext(),
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
                emailTemplateService ?? new Mock<IEmailTemplateService>().Object,
                userService ?? AlwaysTrueUserService().Object,
                userPublicationRoleRepository ?? new Mock<IUserPublicationRoleRepository>().Object,
                userReleaseRoleRepository ?? new Mock<IUserReleaseRoleRepository>().Object,
                userManager ?? MockUserManager().Object);
        }
    }
}
