using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.BAU
{
    public class BauFileImportsControllerTests
    {
        [Fact]
        public async void CancelFileImport()
        {
            var importService = new Mock<IImportService>();

            var cancelRequest = new ReleaseFileImportInfo
            {
                ReleaseId = Guid.NewGuid(),
                DataFileName = "my_data_file.csv"
            };

            importService
                .Setup(s => s.CancelImport(cancelRequest))
                .ReturnsAsync(Unit.Instance);
            
            var controller = new BauFileImportsController(importService.Object);
            
            var result = await controller.CancelFileImport(cancelRequest);
            Assert.IsType<AcceptedResult>(result);
            
            MockUtils.VerifyAllMocks(importService);
        }
        
        [Fact]
        public async void CancelFileImportButNotAllowed()
        {
            var importService = new Mock<IImportService>();

            var cancelRequest = new ReleaseFileImportInfo
            {
                ReleaseId = Guid.NewGuid(),
                DataFileName = "my_data_file.csv"
            };

            importService
                .Setup(s => s.CancelImport(cancelRequest))
                .ReturnsAsync(new ForbidResult());
            
            var controller = new BauFileImportsController(importService.Object);
            
            var result = await controller.CancelFileImport(cancelRequest);
            Assert.IsType<ForbidResult>(result);

            MockUtils.VerifyAllMocks(importService);
        }
    }
}