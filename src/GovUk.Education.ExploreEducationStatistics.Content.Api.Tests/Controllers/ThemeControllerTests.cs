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

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class ThemeControllerTests
    {
        [Fact]
        public async Task GetThemes()
        {
            var fileStorageService = new Mock<IFileStorageService>();

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

            var controller = new ThemeController(fileStorageService.Object);

            var result = await controller.GetThemes();

            Assert.IsAssignableFrom<IEnumerable<ThemeTree<PublicationTreeNode>>>(result.Value);
            Assert.Single(result.Value);

            var theme = result.Value.First();
            Assert.IsType<ThemeTree<PublicationTreeNode>>(theme);
            Assert.Single(theme.Topics);

            var topic = theme.Topics.First();
            Assert.Single(topic.Publications);
        }

        [Fact]
        public async Task GetDownloadThemes()
        {
            var fileStorageService = new Mock<IFileStorageService>();

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

            var controller = new ThemeController(fileStorageService.Object);
            var result = await controller.GetDownloadThemes();

            Assert.IsAssignableFrom<IEnumerable<ThemeTree<PublicationDownloadTreeNode>>>(result.Value);
            Assert.Single(result.Value);

            var theme = result.Value.First();
            Assert.IsType<ThemeTree<PublicationDownloadTreeNode>>(theme);
            Assert.Single(theme.Topics);

            var topic = theme.Topics.First();
            Assert.Single(topic.Publications);

            var publication = topic.Publications.First();
            Assert.Single(publication.DownloadFiles);
        }

        [Fact]
        public async Task GetMethodologyThemes()
        {
            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);

            var controller = new ThemeController(fileStorageService.Object);

            var result = await controller.GetMethodologyThemes();

            // TODO SOW4 EES-2378 Return all public methodologies from content database
            // Assert.IsAssignableFrom<IEnumerable<ThemeTree<MethodologyTreeNode>>>(result.Value);
            Assert.Empty(result.Value);
            // Assert.Single(result.Value);
            //
            // var theme = result.Value.First();
            //
            // Assert.IsType<ThemeTree<MethodologyTreeNode>>(theme);
            // Assert.Single(theme.Topics);
            //
            // var topic = theme.Topics.First();
            // Assert.Single(topic.Publications);

            MockUtils.VerifyAllMocks(fileStorageService);
        }
    }
}
