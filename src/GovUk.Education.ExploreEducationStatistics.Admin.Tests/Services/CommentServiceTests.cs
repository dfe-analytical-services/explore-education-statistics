#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class CommentServiceTests
{
    [Fact]
    public async Task AddComment()
    {
        var releaseVersion = new ReleaseVersion
        {
            Content = ListOf(
                new ContentSection
                {
                    Heading = "New section",
                    Order = 1,
                    Content = ListOf<ContentBlock>(
                        new HtmlBlock
                        {
                            Created = new DateTime(2001, 1, 1),
                            Comments = ListOf(new Comment
                            {
                                Content = "Existing comment"
                            })
                        }
                    )
                }
            )
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupCommentService(contentDbContext: contentDbContext);

            var result = await service.AddComment(
                releaseVersion.Id,
                releaseVersion.Content[0].Id,
                releaseVersion.Content[0].Content[0].Id,
                new CommentSaveRequest
                {
                    Content = "New comment"
                });

            var viewModel = result.AssertRight();
            Assert.Equal("New comment", viewModel.Content);
            Assert.Null(viewModel.Resolved);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var comments = contentDbContext.Comment.ToList();
            Assert.Equal(2, comments.Count);
            Assert.Equal("Existing comment", comments[0].Content);
            Assert.Equal("New comment", comments[1].Content);
        }
    }

    [Fact]
    public async Task AddComment_NoRelease()
    {
        await using (var contentDbContext = InMemoryApplicationDbContext())
        {
            var service = SetupCommentService(contentDbContext: contentDbContext);

            var result = await service.AddComment(
                releaseVersionId: Guid.NewGuid(),
                contentSectionId: Guid.NewGuid(),
                contentBlockId: Guid.NewGuid(),
                new CommentSaveRequest
                {
                    Content = "New comment"
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task AddComment_NoContentSection()
    {
        var releaseVersion = new ReleaseVersion();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupCommentService(contentDbContext: contentDbContext);

            var result = await service.AddComment(
                releaseVersionId: releaseVersion.Id,
                contentSectionId: Guid.NewGuid(),
                contentBlockId: Guid.NewGuid(),
                new CommentSaveRequest
                {
                    Content = "New comment"
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task AddComment_NoContentBlock()
    {
        var releaseVersion = new ReleaseVersion
        {
            Content = ListOf(
                new ContentSection
                {
                    Heading = "New section",
                    Order = 1,
                    Content = ListOf<ContentBlock>()
                }
            )
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupCommentService(contentDbContext: contentDbContext);

            var result = await service.AddComment(
                releaseVersionId: releaseVersion.Id,
                contentSectionId: releaseVersion.Content[0].Id,
                contentBlockId: Guid.NewGuid(),
                new CommentSaveRequest
                {
                    Content = "New comment"
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task SetResolved_Resolve()
    {
        var comment = new Comment
        {
            Content = "Existing comment",
            CreatedById = Guid.NewGuid(),
        };
        var releaseVersion = new ReleaseVersion
        {
            Content = ListOf(
                new ContentSection
                {
                    Content = ListOf<ContentBlock>(
                        new HtmlBlock
                        {
                            Comments = ListOf(comment)
                        }
                    )
                }
            )
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var userId = Guid.NewGuid();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupCommentService(
                contentDbContext: contentDbContext,
                userId: userId);
            var result = await service.SetResolved(comment.Id, resolve: true);

            var viewModel = result.AssertRight();
            Assert.Equal("Existing comment", viewModel.Content);
            viewModel.Resolved.AssertUtcNow();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var comments = contentDbContext.Comment.ToList();
            Assert.Single(comments);
            Assert.Equal("Existing comment", comments[0].Content);
            Assert.Equal(comment.CreatedById, comments[0].CreatedById);
            Assert.Equal(userId, comments[0].ResolvedById);
        }
    }

    [Fact]
    public async Task SetResolved_Unresolve()
    {
        var comment = new Comment
        {
            Content = "Existing comment",
            CreatedById = Guid.NewGuid(),
            Resolved = DateTime.UtcNow,
            ResolvedById = Guid.NewGuid(),
        };
        var releaseVersion = new ReleaseVersion
        {
            Content = ListOf(
                new ContentSection
                {
                    Content = ListOf<ContentBlock>(
                        new HtmlBlock
                        {
                            Comments = ListOf(comment)
                        }
                    )
                }
            )
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var userId = Guid.NewGuid();
            var service = SetupCommentService(
                contentDbContext: contentDbContext,
                userId: userId);
            var result = await service.SetResolved(comment.Id, resolve: false);

            var viewModel = result.AssertRight();
            Assert.Equal("Existing comment", viewModel.Content);
            Assert.Null(viewModel.Resolved);
            Assert.Null(viewModel.ResolvedBy);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var comments = contentDbContext.Comment.ToList();
            Assert.Single(comments);
            Assert.Equal("Existing comment", comments[0].Content);
            Assert.Equal(comment.CreatedById, comments[0].CreatedById);
            Assert.Null(comments[0].Resolved);
            Assert.Null(comments[0].ResolvedById);
        }
    }

    [Fact]
    public async Task SetResolved_NoComment()
    {
        await using (var contentDbContext = InMemoryApplicationDbContext())
        {
            var service = SetupCommentService(contentDbContext: contentDbContext);
            var result = await service.SetResolved(Guid.NewGuid(), resolve: true);
            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateComment()
    {
        var comment = new Comment
        {
            Content = "Existing comment",
            CreatedById = Guid.NewGuid(),
        };
        var releaseVersion = new ReleaseVersion
        {
            Content = ListOf(
                new ContentSection
                {
                    Content = ListOf<ContentBlock>(
                        new HtmlBlock
                        {
                            Comments = ListOf(comment)
                        }
                    )
                }
            )
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var userId = Guid.NewGuid();
            var service = SetupCommentService(
                contentDbContext: contentDbContext,
                userId: userId);
            var result = await service.UpdateComment(comment.Id,
                new CommentSaveRequest
                {
                    Content = "Existing comment updated"
                });

            var viewModel = result.AssertRight();
            Assert.Equal("Existing comment updated", viewModel.Content);
            viewModel.Updated.AssertUtcNow();
            Assert.Null(viewModel.Resolved);
            Assert.Null(viewModel.ResolvedBy);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var comments = contentDbContext.Comment.ToList();
            Assert.Single(comments);
            Assert.Equal("Existing comment updated", comments[0].Content);
            Assert.Equal(comment.CreatedById, comments[0].CreatedById);
            comments[0].Updated.AssertUtcNow();
            Assert.Null(comments[0].ResolvedById);
        }
    }

    [Fact]
    public async Task UpdateComment_NoComment()
    {
        await using (var contentDbContext = InMemoryApplicationDbContext())
        {
            var service = SetupCommentService(contentDbContext: contentDbContext);
            var result = await service.UpdateComment(Guid.NewGuid(),
                new CommentSaveRequest
                {
                    Content = "Existing comment updated"
                });

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task DeleteComment()
    {
        var comment1 = new Comment
        {
            Content = "Comment 1",
            CreatedById = Guid.NewGuid(),
        };
        var comment2 = new Comment
        {
            Content = "Comment 2",
            CreatedById = Guid.NewGuid(),
        };
        var comment3 = new Comment
        {
            Content = "Comment 3",
            CreatedById = Guid.NewGuid(),
        };
        var releaseVersion = new ReleaseVersion
        {
            Content = ListOf(
                new ContentSection
                {
                    Content = ListOf<ContentBlock>(
                        new HtmlBlock
                        {
                            Comments = ListOf(comment1, comment2, comment3)
                        }
                    )
                }
            )
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupCommentService(contentDbContext);
            var result = await service.DeleteComment(comment2.Id);

            var resultRight = result.AssertRight();
            Assert.True(resultRight);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var comments = contentDbContext.Comment.ToList();
            Assert.Equal(2, comments.Count);
            Assert.Equal("Comment 1", comments[0].Content);
            Assert.Equal(comment1.CreatedById, comments[0].CreatedById);
            Assert.Equal("Comment 3", comments[1].Content);
            Assert.Equal(comment3.CreatedById, comments[1].CreatedById);
        }
    }

    [Fact]
    public async Task DeleteComment_NoComment()
    {
        await using (var contentDbContext = InMemoryApplicationDbContext())
        {
            var service = SetupCommentService(contentDbContext);
            var result = await service.DeleteComment(Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    private static CommentService SetupCommentService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IUserService? userService = null,
        Guid? userId = null)
    {
        return new CommentService(
            contentDbContext,
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            userService ?? MockUtils.AlwaysTrueUserService(userId).Object,
            AdminMapper()
        );
    }
}
