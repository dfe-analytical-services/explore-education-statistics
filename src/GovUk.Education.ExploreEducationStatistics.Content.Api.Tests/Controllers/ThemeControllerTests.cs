#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class ThemeControllerTests
    {
        private static readonly List<ThemeTree<PublicationTreeNode>> Themes = new()
        {
            new()
            {
                Topics = new List<TopicTree<PublicationTreeNode>>
                {
                    new()
                    {
                        Publications = new List<PublicationTreeNode>
                        {
                            new()
                        }
                    }
                }
            }
        };

        private static readonly List<AllMethodologiesThemeViewModel> MethodologyThemes = new()
        {
            new AllMethodologiesThemeViewModel
            {
                Id = Guid.NewGuid(),
                Title = "Publication title",
                Topics = AsList(
                    new AllMethodologiesTopicViewModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Topic title",
                        Publications = AsList(
                            new AllMethodologiesPublicationViewModel
                            {
                                Id = Guid.NewGuid(),
                                Title = "Publication title",
                                Methodologies = AsList(
                                    new MethodologyVersionSummaryViewModel
                                    {
                                        Id = Guid.NewGuid(),
                                        Slug = "methodology-slug",
                                        Title = "Methodology title"
                                    }
                                )
                            }
                        )
                    }
                )
            }
        };

        [Fact]
        public async Task GetPublicationTree()
        {
            var themeCacheService = new Mock<IThemeCacheService>(Strict);

            var controller = BuildController(themeCacheService: themeCacheService.Object);

            themeCacheService
                .Setup(s => s.GetPublicationTree(PublicationTreeFilter.FindStatistics))
                .ReturnsAsync(Themes);

            var result = await controller.GetPublicationTree(PublicationTreeFilter.FindStatistics);

            VerifyAllMocks(themeCacheService);

            var publicationTree = result.AssertOkResult();

            var theme = Assert.Single(publicationTree);

            Assert.IsType<ThemeTree<PublicationTreeNode>>(theme);

            var topic = Assert.Single(theme.Topics);

            Assert.Single(topic.Publications);
        }

        [Fact]
        public async Task GetMethodologyThemes()
        {
            var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);

            var controller = BuildController(methodologyCacheService.Object);

            methodologyCacheService
                .Setup(mock => mock.GetSummariesTree())
                .ReturnsAsync(MethodologyThemes);

            var result = await controller.GetMethodologyThemes();

            VerifyAllMocks(methodologyCacheService);

            result.AssertOkResult(MethodologyThemes);
        }

        private static ThemeController BuildController(
            IMethodologyCacheService? methodologyCacheService = null,
            IThemeCacheService? themeCacheService = null)
        {
            return new ThemeController(
                methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict),
                themeCacheService ?? Mock.Of<IThemeCacheService>(Strict));
        }
    }
}
