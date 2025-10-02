using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class RedirectsServiceTests
{
    [Fact]
    public async Task List_MethodologyRedirect_MethodologyNotPublished()
    {
        var methodology = new Methodology
        {
            LatestPublishedVersionId = null,
            Versions = new List<MethodologyVersion> { new() { AlternativeSlug = "redirect-to-slug" } },
        };

        var methodologyRedirect = new MethodologyRedirect
        {
            Slug = "redirect-from-slug",
            MethodologyVersion = methodology.Versions[0],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.MethodologyRedirects.AddAsync(methodologyRedirect);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var redirectsService = SetupRedirectsService(contentDbContext);

            var result = await redirectsService.List();

            var viewModel = result.AssertRight();

            Assert.Empty(viewModel.MethodologyRedirects);
        }
    }

    [Fact]
    public async Task List_MethodologyRedirect_PreviousVersionRedirectOnly()
    {
        var latestPublishedVersionId = Guid.NewGuid();
        var methodology = new Methodology
        {
            LatestPublishedVersionId = latestPublishedVersionId,
            OwningPublicationSlug = "no-redirect-to-1",
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    // previous version
                    AlternativeSlug = "no-redirect-to-2",
                    Version = 0,
                },
                new()
                {
                    Id = latestPublishedVersionId,
                    AlternativeSlug = "redirect-to",
                    Version = 1,
                },
            },
        };

        var methodologyRedirects = new List<MethodologyRedirect>
        {
            new() { Slug = "redirect-from-1", MethodologyVersion = methodology.Versions[0] },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.MethodologyRedirects.AddRangeAsync(methodologyRedirects);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var redirectsService = SetupRedirectsService(contentDbContext);

            var result = await redirectsService.List();

            var viewModel = result.AssertRight();
            var methodologyRedirectsViewModel = viewModel.MethodologyRedirects;

            var redirect = Assert.Single(methodologyRedirectsViewModel);

            Assert.Equal("redirect-from-1", redirect.FromSlug);
            Assert.Equal("redirect-to", redirect.ToSlug);
        }
    }

    [Fact]
    public async Task List_MethodologyRedirect_MultipleMethodologies()
    {
        var latestPublishedVersion1 = Guid.NewGuid();
        var methodology1 = new Methodology
        {
            LatestPublishedVersionId = latestPublishedVersion1,
            OwningPublicationSlug = "redirect-to-slug-1",
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = latestPublishedVersion1,
                    // No AlternativeSlug so uses OwningPublicationSlug
                    Version = 0,
                },
                new()
                {
                    // LatestVersion but unpublished
                    AlternativeSlug = "no-redirect-to-slug-1",
                    Version = 1,
                },
            },
        };

        var latestPublishedVersion2 = Guid.NewGuid();
        var methodology2 = new Methodology
        {
            LatestPublishedVersionId = latestPublishedVersion2,
            OwningPublicationSlug = "no-redirect-to-slug-2",
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = latestPublishedVersion2,
                    AlternativeSlug = "redirect-to-slug-2",
                    Version = 0,
                },
                new()
                {
                    // LatestVersion but unpublished
                    AlternativeSlug = "no-redirect-to-slug-3",
                    Version = 1,
                },
            },
        };

        var methodologyRedirects = new List<MethodologyRedirect>
        {
            new() { Slug = "redirect-from-slug-1", MethodologyVersion = methodology1.Versions[0] },
            new() { Slug = "redirect-from-slug-2", MethodologyVersion = methodology2.Versions[0] },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddRangeAsync(methodology1, methodology2);
            await contentDbContext.MethodologyRedirects.AddRangeAsync(methodologyRedirects);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var redirectsService = SetupRedirectsService(contentDbContext);

            var result = await redirectsService.List();

            var viewModel = result.AssertRight();
            var methodologyRedirectsViewModel = viewModel.MethodologyRedirects;

            Assert.Equal(2, methodologyRedirectsViewModel.Count);

            Assert.Equal("redirect-from-slug-1", methodologyRedirectsViewModel[0].FromSlug);
            Assert.Equal("redirect-to-slug-1", methodologyRedirectsViewModel[0].ToSlug);

            Assert.Equal("redirect-from-slug-2", methodologyRedirectsViewModel[1].FromSlug);
            Assert.Equal("redirect-to-slug-2", methodologyRedirectsViewModel[1].ToSlug);
        }
    }

    [Fact]
    public async Task List_MethodologyRedirect_FilterRedirectIfSameAsCurrentSlug()
    {
        var latestPublishedVersionId = Guid.NewGuid();
        var methodology = new Methodology
        {
            LatestPublishedVersionId = latestPublishedVersionId,
            OwningPublicationSlug = "no-redirect-to-1",
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    // previous version
                    AlternativeSlug = "no-redirect-to-2",
                    Version = 0,
                },
                new()
                {
                    Id = latestPublishedVersionId,
                    AlternativeSlug = "redirect-to",
                    Version = 1,
                },
            },
        };

        var methodologyRedirects = new List<MethodologyRedirect>
        {
            new() { Slug = "redirect-to", MethodologyVersion = methodology.Versions[0] },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.MethodologyRedirects.AddRangeAsync(methodologyRedirects);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var redirectsService = SetupRedirectsService(contentDbContext);

            var result = await redirectsService.List();

            var viewModel = result.AssertRight();
            Assert.Empty(viewModel.MethodologyRedirects);
        }
    }

    [Fact]
    public async Task List_MethodologyRedirect_DuplicateRedirects()
    {
        var latestPublishedVersionId = Guid.NewGuid();
        var methodology = new Methodology
        {
            LatestPublishedVersionId = latestPublishedVersionId,
            OwningPublicationSlug = "no-redirect-to-1",
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    // previous version
                    AlternativeSlug = "no-redirect-to-2",
                    Version = 0,
                },
                new()
                {
                    Id = latestPublishedVersionId,
                    AlternativeSlug = "redirect-to",
                    Version = 1,
                },
            },
        };

        var methodologyRedirects = new List<MethodologyRedirect>
        {
            new() { Slug = "duplicated-redirect", MethodologyVersion = methodology.Versions[0] },
            new() { Slug = "duplicated-redirect", MethodologyVersion = methodology.Versions[1] },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.MethodologyRedirects.AddRangeAsync(methodologyRedirects);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var redirectsService = SetupRedirectsService(contentDbContext);

            var result = await redirectsService.List();

            var viewModel = result.AssertRight();

            var redirect = Assert.Single(viewModel.MethodologyRedirects);
            Assert.Equal("duplicated-redirect", redirect.FromSlug);
            Assert.Equal("redirect-to", redirect.ToSlug);
        }
    }

    private static RedirectsService SetupRedirectsService(ContentDbContext? contentDbContext = null)
    {
        contentDbContext ??= InMemoryContentDbContext();

        return new(contentDbContext);
    }
}
