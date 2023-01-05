#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class PublicationControllerTests
    {
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
                        Topics = new List<PublicationTreeTopicViewModel>
                        {
                            new()
                            {
                                Publications = new List<PublicationTreePublicationViewModel>
                                {
                                    new()
                                }
                            }
                        }
                    }
                });

            var result = await controller.GetPublicationTree(PublicationTreeFilter.DataCatalogue);

            VerifyAllMocks(publicationCacheService);

            var publicationTree = result.AssertOkResult();

            var theme = Assert.Single(publicationTree);

            var topic = Assert.Single(theme.Topics);

            Assert.Single(topic.Publications);
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
