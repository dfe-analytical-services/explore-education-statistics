using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ImportServiceTests
    {
        [Fact]
        public async void CancelFileImport()
        {
            var userService = new Mock<IUserService>();

            var cancelRequest = new ReleaseFileImportInfo
            {
                ReleaseId = Guid.NewGuid(),
                DataFileName = "my_data_file.csv"
            };

            userService
                .Setup(s => s.MatchesPolicy(cancelRequest, SecurityPolicies.CanCancelOngoingImports))
                .ReturnsAsync(true);

            var service = new ImportService(
                null, null, null, GetConfiguration(), null, null, userService.Object);
            
            var result = await service.CancelImport(cancelRequest);
            Assert.True(result.IsRight);
            
            MockUtils.VerifyAllMocks(userService);
        }
        
        [Fact]
        public async void CancelFileImportButNotAllowed()
        {
            var userService = new Mock<IUserService>();

            var cancelRequest = new ReleaseFileImportInfo
            {
                ReleaseId = Guid.NewGuid(),
                DataFileName = "my_data_file.csv"
            };

            userService
                .Setup(s => s.MatchesPolicy(cancelRequest, SecurityPolicies.CanCancelOngoingImports))
                .ReturnsAsync(false);

            var service = new ImportService(
                null, null, null, GetConfiguration(), null, null, userService.Object);
            
            var result = await service.CancelImport(cancelRequest);
            Assert.True(result.IsLeft);
            Assert.IsType<ForbidResult>(result.Left);

            MockUtils.VerifyAllMocks(userService);
        }

        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: false)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}