using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
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
            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);
            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            fileStorageService
                .Setup(
                    s => s.GetDeserialized<IEnumerable<ThemeTree<PublicationTreeNode>>>(
                        "publications/tree.json"
                    )
                )
                .ReturnsAsync(
                    new List<ThemeTree<PublicationTreeNode>>
                    {
                        new ThemeTree<PublicationTreeNode>
                        {
                            Topics = new List<TopicTree<PublicationTreeNode>>
                            {
                                new TopicTree<PublicationTreeNode>
                                {
                                    Publications = new List<PublicationTreeNode>
                                    {
                                        new PublicationTreeNode()
                                    }
                                }
                            }
                        }
                    }
                );

            var controller = BuildThemeController(fileStorageService.Object,
                methodologyService.Object);

            var result = await controller.GetThemes();

            Assert.Single(result.Value);

            var theme = result.Value.First();
            Assert.IsType<ThemeTree<PublicationTreeNode>>(theme);
            Assert.Single(theme.Topics);

            var topic = theme.Topics.First();
            Assert.Single(topic.Publications);

            MockUtils.VerifyAllMocks(fileStorageService, methodologyService);
        }

        [Fact]
        public async Task GetDownloadThemes()
        {
            var fileStorageService = new Mock<IFileStorageService>();
            var methodologyService = new Mock<IMethodologyService>();

            fileStorageService
                .Setup(
                    s => s.GetDeserialized<IEnumerable<ThemeTree<PublicationDownloadTreeNode>>>(
                        "download/tree.json"
                    )
                )
                .ReturnsAsync(
                    new List<ThemeTree<PublicationDownloadTreeNode>>
                    {
                        new ThemeTree<PublicationDownloadTreeNode>
                        {
                            Topics = new List<TopicTree<PublicationDownloadTreeNode>>
                            {
                                new TopicTree<PublicationDownloadTreeNode>
                                {
                                    Publications = new List<PublicationDownloadTreeNode>
                                    {
                                        new PublicationDownloadTreeNode
                                        {
                                            DownloadFiles = new List<FileInfo>
                                            {
                                                new FileInfo()
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                );

            var controller = BuildThemeController(fileStorageService.Object,
                methodologyService.Object);

            var result = await controller.GetDownloadThemes();

            Assert.Single(result.Value);

            var theme = result.Value.First();
            Assert.IsType<ThemeTree<PublicationDownloadTreeNode>>(theme);
            Assert.Single(theme.Topics);

            var topic = theme.Topics.First();
            Assert.Single(topic.Publications);

            var publication = topic.Publications.First();
            Assert.Single(publication.DownloadFiles);

            MockUtils.VerifyAllMocks(fileStorageService, methodologyService);
        }

        [Fact]
        public async Task GetMethodologyThemes()
        {
            var themes = AsList(
                new ThemeTree<PublicationMethodologiesTreeNode>
                {
                    Id = Guid.NewGuid(),
                    Summary = "Publication summary",
                    Title = "Publication title",
                    Topics = AsList(
                        new TopicTree<PublicationMethodologiesTreeNode>
                        {
                            Id = Guid.NewGuid(),
                            Summary = "Topic summary",
                            Title = "Topic title",
                            Publications = AsList(
                                new PublicationMethodologiesTreeNode
                                {
                                    Id = Guid.NewGuid(),
                                    Summary = "Publication summary",
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

            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);
            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            methodologyService.Setup(mock => mock.GetTree())
                .ReturnsAsync(themes);

            var controller = BuildThemeController(fileStorageService.Object,
                methodologyService.Object);

            var result = await controller.GetMethodologyThemes();

            Assert.Equal(themes, result.Value);

            MockUtils.VerifyAllMocks(fileStorageService, methodologyService);
        }

        private static ThemeController BuildThemeController(
            IFileStorageService fileStorageService = null,
            IMethodologyService methodologyService = null
        )
        {
            return new ThemeController(
                fileStorageService ?? new Mock<IFileStorageService>().Object,
                methodologyService ?? new Mock<IMethodologyService>().Object
            );
        }
    }
}
