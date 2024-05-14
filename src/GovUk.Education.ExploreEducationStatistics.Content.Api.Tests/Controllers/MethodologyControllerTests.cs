using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class MethodologyControllerTests
    {
        [Fact]
        public async Task GetLatestMethodologyBySlug()
        {
            var methodologyId = Guid.NewGuid();

            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            methodologyService.Setup(mock => mock.GetLatestMethodologyBySlug("test-slug"))
                .ReturnsAsync(new MethodologyVersionViewModel
                {
                    Id = methodologyId
                });

            var controller = new MethodologyController(methodologyService.Object);

            var result = await controller.GetLatestMethodologyBySlug("test-slug");
            var methodologyViewModel = result.Value;

            Assert.Equal(methodologyId, methodologyViewModel.Id);

            MockUtils.VerifyAllMocks(methodologyService);
        }

        [Fact]
        public async Task GetLatestMethodologyBySlug_NotFound()
        {
            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            methodologyService.Setup(mock => mock.GetLatestMethodologyBySlug(It.IsAny<string>()))
                .ReturnsAsync(new NotFoundResult());

            var controller = new MethodologyController(methodologyService.Object);

            var result = await controller.GetLatestMethodologyBySlug("unknown-slug");

            Assert.IsType<NotFoundResult>(result.Result);

            MockUtils.VerifyAllMocks(methodologyService);
        }

        [Fact]
        public async Task GetSitemapSummaries()
        {
            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            methodologyService.Setup(mock => mock.GetSitemapSummaries())
                .ReturnsAsync(new List<MethodologySitemapSummaryViewModel>()
                {
                    new()
                    {
                        Slug = "test-methodology",
                        LastModified = new DateTime(2024, 01, 03, 10, 14, 23)
                    }
                });

            var controller = new MethodologyController(methodologyService.Object);

            var result = await controller.GetSitemapSummaries();
            var sitemapSummaries = result.Value;

            Assert.Equal("test-methodology", sitemapSummaries.Single().Slug);

            MockUtils.VerifyAllMocks(methodologyService);
        }
    }
}
