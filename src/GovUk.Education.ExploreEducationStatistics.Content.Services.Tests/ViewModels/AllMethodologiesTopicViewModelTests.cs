#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.ViewModels
{
    public class AllMethodologiesTopicViewModelTests
    {
        [Fact]
        public void RemovePublicationNodesWithoutMethodologiesAndSort_RemovesPublicationsWithoutMethodologies()
        {
            var model = new AllMethodologiesTopicViewModel
            {
                Title = "TopicWithPublications",
                Publications = AsList(
                    new AllMethodologiesPublicationViewModel
                    {
                        Title = "PublicationWithoutMethodology",
                        Methodologies = new List<MethodologyVersionSummaryViewModel>()
                    },
                    new AllMethodologiesPublicationViewModel
                    {
                        Title = "PublicationWithMethodology",
                        Methodologies = AsList(
                            new MethodologyVersionSummaryViewModel()
                        )
                    }
                )
            };

            model.RemovePublicationNodesWithoutMethodologiesAndSort();

            Assert.Single(model.Publications);
            Assert.Equal("PublicationWithMethodology", model.Publications[0].Title);
        }

        [Fact]
        public void RemovePublicationNodesWithoutMethodologiesAndSort_SortsPublicationsByTitle()
        {
            var model = new AllMethodologiesTopicViewModel
            {
                Title = "TopicWithPublications",
                Publications = AsList(
                    new AllMethodologiesPublicationViewModel
                    {
                        Title = "Publication C",
                        Methodologies = AsList(
                            new MethodologyVersionSummaryViewModel()
                        )
                    },
                    new AllMethodologiesPublicationViewModel
                    {
                        Title = "Publication A",
                        Methodologies = AsList(
                            new MethodologyVersionSummaryViewModel()
                        )
                    },
                    new AllMethodologiesPublicationViewModel
                    {
                        Title = "Publication B",
                        Methodologies = AsList(
                            new MethodologyVersionSummaryViewModel()
                        )
                    }
                )
            };

            model.RemovePublicationNodesWithoutMethodologiesAndSort();

            Assert.Equal(3, model.Publications.Count);
            Assert.Equal("Publication A", model.Publications[0].Title);
            Assert.Equal("Publication B", model.Publications[1].Title);
            Assert.Equal("Publication C", model.Publications[2].Title);
        }

        [Fact]
        public void RemovePublicationNodesWithoutMethodologiesAndSort_HandlesEmptyPublications()
        {
            var model = new AllMethodologiesTopicViewModel
            {
                Title = "TopicWithoutPublications",
                Publications = new List<AllMethodologiesPublicationViewModel>()
            };

            model.RemovePublicationNodesWithoutMethodologiesAndSort();

            Assert.Empty(model.Publications);
        }
    }
}
