using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public abstract class RedirectsControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    public abstract class ListTests(TestApplicationFactory testApp) : RedirectsControllerTests(testApp)
    {
        public override async Task InitializeAsync() => await StartAzurite();

        public class GeneralTests(TestApplicationFactory testApp) : ListTests(testApp)
        {
            [Fact]
            public async Task RedirectsExistInCache_Returns200WithRedirects()
            {
                var app = BuildApp(enableAzurite: true);
                var client = app.CreateClient();

                var blobCacheService = app.Services.GetRequiredService<IBlobCacheService>();

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
                        new List<RedirectViewModel>()
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

                await blobCacheService.SetItemAsync(new RedirectsCacheKey(), cachedViewModel);

                var response = await ListRedirects(client);

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
                var client = BuildApp(enableAzurite: true).CreateClient();
                var response = await ListRedirects(client);

                var viewModel = response.AssertOk<RedirectsViewModel>();

                Assert.Empty(viewModel.PublicationRedirects);
                Assert.Empty(viewModel.ReleaseRedirectsByPublicationSlug);
                Assert.Empty(viewModel.MethodologyRedirects);
            }
        }

        public class MethodologyRedirectsTests(TestApplicationFactory testApp) : ListTests(testApp)
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

                await TestApp.AddTestData<ContentDbContext>(context => context.Methodologies.Add(methodology));

                var client = BuildApp(enableAzurite: true).CreateClient();
                var response = await ListRedirects(client);

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

                await TestApp.AddTestData<ContentDbContext>(context => context.Methodologies.Add(methodology));

                var app = BuildApp(enableAzurite: true);
                var client = app.CreateClient();

                await ListRedirects(client);

                var blobCacheService = app.Services.GetRequiredService<IBlobCacheService>();

                var cachedValue = await blobCacheService.GetItemAsync(
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

        public class PublicationRedirectsTests(TestApplicationFactory testApp) : ListTests(testApp)
        {
            [Fact]
            public async Task RedirectsExist_Returns200WithRedirects()
            {
                var publicationRedirects = DataFixture
                    .DefaultPublicationRedirect()
                    .WithPublication(DataFixture.DefaultPublication())
                    .GenerateList(2);

                await TestApp.AddTestData<ContentDbContext>(context =>
                    context.PublicationRedirects.AddRange(publicationRedirects)
                );

                var client = BuildApp(enableAzurite: true).CreateClient();
                var response = await ListRedirects(client);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                    context.PublicationRedirects.AddRange(publicationRedirects)
                );

                await StartAzurite();

                var app = BuildApp(enableAzurite: true);
                var client = app.CreateClient();

                await ListRedirects(client);

                var blobCacheService = app.Services.GetRequiredService<IBlobCacheService>();

                var cachedValue = await blobCacheService.GetItemAsync(
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

        public class ReleaseRedirectsTests(TestApplicationFactory testApp) : ListTests(testApp)
        {
            [Fact]
            public async Task ReleaseRedirectDoesNotExistForPublicationWithRedirect_Returns200WithRedirects()
            {
                PublicationRedirect publicationRedirect = DataFixture
                    .DefaultPublicationRedirect()
                    .WithPublication(DataFixture.DefaultPublication());

                await TestApp.AddTestData<ContentDbContext>(context =>
                    context.PublicationRedirects.Add(publicationRedirect)
                );

                var client = BuildApp(enableAzurite: true).CreateClient();
                var response = await ListRedirects(client);

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

                await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));

                var client = BuildApp(enableAzurite: true).CreateClient();
                var response = await ListRedirects(client);

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

                await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));

                var client = BuildApp(enableAzurite: true).CreateClient();
                var response = await ListRedirects(client);

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

                await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));

                var app = BuildApp(enableAzurite: true);
                var client = app.CreateClient();

                await ListRedirects(client);

                var blobCacheService = app.Services.GetRequiredService<IBlobCacheService>();

                var cachedValue = await blobCacheService.GetItemAsync(
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

        private async Task<HttpResponseMessage> ListRedirects(HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            return await client.GetAsync("/api/redirects");
        }
    }

    private WebApplicationFactory<Startup> BuildApp(bool enableAzurite = false)
    {
        List<Action<IWebHostBuilder>> configFuncs = enableAzurite ? [WithAzurite()] : [];
        return BuildWebApplicationFactory(configFuncs);
    }
}
