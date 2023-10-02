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
                    new ContentSection
                    {
                        Heading = "New section",
                        Order = 1
                    },
                    new ContentSection
                    {
                        Heading = "New section",
                        Order = 2
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
            var releaseId = Guid.NewGuid();
            
            var release = new Release
            {
                Id = releaseId,
                Content = ListOf(
                    new ContentSection
                    {
                        Heading = "New section",
                        Order = 1,
                        Content = ListOf<ContentBlock>(
                            new HtmlBlock
                            {
                                Body = "Test html block 1",
                                ReleaseId = releaseId
                            },
                            new HtmlBlock
                            {
                                Body = "Test html block 2",
                                ReleaseId = releaseId
                            },
                            new DataBlock
                            {
                                Name = "Test data block 1",
                                ReleaseId = releaseId
                            }
                        ),
                        ReleaseId = releaseId
                    },
                    new ContentSection
                    {
                        Heading = "New section",
                        Order = 2,
                        Content = ListOf<ContentBlock>(
                            new HtmlBlock
                            {
                                Body = "Test html block 3",
                                ReleaseId = releaseId
                            },
                            new HtmlBlock
                            {
                                Body = "Test html block 4",
                                ReleaseId = releaseId
                            },
                            new DataBlock
                            {
                                Name = "Test data block 2",
                                ReleaseId = releaseId
                            }
                        ),
                        ReleaseId = releaseId
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
                Assert.Equal(release.Content[0].Content[0].Id, contentBlocks[0].Id);
                Assert.Equal(release.Content[0].Content[1].Id, contentBlocks[1].Id);
                Assert.Equal(release.Content[1].Content[0].Id, contentBlocks[2].Id);
                Assert.Equal(release.Content[1].Content[1].Id, contentBlocks[3].Id);

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

            var contentSectionToRemove = new ContentSection
            {
                Order = 1,
                Content = new List<ContentBlock>
                {
                    new HtmlBlock
                    {
                        Release = release
                    },
                    new DataBlock
                    {
                        Release = release
                    },
                    new EmbedBlockLink
                    {
                        EmbedBlock = new EmbedBlock(),
                        Release = release
                    }
                },
                Release = release
            };

            var contentSection2 = new ContentSection
            {
                Order = 2,
                Content = new List<ContentBlock>(),
                Release = release
            };

            var contentSection3 = new ContentSection
            {
                Order = 3,
                Content = new List<ContentBlock>(),
                Release = release
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(release);
                await contentDbContext.ContentSections.AddRangeAsync(
                    contentSectionToRemove, contentSection2, contentSection3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentSection(
                    contentSectionToRemove.ReleaseId,
                    contentSectionToRemove.Id);

                var contentSectionList = result.AssertRight();

                Assert.Equal(2, contentSectionList.Count);

                Assert.Equal(contentSection2.Id, contentSectionList[0].Id);
                Assert.Equal(1, contentSectionList[0].Order);
                Assert.Equal(contentSection3.Id, contentSectionList[1].Id);
                Assert.Equal(2, contentSectionList[1].Order);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var contentSections = contentDbContext.ContentSections.ToList();
                Assert.Equal(2, contentSections.Count);

                Assert.Equal(contentSection2.Id, contentSections[0].Id);
                Assert.Equal(contentSection2.ReleaseId, contentSections[0].ReleaseId);
                Assert.Equal(1, contentSections[0].Order);

                Assert.Equal(contentSection3.Id, contentSections[1].Id);
                Assert.Equal(contentSection3.ReleaseId, contentSections[1].ReleaseId);
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
            var contentSection = new ContentSection
            {
                Content = new List<ContentBlock>
                {
                    new HtmlBlock(),
                },
                Release = new Release()
            };
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ContentSections.AddAsync(contentSection);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentSection(
                    Guid.NewGuid(),
                    contentSection.Id);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task RemoveContentSection_ContentSectionBelongsToAnotherRelease()
        {
            var release = new Release();
            var otherRelease = new Release();
            var relatedContentSection = new ContentSection
            {
                Content = new List<ContentBlock>
                {
                    new HtmlBlock
                    {
                        Release = release
                    }
                },
                Release = release
            };
            var unrelatedContentSection = new ContentSection
            {
                Id = Guid.NewGuid(),
                Content = new List<ContentBlock>
                {
                    new HtmlBlock
                    {
                        Release = otherRelease
                    },
                },
                Release = otherRelease
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ContentSections.AddRangeAsync(
                    relatedContentSection, unrelatedContentSection);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentSection(
                    release.Id,
                    unrelatedContentSection.Id);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task RemoveContentBlock()
        {
            var contentSectionId = Guid.NewGuid();
            var contentBlockId = Guid.NewGuid();
            var contentSection = new ContentSection
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
                Release = new Release()
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ContentSections.AddRangeAsync(contentSection);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentBlock(
                    contentSection.ReleaseId,
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
            var contentSection = new ContentSection
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
                Release = new Release()
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ContentSections.AddRangeAsync(contentSection);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentBlock(
                    Guid.NewGuid(),
                    contentSection.Id,
                    contentBlockId);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task RemoveContentBlock_NoContentSection()
        {
            var release = new Release();
            var contentBlockId = Guid.NewGuid();
            var contentSection = new ContentSection
            {
                Content = new List<ContentBlock>
                {
                    new HtmlBlock
                    {
                        Id = contentBlockId,
                        Release = release
                    },
                    new DataBlock
                    {
                        Release = release
                    },
                    new HtmlBlock
                    {
                        Release = release
                    }
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ContentSections.AddRangeAsync(contentSection);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentBlock(
                    contentSection.ReleaseId,
                    Guid.NewGuid(),
                    contentBlockId);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task RemoveContentBlock_NoContentBlock()
        {
            var contentBlockId = Guid.NewGuid();
            var contentSection = new ContentSection
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
                Release = new Release()
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ContentSections.AddRangeAsync(contentSection);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentBlock(
                    contentSection.ReleaseId,
                    contentSection.Id,
                    Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task RemoveContentBlock_BlockAttachedToIncorrectSection()
        {
            var release = new Release();

            var incorrectContentBlockId = Guid.NewGuid();
            var incorrectContentSection = new ContentSection
            {
                Content = new List<ContentBlock>
                {
                    new HtmlBlock { Id = incorrectContentBlockId },
                },
                Release = release
            };

            var contentSection = new ContentSection
            {
                Content = new List<ContentBlock>
                {
                    new HtmlBlock(),
                },
                Release = release
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.ContentSections.AddRangeAsync(
                    contentSection, incorrectContentSection);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupContentService(contentDbContext: contentDbContext);
                var result = await service.RemoveContentBlock(
                    contentSection.ReleaseId,
                    contentSection.Id,
                    incorrectContentBlockId);

                result.AssertNotFound();
            }
        }

        private static ContentService SetupContentService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IKeyStatisticService keyStatisticService = null,
            IContentSectionRepository contentSectionRepository = null,
            IContentBlockService contentBlockService = null,
            IHubContext<ReleaseContentHub, IReleaseContentHubClient> hubContext = null,
            IUserService userService = null)
        {
            return new ContentService(
                contentDbContext,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                keyStatisticService ?? Mock.Of<IKeyStatisticService>(MockBehavior.Strict),
                contentSectionRepository ?? new ContentSectionRepository(contentDbContext),
                contentBlockService ?? new ContentBlockService(contentDbContext),
                hubContext ?? Mock.Of<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(MockBehavior.Strict),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                AdminMapper()
            );
        }
    }
}
