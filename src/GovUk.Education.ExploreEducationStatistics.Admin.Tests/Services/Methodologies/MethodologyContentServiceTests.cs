#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyContentServiceTests
    {
        [Fact]
        public async Task GetContentBlocks_NoContentSections()
        {
            var methodologyVersion = new MethodologyVersion();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyContentService(contentDbContext: contentDbContext);

                var result = await service.GetContentBlocks<HtmlBlock>(methodologyVersion.Id);

                Assert.True(result.IsRight);

                Assert.Empty(result.Right);
            }
        }

        [Fact]
        public async Task GetContentBlocks_NoContentBlocks()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Annexes = new List<ContentSection>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Heading = "New section",
                        Order = 1
                    }
                },
                Content = new List<ContentSection>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Heading = "New section",
                        Order = 2
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyContentService(contentDbContext: contentDbContext);

                var result = await service.GetContentBlocks<HtmlBlock>(methodologyVersion.Id);

                Assert.True(result.IsRight);

                Assert.Empty(result.Right);
            }
        }

        [Fact]
        public async Task GetContentBlocks()
        {
            var annexHtmlBlock1 = new HtmlBlock
            {
                Id = Guid.NewGuid()
            };

            var annexHtmlBlock2 = new HtmlBlock
            {
                Id = Guid.NewGuid()
            };

            var contentHtmlBlock1 = new HtmlBlock
            {
                Id = Guid.NewGuid()
            };

            var contentHtmlBlock2 = new HtmlBlock
            {
                Id = Guid.NewGuid()
            };

            var methodologyVersion = new MethodologyVersion
            {
                Annexes = new List<ContentSection>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Heading = "New section",
                        Order = 1,
                        Content = new List<ContentBlock>
                        {
                            annexHtmlBlock1,
                            new DataBlock
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Heading = "New section",
                        Order = 2,
                        Content = new List<ContentBlock>
                        {
                            annexHtmlBlock2,
                            new DataBlock
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    }
                },
                Content = new List<ContentSection>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Heading = "New section",
                        Order = 1,
                        Content = new List<ContentBlock>
                        {
                            contentHtmlBlock1,
                            new DataBlock
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Heading = "New section",
                        Order = 2,
                        Content = new List<ContentBlock>
                        {
                            contentHtmlBlock2,
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
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyContentService(contentDbContext: contentDbContext);

                var result = await service.GetContentBlocks<HtmlBlock>(methodologyVersion.Id);

                Assert.True(result.IsRight);

                var contentBlocks = result.Right;

                Assert.Equal(4, contentBlocks.Count);
                Assert.Equal(annexHtmlBlock1.Id, contentBlocks[0].Id);
                Assert.Equal(annexHtmlBlock2.Id, contentBlocks[1].Id);
                Assert.Equal(contentHtmlBlock1.Id, contentBlocks[2].Id);
                Assert.Equal(contentHtmlBlock2.Id, contentBlocks[3].Id);
            }
        }

        [Fact]
        public async Task AddContentSection_Draft()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Status = MethodologyStatus.Draft,
            };
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyContentService = SetupMethodologyContentService(contentDbContext);
                var result = await methodologyContentService.AddContentSection(
                    methodologyVersion.Id,
                    new ContentSectionAddRequest(),
                    MethodologyContentService.ContentListType.Content);

                Assert.True(result.IsRight);
                Assert.Equal(0, result.Right.Content.Count);
                Assert.Equal(0, result.Right.Order);
            }
        }

        [Fact]
        public async Task AddContentSection_Approved()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Status = MethodologyStatus.Approved,
            };
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyContentService = SetupMethodologyContentService(contentDbContext);
                var result = await methodologyContentService.AddContentSection(
                    methodologyVersion.Id,
                    new ContentSectionAddRequest(),
                    MethodologyContentService.ContentListType.Content);

                result.AssertBadRequest(MethodologyMustBeDraft);
            }
        }

        private static MethodologyContentService SetupMethodologyContentService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IUserService? userService = null)
        {
            return new(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                AdminMapper()
            );
        }
    }
}
