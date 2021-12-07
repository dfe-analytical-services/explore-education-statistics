#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    [Collection(BlobCacheServiceTests)]
    public class ThemeControllerTests : BlobCacheServiceTestFixture
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
        
        private static readonly List<AllMethodologiesThemeViewModel> MethodologyThemes = new() {
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
        public async Task GetThemes()
        {
            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.cacheService
                .Setup(s => s.GetItem(
                    It.IsAny<PublicationTreeCacheKey>(),
                    typeof(IList<ThemeTree<PublicationTreeNode>>)))
                .ReturnsAsync(null);
            
            mocks.themeService
                .Setup(s => s.GetPublicationTree(null))
                .ReturnsAsync(Themes);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(
                    It.IsAny<PublicationTreeCacheKey>(), 
                    Themes))
                .Returns(Task.CompletedTask);
            
            var result = await controller.GetThemes();
            VerifyAllMocks(mocks);

            var theme = Assert.Single(result);

            Assert.IsType<ThemeTree<PublicationTreeNode>>(theme);

            var topic = Assert.Single(theme!.Topics);

            Assert.Single(topic!.Publications);
        }

        [Fact]
        public async Task GetMethodologyThemes()
        {
            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.cacheService
                .Setup(s => s.GetItem(
                    It.IsAny<AllMethodologiesCacheKey>(),
                    typeof(List<AllMethodologiesThemeViewModel>)))
                .ReturnsAsync(null);
            
            mocks.methodologyService
                .Setup(mock => mock.GetTree())
                .ReturnsAsync(MethodologyThemes);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(
                    It.IsAny<AllMethodologiesCacheKey>(), 
                    MethodologyThemes))
                .Returns(Task.CompletedTask);

            var result = await controller.GetMethodologyThemes();

            VerifyAllMocks(mocks);

            result.AssertOkResult(MethodologyThemes);
        }

        [Fact]
        public void ThemeTree_SerialiseAndDeserialise()
        {
            var converted = DeserializeObject<ThemeTree<PublicationTreeNode>>(SerializeObject(Themes[0]));
            converted.AssertDeepEqualTo(Themes[0]);
        }
        
        [Fact]
        public void AllMethodologiesThemeViewModel_SerialiseAndDeserialise()
        {
            var converted = DeserializeObject<AllMethodologiesThemeViewModel>(SerializeObject(MethodologyThemes[0]));
            converted.AssertDeepEqualTo(MethodologyThemes[0]);
        }

        private static (ThemeController controller, (
                Mock<IThemeService> themeService, 
                Mock<IMethodologyService> methodologyService,
                Mock<IBlobCacheService> cacheService) mocks) 
                BuildControllerAndDependencies()
        {
            var themeService = new Mock<IThemeService>(Strict);
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var controller = new ThemeController(themeService.Object, methodologyService.Object);
            return (controller, (themeService, methodologyService, CacheService));
        }
    }
}
