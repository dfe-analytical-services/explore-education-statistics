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
                        ""downloadFiles"": [
                        {
                            ""extension"": ""string"",
                            ""name"": ""string"",
                            ""path"": ""string"",
                            ""size"": ""string""
                        }
                        ],
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