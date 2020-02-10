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
        private const string PublicationJson = @"
            {
              ""id"": ""4fd09502-15bb-4d2b-abd1-7fd112aeee14"",
              ""title"": ""string"",
              ""slug"": ""string"",
              ""description"": ""string"",
              ""dataSource"": ""string"",
              ""summary"": ""string"",
              ""nextUpdate"": ""2020-02-05T10:43:58.736Z"",
              ""latestReleaseId"": ""2ca4bbbc-e52d-4cb7-8dd2-541623973d68"",
              ""releases"": [
                {
                  ""id"": ""2ca4bbbc-e52d-4cb7-8dd2-541623973d68"",
                  ""slug"": ""string"",
                  ""title"": ""string""
                }
              ],
              ""legacyReleases"": [
                {
                  ""id"": ""6d43d18a-bc21-4939-b938-12e714490091"",
                  ""description"": ""string"",
                  ""url"": ""string""
                }
              ],
              ""topic"": {
                ""theme"": {
                  ""title"": ""string""
                }
              },
              ""contact"": {
                ""teamName"": ""string"",
                ""teamEmail"": ""string"",
                ""contactName"": ""string"",
                ""contactTelNo"": ""string""
              },
              ""externalMethodology"": {
                ""title"": ""string"",
                ""url"": ""string""
              },
              ""methodology"": {
                ""id"": ""d18931ca-a801-4184-b43a-f48d95c23d2a"",
                ""slug"": ""string"",
                ""summary"": ""string"",
                ""title"": ""string""
              }
            }";

        [Fact]
        public void Get_PublicationTree_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/tree.json")).ReturnsAsync(@"
            [
            {
                ""id"": ""e6117db8-a641-46b4-9ef9-254180696298"",
                ""title"": ""string"",
                ""summary"": ""string"",
                ""topics"": [
                {
                    ""id"": ""a89602d6-9bde-460b-ae5d-7c76b2657f1a"",
                    ""title"": ""string"",
                    ""summary"": ""string"",
                    ""publications"": [
                    {
                        ""legacyPublicationUrl"": ""string"",
                        ""id"": ""5f0a7d85-b8de-4fda-882c-1ea785fb1cab"",
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
                .ReturnsAsync(PublicationJson);

            var controller = new PublicationController(fileStorageService.Object);

            var publicationTitleViewModel = controller.GetPublicationTitle("publication-a").Result.Value;
            Assert.Equal(new Guid("4fd09502-15bb-4d2b-abd1-7fd112aeee14"), publicationTitleViewModel.Id);
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