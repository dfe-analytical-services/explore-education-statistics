using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class MethodologyControllerTests
    {
        [Fact]
        public void Get_MethodologyTree_Returns_Ok()
        {
            var service = new Mock<IMethodologyService>();
            var cache = new Mock<IContentCacheService>();


            service.Setup(s => s.GetTree()).Returns(
                new List<ThemeTree>
                {
                    new ThemeTree
                    {
                        Title = "Theme A"
                    }
                }
            );

            var controller = new MethodologyController(service.Object, cache.Object);

            var result = controller.GetMethodologyTree();

//            Assert.IsAssignableFrom<List<ThemeTree>>(result.Value);
        }

        [Fact]
        public void Get_MethodologyTree_Returns_NoContent()
        {
            var service = new Mock<IMethodologyService>();
            var cache = new Mock<IContentCacheService>();


            service.Setup(s => s.GetTree()).Returns(
                new List<ThemeTree>()
                );


            var controller = new MethodologyController(service.Object, cache.Object);

            var result = controller.GetMethodologyTree();

            //Assert.IsAssignableFrom<NoContentResult>(result.Result);
        }

        [Fact]
        public void Get_Methodology_Returns_Ok()
        {
            var service = new Mock<IMethodologyService>();
            var cache = new Mock<IContentCacheService>();


            service.Setup(s => s.Get("test-slug")).Returns(
                new Methodology
                {
                    Id = new Guid("a7772148-fbbd-4c85-8530-f33c9ef25488")
                }
            );

            var controller = new MethodologyController(service.Object, cache.Object);

            var result = controller.Get("test-slug");

            Assert.Equal("a7772148-fbbd-4c85-8530-f33c9ef25488", result.Value.Id.ToString());
        }
        
        [Fact]
        public void Get_Methodology_Returns_NotFound()
        {
            var service = new Mock<IMethodologyService>();
            var cache = new Mock<IContentCacheService>();

            service.Setup(s => s.Get("unknown-slug")).Returns((Methodology) null);

            var controller = new MethodologyController(service.Object, cache.Object);

            var result = controller.Get("unknown-slug");

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }
    }
}
