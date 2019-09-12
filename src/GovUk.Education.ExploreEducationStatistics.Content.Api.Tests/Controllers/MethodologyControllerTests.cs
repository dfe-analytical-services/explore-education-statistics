using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class MethodologyControllerTests
    {
        [Fact]
        public void Get_MethodologyTree_Returns_Ok()
        {
            var cache = new Mock<IContentCacheService>();


            cache.Setup(s => s.GetMethodologyTreeAsync()).ReturnsAsync(
                JsonConvert.SerializeObject(new List<ThemeTree>
                {
                    new ThemeTree
                    {
                        Title = "Theme A"
                    }
                }
            ));

            var controller = new MethodologyController(cache.Object);

            var result = controller.GetMethodologyTree();

            Assert.IsAssignableFrom<List<ThemeTree>>(JsonConvert.DeserializeObject<List<ThemeTree>>(result.Result.Value));
            Assert.Contains("Theme A", result.Result.Value);
        }

        [Fact]
        public void Get_MethodologyTree_Returns_NoContent()
        {
            var cache = new Mock<IContentCacheService>();


            cache.Setup(s => s.GetMethodologyTreeAsync()).ReturnsAsync(
                (string) null);


            var controller = new MethodologyController(cache.Object);

            var result = controller.GetMethodologyTree();

            Assert.IsAssignableFrom<NoContentResult>(result.Result.Result);
        }

        [Fact]
        public void Get_Methodology_Returns_Ok()
        {
            var cache = new Mock<IContentCacheService>();


            cache.Setup(s => s.GetMethodologyAsync("test-slug")).ReturnsAsync(
                JsonConvert.SerializeObject( new Methodology
                {
                    Id = new Guid("a7772148-fbbd-4c85-8530-f33c9ef25488")
                }
            ));

            var controller = new MethodologyController(cache.Object);

            var result = controller.Get("test-slug");

            Assert.Contains("a7772148-fbbd-4c85-8530-f33c9ef25488", result.Result.Value);
        }
        
        [Fact]
        public void Get_Methodology_Returns_NotFound()
        {
            var cache = new Mock<IContentCacheService>();

            cache.Setup(s => s.GetMethodologyAsync("unknown-slug")).ReturnsAsync((string) null);

            var controller = new MethodologyController(cache.Object);

            var result = controller.Get("unknown-slug");

            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }
    }
}
