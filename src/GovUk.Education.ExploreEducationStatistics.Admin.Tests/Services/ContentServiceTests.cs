using System;
using System.Collections.Generic;
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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ContentServiceTests
    {
        [Fact]
        public async Task GetContentBlocks_NoContentSections()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);

                var result = await service.GetContentBlocks<HtmlBlock>(release.Id);

                Assert.True(result.IsRight);

                Assert.Empty(result.Right);
            }
        }

        [Fact]
        public async Task GetContentBlocks_NoContentBlocks()
        {
            var release = new Release
            {
                Content = new List<ReleaseContentSection>
                {
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 1
                        }
                    },
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 2
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);

                var result = await service.GetContentBlocks<HtmlBlock>(release.Id);

                Assert.True(result.IsRight);

                Assert.Empty(result.Right);
            }
        }

        [Fact]
        public async Task GetContentBlocks()
        {
            var release = new Release
            {
                Content = new List<ReleaseContentSection>
                {
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 1,
                            Content = new List<ContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Created = new DateTime(2001, 1, 1)
                                },
                                new HtmlBlock
                                {
                                    Created = new DateTime(2002, 2, 2)
                                },
                                new DataBlock()
                            }
                        }
                    },
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 2,
                            Content = new List<ContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Created = new DateTime(2003, 3, 3)
                                },
                                new HtmlBlock
                                {
                                    Created = new DateTime(2004, 4, 4)
                                },
                                new DataBlock()
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);

                var result = await service.GetContentBlocks<HtmlBlock>(release.Id);

                Assert.True(result.IsRight);

                var contentBlocks = result.Right;

                Assert.Equal(4, contentBlocks.Count);
                Assert.Equal(release.Content[0].ContentSection.Content[0].Id, contentBlocks[0].Id);
                Assert.Equal(release.Content[0].ContentSection.Content[1].Id, contentBlocks[1].Id);
                Assert.Equal(release.Content[1].ContentSection.Content[0].Id, contentBlocks[2].Id);
                Assert.Equal(release.Content[1].ContentSection.Content[1].Id, contentBlocks[3].Id);

                Assert.Equal(new DateTime(2001, 1, 1), contentBlocks[0].Created);
                Assert.Equal(new DateTime(2002, 2, 2), contentBlocks[1].Created);
                Assert.Equal(new DateTime(2003, 3, 3), contentBlocks[2].Created);
                Assert.Equal(new DateTime(2004, 4, 4), contentBlocks[3].Created);
            }
        }

        [Fact]
        public async Task AddComment()
        {
            var release = new Release
            {
                Content = new List<ReleaseContentSection>
                {
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 1,
                            Content = new List<ContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Created = new DateTime(2001, 1, 1),
                                    Comments = new List<Comment>
                                    {
                                        new Comment {Content = "Existing comment"}
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);

                var result = await service.AddComment(
                    release.Id,
                    release.Content[0].ContentSection.Id,
                    release.Content[0].ContentSection.Content[0].Id,
                    new CommentSaveRequest { Content = "New comment" });

                var viewModel = result.AssertRight();
                Assert.Equal("New comment", viewModel.Content);
                Assert.Null(viewModel.Resolved);

                var comments = contentDbContext.Comment.ToList();
                Assert.Equal(2, comments.Count);
                Assert.Equal("Existing comment", comments[0].Content);
                Assert.Equal("New comment", comments[1].Content);
            }
        }

        [Fact]
        public async Task ResolveComment_True()
        {
            var comment = new Comment
            {
                Content = "Existing comment",
                CreatedById = Guid.NewGuid(),
            };
            var release = new Release
            {
                Content = new List<ReleaseContentSection>
                {
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Content = new List<ContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Comments = new List<Comment>
                                    {
                                        comment
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userId = Guid.NewGuid();
                var service = SetupContentService(
                    contentDbContext: contentDbContext,
                    userId: userId);
                var result = await service.ResolveComment(comment.Id, true);

                var viewModel = result.AssertRight();
                Assert.Equal("Existing comment", viewModel.Content);
                Assert.NotNull(viewModel.Resolved);

                var comments = contentDbContext.Comment.ToList();
                Assert.Single(comments);
                Assert.Equal("Existing comment", comments[0].Content);
                Assert.Equal(comment.CreatedById, comments[0].CreatedById);
                Assert.Equal(userId, comments[0].ResolvedById);
            }
        }

        [Fact]
        public async Task ResolveComment_False()
        {
            var comment = new Comment
            {
                Content = "Existing comment",
                CreatedById = Guid.NewGuid(),
            };
            var release = new Release
            {
                Content = new List<ReleaseContentSection>
                {
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Content = new List<ContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Comments = new List<Comment>
                                    {
                                        comment
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userId = Guid.NewGuid();
                var service = SetupContentService(
                    contentDbContext: contentDbContext,
                    userId: userId);
                var result = await service.ResolveComment(comment.Id, false);

                var viewModel = result.AssertRight();
                Assert.Equal("Existing comment", viewModel.Content);
                Assert.Null(viewModel.Resolved);
                Assert.Null(viewModel.ResolvedBy);

                var comments = contentDbContext.Comment.ToList();
                Assert.Single(comments);
                Assert.Equal("Existing comment", comments[0].Content);
                Assert.Equal(comment.CreatedById, comments[0].CreatedById);
                Assert.Null(comments[0].ResolvedById);
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
            var release = new Release
            {
                Content = new List<ReleaseContentSection>
                {
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Content = new List<ContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Comments = new List<Comment>
                                    {
                                        comment
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userId = Guid.NewGuid();
                var service = SetupContentService(
                    contentDbContext: contentDbContext,
                    userId: userId);
                var result = await service.UpdateComment(comment.Id,
                    new CommentSaveRequest { Content = "Existing comment updated" });

                var viewModel = result.AssertRight();
                Assert.Equal("Existing comment updated", viewModel.Content);
                Assert.Null(viewModel.Resolved);
                Assert.Null(viewModel.ResolvedBy);

                var comments = contentDbContext.Comment.ToList();
                Assert.Single(comments);
                Assert.Equal("Existing comment updated", comments[0].Content);
                Assert.Equal(comment.CreatedById, comments[0].CreatedById);
                Assert.Null(comments[0].ResolvedById);
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
            var release = new Release
            {
                Content = new List<ReleaseContentSection>
                {
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Content = new List<ContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Comments = new List<Comment>
                                    {
                                        comment1, comment2, comment3
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext);
                var result = await service.DeleteComment(comment2.Id);

                var viewModel = result.AssertRight();
                Assert.True(viewModel);

                var comments = contentDbContext.Comment.ToList();
                Assert.Equal(2, comments.Count);
                Assert.Equal("Comment 1", comments[0].Content);
                Assert.Equal(comment1.CreatedById, comments[0].CreatedById);
                Assert.Equal("Comment 3", comments[1].Content);
                Assert.Equal(comment3.CreatedById, comments[1].CreatedById);
            }
        }

        private static ContentService SetupContentService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IReleaseContentSectionRepository releaseContentSectionRepository = null,
            IUserService userService = null,
            Guid? userId = null)
        {
            return new ContentService(
                contentDbContext,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                releaseContentSectionRepository ?? new ReleaseContentSectionRepository(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService(userId).Object,
                AdminMapper()
            );
        }
    }
}
