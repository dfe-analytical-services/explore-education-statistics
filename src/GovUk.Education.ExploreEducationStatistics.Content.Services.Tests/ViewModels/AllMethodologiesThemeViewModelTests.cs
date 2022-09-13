#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.ViewModels
{
    public class AllMethodologiesThemeViewModelTests
    {
        [Fact]
        public void RemoveTopicNodesWithoutMethodologiesAndSort_RemovesTopicsWithoutMethodologies()
        {
            var model = new AllMethodologiesThemeViewModel
            {
                Title = "ThemeWithTopics",
                Topics = AsList(
                    new AllMethodologiesTopicViewModel
                    {
                        Title = "TopicWithoutPublications",
                        Publications = new List<AllMethodologiesPublicationViewModel>()
                    },
                    new AllMethodologiesTopicViewModel
                    {
                        Title = "TopicWithPublicationsButNoMethodologies",
                        Publications = AsList(
                            new AllMethodologiesPublicationViewModel
                            {
                                Title = "PublicationWithNoMethodologies",
                                Methodologies = new List<MethodologyVersionSummaryViewModel>()
                            }
                        )
                    },
                    new AllMethodologiesTopicViewModel
                    {
                        Title = "TopicWithPublications",
                        Publications = AsList(
                            new AllMethodologiesPublicationViewModel
                            {
                                Title = "PublicationWithMethodologies",
                                Methodologies = AsList(
                                    new MethodologyVersionSummaryViewModel()
                                )
                            },
                            new AllMethodologiesPublicationViewModel
                            {
                                Title = "PublicationWithoutMethodologies",
                                Methodologies = new List<MethodologyVersionSummaryViewModel>()
                            }
                        )
                    }
                )
            };

            model.RemoveTopicNodesWithoutMethodologiesAndSort();

            Assert.Single(model.Topics);
            Assert.Equal("TopicWithPublications", model.Topics[0].Title);
            Assert.Single(model.Topics[0].Publications);
            Assert.Equal("PublicationWithMethodologies", model.Topics[0].Publications[0].Title);
        }

        [Fact]
        public void RemoveTopicNodesWithoutMethodologiesAndSort_SortsTopicsAndPublicationsByTitle()
        {
            var model = new AllMethodologiesThemeViewModel
            {
                Title = "ThemeWithTopics",
                Topics = AsList(
                    new AllMethodologiesTopicViewModel
                    {
                        Title = "Topic C",
                        Publications = AsList(
                            new AllMethodologiesPublicationViewModel
                            {
                                Title = "Topic C Publication C",
                                Methodologies = AsList(
                                    new MethodologyVersionSummaryViewModel()
                                )
                            },
                            new AllMethodologiesPublicationViewModel
                            {
                                Title = "Topic C Publication A",
                                Methodologies = AsList(
                                    new MethodologyVersionSummaryViewModel()
                                )
                            },
                            new AllMethodologiesPublicationViewModel
                            {
                                Title = "Topic C Publication B",
                                Methodologies = AsList(
                                    new MethodologyVersionSummaryViewModel()
                                )
                            }
                        )
                    },
                    new AllMethodologiesTopicViewModel
                    {
                        Title = "Topic A",
                        Publications = AsList(
                            new AllMethodologiesPublicationViewModel
                            {
                                Title = "Topic A Publication C",
                                Methodologies = AsList(
                                    new MethodologyVersionSummaryViewModel()
                                )
                            },
                            new AllMethodologiesPublicationViewModel
                            {
                                Title = "Topic A Publication A",
                                Methodologies = AsList(
                                    new MethodologyVersionSummaryViewModel()
                                )
                            },
                            new AllMethodologiesPublicationViewModel
                            {
                                Title = "Topic A Publication B",
                                Methodologies = AsList(
                                    new MethodologyVersionSummaryViewModel()
                                )
                            }
                        )
                    },
                    new AllMethodologiesTopicViewModel
                    {
                        Title = "Topic B",
                        Publications = AsList(
                            new AllMethodologiesPublicationViewModel
                            {
                                Title = "Topic B Publication C",
                                Methodologies = AsList(
                                    new MethodologyVersionSummaryViewModel()
                                )
                            },
                            new AllMethodologiesPublicationViewModel
                            {
                                Title = "Topic B Publication A",
                                Methodologies = AsList(
                                    new MethodologyVersionSummaryViewModel()
                                )
                            },
                            new AllMethodologiesPublicationViewModel
                            {
                                Title = "Topic B Publication B",
                                Methodologies = AsList(
                                    new MethodologyVersionSummaryViewModel()
                                )
                            }
                        )
                    }
                )
            };

            model.RemoveTopicNodesWithoutMethodologiesAndSort();

            Assert.Equal(3, model.Topics.Count);

            Assert.Equal("Topic A", model.Topics[0].Title);
            Assert.Equal("Topic B", model.Topics[1].Title);
            Assert.Equal("Topic C", model.Topics[2].Title);

            Assert.Equal(3, model.Topics[0].Publications.Count);
            Assert.Equal("Topic A Publication A", model.Topics[0].Publications[0].Title);
            Assert.Equal("Topic A Publication B", model.Topics[0].Publications[1].Title);
            Assert.Equal("Topic A Publication C", model.Topics[0].Publications[2].Title);

            Assert.Equal(3, model.Topics[1].Publications.Count);
            Assert.Equal("Topic B Publication A", model.Topics[1].Publications[0].Title);
            Assert.Equal("Topic B Publication B", model.Topics[1].Publications[1].Title);
            Assert.Equal("Topic B Publication C", model.Topics[1].Publications[2].Title);

            Assert.Equal(3, model.Topics[2].Publications.Count);
            Assert.Equal("Topic C Publication A", model.Topics[2].Publications[0].Title);
            Assert.Equal("Topic C Publication B", model.Topics[2].Publications[1].Title);
            Assert.Equal("Topic C Publication C", model.Topics[2].Publications[2].Title);
        }

        [Fact]
        public void RemoveTopicNodesWithoutMethodologiesAndSort_HandlesEmptyTopics()
        {
            var model = new AllMethodologiesThemeViewModel
            {
                Title = "ThemeWithoutTopics",
                Topics = new List<AllMethodologiesTopicViewModel>()
            };

            model.RemoveTopicNodesWithoutMethodologiesAndSort();

            Assert.Empty(model.Topics);
        }

        [Fact]
        public void RemoveTopicNodesWithoutMethodologiesAndSort_HandlesEmptyPublications()
        {
            var model = new AllMethodologiesThemeViewModel
            {
                Title = "ThemeWithTopics",
                Topics = AsList(
                    new AllMethodologiesTopicViewModel
                    {
                        Title = "TopicWithoutPublications",
                        Publications = new List<AllMethodologiesPublicationViewModel>()
                    }
                )
            };

            model.RemoveTopicNodesWithoutMethodologiesAndSort();

            Assert.Empty(model.Topics);
        }
    }
}
