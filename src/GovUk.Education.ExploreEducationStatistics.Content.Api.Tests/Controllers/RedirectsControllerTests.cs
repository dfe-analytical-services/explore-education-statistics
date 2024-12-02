#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public abstract class RedirectsControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    public class ListTests(TestApplicationFactory testApp) : RedirectsControllerTests(testApp)
    {
        public override async Task InitializeAsync() => await TestApp.StartAzurite();

        [Fact]
        public async Task RedirectsExist_Returns200WithRedirects()
        {
            var publicationRedirects = DataFixture.DefaultPublicationRedirect()
                .WithPublication(DataFixture.DefaultPublication())
                .GenerateList(3);

            var releaseRedirects = DataFixture.DefaultReleaseRedirect()
                .WithRelease(DataFixture.DefaultRelease())
                .GenerateList(3);

            var methodologyVersions = DataFixture.DefaultMethodologyVersion()
                .ForIndex(3, s => s.SetPublished(DateTime.UtcNow))
                .GenerateList(4);

            Methodology methodology = DataFixture.DefaultMethodology()
                .WithMethodologyVersions(methodologyVersions)
                .WithLatestPublishedVersion(methodologyVersions.Last());

            var methodologyRedirects = DataFixture.DefaultMethodologyRedirect()
                .ForIndex(0, s => s.SetMethodologyVersion(methodologyVersions[0]))
                .ForIndex(1, s => s.SetMethodologyVersion(methodologyVersions[1]))
                .ForIndex(2, s => s.SetMethodologyVersion(methodologyVersions[2]))
                .GenerateList(3);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.PublicationRedirects.AddRange(publicationRedirects);
                context.ReleaseRedirects.AddRange(releaseRedirects);
                context.MethodologyRedirects.AddRange(methodologyRedirects);
            });

            var response = await ListRedirects();

            var viewModel = response.AssertOk<RedirectsViewModel>();

            Assert.All(
                publicationRedirects,
                pr => Assert.Contains(
                    viewModel.Publications, 
                    rvm => pr.Slug == rvm.FromSlug && pr.Publication.Slug == rvm.ToSlug));
            Assert.All(
                releaseRedirects,
                rr => Assert.Contains(
                    viewModel.Releases,
                    rvm => rr.Slug == rvm.FromSlug && rr.Release.Slug == rvm.ToSlug));
            Assert.All(
                methodologyRedirects,
                mr => Assert.Contains(
                    viewModel.Methodologies,
                    rvm => mr.Slug == rvm.FromSlug && mr.MethodologyVersion.Methodology.OwningPublicationSlug == rvm.ToSlug));
        }

        [Fact]
        public async Task RedirectsExist_RedirectsAreCached()
        {
            var publicationRedirects = DataFixture.DefaultPublicationRedirect()
                .WithPublication(DataFixture.DefaultPublication())
                .GenerateList(3);

            var releaseRedirects = DataFixture.DefaultReleaseRedirect()
                .WithRelease(DataFixture.DefaultRelease())
                .GenerateList(3);

            var methodologyVersions = DataFixture.DefaultMethodologyVersion()
                .ForIndex(3, s => s.SetPublished(DateTime.UtcNow))
                .GenerateList(4);

            Methodology methodology = DataFixture.DefaultMethodology()
                .WithMethodologyVersions(methodologyVersions)
                .WithLatestPublishedVersion(methodologyVersions.Last());

            var methodologyRedirects = DataFixture.DefaultMethodologyRedirect()
                .ForIndex(0, s => s.SetMethodologyVersion(methodologyVersions[0]))
                .ForIndex(1, s => s.SetMethodologyVersion(methodologyVersions[1]))
                .ForIndex(2, s => s.SetMethodologyVersion(methodologyVersions[2]))
                .GenerateList(3);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.PublicationRedirects.AddRange(publicationRedirects);
                context.ReleaseRedirects.AddRange(releaseRedirects);
                context.MethodologyRedirects.AddRange(methodologyRedirects);
            });

            var app = BuildApp();
            var client = app.CreateClient();

            await ListRedirects(client);

            var blobCacheService = app.Services.GetRequiredService<IBlobCacheService>();

            var cachedValue = await blobCacheService.GetItemAsync(new RedirectsCacheKey(), typeof(RedirectsViewModel));
            var cachedRedirectsViewModel = Assert.IsType<RedirectsViewModel>(cachedValue);

            Assert.All(
                publicationRedirects,
                pr => Assert.Contains(
                    cachedRedirectsViewModel.Publications,
                    rvm => pr.Slug == rvm.FromSlug && pr.Publication.Slug == rvm.ToSlug));
            Assert.All(
                releaseRedirects,
                rr => Assert.Contains(
                    cachedRedirectsViewModel.Releases,
                    rvm => rr.Slug == rvm.FromSlug && rr.Release.Slug == rvm.ToSlug));
            Assert.All(
                methodologyRedirects,
                mr => Assert.Contains(
                    cachedRedirectsViewModel.Methodologies,
                    rvm => mr.Slug == rvm.FromSlug && mr.MethodologyVersion.Methodology.OwningPublicationSlug == rvm.ToSlug));
        }

        [Fact]
        public async Task RedirectsExistInCache_Returns200WithRedirects()
        {
            var app = BuildApp();
            var client = app.CreateClient();

            var blobCacheService = app.Services.GetRequiredService<IBlobCacheService>();

            var cachedPublicationRedirects = new List<RedirectViewModel>()
            {
                new(FromSlug: "publication_fromSlug_1", ToSlug: "publication_toSlug_1"),
                new(FromSlug: "publication_fromSlug_2", ToSlug: "publication_toSlug_2"),
                new(FromSlug: "publication_fromSlug_3", ToSlug: "publication_toSlug_3")
            };

            var cachedReleaseRedirects = new List<RedirectViewModel>()
            {
                new(FromSlug: "release_fromSlug_1", ToSlug: "release_toSlug_1"),
                new(FromSlug: "release_fromSlug_2", ToSlug: "release_toSlug_2"),
                new(FromSlug: "release_fromSlug_3", ToSlug: "release_toSlug_3")
            };

            var cachedMethodologyRedirects = new List<RedirectViewModel>()
            {
                new(FromSlug: "methodology_fromSlug_1", ToSlug: "methodology_toSlug_1"),
                new(FromSlug: "methodology_fromSlug_2", ToSlug: "methodology_toSlug_2"),
                new(FromSlug: "methodology_fromSlug_3", ToSlug: "methodology_toSlug_3")
            };

            var cachedViewModel = new RedirectsViewModel(
                Publications: cachedPublicationRedirects,
                Releases: cachedReleaseRedirects,
                Methodologies: cachedMethodologyRedirects);

            await blobCacheService.SetItemAsync(new RedirectsCacheKey(), cachedViewModel);

            var response = await ListRedirects(client);

            var viewModel = response.AssertOk<RedirectsViewModel>();

            Assert.All(
                cachedPublicationRedirects,
                cpr => Assert.Contains(
                    viewModel.Publications,
                    rvm => cpr.FromSlug == rvm.FromSlug && cpr.ToSlug == rvm.ToSlug));
            Assert.All(
                cachedReleaseRedirects,
                crr => Assert.Contains(
                    viewModel.Releases,
                    rvm => crr.FromSlug == rvm.FromSlug && crr.ToSlug == rvm.ToSlug));
            Assert.All(
                cachedMethodologyRedirects,
                cmr => Assert.Contains(
                    viewModel.Methodologies,
                    rvm => cmr.FromSlug == rvm.FromSlug && cmr.ToSlug == rvm.ToSlug));
        }

        [Fact]
        public async Task NoRedirectsExist_Returns200WithNoRedirects()
        {
            var response = await ListRedirects();

            var viewModel = response.AssertOk<RedirectsViewModel>();

            Assert.Empty(viewModel.Publications);
            Assert.Empty(viewModel.Releases);
            Assert.Empty(viewModel.Methodologies);
        }

        private async Task<HttpResponseMessage> ListRedirects(
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            return await client.GetAsync("/api/redirects");
        }
    }

    private WebApplicationFactory<Startup> BuildApp()
    {
        return TestApp
            .WithAzurite(enabled: true);
    }
}
