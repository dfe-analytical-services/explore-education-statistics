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
    public class ReleaseControllerTests
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

        private const string ReleaseJson = @"
            {
              ""id"": ""2ca4bbbc-e52d-4cb7-8dd2-541623973d68"",
              ""title"": ""string"",
              ""yearTitle"": ""string"",
              ""coverageTitle"": ""string"",
              ""releaseName"": ""string"",
              ""nextReleaseDate"": {
                ""year"": ""string"",
                ""month"": ""string"",
                ""day"": ""string""
              },
              ""published"": ""2020-02-05T10:43:58.736Z"",
              ""slug"": ""string"",
              ""type"": {
                ""id"": ""7129f111-82da-46f4-b989-10a0472ccbff"",
                ""title"": ""string""
              },
              ""updates"": [
                {
                  ""id"": ""4c51f3b3-f4c0-476b-bcb2-07247403ddb1"",
                  ""reason"": ""string"",
                  ""on"": ""2020-02-05T10:43:58.736Z""
                }
              ],
              ""content"": [
                {
                  ""id"": ""15fb7ad3-4438-4a1b-974f-eac3be47b912"",
                  ""order"": 0,
                  ""heading"": ""string"",
                  ""caption"": ""string"",
                  ""content"": [
                    {
                      ""$type"": ""GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels.HtmlBlockViewModel, GovUk.Education.ExploreEducationStatistics.Publisher.Model"",
                      ""id"": ""3ef4eb83-e270-49ea-9794-c9c42d33a273"",
                      ""order"": 0,
                      ""body"": ""string"",
                      ""type"": ""string""
                    }
                  ]
                }
              ],
              ""summarySection"": {
                ""id"": ""dbfa0e38-a53a-4d70-8236-b76f72525cb6"",
                ""order"": 0,
                ""heading"": ""string"",
                ""caption"": ""string"",
                ""content"": [
                  {
                    ""$type"": ""GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels.MarkDownBlockViewModel, GovUk.Education.ExploreEducationStatistics.Publisher.Model"",
                    ""id"": ""b7d97067-e3c8-4c70-b9a8-6886b86ad442"",
                    ""order"": 0,
                    ""body"": ""string"",
                    ""type"": ""string""
                  }
                ]
              },
              ""headlinesSection"": {
                ""id"": ""2b7a6a14-acc7-48ba-86f4-e068b7078e5d"",
                ""order"": 0,
                ""heading"": ""string"",
                ""caption"": ""string"",
                ""content"": [
                  {
                    ""$type"": ""GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels.MarkDownBlockViewModel, GovUk.Education.ExploreEducationStatistics.Publisher.Model"",
                    ""id"": ""bfd250d3-25ca-41fd-947d-19ecbdfaf6c5"",
                    ""order"": 0,
                    ""body"": ""string"",
                    ""type"": ""string""
                  }
                ]
              },
              ""keyStatisticsSection"": {
                ""id"": ""77865177-62a4-4713-9c03-187c336d7865"",
                ""order"": 0,
                ""heading"": ""string"",
                ""caption"": ""string"",
                ""content"": [
                  {
                    ""$type"": ""GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels.MarkDownBlockViewModel, GovUk.Education.ExploreEducationStatistics.Publisher.Model"",
                    ""id"": ""9786b7a3-f9db-4dc2-887e-b99afb0d6d08"",
                    ""order"": 0,
                    ""body"": ""string"",
                    ""type"": ""string""
                  }
                ]
              },
              ""keyStatisticsSecondarySection"": {
                ""id"": ""744d826f-5649-429c-900a-8ab146adc641"",
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
              },
              ""downloadFiles"": [
                {
                  ""extension"": ""string"",
                  ""name"": ""string"",
                  ""path"": ""string"",
                  ""size"": ""string""
                }
              ],
              ""relatedInformation"": [
                {
                  ""id"": ""db2a5cb2-ba86-4e72-802e-576fe0a91a6c"",
                  ""description"": ""string"",
                  ""url"": ""string""
                }
              ]
            }";

        [Fact]
        public void Get_LatestRelease_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/publication-a/publication.json"))
                .ReturnsAsync(PublicationJson);
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/publication-a/latest-release.json"))
                .ReturnsAsync(ReleaseJson);

            var controller = new ReleaseController(fileStorageService.Object);

            var result = controller.GetLatestRelease("publication-a");
            var releaseViewModel = result.Result.Value;
            Assert.True(releaseViewModel.LatestRelease);

            Assert.Single(releaseViewModel.Content);
            var contentSection = releaseViewModel.Content.First();
            Assert.Single(contentSection.Content);
            Assert.IsAssignableFrom<HtmlBlockViewModel>(contentSection.Content.First());

            var headlinesSection = releaseViewModel.HeadlinesSection;
            Assert.Single(headlinesSection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(headlinesSection.Content.First());

            var keyStatisticsSection = releaseViewModel.KeyStatisticsSection;
            Assert.Single(keyStatisticsSection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(keyStatisticsSection.Content.First());

            var keyStatisticsSecondarySection = releaseViewModel.KeyStatisticsSecondarySection;
            Assert.Single(keyStatisticsSecondarySection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(keyStatisticsSecondarySection.Content.First());

            var summarySection = releaseViewModel.SummarySection;
            Assert.Single(summarySection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(summarySection.Content.First());

            var publication = releaseViewModel.Publication;
            Assert.Equal(new Guid("4fd09502-15bb-4d2b-abd1-7fd112aeee14"), publication.Id);
        }

        [Fact]
        public void Get_LatestRelease_Returns_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            var controller = new ReleaseController(fileStorageService.Object);
            var result = controller.GetLatestRelease("publication-a");
            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }

        [Fact]
        public void Get_Release_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/publication-a/publication.json"))
                .ReturnsAsync(PublicationJson);
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/publication-a/releases/2016.json"))
                .ReturnsAsync(ReleaseJson);

            var controller = new ReleaseController(fileStorageService.Object);

            var result = controller.GetRelease("publication-a", "2016");
            var releaseViewModel = result.Result.Value;
            Assert.True(releaseViewModel.LatestRelease);

            Assert.Single(releaseViewModel.Content);
            var contentSection = releaseViewModel.Content.First();
            Assert.Single(contentSection.Content);
            Assert.IsAssignableFrom<HtmlBlockViewModel>(contentSection.Content.First());

            var headlinesSection = releaseViewModel.HeadlinesSection;
            Assert.Single(headlinesSection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(headlinesSection.Content.First());

            var keyStatisticsSection = releaseViewModel.KeyStatisticsSection;
            Assert.Single(keyStatisticsSection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(keyStatisticsSection.Content.First());

            var keyStatisticsSecondarySection = releaseViewModel.KeyStatisticsSecondarySection;
            Assert.Single(keyStatisticsSecondarySection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(keyStatisticsSecondarySection.Content.First());

            var summarySection = releaseViewModel.SummarySection;
            Assert.Single(summarySection.Content);
            Assert.IsAssignableFrom<MarkDownBlockViewModel>(summarySection.Content.First());

            var publication = releaseViewModel.Publication;
            Assert.Equal(new Guid("4fd09502-15bb-4d2b-abd1-7fd112aeee14"), publication.Id);
        }

        [Fact]
        public void Get_Release_Returns_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            var controller = new ReleaseController(fileStorageService.Object);
            var result = controller.GetRelease("publication-a", "2000");
            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }
    }
}