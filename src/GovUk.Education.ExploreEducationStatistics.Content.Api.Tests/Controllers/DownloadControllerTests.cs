using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class DownloadControllerTests
    {
        [Fact]
        public void Get_DownloadTree_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService.Setup(s => s.DownloadTextAsync("download/tree.json")).ReturnsAsync(@"
            [
            {
                ""id"": ""057bfc2f-2e23-4775-be1d-de9700569183"",
                ""title"": ""string"",
                ""summary"": ""string"",
                ""topics"": [
                {
                    ""id"": ""f287aa31-95e2-4c13-bb23-6782ea411726"",
                    ""title"": ""string"",
                    ""summary"": ""string"",
                    ""publications"": [
                    {
                        ""downloadFiles"": [
                        {
                            ""extension"": ""string"",
                            ""name"": ""string"",
                            ""path"": ""string"",
                            ""size"": ""string""
                        }
                        ],
                        ""id"": ""b2aeb86f-1213-47d7-b863-942e4fd080da"",
                        ""title"": ""string"",
                        ""slug"": ""string"",
                        ""summary"": ""string""
                    }
                    ]
                }
                ]
            }
            ]");

            var controller = new DownloadController(fileStorageService.Object);
            var result = controller.GetDownloadTree();
            Assert.Single(result.Result.Value);
            var theme = result.Result.Value.First();
            Assert.IsAssignableFrom<ThemeTree<PublicationDownloadTreeNode>>(theme);
            Assert.Single(theme.Topics);
            var topic = theme.Topics.First();
            Assert.Single(topic.Publications);
            var publication = topic.Publications.First();
            Assert.Single(publication.DownloadFiles);
        }

        [Fact]
        public void Get_DownloadTree_Returns_NoContent()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            var controller = new DownloadController(fileStorageService.Object);

            var result = controller.GetDownloadTree();

            Assert.IsAssignableFrom<NoContentResult>(result.Result.Result);
        }
    }
}