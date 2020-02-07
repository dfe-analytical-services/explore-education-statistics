using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class PublicationControllerTests
    {
        [Fact]
        public void Get_PublicationTree_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/tree.json")).ReturnsAsync(@"
            [
            {
                ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                ""title"": ""string"",
                ""summary"": ""string"",
                ""topics"": [
                {
                    ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                    ""title"": ""string"",
                    ""summary"": ""string"",
                    ""publications"": [
                    {
                        ""legacyPublicationUrl"": ""string"",
                        ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                        ""title"": ""string"",
                        ""slug"": ""string"",
                        ""summary"": ""string""
                    }
                    ]
                }
            ]
        }
        ]");

            var controller = new PublicationController(fileStorageService.Object);

            var result = controller.GetPublicationTree();
            Assert.Single(result.Result.Value);
            var theme = result.Result.Value.First();
            Assert.IsAssignableFrom<ThemeTree<PublicationTreeNode>>(theme);
            Assert.Single(theme.Topics);
            var topic = theme.Topics.First();
            Assert.Single(topic.Publications);
        }

        [Fact]
        public void Get_PublicationTree_Returns_NoContent()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            var controller = new PublicationController(fileStorageService.Object);
            var result = controller.GetPublicationTree();
            Assert.IsAssignableFrom<NoContentResult>(result.Result.Result);
        }

        [Fact]
        public void Get_PublicationTitle_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/publication-a/publication.json"))
                .ReturnsAsync(@"
            {
                ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                ""title"": ""string""
            }");

            var controller = new PublicationController(fileStorageService.Object);

            var publicationTitleViewModel = controller.GetPublicationTitle("publication-a").Result.Value;
            Assert.Equal(new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"), publicationTitleViewModel.Id);
            Assert.Equal("string", publicationTitleViewModel.Title);
        }

        [Fact]
        public void Get_PublicationTitle_Returns_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            var controller = new PublicationController(fileStorageService.Object);
            var result = controller.GetPublicationTitle("missing-publication");
            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }
    }
}