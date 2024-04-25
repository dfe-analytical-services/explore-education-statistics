using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Functions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Functions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Functions;

public class PublishTaxonomyFunctionTests(PublisherFunctionsIntegrationTestFixture fixture)
    : PublisherFunctionsIntegrationTest(fixture)
{
    public class PublishTaxonomyTests(PublisherFunctionsIntegrationTestFixture fixture)
        : PublishTaxonomyFunctionTests(fixture)
    {
        [Fact]
        public async Task MethodologyTree()
        {
            Topic topic = DataFixture
                .DefaultTopic()
                .WithTheme(DataFixture
                    .DefaultTheme());

            Publication publication = DataFixture
                .DefaultPublication()
                .WithTopic(topic);
            
            Methodology methodology = DataFixture
                .DefaultMethodology()
                .WithMethodologyVersions(DataFixture
                    .DefaultMethodologyVersion()
                    .Generate(1))
                .FinishWith(methodology => methodology.LatestPublishedVersion = methodology.Versions[0])
                .WithOwningPublication(publication);
            
            await AddTestData<ContentDbContext>(context =>
            {
                context.Methodologies.Add(methodology);
            });

            List<AllMethodologiesThemeViewModel> expectedMethodologyTree = 
            [
                new AllMethodologiesThemeViewModel()
                {
                    Id = topic.Theme.Id,
                    Title = topic.Theme.Title,
                    Topics =
                    [
                        new AllMethodologiesTopicViewModel
                        {
                            Id = topic.Id,
                            Title = topic.Title,
                            Publications =
                            [
                                new AllMethodologiesPublicationViewModel
                                {
                                    Id = publication.Id,
                                    Title = publication.Title,
                                    Methodologies =
                                    [
                                        new MethodologyVersionSummaryViewModel
                                        {
                                            Id = methodology.Versions[0].Id,
                                            Title = methodology.Versions[0].Title,
                                            Slug = methodology.Versions[0].Slug
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ];
            
            PublicBlobCacheService
                .Setup(s => s.SetItemAsync<object>(new AllMethodologiesCacheKey(), ItIs.DeepEqualTo(expectedMethodologyTree)))
                .Returns(Task.CompletedTask);
            
            PublicBlobCacheService
                .Setup(s => s.SetItemAsync<object>(new PublicationTreeCacheKey(), new List<PublicationTreeThemeViewModel>()))
                .Returns(Task.CompletedTask);
            
            var function = GetRequiredService<PublishTaxonomyFunction>();
            await function.PublishTaxonomy(new PublishTaxonomyMessage(), new TestFunctionContext());
            
            VerifyAllMocks(PublicBlobCacheService);
        }
    }
}
