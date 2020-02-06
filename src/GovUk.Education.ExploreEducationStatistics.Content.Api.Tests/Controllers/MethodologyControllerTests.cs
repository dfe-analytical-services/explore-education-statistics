using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
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
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService.Setup(s => s.DownloadTextAsync("methodology/tree.json")).ReturnsAsync(@"
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

            var controller = new MethodologyController(fileStorageService.Object);

            var result = controller.GetMethodologyTree();
            Assert.Single(result.Result.Value);
            var theme = result.Result.Value.First();
            Assert.IsAssignableFrom<ThemeTree<PublicationTreeNode>>(theme);
            Assert.Single(theme.Topics);
            var topic = theme.Topics.First();
            Assert.Single(topic.Publications);
        }

        [Fact]
        public void Get_MethodologyTree_Returns_NoContent()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            var controller = new MethodologyController(fileStorageService.Object);

            var result = controller.GetMethodologyTree();

            Assert.IsAssignableFrom<NoContentResult>(result.Result.Result);
        }

        [Fact]
        public void Get_Methodology_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService.Setup(s => s.DownloadTextAsync("methodology/methodologies/test-slug.json"))
                .ReturnsAsync(@"
            {
                ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                ""title"": ""string"",
                ""slug"": ""string"",
                ""summary"": ""string"",
                ""published"": ""2020-02-03T17:19:09.892Z"",
                ""lastUpdated"": ""2020-02-03T17:19:09.892Z"",
                ""content"": [
                {
                    ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                    ""order"": 0,
                    ""heading"": ""string"",
                    ""caption"": ""string"",
                    ""content"": [
                    {
                        ""$type"": ""GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels.HtmlBlockViewModel, GovUk.Education.ExploreEducationStatistics.Publisher.Model"",
                        ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                        ""order"": 0,
                        ""body"": ""string"",
                        ""type"": ""string""
                    }
                    ]
                }
                ],
                ""annexes"": [
                {
                    ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                    ""order"": 0,
                    ""heading"": ""string"",
                    ""caption"": ""string"",
                    ""content"": [
                    {
                        ""$type"": ""GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels.MarkDownBlockViewModel, GovUk.Education.ExploreEducationStatistics.Publisher.Model"",
                        ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                        ""order"": 0,
                        ""body"": ""string"",
                        ""type"": ""string""
                    }
                    ]
                }
                ]
            }");

            var controller = new MethodologyController(fileStorageService.Object);

            var result = controller.Get("test-slug");
            var methodologyViewModel = result.Result.Value;
            Assert.Single(methodologyViewModel.Content);
            var contentSection = methodologyViewModel.Content.First();
            Assert.Single(contentSection.Content);
            Assert.IsAssignableFrom<HtmlBlockViewModel>(contentSection.Content.First());
            Assert.Single(methodologyViewModel.Annexes);
            var annex = methodologyViewModel.Annexes.First();
            Assert.Single(annex.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(annex.Content.First());
        }

        [Fact]
        public void Get_Methodology_Returns_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            var controller = new MethodologyController(fileStorageService.Object);

            var result = controller.Get("unknown-slug");

            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }
    }
}