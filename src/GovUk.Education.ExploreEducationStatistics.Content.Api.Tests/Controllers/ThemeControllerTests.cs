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
using Newtonsoft.Json;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    [Collection(CacheTestFixture.CollectionName)]
    public class ThemeControllerTests : IClassFixture<CacheTestFixture>, IDisposable
    {
        public void Dispose()
        {
            BlobCacheAttribute.ClearServices();
        }
        
        [Fact]
        public async Task GetThemes()
        {
            var themes = new List<ThemeTree<PublicationTreeNode>>
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
            
            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.cacheService
                .Setup(s => s.GetItem(
                    It.IsAny<PublicationTreeCacheKey>(),
                    typeof(IList<ThemeTree<PublicationTreeNode>>)))
                .ReturnsAsync(null);

            mocks.themeService
                .Setup(s => s.GetPublicationTree(null))
                .ReturnsAsync(themes);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(
                    It.IsAny<PublicationTreeCacheKey>(), 
                    themes))
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
            var themes = AsList(
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
            );

            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.cacheService
                .Setup(s => s.GetItem(
                    It.IsAny<AllMethodologiesCacheKey>(),
                    typeof(List<AllMethodologiesThemeViewModel>)))
                .ReturnsAsync(null);

            mocks.methodologyService
                .Setup(mock => mock.GetTree())
                .ReturnsAsync(themes);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(
                    It.IsAny<AllMethodologiesCacheKey>(), 
                    themes))
                .Returns(Task.CompletedTask);

            var result = await controller.GetMethodologyThemes();

            VerifyAllMocks(mocks);

            result.AssertOkResult(themes);
        }

        [Fact]
        public void ThemeTree_SerialiseAndDeserialise()
        {
            var original = new ThemeTree<PublicationTreeNode>
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
            };

            var converted = JsonConvert.DeserializeObject(
                JsonConvert.SerializeObject(original), 
                typeof(ThemeTree<PublicationTreeNode>));
            
            Assert.Equal(original, converted);
        }

        [Fact]
        public void AllMethodologiesThemeViewModel_SerialiseAndDeserialise()
        {
            var original = new AllMethodologiesThemeViewModel
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
            };

            var converted = JsonConvert.DeserializeObject(
                JsonConvert.SerializeObject(original), 
                typeof(AllMethodologiesThemeViewModel));
            
            Assert.Equal(original, converted);
        }

        private static (ThemeController controller, (
                Mock<IThemeService> themeService, 
                Mock<IMethodologyService> methodologyService,
                Mock<IBlobCacheService> cacheService) mocks) 
                BuildControllerAndDependencies()
        {
            var blobCacheService = new Mock<IBlobCacheService>(Strict);
            BlobCacheAttribute.AddService("default", blobCacheService.Object);
            
            var themeService = new Mock<IThemeService>(Strict);
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var controller = new ThemeController(themeService.Object, methodologyService.Object);
            return (controller, (themeService, methodologyService, blobCacheService));
        }
    }
}
