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

        var latestPageVersion = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Test page",
            Slug = "test-page",
            Description = "Test page description",
            Version = 1,
            Order = 0,
            Published = new DateTime(2005, 12, 25, 12, 00, 00),
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = createdByUserId,
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = updatedByUserId,
        };

        var previousPageVersion = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Slug = "test-page",
            Version = 0,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.AddRange(latestPageVersion, previousPageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.GetPage(latestPageVersion.Id);
            var fetchedPage = result.AssertRight();

            Assert.Multiple(
                () => Assert.Equal(latestPageVersion.Id, fetchedPage.Id),
                () => Assert.Equal(latestPageVersion.Title, fetchedPage.Title),
                () => Assert.Equal(latestPageVersion.Slug, fetchedPage.Slug),
                () => Assert.Equal(latestPageVersion.Description, fetchedPage.Description),
                () => Assert.Equal(latestPageVersion.Version, fetchedPage.Version),
                () => Assert.Equal(latestPageVersion.Published, fetchedPage.Published),
                () => Assert.Equal(latestPageVersion.Order, fetchedPage.Order)
            );
        }
    }

    [Fact]
    public async Task GetPage_NotFound()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        var service = CreateService(contentDbContext);

        var result = await service.GetPage(Guid.NewGuid());
        result.AssertNotFound();
    }

    [Fact]
    public async Task ListLatestPages_Success()
    {
        var createdByUserId = Guid.NewGuid();
        var updatedByUserId = Guid.NewGuid();

        var educationInNumbersPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Education in numbers",
            Slug = null,
            Description = "Education in numbers description",
            Version = 0,
            Order = 3,
            Published = new DateTime(2002, 12, 25, 12, 00, 00),
            Created = new DateTime(2002, 12, 23, 12, 00, 00),
            CreatedById = createdByUserId,
            Updated = new DateTime(2002, 12, 24, 12, 00, 00),
            UpdatedById = updatedByUserId,
        };

        var page1Latest = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page 1",
            Slug = "page-1",
            Description = "Test 1 description",
            Version = 1,
            Order = 1,
            Published = new DateTime(2005, 12, 25, 12, 00, 00),
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = createdByUserId,
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = updatedByUserId,
        };

        var page1Previous = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Slug = "page-1",
            Version = 0,
            Order = 1,
        };

        var page2 = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page 2",
            Slug = "page-2",
            Description = "Test 2 description",
            Version = 0,
            Order = 0,
            Published = null,
            Created = new DateTime(2004, 12, 23, 12, 00, 00),
            CreatedById = createdByUserId,
            Updated = new DateTime(2004, 12, 24, 12, 00, 00),
            UpdatedById = updatedByUserId,
        };

        var page3 = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page 3",
            Slug = "page-3",
            Description = "Test 3 description",
            Version = 0,
            Order = 2,
            Published = new DateTime(2003, 12, 25, 12, 00, 00),
            Created = new DateTime(2003, 12, 23, 12, 00, 00),
            CreatedById = createdByUserId,
            Updated = new DateTime(2003, 12, 24, 12, 00, 00),
            UpdatedById = updatedByUserId,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.AddRange(
                educationInNumbersPage, page1Latest, page1Previous, page2, page3);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.ListLatestPages();
            var pageSummaryList = result.AssertRight();

            // returned in order
            Assert.Multiple(
                () => Assert.Equal(page2.Id, pageSummaryList[0].Id),
                () => Assert.Equal(page2.Title, pageSummaryList[0].Title),
                () => Assert.Equal(page2.Slug, pageSummaryList[0].Slug),
                () => Assert.Equal(page2.Description, pageSummaryList[0].Description),
                () => Assert.Equal(page2.Version, pageSummaryList[0].Version),
                () => Assert.Equal(page2.Published, pageSummaryList[0].Published),
                () => Assert.Equal(page2.Order, pageSummaryList[0].Order),
                () => Assert.Null(pageSummaryList[0].PreviousVersionId)
            );

            Assert.Multiple(
                () => Assert.Equal(page1Latest.Id, pageSummaryList[1].Id),
                () => Assert.Equal(page1Latest.Title, pageSummaryList[1].Title),
                () => Assert.Equal(page1Latest.Slug, pageSummaryList[1].Slug),
                () => Assert.Equal(page1Latest.Description, pageSummaryList[1].Description),
                () => Assert.Equal(page1Latest.Version, pageSummaryList[1].Version),
                () => Assert.Equal(page1Latest.Published, pageSummaryList[1].Published),
                () => Assert.Equal(page1Latest.Order, pageSummaryList[1].Order),
                () => Assert.Equal(page1Previous.Id, pageSummaryList[1].PreviousVersionId)
            );

            Assert.Multiple(
                () => Assert.Equal(page3.Id, pageSummaryList[2].Id),
                () => Assert.Equal(page3.Title, pageSummaryList[2].Title),
                () => Assert.Equal(page3.Slug, pageSummaryList[2].Slug),
                () => Assert.Equal(page3.Description, pageSummaryList[2].Description),
                () => Assert.Equal(page3.Version, pageSummaryList[2].Version),
                () => Assert.Equal(page3.Published, pageSummaryList[2].Published),
                () => Assert.Equal(page3.Order, pageSummaryList[2].Order),
                () => Assert.Null(pageSummaryList[2].PreviousVersionId)
            );

            Assert.Multiple(
                () => Assert.Equal(educationInNumbersPage.Id, pageSummaryList[3].Id),
                () => Assert.Equal(educationInNumbersPage.Title, pageSummaryList[3].Title),
                () => Assert.Equal(educationInNumbersPage.Slug, pageSummaryList[3].Slug),
                () => Assert.Equal(educationInNumbersPage.Description, pageSummaryList[3].Description),
                () => Assert.Equal(educationInNumbersPage.Version, pageSummaryList[3].Version),
                () => Assert.Equal(educationInNumbersPage.Published, pageSummaryList[3].Published),
                () => Assert.Equal(educationInNumbersPage.Order, pageSummaryList[3].Order),
                () => Assert.Null(pageSummaryList[3].PreviousVersionId)
            );
        }
    }

    [Fact]
    public async Task CreatePage_Success()
    {
        var testPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Test page",
            Slug = "test-page",
            Description = "Test page description",
            Version = 0,
            Order = 0,
            Published = new DateTime(2005, 12, 25, 12, 00, 00),
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.Add(testPage);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.CreatePage(
                new CreateEducationInNumbersPageRequest
                {
                    Title = "New page",
                    Description = "New page description",
                });
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
            var pages = contentDbContext.EducationInNumbersPages
                .OrderBy(p => p.Order)
                .ToList();

            Assert.Equal(2, pages.Count);

            Assert.Equal(testPage.Id, pages[0].Id);

            Assert.Multiple(
                () => Assert.Equal("New page", pages[1].Title),
                () => Assert.Equal("new-page", pages[1].Slug),
                () => Assert.Equal("New page description", pages[1].Description),
                () => Assert.Equal(0, pages[1].Version),
                () => Assert.Equal(1, pages[1].Order),
                () => Assert.Null(pages[1].Published),
                () => pages[1].Created.AssertUtcNow(),
                () => Assert.Equal(_userId, pages[1].CreatedById),
                () => Assert.Null(pages[1].Updated),
                () => Assert.Null(pages[1].UpdatedById)
            );
        }
    }

    [Fact]
    public async Task CreatePage_TitleNotUnique_ValidationError()
    {
        var testPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Test page",
            Slug = "test-page",
            Description = "Test page description",
            Version = 0,
            Order = 0,
            Published = new DateTime(2005, 12, 25, 12, 00, 00),
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.Add(testPage);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.CreatePage(
                new CreateEducationInNumbersPageRequest
                {
                    Title = "Test page",
                    Description = "Test page description",
                });

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasGlobalError(ValidationErrorMessages.TitleNotUnique);
        }
    }

    [Fact]
    public async Task CreatePage_SlugNotUnique_ValidationError()
    {
        var testPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Test page",
            Slug = "test-page",
            Description = "Test page description",
            Version = 0,
            Order = 0,
            Published = new DateTime(2005, 12, 25, 12, 00, 00),
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.Add(testPage);
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
                });

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasGlobalError(ValidationErrorMessages.SlugNotUnique);
        }
    }

    [Fact]
    public async Task CreateAmendment_Success()
    {
        var originalPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page title",
            Slug = "page-title",
            Description = "Page description",
            Version = 0,
            Order = 0,
            Published = new DateTime(2005, 12, 25, 12, 00, 00),
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.Add(originalPage);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.CreateAmendment(originalPage.Id);
            var amendment = result.AssertRight();

            Assert.Multiple(
                () => Assert.Equal("Page title", amendment.Title),
                () => Assert.Equal("page-title", amendment.Slug),
                () => Assert.Equal("Page description", amendment.Description),
                () => Assert.Equal(originalPage.Version + 1, amendment.Version),
                () => Assert.Null(amendment.Published),
                () => Assert.Equal(0, amendment.Order)
            );
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var pages = contentDbContext.EducationInNumbersPages
                .OrderBy(p => p.Version)
                .ToList();

            Assert.Equal(2, pages.Count);

            Assert.Equal(originalPage.Id, pages[0].Id);

            Assert.Multiple(
                () => Assert.Equal("Page title", pages[1].Title),
                () => Assert.Equal("page-title", pages[1].Slug),
                () => Assert.Equal("Page description", pages[1].Description),
                () => Assert.Equal(1, pages[1].Version),
                () => Assert.Equal(0, pages[1].Order),
                () => Assert.Null(pages[1].Published),
                () => pages[1].Created.AssertUtcNow(),
                () => Assert.Equal(_userId, pages[1].CreatedById),
                () => Assert.Null(pages[1].Updated),
                () => Assert.Null(pages[1].UpdatedById)
            );
        }
    }

    [Fact]
    public async Task CreateAmendment_ClonesContent_Success()
    {
        var originalPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page title",
            Slug = "page-title",
            Description = "Page description",
            Version = 0,
            Order = 0,
            Published = DateTime.UtcNow,
            Content =
            [
                new EinContentSection
                {
                    Id = Guid.NewGuid(),
                    Order = 0,
                    Heading = "Section 1",
                    Caption = "Section 1 caption",
                    Content =
                    [
                        new EinHtmlBlock
                        {
                            Id = Guid.NewGuid(),
                            Order = 0,
                            Body = "Block 1.1 body"
                        },

                        new EinHtmlBlock
                        {
                            Id = Guid.NewGuid(),
                            Order = 1,
                            Body = "Block 1.2 body"
                        }
                    ]
                },

                new EinContentSection
                {
                    Id = Guid.NewGuid(),
                    Order = 1,
                    Heading = "Section 2",
                    Caption = "Section 2 caption",
                    Content =
                    [
                        new EinHtmlBlock
                        {
                            Id = Guid.NewGuid(),
                            Order = 0,
                            Body = "Block 2.1 body"
                        }
                    ]
                }
            ]
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.Add(originalPage);
            await contentDbContext.SaveChangesAsync();
        }

        Guid amendmentId;
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.CreateAmendment(originalPage.Id);
            var amendment = result.AssertRight();
            amendmentId = amendment.Id;
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var amendment = await contentDbContext.EducationInNumbersPages
                .Include(p => p.Content)
                .ThenInclude(s => s.Content)
                .SingleAsync(p => p.Id == amendmentId);

            Assert.NotEqual(originalPage.Id, amendment.Id);
            Assert.Equal(originalPage.Content.Count, amendment.Content.Count);

            var originalSection1 = originalPage.Content[0];
            var amendmentSection1 = amendment.Content[0];
            Assert.NotEqual(originalSection1.Id, amendmentSection1.Id);
            Assert.Equal(amendment.Id, amendmentSection1.EducationInNumbersPageId);
            Assert.Equal(originalSection1.Order, amendmentSection1.Order);
            Assert.Equal(originalSection1.Heading, amendmentSection1.Heading);
            Assert.Equal(originalSection1.Caption, amendmentSection1.Caption);
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

            var originalSection2 = originalPage.Content[1];
            var amendmentSection2 = amendment.Content[1];
            Assert.NotEqual(originalSection2.Id, amendmentSection2.Id);
            Assert.Equal(amendment.Id, amendmentSection2.EducationInNumbersPageId);
            Assert.Equal(originalSection2.Order, amendmentSection2.Order);
            Assert.Equal(originalSection2.Heading, amendmentSection2.Heading);
            Assert.Equal(originalSection2.Caption, amendmentSection2.Caption);
            Assert.Equal(originalSection2.Content.Count, amendmentSection2.Content.Count);

            var amendmentSection2Block1 = Assert.IsType<EinHtmlBlock>(amendmentSection2.Content[0]);
            var originalSection2Block1 = (EinHtmlBlock) originalSection2.Content[0];
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
        var originalPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page title",
            Slug = "page-title",
            Description = "Page description",
            Version = 0,
            Order = 0,
            Published = null,
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.Add(originalPage);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAmendment(originalPage.Id));
            Assert.Equal("Can only create amendment of a published page", exception.Message);
        }
    }

    [Fact]
    public async Task CreateAmendment_AmendmentAlreadyExists_ThrowsException()
    {
        var originalPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page title",
            Slug = "page-title",
            Description = "Page description",
            Version = 0,
            Order = 0,
            Published = new DateTime(2005, 12, 25, 12, 00, 00),
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var amendment = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page title",
            Slug = "page-title",
            Description = "Page description",
            Version = 1,
            Order = 0,
            Published = null,
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };
        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.AddRange(originalPage, amendment);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAmendment(originalPage.Id));
            Assert.Equal($"Amendment already exists for page {originalPage.Id}", exception.Message);
        }
    }

    [Fact]
    public async Task UpdatePage_Success()
    {
        var originalPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page title",
            Slug = "page-title",
            Description = "Page description",
            Version = 0,
            Order = 0,
            Published = null,
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.AddRange(originalPage);
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
                });
            var updatedPage = result.AssertRight();

            Assert.Multiple(
                () => Assert.Equal("New page title", updatedPage.Title),
                () => Assert.Equal("new-page-title", updatedPage.Slug),
                () => Assert.Equal("New page description", updatedPage.Description),
                () => Assert.Equal(originalPage.Version, updatedPage.Version), // we're updating this version of the page, not creating a new page version
                () => Assert.Null(updatedPage.Published),
                () => Assert.Equal(0, updatedPage.Order)
            );
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var pages = contentDbContext.EducationInNumbersPages
                .OrderBy(p => p.Version)
                .ToList();

            var updatedPage = Assert.Single(pages);

            Assert.Multiple(
                () => Assert.Equal(originalPage.Id, updatedPage.Id),
                () => Assert.Equal("New page title", updatedPage.Title),
                () => Assert.Equal("new-page-title", updatedPage.Slug),
                () => Assert.Equal("New page description", updatedPage.Description),
                () => Assert.Equal(0, updatedPage.Version),
                () => Assert.Equal(0, updatedPage.Order),
                () => Assert.Null(updatedPage.Published),
                () => Assert.Equal(originalPage.Created, updatedPage.Created),
                () => Assert.Equal(originalPage.CreatedById, updatedPage.CreatedById),
                () => updatedPage.Updated.AssertUtcNow(),
                () => Assert.Equal(_userId, updatedPage.UpdatedById)
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
                });
            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdatePage_CannotUpdatePublishedPage_ThrowsException()
    {
        var originalPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page title",
            Slug = "page-title",
            Description = "Page description",
            Version = 0,
            Order = 0,
            Published = new DateTime(2006, 12, 25, 12, 00, 00),
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.Add(originalPage);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdatePage(
                originalPage.Id,
                new UpdateEducationInNumbersPageRequest
                {
                    Title = "New page title",
                    Description = "New page description",
                }));
            Assert.Equal("Cannot update details of already published page", exception.Message);
        }
    }

    [Fact]
    public async Task UpdatePage_CannotUpdateAmendment_ThrowsException()
    {
        var latestPageVersion = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page title",
            Slug = "page-title",
            Description = "Page description",
            Version = 1,
            Order = 0,
            Published = null,
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var previousPageVersion = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page title",
            Slug = "page-title",
            Description = "Page description",
            Version = 0,
            Order = 0,
            Published = new DateTime(2005, 12, 25, 12, 00, 00),
            Created = new DateTime(2005, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2005, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.AddRange(latestPageVersion, previousPageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var exception = await Assert.ThrowsAsync<Exception>(() => service.UpdatePage(
                latestPageVersion.Id,
                new UpdateEducationInNumbersPageRequest
                {
                    Title = "Something something",
                    Description = "Random description"
                }));
            Assert.Equal("Cannot update details for a page amendment", exception.Message);
        }
    }

    [Fact]
    public async Task UpdatePage_TitleNotUnique_ValidationError()
    {
        var originalPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page title",
            Slug = "page-title",
            Description = "Page description",
            Version = 0,
            Order = 0,
            Published = null,
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var otherPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Other page",
            Slug = "other-page",
            Description = "Other page description",
            Version = 0,
            Order = 0,
            Published = null,
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };
        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.AddRange(originalPage, otherPage);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);

            var result = await service.UpdatePage(
                originalPage.Id,
                new UpdateEducationInNumbersPageRequest
                {
                    Title = "Other page",
                    Description = "New page description",
                });

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasGlobalError(ValidationErrorMessages.TitleNotUnique);
        }
    }

    [Fact]
    public async Task UpdatePage_SlugNotUnique_ValidationError()
    {
        var originalPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page title",
            Slug = "page-title",
            Description = "Page description",
            Version = 0,
            Order = 0,
            Published = null,
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };

        var otherPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Other page",
            Slug = "other-page",
            Description = "Other page description",
            Version = 0,
            Order = 0,
            Published = null,
            Created = new DateTime(2006, 12, 23, 12, 00, 00),
            CreatedById = Guid.NewGuid(),
            Updated = new DateTime(2006, 12, 24, 12, 00, 00),
            UpdatedById = Guid.NewGuid(),
        };
        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.AddRange(originalPage, otherPage);
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
                });

            var validationProblem = result.AssertBadRequestWithValidationProblem();

            validationProblem.AssertHasGlobalError(ValidationErrorMessages.SlugNotUnique);
        }
    }

    [Fact]
    public async Task PublishPage_Success()
    {
        var page = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page title",
            Slug = "page-title",
            Published = null,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.Add(page);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);
            var result = await service.PublishPage(page.Id);
            var publishedPage = result.AssertRight();

            Assert.Equal(page.Id, publishedPage.Id);
            publishedPage.Published.AssertUtcNow();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var savedPage = await contentDbContext.EducationInNumbersPages.FindAsync(page.Id);
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
        var page = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = "Page title",
            Slug = "page-title",
            Published = DateTime.UtcNow
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.Add(page);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.PublishPage(page.Id));
            Assert.Equal("Cannot publish already published page", exception.Message);
        }
    }

    [Fact]
    public async Task Reorder_Success()
    {
        var page1 = new EducationInNumbersPage { Id = Guid.NewGuid(), Slug = "page-1", Order = 0 };
        var page2 = new EducationInNumbersPage { Id = Guid.NewGuid(), Slug = "page-2", Order = 1 };
        var page3 = new EducationInNumbersPage { Id = Guid.NewGuid(), Slug = "page-3", Order = 2 };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.AddRange(page1, page2, page3);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);
            var newOrder = new List<Guid> { page3.Id, page1.Id, page2.Id };
            var result = await service.Reorder(newOrder);
            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var savedPage1 = await contentDbContext.EducationInNumbersPages.FindAsync(page1.Id);
            Assert.NotNull(savedPage1);
            Assert.Equal(1, savedPage1.Order);
            savedPage1.Updated.AssertUtcNow();
            Assert.Equal(_userId, savedPage1.UpdatedById);

            var savedPage2 = await contentDbContext.EducationInNumbersPages.FindAsync(page2.Id);
            Assert.NotNull(savedPage2);
            Assert.Equal(2, savedPage2.Order);
            savedPage2.Updated.AssertUtcNow();
            Assert.Equal(_userId, savedPage2.UpdatedById);

            var savedPage3 = await contentDbContext.EducationInNumbersPages.FindAsync(page3.Id);
            Assert.NotNull(savedPage3);
            Assert.Equal(0, savedPage3.Order);
            savedPage3.Updated.AssertUtcNow();
            Assert.Equal(_userId, savedPage3.UpdatedById);
        }
    }

    [Fact]
    public async Task Reorder_WithAmendment_Success()
    {
        var page1V0 = new EducationInNumbersPage { Id = Guid.NewGuid(), Slug = "page-1", Version = 0, Order = 0, Published = DateTime.UtcNow };
        var page1V1 = new EducationInNumbersPage { Id = Guid.NewGuid(), Slug = "page-1", Version = 1, Order = 0, Published = null };
        var page2 = new EducationInNumbersPage { Id = Guid.NewGuid(), Slug = "page-2", Version = 0, Order = 1 };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.AddRange(page1V0, page1V1, page2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);
            var newOrder = new List<Guid> { page2.Id, page1V1.Id };
            var result = await service.Reorder(newOrder);
            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            // if there is an amendment, we update the original and the amendment, as the amendment could be cancelled
            var savedPage1V0 = await contentDbContext.EducationInNumbersPages.FindAsync(page1V0.Id);
            Assert.NotNull(savedPage1V0);
            Assert.Equal(1, savedPage1V0.Order);
            savedPage1V0.Updated.AssertUtcNow();
            Assert.Equal(_userId, savedPage1V0.UpdatedById);

            var savedPage1V1 = await contentDbContext.EducationInNumbersPages.FindAsync(page1V1.Id);
            Assert.NotNull(savedPage1V1);
            Assert.Equal(1, savedPage1V1.Order);
            savedPage1V1.Updated.AssertUtcNow();
            Assert.Equal(_userId, savedPage1V1.UpdatedById);

            var savedPage2 = await contentDbContext.EducationInNumbersPages.FindAsync(page2.Id);
            Assert.NotNull(savedPage2);
            Assert.Equal(0, savedPage2.Order);
            savedPage2.Updated.AssertUtcNow();
            Assert.Equal(_userId, savedPage2.UpdatedById);
        }
    }

    [Fact]
    public async Task Reorder_PageIdsDiffer_ValidationError()
    {
        var page1 = new EducationInNumbersPage { Id = Guid.NewGuid(), Slug = "page-1", Order = 0 };
        var page2 = new EducationInNumbersPage { Id = Guid.NewGuid(), Slug = "page-2", Order = 1 };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.AddRange(page1, page2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);
            var newOrder = new List<Guid> { page1.Id, Guid.NewGuid() };
            var result = await service.Reorder(newOrder);

            var validationProblem = result.AssertBadRequestWithValidationProblem();
            validationProblem.AssertHasGlobalError(ValidationErrorMessages.EinProvidedPageIdsDifferFromActualPageIds);
        }
    }

    [Fact]
    public async Task Delete_UnpublishedAmendment_Success()
    {
        var pageV0 = new EducationInNumbersPage { Id = Guid.NewGuid(), Slug = "page-1", Version = 0, Published = DateTime.UtcNow };
        var pageV1 = new EducationInNumbersPage { Id = Guid.NewGuid(), Slug = "page-1", Version = 1, Published = null };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.AddRange(pageV0, pageV1);
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
            var pages = contentDbContext.EducationInNumbersPages.ToList();
            var savedPage = Assert.Single(pages);
            Assert.Equal(pageV0.Id, savedPage.Id);

            // NOTE: Associated content is cascade deleted by the DB - cannot test that with the InMemory db
        }
    }

    [Fact]
    public async Task Delete_UnpublishedOriginalPage_Success()
    {
        var page = new EducationInNumbersPage { Id = Guid.NewGuid(), Slug = "page-1", Version = 0, Published = null };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.Add(page);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);
            var result = await service.Delete(page.Id);
            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            Assert.Empty(contentDbContext.EducationInNumbersPages);

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
        var page = new EducationInNumbersPage { Id = Guid.NewGuid(), Slug = "page-1", Published = DateTime.UtcNow };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.EducationInNumbersPages.Add(page);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = CreateService(contentDbContext);
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.Delete(page.Id));
            Assert.Equal("Cannot delete published page", exception.Message);
        }
    }

    private EducationInNumbersService CreateService(
        ContentDbContext contentDbContext,
        IUserService? userService = null)
    {
        return new EducationInNumbersService(
            contentDbContext,
            userService ?? AlwaysTrueUserService(_userId).Object);
    }
}
