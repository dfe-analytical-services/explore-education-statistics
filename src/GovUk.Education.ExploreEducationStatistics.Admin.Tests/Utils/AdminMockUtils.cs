﻿using System;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils
{
    public static class AdminMockUtils
    {
        public static Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null,
                null);
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "test@example.com"
            };

            mock.Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            return mock;
        } 
    }
}
