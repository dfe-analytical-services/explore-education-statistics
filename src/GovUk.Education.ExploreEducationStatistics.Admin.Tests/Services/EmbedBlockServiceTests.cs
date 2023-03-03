#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class EmbedBlockServiceTests
{
    private readonly Release _release = new()
    {
        Id = Guid.NewGuid(),
    };

    [Fact]
    public async Task Create()
    {
        var contentSectionId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(new ReleaseContentSection
            {
                Release = _release,
                ContentSection = new ContentSection
                {
                    Id = contentSectionId,
                }
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Create(_release.Id,
                new EmbedBlockCreateRequest
                {
                    Title = "Test title",
                    Url = "https://department-for-education.shinyapps.io/test-page",
                    ContentSectionId = contentSectionId,
                });

            var viewModel = result.AssertRight();

            Assert.Equal(1, viewModel.Order);
            Assert.Equal("Test title", viewModel.Title);
            Assert.Equal("https://department-for-education.shinyapps.io/test-page", viewModel.Url);
            Assert.Empty(viewModel.Comments);
            Assert.Null(viewModel.Locked);
            Assert.Null(viewModel.LockedUntil);
            Assert.Null(viewModel.LockedBy);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var embedBlockLinks = context.EmbedBlockLinks.ToList();
            var embedBlockLink = Assert.Single(embedBlockLinks);
            Assert.Equal(1, embedBlockLink.Order);
            Assert.Equal(contentSectionId, embedBlockLink.ContentSectionId);

            var embedBlocks = context.EmbedBlocks.ToList();
            var embedBlock = Assert.Single(embedBlocks);
            Assert.Equal(embedBlockLink.EmbedBlockId, embedBlock.Id);
            Assert.Equal("Test title", embedBlock.Title);
            Assert.Equal("https://department-for-education.shinyapps.io/test-page", embedBlock.Url);
        }
    }

    [Fact]
    public async Task Create_InvalidDomain()
    {
        var contentSectionId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(new ReleaseContentSection
            {
                Release = _release,
                ContentSection = new ContentSection
                {
                    Id = contentSectionId,
                }
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Create(_release.Id,
                new EmbedBlockCreateRequest
                {
                    Title = "Test title",
                    Url = "http://www.invalid.com/test-page",
                    ContentSectionId = contentSectionId,
                });

            result.AssertBadRequest(EmbedBlockUrlDomainNotPermitted);
            Assert.Empty(context.EmbedBlockLinks);
        }
    }

    [Fact]
    public async Task Create_CorrectOrder()
    {
        var contentSectionId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(new ReleaseContentSection
            {
                Release = _release,
                ContentSection = new ContentSection
                {
                    Id = contentSectionId,
                    Content = new()
                    {
                        new HtmlBlock { Order = 1 },
                        new HtmlBlock { Order = 2 },
                        new HtmlBlock { Order = 3 },
                    }
                }
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Create(_release.Id,
                new EmbedBlockCreateRequest
                {
                    Title = "Test title",
                    Url = "https://department-for-education.shinyapps.io/test-page",
                    ContentSectionId = contentSectionId,
                });

            var viewModel = result.AssertRight();

            Assert.Equal(4, viewModel.Order);
            Assert.Equal("Test title", viewModel.Title);
            Assert.Equal("https://department-for-education.shinyapps.io/test-page", viewModel.Url);
            Assert.Empty(viewModel.Comments);
            Assert.Null(viewModel.Locked);
            Assert.Null(viewModel.LockedUntil);
            Assert.Null(viewModel.LockedBy);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var htmlBlocks = context.HtmlBlocks.ToList();
            Assert.Equal(3, htmlBlocks.Count);

            var embedBlockLinks = context.EmbedBlockLinks.ToList();
            var embedBlockLink = Assert.Single(embedBlockLinks);
            Assert.Equal(4, embedBlockLink.Order);
            Assert.Equal(contentSectionId, embedBlockLink.ContentSectionId);

            var embedBlocks = context.EmbedBlocks.ToList();
            var embedBlock = Assert.Single(embedBlocks);
            Assert.Equal(embedBlockLink.EmbedBlockId, embedBlock.Id);
            Assert.Equal("Test title", embedBlock.Title);
            Assert.Equal("https://department-for-education.shinyapps.io/test-page", embedBlock.Url);

            Assert.Equal(embedBlock.Id, embedBlockLink.EmbedBlockId);
        }
    }

    [Fact]
    public async Task Create_ReleaseDoesNotExist()
    {
        var contentSectionId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(new ReleaseContentSection
            {
                Release = _release,
                ContentSection = new ContentSection
                {
                    Id = contentSectionId,
                }
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Create(Guid.NewGuid(),
                new EmbedBlockCreateRequest
                {
                    Title = "Test title",
                    Url = "https://department-for-education.shinyapps.io/test-page",
                    ContentSectionId = contentSectionId,
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Create_ContentSectionDoesNotExist()
    {
        var contentSectionId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(new ReleaseContentSection
            {
                Release = _release,
                ContentSection = new ContentSection
                {
                    Id = contentSectionId,
                }
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Create(_release.Id,
                new EmbedBlockCreateRequest
                {
                    Title = "Test title",
                    Url = "https://department-for-education.shinyapps.io/test-page",
                    ContentSectionId = Guid.NewGuid(),
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Create_ContentSectionAttachedToDifferentRelease()
    {
        var contentSectionId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(
                new ReleaseContentSection
                {
                    Release = new Release(),
                    ContentSection = new ContentSection
                    {
                        Id = contentSectionId,
                    }
                },
                new ReleaseContentSection
                {
                    Release = _release,
                    ContentSection = new ContentSection(),
                });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Create(_release.Id,
                new EmbedBlockCreateRequest
                {
                    Title = "Test title",
                    Url = "https://department-for-education.shinyapps.io/test-page",
                    ContentSectionId = contentSectionId,
                });

            result.AssertBadRequest(ContentSectionNotAttachedToRelease);
        }
    }

    [Fact]
    public async Task Update()
    {
        var contentSectionId = Guid.NewGuid();
        var contentBlockId = Guid.NewGuid();
        var embedBlockId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(new ReleaseContentSection
            {
                Release = _release,
                ContentSection = new ContentSection
                {
                    Id = contentSectionId,
                    Content = new()
                    {
                        new EmbedBlockLink
                        {
                            Id = contentBlockId,
                            Order = 93,
                            EmbedBlockId = embedBlockId,
                        },
                    }
                }
            });
            await context.EmbedBlocks.AddRangeAsync(new EmbedBlock
            {
                Id = embedBlockId,
                Title = "Test title",
                Url = "https://department-for-education.shinyapps.io/test-page",
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Update(_release.Id,
                contentBlockId,
                new EmbedBlockUpdateRequest
                {
                    Title = "Test title updated",
                    Url = "https://department-for-education.shinyapps.io/updated-test-page",
                });

            var viewModel = result.AssertRight();

            Assert.Equal(93, viewModel.Order);
            Assert.Equal("Test title updated", viewModel.Title);
            Assert.Equal("https://department-for-education.shinyapps.io/updated-test-page", viewModel.Url);
            Assert.Empty(viewModel.Comments);
            Assert.Null(viewModel.Locked);
            Assert.Null(viewModel.LockedUntil);
            Assert.Null(viewModel.LockedBy);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var embedBlockLinks = context.EmbedBlockLinks.ToList();
            var embedBlockLink = Assert.Single(embedBlockLinks);
            Assert.Equal(93, embedBlockLink.Order);
            Assert.Equal(contentSectionId, embedBlockLink.ContentSectionId);
            Assert.Equal(embedBlockId, embedBlockLink.EmbedBlockId);

            var embedBlocks = context.EmbedBlocks.ToList();
            var embedBlock = Assert.Single(embedBlocks);
            Assert.Equal(embedBlockLink.EmbedBlockId, embedBlock.Id);
            Assert.Equal("Test title updated", embedBlock.Title);
            Assert.Equal("https://department-for-education.shinyapps.io/updated-test-page", embedBlock.Url);
        }
    }
    
    [Fact]
    public async Task Update_InvalidDomain()
    {
        var contentSectionId = Guid.NewGuid();
        var contentBlockId = Guid.NewGuid();
        var embedBlockId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(new ReleaseContentSection
            {
                Release = _release,
                ContentSection = new ContentSection
                {
                    Id = contentSectionId,
                    Content = new()
                    {
                        new EmbedBlockLink
                        {
                            Id = contentBlockId,
                            Order = 93,
                            EmbedBlockId = embedBlockId,
                        },
                    }
                }
            });
            await context.EmbedBlocks.AddRangeAsync(new EmbedBlock
            {
                Id = embedBlockId,
                Title = "Test title",
                Url = "https://department-for-education.shinyapps.io/test-page",
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Update(_release.Id,
                contentBlockId,
                new EmbedBlockUpdateRequest
                {
                    Title = "Test title updated",
                    Url = "http://www.invalid.com/updated-test-page",
                });

            result.AssertBadRequest(EmbedBlockUrlDomainNotPermitted);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            // Expect to not see any changes.
            var embedBlockLinks = context.EmbedBlockLinks.ToList();
            var embedBlockLink = Assert.Single(embedBlockLinks);
            Assert.Equal(93, embedBlockLink.Order);
            Assert.Equal(contentSectionId, embedBlockLink.ContentSectionId);
            Assert.Equal(embedBlockId, embedBlockLink.EmbedBlockId);

            var embedBlocks = context.EmbedBlocks.ToList();
            var embedBlock = Assert.Single(embedBlocks);
            Assert.Equal(embedBlockLink.EmbedBlockId, embedBlock.Id);
            Assert.Equal("Test title", embedBlock.Title);
            Assert.Equal("https://department-for-education.shinyapps.io/test-page", embedBlock.Url);
        }
    }

    [Fact]
    public async Task Update_ReleaseDoesNotExist()
    {
        var contentSectionId = Guid.NewGuid();
        var contentBlockId = Guid.NewGuid();
        var embedBlockId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(new ReleaseContentSection
            {
                Release = _release,
                ContentSection = new ContentSection
                {
                    Id = contentSectionId,
                    Content = new()
                    {
                        new EmbedBlockLink
                        {
                            Id = contentBlockId,
                            Order = 93,
                            EmbedBlockId = embedBlockId,
                        },
                    }
                }
            });
            await context.EmbedBlocks.AddRangeAsync(new EmbedBlock
            {
                Id = embedBlockId,
                Title = "Test title",
                Url = "https://department-for-education.shinyapps.io/test-page",
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Update(Guid.NewGuid(),
                contentBlockId,
                new EmbedBlockUpdateRequest
                {
                    Title = "Test title update",
                    Url = "https://department-for-education.shinyapps.io/updated-test-page",
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Update_ContentBlockDoesNotExist()
    {
        var contentSectionId = Guid.NewGuid();
        var contentBlockId = Guid.NewGuid();
        var embedBlockId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(new ReleaseContentSection
            {
                Release = _release,
                ContentSection = new ContentSection
                {
                    Id = contentSectionId,
                    Content = new()
                    {
                        new EmbedBlockLink
                        {
                            Id = contentBlockId,
                            Order = 93,
                            EmbedBlockId = embedBlockId,
                        },
                    }
                }
            });
            await context.EmbedBlocks.AddRangeAsync(new EmbedBlock
            {
                Id = embedBlockId,
                Title = "Test title",
                Url = "https://department-for-education.shinyapps.io/test-page",
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Update(_release.Id,
                contentBlockId: Guid.NewGuid(),
                new EmbedBlockUpdateRequest
                {
                    Title = "Test title update",
                    Url = "https://department-for-education.shinyapps.io/updated-test-page",
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Update_ContentSectionAttachedToDifferentRelease()
    {
        var contentSectionId = Guid.NewGuid();
        var contentBlockId = Guid.NewGuid();
        var embedBlockId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(
                new ReleaseContentSection
                {
                    Release = new Release(),
                    ContentSection = new ContentSection
                    {
                        Id = contentSectionId,
                        Content = new()
                        {
                            new EmbedBlockLink
                            {
                                Id = contentBlockId,
                                Order = 93,
                                EmbedBlockId = embedBlockId,
                            },
                        },
                    },
                },
                new ReleaseContentSection
                {
                    Release = _release,
                    ContentSection = new ContentSection(),
                });
            await context.EmbedBlocks.AddRangeAsync(new EmbedBlock
            {
                Id = embedBlockId,
                Title = "Test title",
                Url = "https://department-for-education.shinyapps.io/test-page",
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Update(_release.Id,
                contentBlockId,
                new EmbedBlockUpdateRequest
                {
                    Title = "Test title update",
                    Url = "https://department-for-education.shinyapps.io/updated-test-page",
                });

            result.AssertBadRequest(ContentSectionNotAttachedToRelease);
        }
    }

    [Fact]
    public async Task Delete()
    {
        var contentSectionId = Guid.NewGuid();
        var contentBlockId = Guid.NewGuid();
        var embedBlockId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(new ReleaseContentSection
            {
                Release = _release,
                ContentSection = new ContentSection
                {
                    Id = contentSectionId,
                    Content = new()
                    {
                        new EmbedBlockLink
                        {
                            Id = contentBlockId,
                            Order = 93,
                            EmbedBlockId = embedBlockId,
                        },
                    }
                }
            });
            await context.EmbedBlocks.AddRangeAsync(new EmbedBlock
            {
                Id = embedBlockId,
                Title = "Test title",
                Url = "https://department-for-education.shinyapps.io/test-page",
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Delete(_release.Id, contentBlockId);

            result.AssertRight();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var embedBlockLinks = context.EmbedBlockLinks.ToList();
            Assert.Empty(embedBlockLinks);

            var embedBlocks = context.EmbedBlocks.ToList();
            Assert.Empty(embedBlocks);
        }
    }

    [Fact]
    public async Task Delete_ReleaseDoesNotExist()
    {
        var contentSectionId = Guid.NewGuid();
        var contentBlockId = Guid.NewGuid();
        var embedBlockId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(new ReleaseContentSection
            {
                Release = _release,
                ContentSection = new ContentSection
                {
                    Id = contentSectionId,
                    Content = new()
                    {
                        new EmbedBlockLink
                        {
                            Id = contentBlockId,
                            Order = 93,
                            EmbedBlockId = embedBlockId,
                        },
                    }
                }
            });
            await context.EmbedBlocks.AddRangeAsync(new EmbedBlock
            {
                Id = embedBlockId,
                Title = "Test title",
                Url = "https://department-for-education.shinyapps.io/test-page",
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Delete(Guid.NewGuid(), contentBlockId);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Delete_ContentBlockDoesNotExist()
    {
        var contentSectionId = Guid.NewGuid();
        var contentBlockId = Guid.NewGuid();
        var embedBlockId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(new ReleaseContentSection
            {
                Release = _release,
                ContentSection = new ContentSection
                {
                    Id = contentSectionId,
                    Content = new()
                    {
                        new EmbedBlockLink
                        {
                            Id = contentBlockId,
                            Order = 93,
                            EmbedBlockId = embedBlockId,
                        },
                    }
                }
            });
            await context.EmbedBlocks.AddRangeAsync(new EmbedBlock
            {
                Id = embedBlockId,
                Title = "Test title",
                Url = "https://department-for-education.shinyapps.io/test-page",
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Delete(_release.Id, Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Delete_ContentBlockAttachedToDifferentRelease()
    {
        var contentSectionId = Guid.NewGuid();
        var contentBlockId = Guid.NewGuid();
        var embedBlockId = Guid.NewGuid();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(
                new ReleaseContentSection
                {
                    Release = new Release(),
                    ContentSection = new ContentSection
                    {
                        Id = contentSectionId,
                        Content = new()
                        {
                            new EmbedBlockLink
                            {
                                Id = contentBlockId,
                                Order = 93,
                                EmbedBlockId = embedBlockId,
                            },
                        },
                    },
                },
                new ReleaseContentSection
                {
                    Release = _release,
                    ContentSection = new ContentSection(),
                });
            await context.EmbedBlocks.AddRangeAsync(new EmbedBlock
            {
                Id = embedBlockId,
                Title = "Test title",
                Url = "https://department-for-education.shinyapps.io/test-page",
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = BuildEmbedBlockService(context);
            var result = await service.Delete(_release.Id, contentBlockId);

            result.AssertBadRequest(ContentBlockNotAttachedToRelease);
        }
    }

    private static EmbedBlockService BuildEmbedBlockService(
        ContentDbContext? contentDbContext = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IContentBlockService? contentBlockService = null,
        IUserService? userService = null)
    {
        var context = contentDbContext ?? Mock.Of<ContentDbContext>(Strict);
        return new EmbedBlockService(
            context,
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(context),
            contentBlockService ?? new ContentBlockService(context),
            userService ?? AlwaysTrueUserService().Object,
            AdminMapper());
    }
}
