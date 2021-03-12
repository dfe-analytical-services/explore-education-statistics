using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyContentServiceTests
    {
        [Fact]
        public async Task GetContentBlocks_NoContentSections()
        {
            var methodology = new Methodology();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyContentService(contentDbContext: contentDbContext);

                var result = await service.GetContentBlocks<HtmlBlock>(methodology.Id);

                Assert.True(result.IsRight);

                Assert.Empty(result.Right);
            }
        }

        [Fact]
        public async Task GetContentBlocks_NoContentBlocks()
        {
            var methodology = new Methodology
            {
                Content = new List<ContentSection>
                {
                    new ContentSection
                    {
                        Id = Guid.NewGuid(),
                        Heading = "New section",
                        Order = 1
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyContentService(contentDbContext: contentDbContext);

                var result = await service.GetContentBlocks<HtmlBlock>(methodology.Id);

                Assert.True(result.IsRight);

                Assert.Empty(result.Right);
            }
        }

        [Fact]
        public async Task GetContentBlocks()
        {
            var htmlBlock1 = new HtmlBlock
            {
                Id = Guid.NewGuid()
            };

            var htmlBlock2 = new HtmlBlock
            {
                Id = Guid.NewGuid()
            };

            var methodology = new Methodology
            {
                Content = new List<ContentSection>
                {
                    new ContentSection
                    {
                        Id = Guid.NewGuid(),
                        Heading = "New section",
                        Order = 1,
                        Content = new List<ContentBlock>
                        {
                            htmlBlock1,
                            new DataBlock
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    },
                    new ContentSection
                    {
                        Id = Guid.NewGuid(),
                        Heading = "New section",
                        Order = 2,
                        Content = new List<ContentBlock>
                        {
                            htmlBlock2,
                            new DataBlock
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyContentService(contentDbContext: contentDbContext);

                var result = await service.GetContentBlocks<HtmlBlock>(methodology.Id);

                Assert.True(result.IsRight);

                var contentBlocks = result.Right;

                Assert.Equal(2, contentBlocks.Count);
                Assert.Equal(htmlBlock1.Id, contentBlocks[0].Id);
                Assert.Equal(htmlBlock2.Id, contentBlocks[1].Id);
            }
        }

        private static MethodologyContentService SetupMethodologyContentService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IUserService userService = null)
        {
            return new MethodologyContentService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                AdminMapper()
            );
        }
    }
}
