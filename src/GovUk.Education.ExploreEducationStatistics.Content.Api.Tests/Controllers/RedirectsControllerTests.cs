using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures.Optimised;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Xunit;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

// ReSharper disable once ClassNeverInstantiated.Global
public class RedirectsControllerTestsFixture()
    : OptimisedContentApiCollectionFixture(capabilities: [ContentApiIntegrationTestCapability.Azurite])
{
    public IBlobCacheService BlobCacheService = null!;

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);

        BlobCacheService = lookups.GetService<IBlobCacheService>();
    }

    public override async Task BeforeEachTest()
    {
        await base.BeforeEachTest();

        await GetAzuriteWrapper().ClearTestData();
    }
}

[CollectionDefinition(nameof(RedirectsControllerTestsFixture))]
public class RedirectsControllerTestsCollection : ICollectionFixture<RedirectsControllerTestsFixture>;

[Collection(nameof(RedirectsControllerTestsFixture))]
public abstract class RedirectsControllerTests(RedirectsControllerTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private static readonly DataFixture DataFixture = new();

    public abstract class ListTests(RedirectsControllerTestsFixture fixture) : RedirectsControllerTests(fixture)
    {
        public class GeneralTests(RedirectsControllerTestsFixture fixture) : ListTests(fixture)
        {
            [Fact]
            public async Task RedirectsExistInCache_Returns200WithRedirects()
            {
                List<RedirectViewModel> cachedMethodologyRedirects =
                [
                    new(FromSlug: "original-methodology-slug-1", ToSlug: "updated-methodology-slug-1"),
                    new(FromSlug: "original-methodology-slug-2", ToSlug: "updated-methodology-slug-2"),
                ];

                List<RedirectViewModel> cachedPublicationRedirects =
                [
                    new(FromSlug: "original-publication-slug-1", ToSlug: "updated-publication-slug-1"),
                    new(FromSlug: "original-publication-slug-2", ToSlug: "updated-publication-slug-2"),
                ];

                var cachedReleaseRedirectsByPublicationSlug = new Dictionary<string, List<RedirectViewModel>>
                {
                    {
                        "updated-publication-slug-1",
                        new List<RedirectViewModel>
                        {
                            new(FromSlug: "original-release-slug-1", ToSlug: "updated-release-slug-1"),
                            new(FromSlug: "original-release-slug-2", ToSlug: "updated-release-slug-2"),
                        }
                    },
                };

                var cachedViewModel = new RedirectsViewModel(
                    PublicationRedirects: cachedPublicationRedirects,
                    ReleaseRedirectsByPublicationSlug: cachedReleaseRedirectsByPublicationSlug,
                    MethodologyRedirects: cachedMethodologyRedirects
                );

                await fixture.BlobCacheService.SetItemAsync(new RedirectsCacheKey(), cachedViewModel);

                var response = await ListRedirects();

                var viewModel = response.AssertOk<RedirectsViewModel>();

                Assert.Equal(cachedMethodologyRedirects.Count, viewModel.MethodologyRedirects.Count);
                Assert.All(
                    cachedMethodologyRedirects,
                    cmr =>
                        Assert.Contains(
                            viewModel.MethodologyRedirects,
                            rvm => cmr.FromSlug == rvm.FromSlug && cmr.ToSlug == rvm.ToSlug
                        )
                );

                Assert.Equal(cachedPublicationRedirects.Count, viewModel.PublicationRedirects.Count);
                Assert.All(
                    cachedPublicationRedirects,
                    cpr =>
                        Assert.Contains(
                            viewModel.PublicationRedirects,
                            rvm => cpr.FromSlug == rvm.FromSlug && cpr.ToSlug == rvm.ToSlug
                        )
                );

                var cachedReleaseRedirectsForPublication = Assert.Single(cachedReleaseRedirectsByPublicationSlug);
                Assert.Equal(
                    cachedReleaseRedirectsForPublication.Value.Count,
                    viewModel.ReleaseRedirectsByPublicationSlug["updated-publication-slug-1"].Count
                );
                Assert.Equal("updated-publication-slug-1", cachedReleaseRedirectsForPublication.Key);
                Assert.All(
                    cachedReleaseRedirectsForPublication.Value,
                    crr =>
                        Assert.Contains(
                            viewModel.ReleaseRedirectsByPublicationSlug["updated-publication-slug-1"],
                            rvm => crr.FromSlug == rvm.FromSlug && crr.ToSlug == rvm.ToSlug
                        )
                );
            }

            [Fact]
            public async Task NoRedirectsExist_Returns200WithNoRedirects()
            {
                var response = await ListRedirects();

                var viewModel = response.AssertOk<RedirectsViewModel>();

                Assert.Empty(viewModel.PublicationRedirects);
                Assert.Empty(viewModel.ReleaseRedirectsByPublicationSlug);
                Assert.Empty(viewModel.MethodologyRedirects);
            }
        }

        public class MethodologyRedirectsTests(RedirectsControllerTestsFixture fixture) : ListTests(fixture)
        {
            [Fact]
            public async Task RedirectsExist_Returns200WithRedirects()
            {
                Methodology methodology = DataFixture
                    .DefaultMethodology()
                    .WithMethodologyVersions(
                        DataFixture
                            .DefaultMethodologyVersion()
                            .WithRedirects([DataFixture.DefaultMethodologyRedirect()])
                            .ForIndex(0, s => s.SetRedirects([DataFixture.DefaultMethodologyRedirect()]))
                            .ForIndex(
                                1,
                                s =>
                                    s.SetRedirects([DataFixture.DefaultMethodologyRedirect()])
                                        .SetPublished(DateTime.UtcNow)
                            )
                            .GenerateList(2)
                    );

                await fixture.GetContentDbContext().AddTestData(context => context.Methodologies.Add(methodology));

                var response = await ListRedirects();

                var viewModel = response.AssertOk<RedirectsViewModel>();

                var redirects = methodology.Versions.SelectMany(mv => mv.MethodologyRedirects).ToList();

                Assert.Equal(redirects.Count, viewModel.MethodologyRedirects.Count);
                Assert.All(
                    redirects,
                    mr =>
                        Assert.Contains(
                            viewModel.MethodologyRedirects,
                            rvm =>
                                mr.Slug == rvm.FromSlug
                                && mr.MethodologyVersion.Methodology.OwningPublicationSlug == rvm.ToSlug
                        )
                );
            }

            [Fact]
            public async Task RedirectsExist_RedirectsAreCached()
            {
                Methodology methodology = DataFixture
                    .DefaultMethodology()
                    .WithMethodologyVersions(
                        DataFixture
                            .DefaultMethodologyVersion()
                            .WithRedirects([DataFixture.DefaultMethodologyRedirect()])
                            .ForIndex(0, s => s.SetRedirects([DataFixture.DefaultMethodologyRedirect()]))
                            .ForIndex(
                                1,
                                s =>
                                    s.SetRedirects([DataFixture.DefaultMethodologyRedirect()])
                                        .SetPublished(DateTime.UtcNow)
                            )
                            .GenerateList(2)
                    );

                await fixture.GetContentDbContext().AddTestData(context => context.Methodologies.Add(methodology));

                await ListRedirects();

                var cachedValue = await fixture.BlobCacheService.GetItemAsync(
                    new RedirectsCacheKey(),
                    typeof(RedirectsViewModel)
                );
                var cachedRedirectsViewModel = Assert.IsType<RedirectsViewModel>(cachedValue);

                var redirects = methodology.Versions.SelectMany(mv => mv.MethodologyRedirects).ToList();

                Assert.Empty(cachedRedirectsViewModel.PublicationRedirects);
                Assert.Empty(cachedRedirectsViewModel.ReleaseRedirectsByPublicationSlug);

                Assert.Equal(redirects.Count, cachedRedirectsViewModel.MethodologyRedirects.Count);
                Assert.All(
                    redirects,
                    mr =>
                        Assert.Contains(
                            cachedRedirectsViewModel.MethodologyRedirects,
                            rvm =>
                                mr.Slug == rvm.FromSlug
                                && mr.MethodologyVersion.Methodology.OwningPublicationSlug == rvm.ToSlug
                        )
                );
            }
        }

        public class PublicationRedirectsTests(RedirectsControllerTestsFixture fixture) : ListTests(fixture)
        {
            [Fact]
            public async Task RedirectsExist_Returns200WithRedirects()
            {
                var publicationRedirects = DataFixture
                    .DefaultPublicationRedirect()
                    .WithPublication(DataFixture.DefaultPublication())
                    .GenerateList(2);

                await fixture
                    .GetContentDbContext()
                    .AddTestData(context => context.PublicationRedirects.AddRange(publicationRedirects));

                var response = await ListRedirects();

                var viewModel = response.AssertOk<RedirectsViewModel>();

                Assert.Equal(publicationRedirects.Count, viewModel.PublicationRedirects.Count);
                Assert.All(
                    publicationRedirects,
                    pr =>
                        Assert.Contains(
                            viewModel.PublicationRedirects,
                            rvm => pr.Slug == rvm.FromSlug && pr.Publication.Slug == rvm.ToSlug
                        )
                );
            }

            [Fact]
            public async Task RedirectsExist_RedirectsAreCached()
            {
                var publicationRedirects = DataFixture
                    .DefaultPublicationRedirect()
                    .WithPublication(DataFixture.DefaultPublication())
                    .GenerateList(2);

                await fixture
                    .GetContentDbContext()
                    .AddTestData(context => context.PublicationRedirects.AddRange(publicationRedirects));

                await ListRedirects();

                var cachedValue = await fixture.BlobCacheService.GetItemAsync(
                    new RedirectsCacheKey(),
                    typeof(RedirectsViewModel)
                );
                var cachedRedirectsViewModel = Assert.IsType<RedirectsViewModel>(cachedValue);

                Assert.Empty(cachedRedirectsViewModel.MethodologyRedirects);
                Assert.Empty(cachedRedirectsViewModel.ReleaseRedirectsByPublicationSlug);

                Assert.Equal(publicationRedirects.Count, cachedRedirectsViewModel.PublicationRedirects.Count);
                Assert.All(
                    publicationRedirects,
                    pr =>
                        Assert.Contains(
                            cachedRedirectsViewModel.PublicationRedirects,
                            rvm => pr.Slug == rvm.FromSlug && pr.Publication.Slug == rvm.ToSlug
                        )
                );
            }
        }

        public class ReleaseRedirectsTests(RedirectsControllerTestsFixture fixture) : ListTests(fixture)
        {
            [Fact]
            public async Task ReleaseRedirectDoesNotExistForPublicationWithRedirect_Returns200WithRedirects()
            {
                PublicationRedirect publicationRedirect = DataFixture
                    .DefaultPublicationRedirect()
                    .WithPublication(DataFixture.DefaultPublication());

                await fixture
                    .GetContentDbContext()
                    .AddTestData(context => context.PublicationRedirects.Add(publicationRedirect));

                var response = await ListRedirects();

                var viewModel = response.AssertOk<RedirectsViewModel>();

                var publicationRedirectViewModel = Assert.Single(viewModel.PublicationRedirects);
                Assert.Equal(publicationRedirect.Slug, publicationRedirectViewModel.FromSlug);
                Assert.Equal(publicationRedirect.Publication.Slug, publicationRedirectViewModel.ToSlug);

                Assert.Empty(viewModel.ReleaseRedirectsByPublicationSlug);
            }

            [Fact]
            public async Task ReleaseRedirectExistsForPublicationWithRedirect_Returns200WithRedirects()
            {
                Publication publication = DataFixture
                    .DefaultPublication()
                    .WithReleases([
                        DataFixture
                            .DefaultRelease(publishedVersions: 1)
                            .WithSlug("updated-release-slug-1")
                            .WithRedirects(
                                DataFixture
                                    .DefaultReleaseRedirect()
                                    .ForIndex(0, s => s.SetSlug("first-release-slug-1"))
                                    .ForIndex(1, s => s.SetSlug("second-release-slug-1"))
                                    .GenerateList(2)
                            ),
                    ])
                    .WithSlug("updated-publication-slug-1")
                    .WithRedirects([DataFixture.DefaultPublicationRedirect().WithSlug("original-publication-slug-1")]);

                await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

                var response = await ListRedirects();

                var viewModel = response.AssertOk<RedirectsViewModel>();

                var publicationRedirectViewModel = Assert.Single(viewModel.PublicationRedirects);
                Assert.Equal("original-publication-slug-1", publicationRedirectViewModel.FromSlug);
                Assert.Equal("updated-publication-slug-1", publicationRedirectViewModel.ToSlug);

                var releaseRedirectsForPublication = Assert.Single(viewModel.ReleaseRedirectsByPublicationSlug);
                Assert.Equal("updated-publication-slug-1", releaseRedirectsForPublication.Key);

                Assert.Equal(2, releaseRedirectsForPublication.Value.Count);
                Assert.Contains(
                    releaseRedirectsForPublication.Value,
                    r => r.FromSlug == "first-release-slug-1" && r.ToSlug == "updated-release-slug-1"
                );
                Assert.Contains(
                    releaseRedirectsForPublication.Value,
                    r => r.FromSlug == "second-release-slug-1" && r.ToSlug == "updated-release-slug-1"
                );
            }

            [Fact]
            public async Task ReleaseRedirectExistsForPublicationWithoutRedirect_Returns200WithRedirects()
            {
                Publication publication = DataFixture
                    .DefaultPublication()
                    .WithReleases([
                        DataFixture
                            .DefaultRelease(publishedVersions: 1)
                            .WithSlug("updated-release-slug-1")
                            .WithRedirects(
                                DataFixture
                                    .DefaultReleaseRedirect()
                                    .ForIndex(0, s => s.SetSlug("first-release-slug-1"))
                                    .ForIndex(1, s => s.SetSlug("second-release-slug-1"))
                                    .GenerateList(2)
                            ),
                    ])
                    .WithSlug("original-publication-slug-1");

                await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

                var response = await ListRedirects();

                var viewModel = response.AssertOk<RedirectsViewModel>();

                Assert.Empty(viewModel.PublicationRedirects);

                var releaseRedirectsForPublication = Assert.Single(viewModel.ReleaseRedirectsByPublicationSlug);
                Assert.Equal("original-publication-slug-1", releaseRedirectsForPublication.Key);

                Assert.Equal(2, releaseRedirectsForPublication.Value.Count);
                Assert.Contains(
                    releaseRedirectsForPublication.Value,
                    r => r.FromSlug == "first-release-slug-1" && r.ToSlug == "updated-release-slug-1"
                );
                Assert.Contains(
                    releaseRedirectsForPublication.Value,
                    r => r.FromSlug == "second-release-slug-1" && r.ToSlug == "updated-release-slug-1"
                );
            }

            [Fact]
            public async Task RedirectsExist_RedirectsAreCached()
            {
                Publication publication = DataFixture
                    .DefaultPublication()
                    .WithReleases([
                        DataFixture
                            .DefaultRelease(publishedVersions: 1)
                            .WithSlug("updated-release-slug-1")
                            .WithRedirects(
                                DataFixture
                                    .DefaultReleaseRedirect()
                                    .ForIndex(0, s => s.SetSlug("first-release-slug-1"))
                                    .ForIndex(1, s => s.SetSlug("second-release-slug-1"))
                                    .GenerateList(2)
                            ),
                    ])
                    .WithSlug("original-publication-slug-1");

                await fixture.GetContentDbContext().AddTestData(context => context.Publications.Add(publication));

                await ListRedirects();

                var cachedValue = await fixture.BlobCacheService.GetItemAsync(
                    new RedirectsCacheKey(),
                    typeof(RedirectsViewModel)
                );
                var cachedRedirectsViewModel = Assert.IsType<RedirectsViewModel>(cachedValue);

                Assert.Empty(cachedRedirectsViewModel.MethodologyRedirects);
                Assert.Empty(cachedRedirectsViewModel.PublicationRedirects);

                var releaseRedirectsForPublication = Assert.Single(
                    cachedRedirectsViewModel.ReleaseRedirectsByPublicationSlug
                );
                Assert.Equal("original-publication-slug-1", releaseRedirectsForPublication.Key);

                Assert.Equal(2, releaseRedirectsForPublication.Value.Count);
                Assert.Contains(
                    releaseRedirectsForPublication.Value,
                    r => r.FromSlug == "first-release-slug-1" && r.ToSlug == "updated-release-slug-1"
                );
                Assert.Contains(
                    releaseRedirectsForPublication.Value,
                    r => r.FromSlug == "second-release-slug-1" && r.ToSlug == "updated-release-slug-1"
                );
            }
        }

        private async Task<HttpResponseMessage> ListRedirects()
        {
            return await fixture.CreateClient().GetAsync("/api/redirects");
        }
    }
}
