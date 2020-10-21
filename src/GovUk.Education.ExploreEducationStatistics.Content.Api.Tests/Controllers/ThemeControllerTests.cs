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
    public class ThemeControllerTests
    {
        private const string PublicationJson = @"
            {
              ""id"": ""4fd09502-15bb-4d2b-abd1-7fd112aeee14"",
              ""title"": ""string"",
              ""slug"": ""string"",
              ""description"": ""string"",
              ""dataSource"": ""string"",
              ""summary"": ""string"",
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
                ""title"": ""externalMethodologyTitle"",
                ""url"": ""externalMethodologyUrl""
              },
              ""methodology"": {
                ""id"": ""d18931ca-a801-4184-b43a-f48d95c23d2a"",
                ""slug"": ""methodologySlug"",
                ""summary"": ""methodologySummary"",
                ""title"": ""methodologyTitle""
              }
            }";

        [Fact]
        public void GetThemes_Returns_Ok()
        {
            var fileStorageService = new Mock<IBlobStorageService>();
            fileStorageService.Setup(
                s => s.DownloadBlobText(
                    PublicContentContainerName,
                    "publications/tree.json"
                )
            ).ReturnsAsync(
                @"
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
        ]"
            );

            var controller = new ThemeController(fileStorageService.Object);

            var result = controller.GetThemes();
            Assert.Single(result.Result.Value);
            var theme = result.Result.Value.First();
            Assert.IsAssignableFrom<ThemeTree<PublicationTreeNode>>(theme);
            Assert.Single(theme.Topics);
            var topic = theme.Topics.First();
            Assert.Single(topic.Publications);
        }

        [Fact]
        public void GetThemes_Returns_NoContent()
        {
            var fileStorageService = new Mock<IBlobStorageService>();
            var controller = new ThemeController(fileStorageService.Object);
            var result = controller.GetThemes();
            Assert.IsAssignableFrom<NoContentResult>(result.Result.Result);
        }

        [Fact]
        public void GetDownloadThemes_Returns_Ok()
        {
            var fileStorageService = new Mock<IBlobStorageService>();

            fileStorageService.Setup(
                s => s.DownloadBlobText(
                    PublicContentContainerName,
                    "download/tree.json"
                )
            ).ReturnsAsync(
                @"
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
            ]"
            );

            var controller = new ThemeController(fileStorageService.Object);
            var result = controller.GetDownloadThemes();
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
        public void GetDownloadThemes_Returns_NoContent()
        {
            var fileStorageService = new Mock<IBlobStorageService>();

            var controller = new ThemeController(fileStorageService.Object);

            var result = controller.GetDownloadThemes();

            Assert.IsAssignableFrom<NoContentResult>(result.Result.Result);
        }

        [Fact]
        public void GetMethodologyThemes_Returns_Ok()
        {
            var fileStorageService = new Mock<IBlobStorageService>();

            fileStorageService.Setup(
                s => s.DownloadBlobText(
                    PublicContentContainerName,
                    "methodology/tree.json"
                )
            ).ReturnsAsync(
                @"
            [
            {
                ""id"": ""5dde1ee6-34b6-41f5-bdc0-10f7fe0c46fe"",
                ""title"": ""string"",
                ""summary"": ""string"",
                ""topics"": [
                {
                    ""id"": ""46dae1b0-b2ec-4759-b04e-84d184afe6db"",
                    ""title"": ""string"",
                    ""summary"": ""string"",
                    ""publications"": [
                    {
                        ""methodology"": {
                            ""id"": ""c3fc42f6-996a-4d8b-9b8d-98a536197ea7"",
                            ""slug"": ""string"",
                            ""summary"": ""string"",
                            ""title"": ""string""
                        },
                        ""id"": ""c00e6006-e82e-409d-920e-51860df6e076"",
                        ""title"": ""string"",
                        ""slug"": ""string"",
                        ""summary"": ""string""
                    }
                    ]
                }
                ]
            }
            ]"
            );

            var controller = new ThemeController(fileStorageService.Object);

            var result = controller.GetMethodologyThemes();
            Assert.Single(result.Result.Value);
            var theme = result.Result.Value.First();
            Assert.IsAssignableFrom<ThemeTree<MethodologyTreeNode>>(theme);
            Assert.Single(theme.Topics);
            var topic = theme.Topics.First();
            Assert.Single(topic.Publications);
        }

        [Fact]
        public void GetMethodologyThemes_Returns_NoContent()
        {
            var fileStorageService = new Mock<IBlobStorageService>();

            var controller = new ThemeController(fileStorageService.Object);

            var result = controller.GetMethodologyThemes();

            Assert.IsAssignableFrom<NoContentResult>(result.Result.Result);
        }
    }
}