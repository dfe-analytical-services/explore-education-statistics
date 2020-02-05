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
      private const string ReleaseJson = @"
            {
              ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
              ""title"": ""string"",
              ""yearTitle"": ""string"",
              ""coverageTitle"": ""string"",
              ""releaseName"": ""string"",
              ""published"": ""2020-02-05T10:43:58.736Z"",
              ""slug"": ""string"",
              ""publicationId"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
              ""publication"": {
                ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                ""title"": ""string"",
                ""slug"": ""string"",
                ""description"": ""string"",
                ""dataSource"": ""string"",
                ""summary"": ""string"",
                ""nextUpdate"": ""2020-02-05T10:43:58.736Z"",
                ""releases"": [
                  {
                    ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                    ""slug"": ""string"",
                    ""title"": ""string""
                  }
                ],
                ""legacyReleases"": [
                  {
                    ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
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
                  ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                  ""teamName"": ""string"",
                  ""teamEmail"": ""string"",
                  ""contactName"": ""string"",
                  ""contactTelNo"": ""string""
                },
                ""methodology"": {
                  ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                  ""slug"": ""string"",
                  ""summary"": ""string"",
                  ""title"": ""string""
                }
              },
              ""latestRelease"": true,
              ""type"": {
                ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                ""title"": ""string""
              },
              ""updates"": [
                {
                  ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                  ""reason"": ""string"",
                  ""on"": ""2020-02-05T10:43:58.736Z""
                }
              ],
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
              ""summarySection"": {
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
              },
              ""headlinesSection"": {
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
              },
              ""keyStatisticsSection"": {
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
              },
              ""keyStatisticsSecondarySection"": {
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
                  ""id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
                  ""description"": ""string"",
                  ""url"": ""string""
                }
              ]
            }";

        [Fact]
        public void Get_LatestRelease_Returns_Ok()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/publication-a/latest-release.json"))
                .ReturnsAsync(ReleaseJson);

            var controller = new ReleaseController(fileStorageService.Object);

            var result = controller.GetLatestRelease("publication-a");
            var releaseViewModel = result.Result.Value;
            
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
            fileStorageService.Setup(s => s.DownloadTextAsync("publications/publication-a/releases/2016.json")
                ).ReturnsAsync(ReleaseJson);

            var controller = new ReleaseController(fileStorageService.Object);

            var result = controller.GetRelease("publication-a", "2016");
            var releaseViewModel = result.Result.Value;

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