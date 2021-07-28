using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
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
                Content = AsList(
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
                Content = AsList(
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 1,
                            Content = AsList<ContentBlock>(
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
                            Content = AsList<ContentBlock>(
                                new HtmlBlock
                                {
                                    Created = new DateTime(2003, 3, 3)
                                },
                                new HtmlBlock
                                {
                                    Created = new DateTime(2004, 4, 4)
                                },
                                new DataBlock()
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
        public async Task GetContentSections()
        {
            var release = new Release
            {
                Content = AsList(
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 1,
                            Content = AsList<ContentBlock>(
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
                            Content = AsList<ContentBlock>(
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
        public async Task GetContentSections_NoContentBlocks()
        {
            var release = new Release
            {
                Content = AsList(
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Heading = "New section",
                            Order = 1,
                            Content = AsList<ContentBlock>()
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

        private static ContentService SetupContentService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IReleaseContentSectionRepository releaseContentSectionRepository = null,
            IUserService userService = null)
        {
            return new ContentService(
                contentDbContext,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                releaseContentSectionRepository ?? new ReleaseContentSectionRepository(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                AdminMapper()
            );
        }
    }
}
