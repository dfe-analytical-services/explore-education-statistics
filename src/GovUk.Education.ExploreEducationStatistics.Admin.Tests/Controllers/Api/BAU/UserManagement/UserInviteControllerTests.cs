using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.BAU.UserManagement
{
    public class UserInviteControllerTests
    {
        [Fact]
        public async void InviteUser_Returns_Ok()
        {
//            var userManagementService = new Mock<IUserManagementService>();
//            userManagementService.Setup(s => s.InviteAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>()))
//                .ReturnsAsync(true);
//
//            var controller = new UserInviteController(userManagementService.Object);
//
//            var result = controller.InviteUser(new UserInviteViewModel
//            {
//                Email = "example@example.com",
//                RoleId = "1234",
//            });
//
//            Assert.IsAssignableFrom<OkObjectResult>(result.Result);
        }
    }
}