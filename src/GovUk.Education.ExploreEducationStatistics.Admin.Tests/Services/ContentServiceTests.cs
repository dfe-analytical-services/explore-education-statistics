using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Clients;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
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

        [Fact]
        public async Task RemoveContentBlock()
        {
            var contentSectionId = Guid.NewGuid();
            var contentBlockId = Guid.NewGuid();
            var releaseContentSection = new ReleaseContentSection
            {
                Release = new Release(),
                ContentSection = new ContentSection
                {
                    Id = contentSectionId,
                    Content = new List<ContentBlock>
                    {
                        new HtmlBlock
                        {
                            Id = contentBlockId,
                            Order = 0,
                        },
                        new DataBlock { Order = 1 },
                        new HtmlBlock { Order = 2 },
                    },
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseContentSections.AddRangeAsync(releaseContentSection);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentBlock(
                    releaseContentSection.ReleaseId,
                    contentSectionId,
                    contentBlockId);

                var viewModelList = result.AssertRight();

                Assert.Equal(2, viewModelList.Count);
                Assert.Null(viewModelList.Find(viewModel => viewModel.Id == contentBlockId));
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var contentBlocks = contentDbContext.ContentBlocks
                    .OrderBy(cb => cb.Order)
                    .ToList();

                Assert.Equal(2, contentBlocks.Count);

                Assert.Equal(0, contentBlocks[0].Order);
                Assert.IsType<DataBlock>(contentBlocks[0]);
                Assert.Equal(1, contentBlocks[1].Order);
                Assert.IsType<HtmlBlock>(contentBlocks[1]);
            }
        }

        [Fact]
        public async Task RemoveContentBlock_NoRelease()
        {
            var contentBlockId = Guid.NewGuid();
            var releaseContentSection = new ReleaseContentSection
            {
                Release = new Release(),
                ContentSection = new ContentSection
                {
                    Content = new List<ContentBlock>
                    {
                        new HtmlBlock
                        {
                            Id = contentBlockId,
                        },
                        new DataBlock(),
                        new HtmlBlock(),
                    },
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseContentSections.AddRangeAsync(releaseContentSection);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentBlock(
                    Guid.NewGuid(),
                    releaseContentSection.ContentSectionId,
                    contentBlockId);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task RemoveContentBlock_NoContentSection()
        {
            var contentBlockId = Guid.NewGuid();
            var releaseContentSection = new ReleaseContentSection
            {
                Release = new Release(),
                ContentSection = new ContentSection
                {
                    Content = new List<ContentBlock>
                    {
                        new HtmlBlock
                        {
                            Id = contentBlockId,
                        },
                        new DataBlock(),
                        new HtmlBlock(),
                    },
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseContentSections.AddRangeAsync(releaseContentSection);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentBlock(
                    releaseContentSection.ReleaseId,
                    Guid.NewGuid(),
                    contentBlockId);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task RemoveContentBlock_NoContentBlock()
        {
            var contentBlockId = Guid.NewGuid();
            var releaseContentSection = new ReleaseContentSection
            {
                Release = new Release(),
                ContentSection = new ContentSection
                {
                    Content = new List<ContentBlock>
                    {
                        new HtmlBlock
                        {
                            Id = contentBlockId,
                        },
                        new DataBlock(),
                        new HtmlBlock(),
                    },
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseContentSections.AddRangeAsync(releaseContentSection);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentBlock(
                    releaseContentSection.ReleaseId,
                    releaseContentSection.ContentSectionId,
                    Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task RemoveContentBlock_BlockAttachedToIncorrectSection()
        {
            var release = new Release();

            var incorrectContentBlockId = Guid.NewGuid();
            var incorrectReleaseContentSection = new ReleaseContentSection()
            {
                Release = release,
                ContentSection = new ContentSection
                {
                    Content = new List<ContentBlock>
                    {
                        new HtmlBlock { Id = incorrectContentBlockId },
                    },
                },
            };

            var releaseContentSection = new ReleaseContentSection
            {
                Release = release,
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
                await contentDbContext.ReleaseContentSections.AddRangeAsync(
                    releaseContentSection, incorrectReleaseContentSection);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentBlock(
                    releaseContentSection.ReleaseId,
                    releaseContentSection.ContentSectionId,
                    incorrectContentBlockId);

                result.AssertNotFound();
            }
        }

        private static ContentService SetupContentService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IKeyStatisticService keyStatisticService = null,
            IReleaseContentSectionRepository releaseContentSectionRepository = null,
            IContentBlockService contentBlockService = null,
            IHubContext<ReleaseContentHub, IReleaseContentHubClient> hubContext = null,
            IUserService userService = null)
        {
            return new ContentService(
                contentDbContext,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                keyStatisticService ?? Mock.Of<IKeyStatisticService>(MockBehavior.Strict),
                releaseContentSectionRepository ?? new ReleaseContentSectionRepository(contentDbContext),
                contentBlockService ?? new ContentBlockService(contentDbContext),
                hubContext ?? Mock.Of<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(MockBehavior.Strict),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                AdminMapper()
            );
        }
    }
}
