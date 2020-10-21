using System;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;

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
        public void Get_PublicationTitle_Returns_Ok()
        {
            var fileStorageService = new Mock<IBlobStorageService>();
            fileStorageService.Setup(
                    s => s.DownloadBlobText(
                        PublicContentContainerName,
                        "publications/publication-a/publication.json"
                    )
                )
                .ReturnsAsync(PublicationJson);

            var controller = new PublicationController(fileStorageService.Object);

            var publicationTitleViewModel = controller.GetPublicationTitle("publication-a").Result.Value;
            Assert.Equal(new Guid("4fd09502-15bb-4d2b-abd1-7fd112aeee14"), publicationTitleViewModel.Id);
            Assert.Equal("string", publicationTitleViewModel.Title);
        }

        [Fact]
        public void Get_PublicationTitle_Returns_NotFound()
        {
            var fileStorageService = new Mock<IBlobStorageService>();
            var controller = new PublicationController(fileStorageService.Object);
            var result = controller.GetPublicationTitle("missing-publication");
            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }

        [Fact]
        public void Get_PublicationMethodology_Returns_Ok()
        {
            var fileStorageService = new Mock<IBlobStorageService>();
            fileStorageService.Setup(
                    s => s.DownloadBlobText(
                        PublicContentContainerName,
                        "publications/publication-a/publication.json"
                    )
                )
                .ReturnsAsync(PublicationJson);

            var controller = new PublicationController(fileStorageService.Object);

            var publicationMethodologyViewModel = controller.GetPublicationMethodology("publication-a").Result.Value;
            Assert.Equal("externalMethodologyTitle", publicationMethodologyViewModel.ExternalMethodology.Title);
            Assert.Equal("externalMethodologyUrl", publicationMethodologyViewModel.ExternalMethodology.Url);
            Assert.Equal(
                new Guid("d18931ca-a801-4184-b43a-f48d95c23d2a"),
                publicationMethodologyViewModel.Methodology.Id
            );
            Assert.Equal("methodologySlug", publicationMethodologyViewModel.Methodology.Slug);
            Assert.Equal("methodologySummary", publicationMethodologyViewModel.Methodology.Summary);
            Assert.Equal("methodologyTitle", publicationMethodologyViewModel.Methodology.Title);
        }

        [Fact]
        public void Get_PublicationMethodology_Returns_NotFound()
        {
            var fileStorageService = new Mock<IBlobStorageService>();
            var controller = new PublicationController(fileStorageService.Object);
            var result = controller.GetPublicationMethodology("missing-publication");
            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }
    }
}