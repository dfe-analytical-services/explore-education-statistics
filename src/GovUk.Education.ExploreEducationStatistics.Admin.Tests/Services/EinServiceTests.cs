#nullable enable

using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class EducationInNumbersServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task GetPage_Success()
    {
        var createdByUserId = Guid.NewGuid();
        var updatedByUserId = Guid.NewGuid();

        var page = new EinPage
        {
            Id = Guid.NewGuid(),
            Title = "Test page",
            Slug = "test-page",
            Description = "Test page description",
            Order = 0,
        };
        var latestPageVersion = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            Version = 1,
            Published = new DateTime(2005, 12, 25, 12, 00, 00),
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = createdByUserId,
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = updatedByUserId,
            EinPageId = page.Id,
        };
        var previousPageVersion = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            Version = 0,
            EinPageId = page.Id,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPages.Add(page);
            contentDbContext.EinPageVersions.AddRange(latestPageVersion, previousPageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.GetPageVersion(latestPageVersion.Id);
            var fetchedPageVersion = result.AssertRight();

            Assert.Multiple(
                () => Assert.Equal(latestPageVersion.Id, fetchedPageVersion.Id),
                () => Assert.Equal(page.Title, fetchedPageVersion.Title),
                () => Assert.Equal(page.Slug, fetchedPageVersion.Slug),
                () => Assert.Equal(page.Description, fetchedPageVersion.Description),
                () => Assert.Equal(latestPageVersion.Version, fetchedPageVersion.Version),
                () => Assert.Equal(latestPageVersion.Published, fetchedPageVersion.Published),
                () => Assert.Equal(page.Order, fetchedPageVersion.Order)
            );
        }
    }

    [Fact]
    public async Task GetPage_NotFound()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        var service = CreateService(contentDbContext);

        var result = await service.GetPageVersion(Guid.NewGuid());
        result.AssertNotFound();
    }

    [Fact]
    public async Task ListLatestPages_Success()
    {
        var createdByUserId = Guid.NewGuid();
        var updatedByUserId = Guid.NewGuid();

        var einPageId = Guid.NewGuid();
        var einPageLatestVersionId = Guid.NewGuid();
        var einPage = new EinPage
        {
            Id = einPageId,
            Title = "Education in numbers",
            Slug = null,
            Description = "Education in numbers description",
            Order = 3,
            LatestVersionId = einPageLatestVersionId,
            LatestPublishedVersionId = einPageLatestVersionId,
            PageVersions =
            [
                new EinPageVersion
                {
                    Id = einPageLatestVersionId,
                    EinPageId = einPageId,
                    Version = 0,
                    Published = new DateTime(2002, 12, 25, 12, 00, 00),
                    Created = new DateTime(2002, 12, 23, 12, 00, 00),
                    CreatedById = createdByUserId,
                    Updated = new DateTime(2002, 12, 24, 12, 00, 00),
                    UpdatedById = updatedByUserId,
                },
            ],
        };

        var page1LatestVersionId = Guid.NewGuid();
        var page1 = new EinPage
        {
            Id = Guid.NewGuid(),
            Title = "Page 1",
            Slug = "page-1",
            Description = "Test 1 description",
            Order = 1,
            LatestVersionId = page1LatestVersionId,
            LatestPublishedVersionId = page1LatestVersionId,
        };
        var page1LatestVersion = new EinPageVersion
        {
            Id = page1LatestVersionId,
            EinPageId = page1.Id,
            Version = 1,
            Published = new DateTime(2005, 12, 25, 12, 00, 00),
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = createdByUserId,
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = updatedByUserId,
        };

        var page1Previous = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            EinPageId = page1.Id,
            Version = 0,
        };

        var page2Id = Guid.NewGuid();
        var page2LatestVersionId = Guid.NewGuid();
        var page2 = new EinPage
        {
            Id = page2Id,
            Title = "Page 2",
            Slug = "page-2",
            Description = "Test 2 description",
            Order = 0,
            LatestVersionId = page2LatestVersionId,
            LatestPublishedVersionId = page2LatestVersionId,
            PageVersions =
            [
                new EinPageVersion
                {
                    Id = page2LatestVersionId,
                    EinPageId = page2Id,
                    Version = 0,
                    Published = null,
                    Created = new DateTime(2004, 12, 23, 12, 00, 00),
                    CreatedById = createdByUserId,
                    Updated = new DateTime(2004, 12, 24, 12, 00, 00),
                    UpdatedById = updatedByUserId,
                },
            ],
        };

        var page3Id = Guid.NewGuid();
        var page3LatestVersionId = Guid.NewGuid();
        var page3 = new EinPage
        {
            Id = page3Id,
            Title = "Page 3",
            Slug = "page-3",
            Description = "Test 3 description",
            Order = 2,
            LatestVersionId = page3LatestVersionId,
            LatestPublishedVersionId = page3LatestVersionId,
            PageVersions =
            [
                new EinPageVersion
                {
                    Id = page3LatestVersionId,
                    EinPageId = page3Id,
                    Version = 0,
                    Published = new DateTime(2003, 12, 25, 12, 00, 00),
                    Created = new DateTime(2003, 12, 23, 12, 00, 00),
                    CreatedById = createdByUserId,
                    Updated = new DateTime(2003, 12, 24, 12, 00, 00),
                    UpdatedById = updatedByUserId,
                },
            ],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPages.AddRange(einPage, page1, page2, page3);
            contentDbContext.EinPageVersions.AddRange(page1LatestVersion, page1Previous);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.ListLatestPages();
            var pageSummaryList = result.AssertRight();

            // returned in order
            Assert.Multiple(
                () => Assert.Equal(page2.PageVersions[0].Id, pageSummaryList[0].Id),
                () => Assert.Equal(page2.Title, pageSummaryList[0].Title),
                () => Assert.Equal(page2.Slug, pageSummaryList[0].Slug),
                () => Assert.Equal(page2.Description, pageSummaryList[0].Description),
                () => Assert.Equal(page2.PageVersions[0].Version, pageSummaryList[0].Version),
                () => Assert.Equal(page2.PageVersions[0].Published, pageSummaryList[0].Published),
                () => Assert.Equal(page2.Order, pageSummaryList[0].Order),
                () => Assert.Null(pageSummaryList[0].PreviousVersionId)
            );

            Assert.Multiple(
                () => Assert.Equal(page1LatestVersion.Id, pageSummaryList[1].Id),
                () => Assert.Equal(page1LatestVersion.EinPage.Title, pageSummaryList[1].Title),
                () => Assert.Equal(page1LatestVersion.EinPage.Slug, pageSummaryList[1].Slug),
                () => Assert.Equal(page1LatestVersion.EinPage.Description, pageSummaryList[1].Description),
                () => Assert.Equal(page1LatestVersion.Version, pageSummaryList[1].Version),
                () => Assert.Equal(page1LatestVersion.Published, pageSummaryList[1].Published),
                () => Assert.Equal(page1LatestVersion.EinPage.Order, pageSummaryList[1].Order),
                () => Assert.Equal(page1Previous.Id, pageSummaryList[1].PreviousVersionId)
            );

            Assert.Multiple(
                () => Assert.Equal(page3.PageVersions[0].Id, pageSummaryList[2].Id),
                () => Assert.Equal(page3.Title, pageSummaryList[2].Title),
                () => Assert.Equal(page3.Slug, pageSummaryList[2].Slug),
                () => Assert.Equal(page3.Description, pageSummaryList[2].Description),
                () => Assert.Equal(page3.PageVersions[0].Version, pageSummaryList[2].Version),
                () => Assert.Equal(page3.PageVersions[0].Published, pageSummaryList[2].Published),
                () => Assert.Equal(page3.Order, pageSummaryList[2].Order),
                () => Assert.Null(pageSummaryList[2].PreviousVersionId)
            );

            Assert.Multiple(
                () => Assert.Equal(einPage.PageVersions[0].Id, pageSummaryList[3].Id),
                () => Assert.Equal(einPage.Title, pageSummaryList[3].Title),
                () => Assert.Equal(einPage.Slug, pageSummaryList[3].Slug),
                () => Assert.Equal(einPage.Description, pageSummaryList[3].Description),
                () => Assert.Equal(einPage.PageVersions[0].Version, pageSummaryList[3].Version),
                () => Assert.Equal(einPage.PageVersions[0].Published, pageSummaryList[3].Published),
                () => Assert.Equal(einPage.Order, pageSummaryList[3].Order),
                () => Assert.Null(pageSummaryList[3].PreviousVersionId)
            );
        }
    }

    [Fact]
    public async Task CreatePage_Success()
    {
        var testPageId = Guid.NewGuid();
        var testPageVersionId = Guid.NewGuid();
        var testPage = new EinPage
        {
            Id = testPageId,
            Title = "Test page",
            Slug = "test-page",
            Description = "Test page description",
            Order = 0,
            LatestVersionId = testPageVersionId,
            LatestPublishedVersionId = testPageVersionId,
            PageVersions =
            [
                new EinPageVersion
                {
                    Id = testPageVersionId,
                    Version = 0,
                    Published = new DateTime(2005, 12, 25, 12, 00, 00),
                    Created = new DateTime(2005, 12, 23, 12, 00, 00),
                    CreatedById = Guid.NewGuid(),
                    Updated = new DateTime(2005, 12, 24, 12, 00, 00),
                    UpdatedById = Guid.NewGuid(),
                    EinPageId = testPageId,
                },
            ],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPages.Add(testPage);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.CreatePage(
                new CreateEducationInNumbersPageRequest { Title = "New page", Description = "New page description" }
            );
            var fetchedPage = result.AssertRight();

            Assert.Multiple(
                () => Assert.Equal("New page", fetchedPage.Title),
                () => Assert.Equal("new-page", fetchedPage.Slug),
                () => Assert.Equal("New page description", fetchedPage.Description),
                () => Assert.Equal(0, fetchedPage.Version),
                () => Assert.Null(fetchedPage.Published),
                () => Assert.Equal(1, fetchedPage.Order)
            );
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var pages = contentDbContext.EinPages.OrderBy(p => p.Order).ToList();
            Assert.Equal(2, pages.Count);
            Assert.Equal(testPage.Id, pages[0].Id);
            Assert.Multiple(
                () => Assert.Equal("New page", pages[1].Title),
                () => Assert.Equal("new-page", pages[1].Slug),
                () => Assert.Equal("New page description", pages[1].Description),
                () => Assert.Equal(1, pages[1].Order)
            );

            Assert.Equal(2, contentDbContext.EinPageVersions.ToList().Count);

            var newPageVersion = contentDbContext.EinPageVersions.Single(pageVersion =>
                pageVersion.Id != testPageVersionId
            );
            Assert.Multiple(
                () => Assert.Equal(0, newPageVersion.Version),
                () => Assert.Null(newPageVersion.Published),
                () => newPageVersion.Created.AssertUtcNow(),
                () => Assert.Equal(_userId, newPageVersion.CreatedById),
                () => Assert.Null(newPageVersion.Updated),
                () => Assert.Null(newPageVersion.UpdatedById),
                () => Assert.Equal(pages[1].Id, newPageVersion.EinPageId)
            );
        }
    }

    [Fact]
    public async Task CreatePage_TitleNotUnique_ValidationError()
    {
        var testPage = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            EinPage = new EinPage
            {
                Title = "Test page",
                Slug = "test-page",
                Description = "Test page description",
                Order = 0,
            },
            Version = 0,
            Published = new DateTime(2005, 12, 25, 12, 00, 00),
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.Add(testPage);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.CreatePage(
                new CreateEducationInNumbersPageRequest { Title = "Test page", Description = "Test page description" }
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasGlobalError(ValidationErrorMessages.TitleNotUnique);
        }
    }

    [Fact]
    public async Task CreatePage_SlugNotUnique_ValidationError()
    {
        var testPage = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            EinPage = new EinPage
            {
                Title = "Test page",
                Slug = "test-page",
                Description = "Test page description",
                Order = 0,
            },
            Version = 0,
            Published = new DateTime(2005, 12, 25, 12, 00, 00),
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.Add(testPage);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.CreatePage(
                new CreateEducationInNumbersPageRequest
                {
                    Title = "Test page!", // exclaimation is included in title, but not in generated slug
                    Description = "Test page description",
                }
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasGlobalError(ValidationErrorMessages.SlugNotUnique);
        }
    }

    [Fact]
    public async Task CreateAmendment_Success()
    {
        var originalPageVersionId = Guid.NewGuid();
        var originalPageVersion = new EinPageVersion
        {
            Id = originalPageVersionId,
            EinPage = new EinPage
            {
                Title = "Page title",
                Slug = "page-title",
                Description = "Page description",
                Order = 0,
                LatestVersionId = originalPageVersionId,
                LatestPublishedVersionId = originalPageVersionId,
            },
            Version = 0,
            Published = new DateTime(2005, 12, 25, 12, 00, 00),
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.Add(originalPageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.CreateAmendment(originalPageVersion.Id);
            var amendment = result.AssertRight();

            Assert.Multiple(
                () => Assert.Equal("Page title", amendment.Title),
                () => Assert.Equal("page-title", amendment.Slug),
                () => Assert.Equal("Page description", amendment.Description),
                () => Assert.Equal(originalPageVersion.Version + 1, amendment.Version),
                () => Assert.Null(amendment.Published),
                () => Assert.Equal(0, amendment.Order)
            );
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var pageVersions = contentDbContext.EinPageVersions.OrderBy(p => p.Version).ToList();

            Assert.Equal(2, pageVersions.Count);

            Assert.Equal(originalPageVersion.Id, pageVersions[0].Id);

            Assert.Multiple(
                () => Assert.Equal(1, pageVersions[1].Version),
                () => Assert.Null(pageVersions[1].Published),
                () => pageVersions[1].Created.AssertUtcNow(),
                () => Assert.Equal(_userId, pageVersions[1].CreatedById),
                () => Assert.Null(pageVersions[1].Updated),
                () => Assert.Null(pageVersions[1].UpdatedById),
                () => Assert.Equal(originalPageVersion.EinPageId, pageVersions[1].EinPageId)
            );

            var page = Assert.Single(contentDbContext.EinPages.ToList());
            Assert.Equal(pageVersions[1].Id, page.LatestVersionId);
        }
    }

    [Fact]
    public async Task CreateAmendment_ClonesContent_Success()
    {
        var originalPageVersionId = Guid.NewGuid();
        var originalPageVersion = new EinPageVersion
        {
            Id = originalPageVersionId,
            EinPage = new EinPage
            {
                Title = "Page title",
                Slug = "page-title",
                Description = "Page description",
                Order = 0,
                LatestVersionId = originalPageVersionId,
                LatestPublishedVersionId = originalPageVersionId,
            },
            Version = 0,
            Published = DateTime.UtcNow,
            Content =
            [
                new EinContentSection
                {
                    Id = Guid.NewGuid(),
                    Order = 0,
                    Heading = "Section 1",
                    Content =
                    [
                        new EinHtmlBlock
                        {
                            Id = Guid.NewGuid(),
                            Order = 0,
                            Body = "Block 1.1 body",
                        },
                        new EinHtmlBlock
                        {
                            Id = Guid.NewGuid(),
                            Order = 1,
                            Body = "Block 1.2 body",
                        },
                    ],
                },
                new EinContentSection
                {
                    Id = Guid.NewGuid(),
                    Order = 1,
                    Heading = "Section 2",
                    Content =
                    [
                        new EinHtmlBlock
                        {
                            Id = Guid.NewGuid(),
                            Order = 0,
                            Body = "Block 2.1 body",
                        },
                    ],
                },
            ],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.Add(originalPageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        Guid amendmentId;
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.CreateAmendment(originalPageVersion.Id);
            var amendment = result.AssertRight();
            amendmentId = amendment.Id;
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var amendment = await contentDbContext
                .EinPageVersions.Include(p => p.Content)
                    .ThenInclude(s => s.Content)
                .SingleAsync(p => p.Id == amendmentId);

            Assert.NotEqual(originalPageVersion.Id, amendment.Id);
            Assert.Equal(originalPageVersion.Content.Count, amendment.Content.Count);

            var originalSection1 = originalPageVersion.Content[0];
            var amendmentSection1 = amendment.Content[0];
            Assert.NotEqual(originalSection1.Id, amendmentSection1.Id);
            Assert.Equal(amendment.Id, amendmentSection1.EinPageVersionId);
            Assert.Equal(originalSection1.Order, amendmentSection1.Order);
            Assert.Equal(originalSection1.Heading, amendmentSection1.Heading);
            Assert.Equal(originalSection1.Content.Count, amendmentSection1.Content.Count);

            var originalSection1Block1 = (EinHtmlBlock)originalSection1.Content[0];
            var amendmentSection1Block1 = Assert.IsType<EinHtmlBlock>(amendmentSection1.Content[0]);
            Assert.NotEqual(originalSection1Block1.Id, amendmentSection1Block1.Id);
            Assert.Equal(amendmentSection1.Id, amendmentSection1Block1.EinContentSectionId);
            Assert.Equal(originalSection1Block1.Order, amendmentSection1Block1.Order);
            Assert.Equal(originalSection1Block1.Body, amendmentSection1Block1.Body);

            var originalSection1Block2 = (EinHtmlBlock)originalSection1.Content[1];
            var amendmentSection1Block2 = Assert.IsType<EinHtmlBlock>(amendmentSection1.Content[1]);
            Assert.NotEqual(originalSection1Block2.Id, amendmentSection1Block2.Id);
            Assert.Equal(amendmentSection1.Id, amendmentSection1Block2.EinContentSectionId);
            Assert.Equal(originalSection1Block2.Order, amendmentSection1Block2.Order);
            Assert.Equal(originalSection1Block2.Body, amendmentSection1Block2.Body);

            var originalSection2 = originalPageVersion.Content[1];
            var amendmentSection2 = amendment.Content[1];
            Assert.NotEqual(originalSection2.Id, amendmentSection2.Id);
            Assert.Equal(amendment.Id, amendmentSection2.EinPageVersionId);
            Assert.Equal(originalSection2.Order, amendmentSection2.Order);
            Assert.Equal(originalSection2.Heading, amendmentSection2.Heading);
            Assert.Equal(originalSection2.Content.Count, amendmentSection2.Content.Count);

            var amendmentSection2Block1 = Assert.IsType<EinHtmlBlock>(amendmentSection2.Content[0]);
            var originalSection2Block1 = (EinHtmlBlock)originalSection2.Content[0];
            Assert.NotEqual(originalSection2Block1.Id, amendmentSection2Block1.Id);
            Assert.Equal(amendmentSection2.Id, amendmentSection2Block1.EinContentSectionId);
            Assert.Equal(originalSection2Block1.Order, amendmentSection2Block1.Order);
            Assert.Equal(originalSection2Block1.Body, amendmentSection2Block1.Body);
        }
    }

    [Fact]
    public async Task CreateAmendment_OriginalNotFound()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        var service = CreateService(contentDbContext);

        var result = await service.CreateAmendment(Guid.NewGuid());
        result.AssertNotFound();
    }

    [Fact]
    public async Task CreateAmendment_OriginalNotPublished_ThrowsException()
    {
        var originalPageVersionId = Guid.NewGuid();
        var originalPageVersion = new EinPageVersion
        {
            Id = originalPageVersionId,
            EinPage = new EinPage
            {
                Title = "Page title",
                Slug = "page-title",
                Description = "Page description",
                Order = 0,
                LatestVersionId = originalPageVersionId,
            },
            Version = 0,
            Published = null,
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.Add(originalPageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateAmendment(originalPageVersion.Id)
            );
            Assert.Equal("Can only create amendment of latest published page version", exception.Message);
        }
    }

    [Fact]
    public async Task CreateAmendment_AmendmentAlreadyExists_ThrowsException()
    {
        var originalPageId = Guid.NewGuid();
        var originalPageVersionId = Guid.NewGuid();
        var amendmentVersionId = Guid.NewGuid();
        var originalPage = new EinPage
        {
            Id = originalPageId,
            Title = "Page title",
            Slug = "page-title",
            Description = "Page description",
            Order = 0,
            LatestVersionId = amendmentVersionId,
            LatestPublishedVersionId = originalPageVersionId,
        };
        var originalPageVersion = new EinPageVersion
        {
            Id = originalPageVersionId,
            EinPage = originalPage,
            EinPageId = originalPageId,
            Version = 0,
            Published = new DateTime(2005, 12, 25, 12, 00, 00),
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };
        var amendmentPageVersion = new EinPageVersion
        {
            Id = amendmentVersionId,
            EinPage = originalPage,
            EinPageId = originalPageId,
            Version = 1,
            Published = null,
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPages.Add(originalPage);
            contentDbContext.EinPageVersions.AddRange(originalPageVersion, amendmentPageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.CreateAmendment(originalPageVersion.Id)
            );
            Assert.Equal($"Amendment already exists for page version {originalPageVersion.Id}", exception.Message);
        }
    }

    [Fact]
    public async Task UpdatePage_Success()
    {
        var originalPage = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            EinPage = new EinPage
            {
                Title = "Page title",
                Slug = "page-title",
                Description = "Page description",
                Order = 0,
            },
            Version = 0,
            Published = null,
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.AddRange(originalPage);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.UpdatePage(
                originalPage.Id,
                new UpdateEducationInNumbersPageRequest
                {
                    Title = "New page title",
                    Description = "New page description",
                }
            );
            var updatedPageVersion = result.AssertRight();

            Assert.Multiple(
                () => Assert.Equal("New page title", updatedPageVersion.Title),
                () => Assert.Equal("new-page-title", updatedPageVersion.Slug),
                () => Assert.Equal("New page description", updatedPageVersion.Description),
                () => Assert.Equal(originalPage.Version, updatedPageVersion.Version), // we're updating this version of the page, not creating a new page version
                () => Assert.Null(updatedPageVersion.Published),
                () => Assert.Equal(0, updatedPageVersion.Order)
            );
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var pageVersions = contentDbContext
                .EinPageVersions.Include(pageVersion => pageVersion.EinPage)
                .OrderBy(p => p.Version)
                .ToList();

            var updatedPageVersion = Assert.Single(pageVersions);

            Assert.Multiple(
                () => Assert.Equal(originalPage.Id, updatedPageVersion.Id),
                () => Assert.Equal(0, updatedPageVersion.Version),
                () => Assert.Null(updatedPageVersion.Published),
                () => Assert.Equal(originalPage.Created, updatedPageVersion.Created),
                () => Assert.Equal(originalPage.CreatedById, updatedPageVersion.CreatedById),
                () => Assert.Equal(originalPage.Updated, updatedPageVersion.Updated),
                () => Assert.Equal(originalPage.UpdatedById, updatedPageVersion.UpdatedById)
            );

            var updatedPage = updatedPageVersion.EinPage;
            Assert.Multiple(
                () => Assert.Equal("New page title", updatedPage.Title),
                () => Assert.Equal("new-page-title", updatedPage.Slug),
                () => Assert.Equal("New page description", updatedPage.Description),
                () => Assert.Equal(0, updatedPage.Order)
            );
        }
    }

    [Fact]
    public async Task UpdatePage_NotFound()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.UpdatePage(
                Guid.NewGuid(),
                new UpdateEducationInNumbersPageRequest
                {
                    Title = "New page title",
                    Description = "New page description",
                }
            );
            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdatePage_CannotUpdatePublishedPage_ThrowsException()
    {
        var originalPageVersionId = Guid.NewGuid();
        var originalPageVersion = new EinPageVersion
        {
            Id = originalPageVersionId,
            EinPage = new EinPage
            {
                Title = "Page title",
                Slug = "page-title",
                Description = "Page description",
                Order = 0,
                LatestVersionId = originalPageVersionId,
                LatestPublishedVersionId = originalPageVersionId,
            },
            Version = 0,
            Published = new DateTime(2006, 12, 25, 12, 00, 00),
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.Add(originalPageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdatePage(
                    originalPageVersion.Id,
                    new UpdateEducationInNumbersPageRequest
                    {
                        Title = "New page title",
                        Description = "New page description",
                    }
                )
            );
            Assert.Equal("Cannot update details of already published page", exception.Message);
        }
    }

    [Fact]
    public async Task UpdatePage_TitleNotUnique_ValidationError()
    {
        var originalPage = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            EinPage = new EinPage
            {
                Title = "Page title",
                Slug = "page-title",
                Description = "Page description",
                Order = 0,
            },
            Version = 0,
            Published = null,
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var otherPage = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            EinPage = new EinPage
            {
                Title = "Other page",
                Slug = "other-page",
                Description = "Other page description",
                Order = 0,
            },
            Version = 0,
            Published = null,
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };
        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.AddRange(originalPage, otherPage);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.UpdatePage(
                originalPage.Id,
                new UpdateEducationInNumbersPageRequest { Title = "Other page", Description = "New page description" }
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasGlobalError(ValidationErrorMessages.TitleNotUnique);
        }
    }

    [Fact]
    public async Task UpdatePage_SlugNotUnique_ValidationError()
    {
        var originalPage = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            EinPage = new EinPage
            {
                Title = "Page title",
                Slug = "page-title",
                Description = "Page description",
                Order = 0,
            },
            Version = 0,
            Published = null,
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var otherPage = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            EinPage = new EinPage
            {
                Title = "Other page",
                Slug = "other-page",
                Description = "Other page description",
                Order = 0,
            },
            Version = 0,
            Published = null,
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };
        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.AddRange(originalPage, otherPage);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.UpdatePage(
                originalPage.Id,
                new UpdateEducationInNumbersPageRequest
                {
                    Title = "Other page!", // exclamation is filtered from generated slug, so new slug is other-page
                    Description = "New page description",
                }
            );

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasGlobalError(ValidationErrorMessages.SlugNotUnique);
        }
    }

    [Fact]
    public async Task PublishPage_Success()
    {
        var pageId = Guid.NewGuid();
        var pageVersionId = Guid.NewGuid();
        var pageVersion = new EinPageVersion
        {
            Id = pageVersionId,
            EinPageId = pageId,
            EinPage = new EinPage
            {
                Id = pageId,
                Title = "Page title",
                Slug = "page-title",
                LatestVersionId = pageVersionId,
                LatestPublishedVersion = null,
            },
            Published = null,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.Add(pageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);
            var result = await service.PublishPage(pageVersion.Id);
            var publishedPage = result.AssertRight();

            Assert.Equal(pageVersion.Id, publishedPage.Id);
            publishedPage.Published.AssertUtcNow();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var savedPage = await contentDbContext.EinPageVersions.FindAsync(pageVersion.Id);
            Assert.NotNull(savedPage);
            savedPage.Published.AssertUtcNow();
            savedPage.Updated.AssertUtcNow();
            Assert.Equal(_userId, savedPage.UpdatedById);
        }
    }

    [Fact]
    public async Task PublishPage_NotFound()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        var service = CreateService(contentDbContext);

        var result = await service.PublishPage(Guid.NewGuid());
        result.AssertNotFound();
    }

    [Fact]
    public async Task PublishPage_AlreadyPublished_ThrowsException()
    {
        var pageId = Guid.NewGuid();
        var pageVersion = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            EinPageId = pageId,
            EinPage = new EinPage
            {
                Id = pageId,
                Title = "Page title",
                Slug = "page-title",
            },
            Published = DateTime.UtcNow,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.Add(pageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.PublishPage(pageVersion.Id));
            Assert.Equal("Cannot publish already published page version", exception.Message);
        }
    }

    [Fact]
    public async Task Reorder_Success()
    {
        var rootPageVersionId = Guid.NewGuid();
        var rootPageVersion = new EinPageVersion
        {
            Id = rootPageVersionId,
            EinPage = new EinPage
            {
                Slug = null,
                Order = 0,
                LatestVersionId = rootPageVersionId,
            },
        };
        var page1VersionId = Guid.NewGuid();
        var page1Version = new EinPageVersion
        {
            Id = page1VersionId,
            EinPage = new EinPage
            {
                Slug = "page-1",
                Order = 1,
                LatestVersionId = page1VersionId,
            },
        };
        var page2VersionId = Guid.NewGuid();
        var page2Version = new EinPageVersion
        {
            Id = page2VersionId,
            EinPage = new EinPage
            {
                Slug = "page-2",
                Order = 2,
                LatestVersionId = page2VersionId,
            },
        };
        var page3VersionId = Guid.NewGuid();
        var page3Version = new EinPageVersion
        {
            Id = page3VersionId,
            EinPage = new EinPage
            {
                Slug = "page-3",
                Order = 3,
                LatestVersionId = page3VersionId,
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.AddRange(rootPageVersion, page1Version, page2Version, page3Version);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);
            var newOrder = new List<Guid> { rootPageVersion.Id, page3Version.Id, page1Version.Id, page2Version.Id };
            var result = await service.Reorder(newOrder);
            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbRootPageVersion = await contentDbContext
                .EinPageVersions.Include(pageVersion => pageVersion.EinPage)
                .SingleAsync(pageVersion => pageVersion.Id == rootPageVersion.Id);
            Assert.NotNull(dbRootPageVersion);
            Assert.Equal(0, dbRootPageVersion.EinPage.Order);

            var savedPage1Version = await contentDbContext
                .EinPageVersions.Include(pageVersion => pageVersion.EinPage)
                .SingleAsync(pageVersion => pageVersion.Id == page1Version.Id);
            Assert.NotNull(savedPage1Version);
            Assert.Equal(2, savedPage1Version.EinPage.Order);

            var savedPage2 = await contentDbContext
                .EinPageVersions.Include(pageVersion => pageVersion.EinPage)
                .SingleAsync(pageVersion => pageVersion.Id == page2Version.Id);
            Assert.NotNull(savedPage2);
            Assert.Equal(3, savedPage2.EinPage.Order);

            var savedPage3 = await contentDbContext
                .EinPageVersions.Include(pageVersion => pageVersion.EinPage)
                .SingleAsync(pageVersion => pageVersion.Id == page3Version.Id);
            Assert.NotNull(savedPage3);
            Assert.Equal(1, savedPage3.EinPage.Order);
        }
    }

    [Fact]
    public async Task Reorder_PageIdsDiffer_ValidationError()
    {
        var rootPageVersionId = Guid.NewGuid();
        var rootPageVersion = new EinPageVersion
        {
            Id = rootPageVersionId,
            EinPage = new EinPage
            {
                Slug = null,
                Order = 0,
                LatestVersionId = rootPageVersionId,
            },
        };
        var page1VersionId = Guid.NewGuid();
        var page1Version = new EinPageVersion
        {
            Id = page1VersionId,
            EinPage = new EinPage
            {
                Slug = "page-1",
                Order = 1,
                LatestVersionId = page1VersionId,
            },
        };
        var page2VersionId = Guid.NewGuid();
        var page2Version = new EinPageVersion
        {
            Id = page2VersionId,
            EinPage = new EinPage
            {
                Slug = "page-2",
                Order = 2,
                LatestVersionId = page2VersionId,
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.AddRange(rootPageVersion, page1Version, page2Version);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);
            var newOrder = new List<Guid> { rootPageVersion.Id, page1Version.Id, Guid.NewGuid() };
            var result = await service.Reorder(newOrder);

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            validationProblem.AssertHasGlobalError(ValidationErrorMessages.EinProvidedPageIdsDifferFromActualPageIds);
        }
    }

    [Fact]
    public async Task Delete_UnpublishedAmendment_Success()
    {
        var rootPageVersion = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            EinPage = new EinPage { Slug = null, Order = 0 },
        };
        var pageId = Guid.NewGuid();
        var pageV0Id = Guid.NewGuid();
        var pageV1Id = Guid.NewGuid();
        var page = new EinPage
        {
            Id = pageId,
            Slug = "page-1",
            Order = 1,
            LatestVersionId = pageV1Id,
            LatestPublishedVersionId = pageV0Id,
        };
        var pageV0 = new EinPageVersion
        {
            Id = pageV0Id,
            EinPageId = pageId,
            EinPage = page,
            Version = 0,
            Published = DateTime.UtcNow,
        };
        var pageV1 = new EinPageVersion
        {
            Id = pageV1Id,
            EinPageId = pageId,
            EinPage = page,
            Version = 1,
            Published = null,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPages.AddRange(page);
            contentDbContext.EinPageVersions.AddRange(rootPageVersion, pageV0, pageV1);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);
            var result = await service.Delete(pageV1.Id);
            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbPageVersions = contentDbContext.EinPageVersions.ToList();

            Assert.Equal(2, dbPageVersions.Count);
            Assert.NotNull(dbPageVersions.Find(pageVersion => pageVersion.Id == rootPageVersion.Id));
            Assert.NotNull(dbPageVersions.Find(pageVersion => pageVersion.Id == pageV0.Id));

            var dbPage = contentDbContext.EinPages.Single(p => p.Id == pageId);
            Assert.Equal(pageV0Id, dbPage.LatestVersionId);
            Assert.Equal(pageV0Id, dbPage.LatestPublishedVersionId);

            // NOTE: Associated content is cascade deleted by the DB - cannot test that with the InMemory db
        }
    }

    [Fact]
    public async Task Delete_UnpublishedOriginalPage_Success()
    {
        var pageVersionId = Guid.NewGuid();
        var page = new EinPage
        {
            Id = Guid.NewGuid(),
            Slug = "page-1",
            LatestVersionId = pageVersionId,
        };
        var pageVersion = new EinPageVersion
        {
            Id = pageVersionId,
            EinPage = page,
            EinPageId = page.Id,
            Version = 0,
            Published = null,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.Add(pageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);
            var result = await service.Delete(pageVersion.Id);
            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            Assert.Empty(contentDbContext.EinPageVersions);

            Assert.Empty(contentDbContext.EinPages); // since we've deleted the only page version, the page is also removed

            // NOTE: Associated content is cascade deleted by the DB - cannot test that with the InMemory db
        }
    }

    [Fact]
    public async Task Delete_NotFound()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        var service = CreateService(contentDbContext);

        var result = await service.Delete(Guid.NewGuid());
        result.AssertNotFound();
    }

    [Fact]
    public async Task Delete_PublishedPage_ThrowsException()
    {
        var pageVersion = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            EinPage = new EinPage { Slug = "page-1" },
            Published = DateTime.UtcNow,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.Add(pageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.Delete(pageVersion.Id));
            Assert.Equal("Can only delete unpublished page versions", exception.Message);
        }
    }

    [Fact]
    public async Task FullDelete_Success()
    {
        var rootPage = new EinPage // shouldn't be removed
        {
            Id = Guid.NewGuid(),
            Slug = null,
            Order = 0,
        };
        var page = new EinPage
        {
            Id = Guid.NewGuid(),
            Slug = "page-1",
            Order = 1,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPages.AddRange(rootPage, page);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext, enableEinPublishedPageDeletion: true);

            var result = await service.FullDelete(page.Slug);
            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var pages = contentDbContext.EinPages.ToList();
            var dbRootPage = Assert.Single(pages);
            Assert.Equal(rootPage.Id, dbRootPage.Id);

            // NOTE: Associated page versions, content is cascade deleted by the DB - cannot test that with the InMemory db
        }
    }

    [Fact]
    public async Task FullDelete_DisabledByEnvVar_ThrowsException()
    {
        var pageVersion = new EinPageVersion
        {
            Id = Guid.NewGuid(),
            EinPage = new EinPage { Slug = "page-1" },
            Published = DateTime.UtcNow,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.Add(pageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext, enableEinPublishedPageDeletion: false);

            var exception = await Assert.ThrowsAsync<Exception>(() => service.FullDelete(pageVersion.EinPage.Slug));
            Assert.Equal("Full delete not enabled", exception.Message);
        }
    }

    private EducationInNumbersService CreateService(
        ContentDbContext contentDbContext,
        IUserService? userService = null,
        bool enableEinPublishedPageDeletion = false
    )
    {
        var appOptions = Microsoft.Extensions.Options.Options.Create(
            new AppOptions { EnableEinPublishedPageDeletion = enableEinPublishedPageDeletion }
        );

        return new EducationInNumbersService(
            appOptions,
            contentDbContext,
            userService ?? AlwaysTrueUserService(_userId).Object
        );
    }
}
