using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils
{
    public static class AdminMockUtils
    {
        public static Mock<UserManager<ApplicationUser>> MockUserManager(MockBehavior behaviour = MockBehavior.Strict)
        {
            var store = new Mock<IUserStore<ApplicationUser>>(behaviour);
            var mock = new Mock<UserManager<ApplicationUser>>(behaviour,
                store.Object, null, null, null, null, null, null, null, null);
            mock.SetupSet(s => s.Logger = null);
            return mock;
        }

        public static Mock<UserManager<ApplicationUser>> MockUserManager(ApplicationUser user)
        {
            var mock = MockUserManager();
            mock
                .Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            return mock;
        }
    }
}
