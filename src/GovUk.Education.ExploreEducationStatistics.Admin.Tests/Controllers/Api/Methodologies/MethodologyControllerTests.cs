using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Methodologies
{
    public class MethodologyControllerTests
    {
        private readonly Guid _id = Guid.NewGuid();

        [Fact]
        public async void CreateMethodology_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>();

            methodologyService.Setup(s => s.CreateMethodology(_id))
                .ReturnsAsync(new MethodologySummaryViewModel());
            
            var controller = new MethodologyController(methodologyService.Object);

            // Method under test
            var result = await controller.CreateMethodology(_id);
            result.AssertOkResult();
        }
        
        [Fact]
        public async void GetMethodologySummary_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>();

            methodologyService.Setup(s => s.GetSummary(_id))
                .ReturnsAsync(new MethodologySummaryViewModel());
            var controller = new MethodologyController(methodologyService.Object);

            // Method under test
            var result = await controller.GetMethodologySummary(_id);
            result.AssertOkResult();
        }

        [Fact]
        public async void GetMethodologySummary_Returns_NotFound()
        {
            var methodologyService = new Mock<IMethodologyService>();

            methodologyService.Setup(s => s.GetSummary(It.Is<Guid>(guid => guid != _id)))
                .ReturnsAsync(new NotFoundResult());
            var controller = new MethodologyController(methodologyService.Object);

            // Method under test
            var result = await controller.GetMethodologySummary(Guid.NewGuid());
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public async void UpdateMethodology_Returns_Ok()
        {
            var request = new MethodologyUpdateRequest();

            var methodologyService = new Mock<IMethodologyService>();

            methodologyService.Setup(s => s.UpdateMethodology(It.IsAny<Guid>(), request))
                .ReturnsAsync(new Either<ActionResult, MethodologySummaryViewModel>(new MethodologySummaryViewModel()));
            var controller = new MethodologyController(methodologyService.Object);

            // Method under test
            var result = await controller.UpdateMethodology(Guid.NewGuid(), request);
            result.AssertOkResult();
        }
    }
}
