#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class PublicationControllerTests
    {
        private const string SitemapItemLastModifiedTime = "2024-02-05T09:36:45.00Z";
        private const string PublicationLastModifiedTime = "2024-01-03T10:14:23.00Z";

        [Fact]
        public async Task GetPublicationTitle()
        {
            var publicationId = Guid.NewGuid();

            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

            publicationCacheService.Setup(mock => mock.GetPublication("publication-a"))
                .ReturnsAsync(new PublicationCacheViewModel
                {
                    Id = publicationId,
                    Title = "Test title",
                });

            var controller = BuildPublicationController(publicationCacheService: publicationCacheService.Object);

            var result = await controller.GetPublicationTitle("publication-a");

            VerifyAllMocks(publicationCacheService);

            var publicationTitleViewModel = result.AssertOkResult();

            Assert.Equal(publicationId, publicationTitleViewModel.Id);
            Assert.Equal("Test title", publicationTitleViewModel.Title);
        }

        [Fact]
        public async Task GetPublicationTitle_NotFound()
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

            publicationCacheService.Setup(mock => mock.GetPublication("missing-publication"))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildPublicationController(publicationCacheService: publicationCacheService.Object);

            var result = await controller.GetPublicationTitle("missing-publication");

            VerifyAllMocks(publicationCacheService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task GetPublicationTree()
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

            var controller = BuildPublicationController(publicationCacheService: publicationCacheService.Object);

            publicationCacheService
                .Setup(s => s.GetPublicationTree(PublicationTreeFilter.DataCatalogue))
                .ReturnsAsync(new List<PublicationTreeThemeViewModel>
                {
                    new()
                    {
                        Publications =
                        [
                            new()
                        ]
                    }
                });

            var result = await controller.GetPublicationTree(PublicationTreeFilter.DataCatalogue);

            VerifyAllMocks(publicationCacheService);

            var publicationTree = result.AssertOkResult();

            var theme = Assert.Single(publicationTree);

            Assert.Single(theme.Publications);
        }

        [Fact]
        public async Task ListSitemapItems()
        {
            var publicationService = new Mock<IPublicationService>(Strict);

            publicationService.Setup(mock => mock.ListSitemapItems(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PublicationSitemapItemViewModel>
                {
                    new()
                    {
                        Slug = "test-publication",
                        LastModified = DateTime.Parse(PublicationLastModifiedTime),
                        Releases =
                        [
                            new ReleaseSitemapItemViewModel
                            {
                                Slug = "test-release",
                                LastModified = DateTime.Parse(SitemapItemLastModifiedTime)
                            }
                        ]
                    }
                });

            var controller = BuildPublicationController(publicationService: publicationService.Object);

            var response = await controller.ListSitemapItems();
            var sitemapItems = response.AssertOkResult();

            var item = Assert.Single(sitemapItems);
            Assert.Equal("test-publication", item.Slug);

            Assert.NotNull(item);
            Assert.NotNull(item.Releases);
            var release = Assert.Single(item.Releases);
            Assert.Equal("test-release", release.Slug);

            VerifyAllMocks(publicationService);
        }

        private static PublicationController BuildPublicationController(
            IPublicationCacheService? publicationCacheService = null,
            IPublicationService? publicationService = null
        )
        {
            return new PublicationController(
                publicationCacheService ?? Mock.Of<IPublicationCacheService>(Strict),
                publicationService ?? Mock.Of<IPublicationService>(Strict)
            );
        }
    }
}
