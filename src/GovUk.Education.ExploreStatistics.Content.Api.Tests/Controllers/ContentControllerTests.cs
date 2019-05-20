
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Content.Api.Tests.Controllers
{
    public class ContentControllerTests
    {
        [Fact]
        public void Get_ContentTree_Returns_Ok()
        {
            var contentService = new Mock<IContentService>();
            var publicationService = new Mock<IPublicationService>();
            var releaseService = new Mock<IReleaseService>();

            contentService.Setup(s => s.GetContentTree()).Returns(
                new List<ThemeTree>
                {
                    new ThemeTree
                    {
                        Title = "Theme A"
                    }
                }
            );
            publicationService.Setup(s => s.GetPublication("test"));
            releaseService.Setup(s => s.GetRelease("test"));

            var controller = new ContentController(contentService.Object,publicationService.Object,releaseService.Object);

            var result = controller.GetContentTree();

            Assert.IsAssignableFrom<List<ThemeTree>>(result.Value);
        }

        [Fact]
        public void Get_ContentTree_Returns_NoContent()
        {
            var contentService = new Mock<IContentService>();
            var publicationService = new Mock<IPublicationService>();
            var releaseService = new Mock<IReleaseService>();

            contentService.Setup(s => s.GetContentTree()).Returns(new List<ThemeTree>());
            publicationService.Setup(s => s.GetPublication("test"));
            releaseService.Setup(s => s.GetRelease("test"));
            
            var controller = new ContentController(contentService.Object,publicationService.Object,releaseService.Object);

            var result = controller.GetContentTree();

            Assert.IsAssignableFrom<NoContentResult>(result.Result);
        }
    }
}