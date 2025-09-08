using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Functions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Functions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Functions;

[Collection(CacheTestFixture.CollectionName)]
public class PublishTaxonomyFunctionTests(PublisherFunctionsIntegrationTestFixture fixture)
    : PublisherFunctionsIntegrationTest(fixture)
{
    public class PublishTaxonomyTests(
        PublisherFunctionsIntegrationTestFixture fixture,
        CacheServiceTestFixture cacheFixture
    ) : PublishTaxonomyFunctionTests(fixture), IClassFixture<CacheServiceTestFixture>
    {
        [Fact]
        public async Task MethodologyTree()
        {
            var theme = DataFixture.DefaultTheme().Generate();

            var publication = DataFixture.DefaultPublication().WithTheme(theme).Generate();

            var methodology = DataFixture
                .DefaultMethodology()
                .WithMethodologyVersions(DataFixture.DefaultMethodologyVersion().Generate(1))
                .FinishWith(methodology => methodology.LatestPublishedVersion = methodology.Versions[0])
                .WithOwningPublication(publication)
                .Generate();

            await AddTestData<ContentDbContext>(context =>
            {
                context.Methodologies.Add(methodology);
            });

            List<AllMethodologiesThemeViewModel> expectedMethodologyTree =
            [
                new AllMethodologiesThemeViewModel
                {
                    Id = theme.Id,
                    Title = theme.Title,
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
                                    Slug = methodology.Versions[0].Slug,
                                },
                            ],
                        },
                    ],
                },
            ];

            cacheFixture
                .PublicBlobCacheService.Setup(s =>
                    s.SetItemAsync<object>(new AllMethodologiesCacheKey(), ItIs.DeepEqualTo(expectedMethodologyTree))
                )
                .Returns(Task.CompletedTask);

            cacheFixture
                .PublicBlobCacheService.Setup(s =>
                    s.SetItemAsync<object>(new PublicationTreeCacheKey(), new List<PublicationTreeThemeViewModel>())
                )
                .Returns(Task.CompletedTask);

            var function = GetRequiredService<PublishTaxonomyFunction>();
            await function.PublishTaxonomy(new PublishTaxonomyMessage(), new TestFunctionContext());

            VerifyAllMocks(cacheFixture.PublicBlobCacheService);
        }
    }
}
