using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyContentServicePermissionTests
    {
        [Fact]
        public async Task GetContent()
        {
            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent()
            };
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<Methodology>(m => m.Id == methodology.Id,
                        CanViewSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.GetContent(methodology.Id);
                        });
            }
        }
        
        [Fact]
        public async Task GetContentBlocks()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<Methodology>(m => m.Id == methodology.Id,
                        CanViewSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.GetContentBlocks<ContentBlock>(methodology.Id);
                        });
            }
        }
        
        [Fact]
        public async Task GetContentSections()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<Methodology>(m => m.Id == methodology.Id,
                        CanViewSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.GetContentSections(methodology.Id,
                                MethodologyContentService.ContentListType.Content);
                        });
            }
        }
        
        [Fact]
        public async Task GetContentSection()
        {
            var methodology = new Methodology
            {
                Content = new List<ContentSection>
                {
                    new ContentSection()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<Methodology>(m => m.Id == methodology.Id,
                        CanViewSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.GetContentSection(methodology.Id,
                                methodology.Content.First().Id);
                        });
            }
        }
        
        [Fact]
        public async Task ReorderContentSections()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<Methodology>(m => m.Id == methodology.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.ReorderContentSections(methodology.Id,
                                new Dictionary<Guid, int>());
                        });
            }
        }

        [Fact]
        public async Task AddContentSection()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<Methodology>(m => m.Id == methodology.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.AddContentSection(
                                methodology.Id,
                                new ContentSectionAddRequest(),
                                MethodologyContentService.ContentListType.Content);
                        });
            }
        }

        [Fact]
        public async Task UpdateContentSectionHeading()
        {
            var methodology = new Methodology
            {
                Content = new List<ContentSection>
                {
                    new ContentSection()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<Methodology>(m => m.Id == methodology.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.UpdateContentSectionHeading(
                                methodology.Id,
                                methodology.Content.First().Id,
                                "New heading");
                        });
            }
        }

        [Fact]
        public async Task RemoveContentSection()
        {
            var methodology = new Methodology
            {
                Content = new List<ContentSection>
                {
                    new ContentSection()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<Methodology>(m => m.Id == methodology.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.RemoveContentSection(
                                methodology.Id,
                                methodology.Content.First().Id);
                        });
            }
        }
        
        [Fact]
        public async Task ReorderContentBlocks()
        {
            var methodology = new Methodology
            {
                Content = new List<ContentSection>
                {
                    new ContentSection()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<Methodology>(m => m.Id == methodology.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.ReorderContentBlocks(
                                methodology.Id,
                                methodology.Content.First().Id,
                                new Dictionary<Guid, int>()
                            );
                        });
            }
        }
        
        [Fact]
        public async Task AddContentBlock()
        {
            var methodology = new Methodology
            {
                Content = new List<ContentSection>
                {
                    new ContentSection
                    {
                        Id = Guid.NewGuid()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<Methodology>(m => m.Id == methodology.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.AddContentBlock(
                                methodology.Id,
                                methodology.Content.First().Id,
                                new ContentBlockAddRequest()
                            );
                        });
            }
        }
        
        [Fact]
        public async Task RemoveContentBlock()
        {
            var methodology = new Methodology
            {
                Content = new List<ContentSection>
                {
                    new ContentSection
                    {
                        Content = new List<ContentBlock>
                        {
                            new HtmlBlock()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<Methodology>(m => m.Id == methodology.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.RemoveContentBlock(
                                methodology.Id,
                                methodology.Content.First().Id,
                                methodology.Content.First().Content.First().Id
                            );
                        });
            }
        }
        
        [Fact]
        public async Task UpdateTextBasedContentBlock()
        {
            var methodology = new Methodology
            {
                Content = new List<ContentSection>
                {
                    new ContentSection
                    {
                        Content = new List<ContentBlock>
                        {
                            new HtmlBlock()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<Methodology>(m => m.Id == methodology.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.UpdateTextBasedContentBlock(
                                methodology.Id,
                                methodology.Content.First().Id,
                                methodology.Content.First().Content.First().Id,
                                new ContentBlockUpdateRequest()
                            );
                        });
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
