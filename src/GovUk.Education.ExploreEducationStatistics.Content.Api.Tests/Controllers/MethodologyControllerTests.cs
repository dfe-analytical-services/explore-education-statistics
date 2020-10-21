using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class MethodologyControllerTests
    {
        [Fact]
        public void Get_Methodology_Returns_Ok()
        {
            var fileStorageService = new Mock<IBlobStorageService>();

            fileStorageService.Setup(
                    s => s.DownloadBlobText(
                        PublicContentContainerName,
                        "methodology/methodologies/test-slug.json"
                    )
                )
                .ReturnsAsync(
                    @"
            {
                ""id"": ""c04e6613-1f98-4998-86a0-7570c2034d3a"",
                ""title"": ""string"",
                ""slug"": ""string"",
                ""summary"": ""string"",
                ""published"": ""2020-02-03T17:19:09.892Z"",
                ""lastUpdated"": ""2020-02-03T17:19:09.892Z"",
                ""content"": [
                {
                    ""id"": ""28ada0ae-504d-480a-8757-a752d2994c89"",
                    ""order"": 0,
                    ""heading"": ""string"",
                    ""caption"": ""string"",
                    ""content"": [
                    {
                        ""$type"": ""GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels.HtmlBlockViewModel, GovUk.Education.ExploreEducationStatistics.Publisher.Model"",
                        ""id"": ""382fa9aa-0fc6-4f00-957d-195862b01272"",
                        ""order"": 0,
                        ""body"": ""string"",
                        ""type"": ""string""
                    }
                    ]
                }
                ],
                ""annexes"": [
                {
                    ""id"": ""305dd865-47b4-4e05-988c-01e36fa15a95"",
                    ""order"": 0,
                    ""heading"": ""string"",
                    ""caption"": ""string"",
                    ""content"": [
                    {
                        ""$type"": ""GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels.MarkDownBlockViewModel, GovUk.Education.ExploreEducationStatistics.Publisher.Model"",
                        ""id"": ""32daa5e8-0ee9-4134-9fef-f2ed79a49144"",
                        ""order"": 0,
                        ""body"": ""string"",
                        ""type"": ""string""
                    }
                    ]
                }
                ]
            }"
                );

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
            var fileStorageService = new Mock<IBlobStorageService>();

            var controller = new MethodologyController(fileStorageService.Object);

            var result = controller.Get("unknown-slug");

            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }
    }
}