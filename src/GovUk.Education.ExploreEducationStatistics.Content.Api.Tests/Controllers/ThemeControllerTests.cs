#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class ThemeControllerTests
    {
        [Fact]
        public async Task GetThemes()
        {
            var themeService = new Mock<IThemeService>(MockBehavior.Strict);

            themeService
                .Setup(s => s.GetPublicationTree(null))
                .ReturnsAsync(
                    new List<ThemeTree<PublicationTreeNode>>
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
                    }
                );

            var controller = BuildThemeController(themeService.Object);

            var result = await controller.GetThemes();

            var theme = Assert.Single(result);

            Assert.IsType<ThemeTree<PublicationTreeNode>>(theme);

            var topic = Assert.Single(theme!.Topics);

            Assert.Single(topic!.Publications);

            MockUtils.VerifyAllMocks(themeService);
        }

        [Fact]
        public async Task GetDownloadThemes()
        {
            var themeService = new Mock<IThemeService>(MockBehavior.Strict);

            themeService
                .Setup(s => s.GetPublicationDownloadsTree())
                .ReturnsAsync(
                    new List<ThemeTree<PublicationDownloadsTreeNode>>
                    {
                        new()
                        {
                            Topics = new List<TopicTree<PublicationDownloadsTreeNode>>
                            {
                                new()
                                {
                                    Publications = new List<PublicationDownloadsTreeNode>
                                    {
                                        new()
                                        {
                                            DownloadFiles = new List<FileInfo>
                                            {
                                                new()
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                );

            var controller = BuildThemeController(themeService.Object);

            var result = await controller.GetDownloadThemes();

            var theme = Assert.Single(result);
            Assert.IsType<ThemeTree<PublicationDownloadsTreeNode>>(theme);

            var topic = Assert.Single(theme!.Topics);
            var publication = Assert.Single(topic!.Publications);

            Assert.Single(publication!.DownloadFiles);

            MockUtils.VerifyAllMocks(themeService);
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
                                        new MethodologySummaryViewModel
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

            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            methodologyService.Setup(mock => mock.GetTree())
                .ReturnsAsync(themes);

            var controller = BuildThemeController(methodologyService: methodologyService.Object);

            var result = await controller.GetMethodologyThemes();

            Assert.Equal(themes, result.Value);

            MockUtils.VerifyAllMocks(methodologyService);
        }

        private static ThemeController BuildThemeController(
            IThemeService? themeService = null,
            IMethodologyService? methodologyService = null
        )
        {
            return new(
                themeService ?? Mock.Of<IThemeService>(),
                methodologyService ?? Mock.Of<IMethodologyService>()
            );
        }
    }
}
