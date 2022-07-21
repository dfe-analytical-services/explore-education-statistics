#nullable enable
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
            var methodologyVersion = new MethodologyVersion
            {
                Methodology = new Methodology()
            };
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<MethodologyVersion>(m => m.Id == methodologyVersion.Id,
                        CanViewSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.GetContent(methodologyVersion.Id);
                        });
            }
        }
        
        [Fact]
        public async Task GetContentBlocks()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<MethodologyVersion>(m => m.Id == methodologyVersion.Id,
                        CanViewSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.GetContentBlocks<ContentBlock>(methodologyVersion.Id);
                        });
            }
        }

        [Fact]
        public async Task GetContentSection()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent {
                    Content = new List<ContentSection>
                    {
                        new()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<MethodologyVersion>(m => m.Id == methodologyVersion.Id,
                        CanViewSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.GetContentSection(methodologyVersion.Id,
                                methodologyVersion.MethodologyContent.Content.First().Id);
                        });
            }
        }
        
        [Fact]
        public async Task ReorderContentSections()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<MethodologyVersion>(m => m.Id == methodologyVersion.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.ReorderContentSections(methodologyVersion.Id,
                                new Dictionary<Guid, int>());
                        });
            }
        }

        [Fact]
        public async Task AddContentSection()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<MethodologyVersion>(m => m.Id == methodologyVersion.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.AddContentSection(
                                methodologyVersion.Id,
                                new ContentSectionAddRequest(),
                                MethodologyContentService.ContentListType.Content);
                        });
            }
        }

        [Fact]
        public async Task UpdateContentSectionHeading()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent {
                    Content = new List<ContentSection>
                    {
                        new()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<MethodologyVersion>(m => m.Id == methodologyVersion.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.UpdateContentSectionHeading(
                                methodologyVersion.Id,
                                methodologyVersion.MethodologyContent.Content.First().Id,
                                "New heading");
                        });
            }
        }

        [Fact]
        public async Task RemoveContentSection()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent {
                    Content = new List<ContentSection>
                    {
                        new()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<MethodologyVersion>(m => m.Id == methodologyVersion.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.RemoveContentSection(
                                methodologyVersion.Id,
                                methodologyVersion.MethodologyContent.Content.First().Id);
                        });
            }
        }
        
        [Fact]
        public async Task ReorderContentBlocks()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent {
                    Content = new List<ContentSection>
                    {
                        new()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<MethodologyVersion>(m => m.Id == methodologyVersion.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.ReorderContentBlocks(
                                methodologyVersion.Id,
                                methodologyVersion.MethodologyContent.Content.First().Id,
                                new Dictionary<Guid, int>()
                            );
                        });
            }
        }
        
        [Fact]
        public async Task AddContentBlock()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent {
                    Content = new List<ContentSection>
                    {
                        new()
                        {
                            Id = Guid.NewGuid()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<MethodologyVersion>(m => m.Id == methodologyVersion.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.AddContentBlock(
                                methodologyVersion.Id,
                                methodologyVersion.MethodologyContent.Content.First().Id,
                                new ContentBlockAddRequest()
                            );
                        });
            }
        }
        
        [Fact]
        public async Task RemoveContentBlock()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent {
                    Content = new List<ContentSection>
                    {
                        new()
                        {
                            Content = new List<ContentBlock>
                            {
                                new HtmlBlock()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<MethodologyVersion>(m => m.Id == methodologyVersion.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.RemoveContentBlock(
                                methodologyVersion.Id,
                                methodologyVersion.MethodologyContent.Content.First().Id,
                                methodologyVersion.MethodologyContent.Content.First().Content.First().Id
                            );
                        });
            }
        }
        
        [Fact]
        public async Task UpdateTextBasedContentBlock()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent {
                    Content = new List<ContentSection>
                    {
                        new()
                        {
                            Content = new List<ContentBlock>
                            {
                                new HtmlBlock()
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
                await PolicyCheckBuilder<SecurityPolicies>()
                    .SetupResourceCheckToFailWithMatcher<MethodologyVersion>(m => m.Id == methodologyVersion.Id,
                        CanUpdateSpecificMethodology)
                    .AssertForbidden(
                        userService =>
                        {
                            var methodologyContentService = SetupMethodologyContentService(contentDbContext, userService: userService.Object);

                            return methodologyContentService.UpdateTextBasedContentBlock(
                                methodologyVersion.Id,
                                methodologyVersion.MethodologyContent.Content.First().Id,
                                methodologyVersion.MethodologyContent.Content.First().Content.First().Id,
                                new ContentBlockUpdateRequest()
                            );
                        });
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
