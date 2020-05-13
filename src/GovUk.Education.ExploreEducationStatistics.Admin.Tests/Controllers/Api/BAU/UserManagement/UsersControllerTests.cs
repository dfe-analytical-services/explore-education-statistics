using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
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
            userManagementService.Setup(s => s.ListAsync())
                .ReturnsAsync(new List<UserViewModel>
                {
                    new UserViewModel {Id = Guid.NewGuid().ToString()},
                    new UserViewModel {Id = Guid.NewGuid().ToString()}
                });
            var controller = new UsersController(userManagementService.Object);

            var result = await controller.GetUserList();

            Assert.IsAssignableFrom<OkObjectResult>(result.Result);

            var model = (List<UserViewModel>) ((OkObjectResult) result.Result).Value;
            Assert.IsAssignableFrom<List<UserViewModel>>(model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async void GetUserList_Returns_Not_Found()
        {
            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.ListAsync())
                .ReturnsAsync(new List<UserViewModel>());

            var controller = new UsersController(userManagementService.Object);

            var result = await controller.GetUserList();

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public async void GetUser_Returns_Ok()
        {
            var userId = Guid.NewGuid().ToString();
            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.GetAsync(userId))
                .ReturnsAsync(
                    new UserViewModel {Id = userId}
                );
            var controller = new UsersController(userManagementService.Object);

            var result = await controller.GetUser(userId);

            Assert.IsAssignableFrom<OkObjectResult>(result.Result);

            var model = (UserViewModel) ((OkObjectResult) result.Result).Value;
            Assert.IsAssignableFrom<UserViewModel>(model);
            Assert.Equal(userId, model.Id);
        }

        [Fact]
        public async void GetUser_Returns_Not_Found()
        {
            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.GetAsync("not-found"))
                .ReturnsAsync(new UserViewModel());

            var controller = new UsersController(userManagementService.Object);

            var result = await controller.GetUser(Guid.NewGuid().ToString());

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }


        [Fact]
        public async void GetUserReleaseRoles_Returns_Ok()
        {
            var userId = Guid.NewGuid();
            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.GetUserReleaseRoles(userId.ToString()))
                .Returns(new List<UserReleaseRoleViewModel>
                {
                    new UserReleaseRoleViewModel {Publication = new IdTitlePair {Id = Guid.NewGuid(), Title = "Pub a"}},
                    new UserReleaseRoleViewModel {Publication = new IdTitlePair {Id = Guid.NewGuid(), Title = "Pub b"}}
                });

            var controller = new UsersController(userManagementService.Object);

            var result = controller.GetUserReleaseRoles(userId);

            Assert.IsAssignableFrom<OkObjectResult>(result.Result);

            var model = (List<UserReleaseRoleViewModel>) ((OkObjectResult) result.Result).Value;

            Assert.IsAssignableFrom<List<UserReleaseRoleViewModel>>(model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async void GetUserReleaseRoles_Returns_Not_Found()
        {
            var userId = Guid.NewGuid();
            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.GetUserReleaseRoles(userId.ToString()))
                .Returns(new List<UserReleaseRoleViewModel>());

            var controller = new UsersController(userManagementService.Object);

            var result = controller.GetUserReleaseRoles(userId);

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public async void GetUserReleaseRole_Returns_Ok()
        {
            var userId = Guid.NewGuid();
            var releaseRoleId = Guid.NewGuid();

            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.GetUserReleaseRole(userId, releaseRoleId))
                .ReturnsAsync(new UserReleaseRoleViewModel
                {
                    Publication = new IdTitlePair {Id = Guid.NewGuid(), Title = "Pub A"}
                });

            var controller = new UsersController(userManagementService.Object);

            var result = await controller.GetUserReleaseRole(userId, releaseRoleId);

            Assert.IsAssignableFrom<OkObjectResult>(result.Result);

            var model = (UserReleaseRoleViewModel) ((OkObjectResult) result.Result).Value;

            Assert.IsAssignableFrom<UserReleaseRoleViewModel>(model);
            Assert.Equal("Pub A", model.Publication.Title);
        }

        [Fact]
        public async void GetUserReleaseRole_Returns_Not_Found()
        {
            var userId = Guid.NewGuid();
            var releaseRoleId = Guid.NewGuid();

            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.GetUserReleaseRole(userId, releaseRoleId))
                .ReturnsAsync((UserReleaseRoleViewModel) null);

            var controller = new UsersController(userManagementService.Object);

            var result = await controller.GetUserReleaseRole(userId, releaseRoleId);

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public async void GetReleases_Returns_Ok()
        {
            var userManagementService = new Mock<IUserManagementService>();

            var controller = new UsersController(userManagementService.Object);

            var result = await controller.GetReleases();

            Assert.IsAssignableFrom<OkResult>(result.Result);
        }

        [Fact]
        public async void GetRoles_Returns_Ok()
        {
            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.ListRolesAsync())
                .ReturnsAsync(new List<RoleViewModel>
                {
                    new RoleViewModel {Name = "Role1", Id = Guid.NewGuid().ToString()},
                    new RoleViewModel {Name = "Role2", Id = Guid.NewGuid().ToString()}
                });

            var controller = new UsersController(userManagementService.Object);

            var result = await controller.GetRoles();

            Assert.IsAssignableFrom<OkObjectResult>(result.Result);

            var model = (List<RoleViewModel>) ((OkObjectResult) result.Result).Value;
            Assert.IsAssignableFrom<List<RoleViewModel>>(model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async void GetRoles_Returns_Not_Found()
        {
            var userManagementService = new Mock<IUserManagementService>();

            userManagementService.Setup(s => s.ListRolesAsync())
                .ReturnsAsync(new List<RoleViewModel>());

            var controller = new UsersController(userManagementService.Object);

            var result = await controller.GetRoles();

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public void GetReleaseRoles_Returns_Ok()
        {
            var userManagementService = new Mock<IUserManagementService>();

            var controller = new UsersController(userManagementService.Object);

            var result = controller.GetReleaseRoles();
            Assert.IsAssignableFrom<List<EnumExtensions.EnumValue>>(result);
            Assert.Equal(5, result.Count);
        }
    }
}