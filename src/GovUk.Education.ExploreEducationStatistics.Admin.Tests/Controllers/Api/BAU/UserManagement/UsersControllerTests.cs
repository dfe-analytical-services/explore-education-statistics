using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.BAU.UserManagement
{
    public class UsersControllerTests
    {
        [Fact]
        public async void GetUserList_Returns_Ok()
        {
            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.ListAllUsers())
                .ReturnsAsync(new List<UserViewModel>
                {
                    new UserViewModel {Id = Guid.NewGuid().ToString()},
                    new UserViewModel {Id = Guid.NewGuid().ToString()}
                });
            var controller = new UsersController(userManagementService.Object);

            var actionResult = await controller.GetUserList();
            var result = actionResult.Value;

            Assert.IsType<List<UserViewModel>>(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async void GetUserList_Empty()
        {
            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.ListAllUsers())
                .ReturnsAsync(new List<UserViewModel>());

            var controller = new UsersController(userManagementService.Object);

            var actionResult = await controller.GetUserList();
            var result = actionResult.Value;

            Assert.IsType<List<UserViewModel>>(result);
            Assert.Empty(result);
        }

        [Fact]
        public async void GetUser_Returns_Ok()
        {
            var userId = Guid.NewGuid().ToString();
            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.GetUser(userId))
                .ReturnsAsync(
                    new UserViewModel {Id = userId}
                );
            var controller = new UsersController(userManagementService.Object);

            var actionResult = await controller.GetUser(userId);
            var result = actionResult.Value;

            Assert.IsType<UserViewModel>(result);
            Assert.Equal(userId, result.Id);
        }

        [Fact]
        public async void GetReleases_Returns_Ok()
        {
            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.ListReleases())
                .ReturnsAsync(new List<IdTitlePair>
                {
                    new IdTitlePair {Title = "Release 1", Id = Guid.NewGuid()},
                    new IdTitlePair {Title = "Release 2", Id = Guid.NewGuid()}
                });
            
            var controller = new UsersController(userManagementService.Object);

            var actionResult = await controller.GetReleases();
            var result = actionResult.Value;

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async void GetRoles_Returns_Ok()
        {
            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.ListRoles())
                .ReturnsAsync(new List<RoleViewModel>
                {
                    new RoleViewModel {Name = "Role1", Id = Guid.NewGuid().ToString()},
                    new RoleViewModel {Name = "Role2", Id = Guid.NewGuid().ToString()}
                });

            var controller = new UsersController(userManagementService.Object);

            var actionResult = await controller.GetRoles();
            var result = actionResult.Value;

            Assert.IsType<List<RoleViewModel>>(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async void GetReleaseRoles_Returns_Ok()
        {
            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.ListReleaseRoles())
                .ReturnsAsync(EnumExtensions.GetValues<ReleaseRole>());

            var controller = new UsersController(userManagementService.Object);

            var actionResult = await controller.GetReleaseRoles();
            var result = actionResult.Value;
            Assert.IsType<List<EnumExtensions.EnumValue>>(result);
            Assert.Equal(5, result.Count);
        }
    }
}
