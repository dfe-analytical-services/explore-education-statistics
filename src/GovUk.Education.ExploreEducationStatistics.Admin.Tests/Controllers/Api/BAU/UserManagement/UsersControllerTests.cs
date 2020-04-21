using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Moq;
using Xunit;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.BAU.UserManagement
{
    public class UsersControllerTests
    {
        [Fact]
        public async void GetRoles_Returns_Ok()
        {
            var userManagementService = new Mock<IUserManagementService>();
            userManagementService.Setup(s => s.ListRolesAsync())
                .ReturnsAsync(new List<RoleViewModel>
                {
                    new RoleViewModel { Name = "Role1", Id = Guid.NewGuid().ToString()},
                    new RoleViewModel { Name = "Role2", Id = Guid.NewGuid().ToString()}
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

            var result =  await controller.GetRoles();

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