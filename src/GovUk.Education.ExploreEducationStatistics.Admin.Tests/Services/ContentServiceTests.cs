using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Clients;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

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
                Content = ListOf(
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
                )
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
                Content = ListOf(
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 1,
                            Content = ListOf<ContentBlock>(
                                new HtmlBlock
                                {
                                    Body = "Test html block 1"
                                },
                                new HtmlBlock
                                {
                                    Body = "Test html block 2"
                                },
                                new DataBlock
                                {
                                    Name = "Test data block 1"
                                }
                            )
                        }
                    },
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 2,
                            Content = ListOf<ContentBlock>(
                                new HtmlBlock
                                {
                                    Body = "Test html block 3"
                                },
                                new HtmlBlock
                                {
                                    Body = "Test html block 4"
                                },
                                new DataBlock
                                {
                                    Name = "Test data block 2"
                                }
                            )
                        }
                    }
                )
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

                var contentBlocks = result.AssertRight();

                Assert.Equal(4, contentBlocks.Count);
                Assert.Equal(release.Content[0].ContentSection.Content[0].Id, contentBlocks[0].Id);
                Assert.Equal(release.Content[0].ContentSection.Content[1].Id, contentBlocks[1].Id);
                Assert.Equal(release.Content[1].ContentSection.Content[0].Id, contentBlocks[2].Id);
                Assert.Equal(release.Content[1].ContentSection.Content[1].Id, contentBlocks[3].Id);

                Assert.Equal("Test html block 1", contentBlocks[0].Body);
                Assert.Equal("Test html block 2", contentBlocks[1].Body);
                Assert.Equal("Test html block 3", contentBlocks[2].Body);
                Assert.Equal("Test html block 4", contentBlocks[3].Body);
            }
        }

        [Fact]
        public async Task GetContentSections()
        {
            var release = new Release
            {
                Content = ListOf(
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 1,
                            Content = ListOf<ContentBlock>(
                                new HtmlBlock
                                {
                                    Created = new DateTime(2001, 1, 1)
                                },
                                new HtmlBlock
                                {
                                    Created = new DateTime(2002, 2, 2)
                                },
                                new DataBlock()
                            )
                        }
                    },
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 2,
                            Content = ListOf<ContentBlock>(
                                new HtmlBlock
                                {
                                    Created = new DateTime(2003, 3, 3)
                                }
                            )
                        }
                    }
                )
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
                var result = await service.GetContentSections(release.Id);

                var viewModel = result.AssertRight();

                Assert.Equal(2, viewModel.Count);
                Assert.Equal(release.Content[0].ContentSection.Id, viewModel[0].Id);
                Assert.Equal(release.Content[1].ContentSection.Id, viewModel[1].Id);

                Assert.Equal(3, viewModel[0].Content.Count);
                Assert.Single(viewModel[1].Content);
            }
        }

        [Fact]
        public async Task GetContentSections_Comments()
        {
            var user1 = new User
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@test.com"
            };
            var user2 = new User
            {
                FirstName = "Rob",
                LastName = "Rowe",
                Email = "rob@test.com"
            };

            var release = new Release
            {
                Content = ListOf(
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 1,
                            Content = ListOf<ContentBlock>(
                                new HtmlBlock
                                {
                                    Body = "Test html block 1",
                                    Comments = ListOf(
                                        new Comment
                                        {
                                            Content = "Comment 1",
                                            Created = DateTime.Parse("2022-03-16T12:00:00Z"),
                                            CreatedBy = user1
                                        },
                                        new Comment
                                        {
                                            Content = "Comment 2",
                                            Created = DateTime.Parse("2022-03-12T12:00:00Z"),
                                            CreatedBy = user2,
                                            Resolved = DateTime.Parse("2022-03-14T12:00:00Z"),
                                            ResolvedBy = user1
                                        }
                                    )
                                }
                            )
                        }
                    }
                )
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

                var result = await service.GetContentSections(release.Id);

                var contentSections = result.AssertRight();
                var contentSection = Assert.Single(contentSections);

                Assert.Single(contentSection.Content);
                var contentBlock = Assert.IsType<HtmlBlockViewModel>(contentSection.Content[0]);

                Assert.Equal(release.Content[0].ContentSection.Content[0].Id, contentBlock.Id);
                Assert.Equal("Test html block 1", contentBlock.Body);

                Assert.Equal(2, contentBlock.Comments.Count);

                // Comments are ordered in ascending order by created date (oldest first)

                Assert.Equal("Comment 2", contentBlock.Comments[0].Content);

                Assert.Equal(DateTime.Parse("2022-03-12T12:00:00Z"), contentBlock.Comments[0].Created);
                Assert.Equal(user2.Id, contentBlock.Comments[0].CreatedBy.Id);
                Assert.Equal("Rob Rowe", contentBlock.Comments[0].CreatedBy.DisplayName);
                Assert.Equal("rob@test.com", contentBlock.Comments[0].CreatedBy.Email);

                Assert.Equal(DateTime.Parse("2022-03-14T12:00:00Z"), contentBlock.Comments[0].Resolved);
                Assert.Equal(user1.Id, contentBlock.Comments[0].ResolvedBy.Id);
                Assert.Equal("Jane Doe", contentBlock.Comments[0].ResolvedBy.DisplayName);
                Assert.Equal("jane@test.com", contentBlock.Comments[0].ResolvedBy.Email);

                Assert.Equal("Comment 1", contentBlock.Comments[1].Content);

                Assert.Equal(DateTime.Parse("2022-03-16T12:00:00Z"), contentBlock.Comments[1].Created);
                Assert.Equal(user1.Id, contentBlock.Comments[1].CreatedBy.Id);
                Assert.Equal("Jane Doe", contentBlock.Comments[1].CreatedBy.DisplayName);
                Assert.Equal("jane@test.com", contentBlock.Comments[1].CreatedBy.Email);
            }
        }

        [Fact]
        public async Task GetContentSections_LockedBlock()
        {
            var user = new User
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@test.com"
            };

            var release = new Release
            {
                Content = ListOf(
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 1,
                            Content = ListOf<ContentBlock>(
                                new HtmlBlock
                                {
                                    Body = "Test html block 1",
                                    Locked = DateTime.Parse("2022-03-16T12:00:00Z"),
                                    LockedBy = user
                                }
                            )
                        }
                    }
                )
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

                var result = await service.GetContentSections(release.Id);

                var contentSections = result.AssertRight();
                var contentSection = Assert.Single(contentSections);

                Assert.Single(contentSection.Content);
                var contentBlock = Assert.IsType<HtmlBlockViewModel>(contentSection.Content[0]);

                Assert.Equal(release.Content[0].ContentSection.Content[0].Id, contentBlock.Id);
                Assert.Equal("Test html block 1", contentBlock.Body);

                Assert.Equal(DateTime.Parse("2022-03-16T12:00:00Z"), contentBlock.Locked);
                Assert.Equal(user.Id, contentBlock.LockedBy!.Id);
                Assert.Equal("Jane Doe", contentBlock.LockedBy!.DisplayName);
                Assert.Equal("jane@test.com", contentBlock.LockedBy!.Email);
            }
        }

        [Fact]
        public async Task GetContentSections_NoContentBlocks()
        {
            var release = new Release
            {
                Content = ListOf(
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 1,
                            Content = ListOf<ContentBlock>()
                        }
                    }
                )
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
                var result = await service.GetContentSections(release.Id);

                var viewModel = result.AssertRight();

                Assert.Single(viewModel);
                Assert.Equal(release.Content[0].ContentSection.Id, viewModel[0].Id);
                Assert.Empty(viewModel[0].Content);
            }
        }

        [Fact]
        public async Task GetContentSections_NoContentSections()
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
                var result = await service.GetContentSections(release.Id);

                var viewModel = result.AssertRight();

                Assert.Empty(viewModel);
            }
        }

        [Fact]
        public async Task RemoveContentSection()
        {
            var release = new Release();

            var releaseContentSectionToRemove = new ReleaseContentSection
            {
                Release = release,
                ContentSection = new ContentSection
                {
                    Order = 1,
                    Content = new List<ContentBlock>
                    {
                        new HtmlBlock(),
                        new DataBlock(),
                        new EmbedBlockLink
                        {
                            EmbedBlock = new EmbedBlock(),
                        },
                    },
                },
            };

            var releaseContentSection2 = new ReleaseContentSection
            {
                Release = release,
                ContentSection = new ContentSection
                {
                    Order = 2,
                    Content = new List<ContentBlock>(),
                },
            };

            var releaseContentSection3 = new ReleaseContentSection
            {
                Release = release,
                ContentSection = new ContentSection
                {
                    Order = 3,
                    Content = new List<ContentBlock>(),
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(release);
                await contentDbContext.ReleaseContentSections.AddRangeAsync(
                    releaseContentSectionToRemove, releaseContentSection2, releaseContentSection3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentSection(
                    releaseContentSectionToRemove.ReleaseId,
                    releaseContentSectionToRemove.ContentSectionId);

                var contentSectionList = result.AssertRight();

                Assert.Equal(2, contentSectionList.Count);

                Assert.Equal(releaseContentSection2.ContentSectionId, contentSectionList[0].Id);
                Assert.Equal(1, contentSectionList[0].Order);
                Assert.Equal(releaseContentSection3.ContentSectionId, contentSectionList[1].Id);
                Assert.Equal(2, contentSectionList[1].Order);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseContentSections = contentDbContext.ReleaseContentSections.ToList();
                Assert.Equal(2, releaseContentSections.Count);

                Assert.Equal(releaseContentSection2.ContentSectionId, releaseContentSections[0].ContentSectionId);
                Assert.Equal(releaseContentSection2.ReleaseId, releaseContentSections[0].ReleaseId);

                Assert.Equal(releaseContentSection3.ContentSectionId, releaseContentSections[1].ContentSectionId);
                Assert.Equal(releaseContentSection3.ReleaseId, releaseContentSections[1].ReleaseId);


                var contentSections = contentDbContext.ContentSections.ToList();
                Assert.Equal(2, contentSections.Count);

                Assert.Equal(releaseContentSection2.ContentSectionId, contentSections[0].Id);
                Assert.Equal(1, contentSections[0].Order);

                Assert.Equal(releaseContentSection3.ContentSectionId, contentSections[1].Id);
                Assert.Equal(2, contentSections[1].Order);


                var contentBlocks = contentDbContext.ContentBlocks.ToList();

                var dataBlock = Assert.Single(contentBlocks); // data blocks are detached, not deleted
                Assert.IsType<DataBlock>(dataBlock);
                Assert.Equal(0, dataBlock.Order);
                Assert.Null(dataBlock.ContentSectionId);

                var embedBlocks = contentDbContext.EmbedBlocks.ToList();
                Assert.Empty(embedBlocks);
            }
        }

        [Fact]
        public async Task RemoveContentSection_NoRelease()
        {
            var releaseContentSection = new ReleaseContentSection
            {
                Release = new Release(),
                ContentSection = new ContentSection
                {
                    Content = new List<ContentBlock>
                    {
                        new HtmlBlock(),
                    },
                },
            };
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseContentSections.AddAsync(releaseContentSection);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentSection(
                    Guid.NewGuid(),
                    releaseContentSection.ContentSectionId);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task RemoveContentSection_ContentSectionNotAttachedToRelease()
        {
            var contentSectionId = Guid.NewGuid();
            var releaseContentSection = new ReleaseContentSection
            {
                Release = new Release(),
                ContentSection = new ContentSection
                {
                    Content = new List<ContentBlock>
                    {
                        new HtmlBlock(),
                    },
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseContentSections.AddRangeAsync(releaseContentSection);
                await contentDbContext.ContentSections.AddRangeAsync(
                    new ContentSection
                    {
                        Id = contentSectionId,
                        Content = new List<ContentBlock>
                        {
                            new HtmlBlock(),
                        },
                    });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentSection(
                    releaseContentSection.ReleaseId,
                    contentSectionId);

                result.AssertNotFound();
            }
        }

        private static ContentService SetupContentService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IReleaseContentSectionRepository releaseContentSectionRepository = null,
            IContentBlockService contentBlockService = null,
            IHubContext<ReleaseContentHub, IReleaseContentHubClient> hubContext = null,
            IUserService userService = null)
        {
            return new ContentService(
                contentDbContext,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                releaseContentSectionRepository ?? new ReleaseContentSectionRepository(contentDbContext),
                contentBlockService ?? new ContentBlockService(contentDbContext),
                hubContext ?? Mock.Of<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                AdminMapper()
            );
        }
    }
}
