using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserManagementServiceTests
    {
        [Fact]
        public async Task GetUser()
        {
            // TODO EES-2131
        }

        [Fact]
        public async Task UpdateUser()
        {
            // TODO EES-2131
        }

        private static UserManagementService SetupUserManagementService(
            ContentDbContext contentDbContext = null,
            UsersAndRolesDbContext usersAndRolesDbContext = null,
            IPersistenceHelper<UsersAndRolesDbContext> usersAndRolesPersistenceHelper = null,
            IEmailTemplateService emailTemplateService = null,
            IUserRoleService userRoleService = null,
            IUserService userService = null)
        {
            contentDbContext ??= InMemoryApplicationDbContext();
            usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

            return new UserManagementService(
                usersAndRolesDbContext ?? InMemoryUserAndRolesDbContext(),
                contentDbContext,
                usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
                emailTemplateService ?? new Mock<IEmailTemplateService>().Object,
                userRoleService ?? new Mock<IUserRoleService>().Object,
                userService ?? AlwaysTrueUserService().Object
            );
        }
    }
}
