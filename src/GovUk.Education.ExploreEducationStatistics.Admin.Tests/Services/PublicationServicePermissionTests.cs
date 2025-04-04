#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationServicePermissionTests
    {
        private readonly Theme _theme = new()
        {
            Id = Guid.NewGuid(),
            Title = "Test theme"
        };

        private readonly Publication _publication = new()
        {
            Id = Guid.NewGuid(),
            Title = "Test publication",
        };

        [Fact]
        public async Task ListPublications_NoAccessOfSystem()
        {
            await PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(SecurityPolicies.RegisteredUser)
                .AssertForbidden(async userService =>
                {
                    var service = BuildPublicationService(
                        userService: userService.Object);
                    return await service.ListPublications(Guid.NewGuid());
                });
        }

        [Fact]
        public async Task ListPublicationSummaries()
        {
            await PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(SecurityPolicies.CanViewAllPublications)
                .AssertForbidden(async userService =>
                {
                    var service = BuildPublicationService(
                        userService: userService.Object);
                    return await service.ListPublicationSummaries();
                });
        }

        [Fact]
        public async Task CreatePublication()
        {
            await using var context = InMemoryApplicationDbContext();
            context.Add(_theme);
            await context.SaveChangesAsync();

            var userService = AlwaysTrueUserService();
            var publicationService = BuildPublicationService(
                userService: userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service =>
                    await service.CreatePublication(new PublicationCreateRequest
                    {
                        ThemeId = _theme.Id,
                    }),
                _theme,
                userService,
                publicationService,
                SecurityPolicies.CanCreatePublicationForSpecificTheme);
        }

        [Fact]
        public async Task UpdatePublication_CanUpdatePublicationTitles()
        {
            await using var context = InMemoryApplicationDbContext();
            context.Add(_theme);
            context.Add(_publication);
            await context.SaveChangesAsync();

            var userService = AlwaysTrueUserService();
            var publicationService = BuildPublicationService(
                userService: userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service =>
                    await service.UpdatePublication(_publication.Id, new PublicationSaveRequest
                    {
                        ThemeId = _theme.Id,
                        Title = "Updated publication",
                    }),
                _publication,
                userService,
                publicationService,
                SecurityPolicies.CanUpdatePublication);
        }

        [Fact]
        public async Task UpdatePublication_CanUpdatePublication()
        {
            await using var context = InMemoryApplicationDbContext();
            context.Add(_theme);
            context.Add(_publication);
            await context.SaveChangesAsync();

            var userService = AlwaysTrueUserService();
            var publicationService = BuildPublicationService(
                userService: userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service =>
                    await service.UpdatePublication(_publication.Id, new PublicationSaveRequest
                    {
                        ThemeId = _theme.Id,
                        Title = "Updated publication",
                    }),
                _publication,
                userService,
                publicationService,
                SecurityPolicies.CanUpdateSpecificPublicationSummary);
        }

        [Fact]
        public async Task UpdatePublication_CanCreatePublicationForSpecificTheme()
        {
            await using var context = InMemoryApplicationDbContext();
            context.Add(_theme);
            context.Add(_publication);
            await context.SaveChangesAsync();

            var userService = AlwaysTrueUserService();
            var publicationService = BuildPublicationService(
                userService: userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service =>
                    await service.UpdatePublication(_publication.Id, new PublicationSaveRequest
                    {
                        ThemeId = _theme.Id,
                        Title = "Updated publication",
                    }),
                _theme,
                userService,
                publicationService,
                SecurityPolicies.CanCreatePublicationForSpecificTheme);
        }

        [Fact]
        public async Task UpdatePublication_NoPermissionToChangeSummary()
        {
            var publication = new Publication
            {
                Title = "Old publication title",
                Slug = "publication-slug",
                Theme = new() { Title = "Old theme title" },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication);
                await context.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(Strict);
            userService
                .Setup(s =>
                    s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        SecurityPolicies.CanUpdateSpecificPublicationSummary))
                .ReturnsAsync(true);
            userService
                .Setup(s =>
                    s.MatchesPolicy(SecurityPolicies.CanUpdatePublication))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context,
                    userService: userService.Object);

                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveRequest
                    {
                        Title = "New publication title",
                    }
                );

                VerifyAllMocks(userService);

                result.AssertForbidden();
            }
        }

        [Fact]
        public async Task UpdatePublication_NoPermissionToChangeTitle()
        {
            var publication = new Publication
            {
                Title = "Old publication title",
                Slug = "publication-slug",
                Theme = new() { Title = "Old theme title" },
                SupersededById = Guid.NewGuid(),
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication);
                await context.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(Strict);
            userService
                .Setup(s =>
                    s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        SecurityPolicies.CanUpdateSpecificPublicationSummary))
                .ReturnsAsync(true);
            userService
                .Setup(s =>
                    s.MatchesPolicy(SecurityPolicies.CanUpdatePublication))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context,
                    userService: userService.Object);

                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveRequest
                    {
                        Title = "New publication title",
                        Slug = "publication-slug",
                    }
                );

                VerifyAllMocks(userService);

                result.AssertForbidden();
            }
        }

        [Fact]
        public async Task UpdatePublication_NoPermissionToChangeTheme()
        {
            var newTheme = new Theme
            {
                Title = "New theme title"
            };
            var publication = new Publication
            {
                Title = "Publication title",
                Slug = "publication-slug",
                Theme = new() { Title = "Old theme title" },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication, newTheme);
                await context.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(Strict);

            userService
                .Setup(s =>
                    s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        SecurityPolicies.CanUpdateSpecificPublicationSummary))
                .ReturnsAsync(true);

            userService
                .Setup(s =>
                    s.MatchesPolicy(
                        It.Is<Theme>(t => t.Id == newTheme.Id),
                        SecurityPolicies.CanCreatePublicationForSpecificTheme))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context,
                    userService: userService.Object);

                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveRequest
                    {
                        Title = "Publication title",
                        ThemeId = newTheme.Id,
                    }
                );

                result.AssertForbidden();
            }
        }

        [Fact]
        public async Task UpdatePublication_NoPermissionToChangeSupersededById()
        {
            var newTheme = new Theme
            {
                Title = "New theme title"
            };
            var publication = new Publication
            {
                Title = "Publication title",
                Slug = "publication-slug",
                Theme = new() { Title = "Old theme title" },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication, newTheme);
                await context.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(Strict);

            userService
                .Setup(s =>
                    s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        SecurityPolicies.CanUpdateSpecificPublicationSummary))
                .ReturnsAsync(true);

            userService
                .Setup(s =>
                    s.MatchesPolicy(SecurityPolicies.CanUpdatePublication))
                .ReturnsAsync(false);

            userService
                .Setup(s =>
                    s.MatchesPolicy(
                        It.Is<Theme>(t => t.Id == newTheme.Id),
                        SecurityPolicies.CanCreatePublicationForSpecificTheme))
                .ReturnsAsync(false);

            userService
                .Setup(s =>
                    s.MatchesPolicy(
                        It.Is<Theme>(t => t.Id == newTheme.Id),
                        SecurityPolicies.CanCreatePublicationForSpecificTheme))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context,
                    userService: userService.Object);

                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveRequest
                    {
                        Title = "Publication title",
                        ThemeId = publication.Theme.Id,
                        SupersededById = Guid.NewGuid(),
                    }
                );

                result.AssertForbidden();
            }
        }

        [Fact]
        public void GetPublication()
        {
            var userService = AlwaysTrueUserService();
            var publicationService = BuildPublicationService(
                userService: userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service => await service.GetPublication(_publication.Id),
                _publication,
                userService,
                publicationService,
                SecurityPolicies.CanViewSpecificPublication);
        }

        [Fact]
        public async Task GetExternalMethodology()
        {
            var publication = new Publication
            {
                ExternalMethodology = new ExternalMethodology
                {
                    Title = "Test external methodology",
                    Url = "http://test.external.methodology",
                }
            };

            await PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(publication, SecurityPolicies.CanViewSpecificPublication)
                .AssertForbidden(async userService =>
                {
                    var contentDbContext = InMemoryApplicationDbContext();
                    await contentDbContext.Publications.AddAsync(publication);
                    await contentDbContext.SaveChangesAsync();

                    var service = BuildPublicationService(
                        context: contentDbContext,
                        userService: userService.Object);
                    return await service.GetExternalMethodology(publication.Id);
                });
        }

        [Fact]
        public async Task UpdateExternalMethodology()
        {
            var publication = new Publication
            {
                ExternalMethodology = new ExternalMethodology
                {
                    Title = "Test external methodology",
                    Url = "http://test.external.methodology",
                }
            };

            await PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(publication,
                    SecurityPolicies.CanManageExternalMethodologyForSpecificPublication)
                .AssertForbidden(async userService =>
                {
                    var contentDbContext = InMemoryApplicationDbContext();
                    await contentDbContext.Publications.AddAsync(publication);
                    await contentDbContext.SaveChangesAsync();

                    var service = BuildPublicationService(
                        context: contentDbContext,
                        userService: userService.Object);
                    return await service.UpdateExternalMethodology(publication.Id,
                        new ExternalMethodologySaveRequest());
                });
        }

        [Fact]
        public async Task RemoveExternalMethodology()
        {
            var publication = new Publication
            {
                ExternalMethodology = new ExternalMethodology
                {
                    Title = "Test external methodology",
                    Url = "http://test.external.methodology",
                }
            };

            await PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(publication,
                    SecurityPolicies.CanManageExternalMethodologyForSpecificPublication)
                .AssertForbidden(async userService =>
                {
                    var contentDbContext = InMemoryApplicationDbContext();
                    await contentDbContext.Publications.AddAsync(publication);
                    await contentDbContext.SaveChangesAsync();

                    var service = BuildPublicationService(
                        context: contentDbContext,
                        userService: userService.Object);
                    return await service.RemoveExternalMethodology(publication.Id);
                });
        }

        [Fact]
        public async Task GetContact()
        {
            var publication = new Publication
            {
                Contact = new Contact
                {
                    ContactName = "contact name",
                    ContactTelNo = "12345",
                    TeamName = "team name",
                    TeamEmail = "team@email.com",
                },
            };

            await PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(publication, SecurityPolicies.CanViewSpecificPublication)
                .AssertForbidden(async userService =>
                {
                    var contentDbContext = InMemoryApplicationDbContext();
                    await contentDbContext.Publications.AddAsync(publication);
                    await contentDbContext.SaveChangesAsync();

                    var service = BuildPublicationService(
                        context: contentDbContext,
                        userService: userService.Object);
                    return await service.GetContact(publication.Id);
                });
        }

        [Fact]
        public async Task UpdateContact()
        {
            var publication = new Publication
            {
                Contact = new Contact
                {
                    ContactName = "test",
                    ContactTelNo = "1234",
                    TeamEmail = "test@test.com",
                    TeamName = "test",
                },
            };

            await PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(publication, SecurityPolicies.CanUpdateContact)
                .AssertForbidden(async userService =>
                {
                    var contentDbContext = InMemoryApplicationDbContext();
                    await contentDbContext.Publications.AddAsync(publication);
                    await contentDbContext.SaveChangesAsync();

                    var service = BuildPublicationService(
                        context: contentDbContext,
                        userService: userService.Object);
                    return await service.UpdateContact(publication.Id, new ContactSaveRequest());
                });
        }

        [Fact]
        public void ListLatestReleaseVersions()
        {
            var userService = AlwaysTrueUserService();
            var publicationService = BuildPublicationService(
                userService: userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service => await service.ListLatestReleaseVersions(_publication.Id),
                _publication,
                userService,
                publicationService,
                SecurityPolicies.CanViewSpecificPublication);
        }

        [Fact]
        public async Task GetReleaseSeries()
        {
            var publication = new Publication();

            await PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(publication, SecurityPolicies.CanViewSpecificPublication)
                .AssertForbidden(async userService =>
                {
                    var contentDbContext = InMemoryApplicationDbContext();
                    await contentDbContext.Publications.AddAsync(publication);
                    await contentDbContext.SaveChangesAsync();

                    var service = BuildPublicationService(
                        context: contentDbContext,
                        userService: userService.Object);
                    return await service.GetReleaseSeries(publication.Id);
                });
        }

        [Fact]
        public async Task AddReleaseSeriesLegacyLinks()
        {
            var publication = new Publication();

            await PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(publication, SecurityPolicies.CanManagePublicationReleaseSeries)
                .AssertForbidden(async userService =>
                {
                    var contentDbContext = InMemoryApplicationDbContext();
                    await contentDbContext.Publications.AddAsync(publication);
                    await contentDbContext.SaveChangesAsync();

                    var service = BuildPublicationService(
                        context: contentDbContext,
                        userService: userService.Object);
                    return await service.AddReleaseSeriesLegacyLink(publication.Id,
                        new ReleaseSeriesLegacyLinkAddRequest
                        {
                            Description = "Test description",
                            Url = "https://test.url"
                        });
                });
        }

        [Fact]
        public async Task UpdateReleaseSeries()
        {
            var publication = new Publication();

            await PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(publication, SecurityPolicies.CanManagePublicationReleaseSeries)
                .AssertForbidden(async userService =>
                {
                    var contentDbContext = InMemoryApplicationDbContext();
                    await contentDbContext.Publications.AddAsync(publication);
                    await contentDbContext.SaveChangesAsync();

                    var service = BuildPublicationService(
                        context: contentDbContext,
                        userService: userService.Object);
                    return await service.UpdateReleaseSeries(publication.Id, new List<ReleaseSeriesItemUpdateRequest>());
                });
        }

        private static PublicationService BuildPublicationService(
            ContentDbContext? context = null,
            IUserService? userService = null,
            IPublicationRepository? publicationRepository = null,
            IReleaseVersionRepository? releaseVersionRepository = null,
            IMethodologyService? methodologyService = null,
            IPublicationCacheService? publicationCacheService = null,
            IReleaseCacheService? releaseCacheService = null,
            IMethodologyCacheService? methodologyCacheService = null,
            IRedirectsCacheService? redirectsCacheService = null,
            IAdminEventRaiserService? adminEventRaiserService = null)
        {
            context ??= Mock.Of<ContentDbContext>();

            return new(
                context,
                AdminMapper(),
                new PersistenceHelper<ContentDbContext>(context),
                userService ?? AlwaysTrueUserService().Object,
                publicationRepository ?? Mock.Of<IPublicationRepository>(Strict),
                releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(Strict),
                methodologyService ?? Mock.Of<IMethodologyService>(Strict),
                publicationCacheService ?? Mock.Of<IPublicationCacheService>(Strict),
                releaseCacheService ?? Mock.Of<IReleaseCacheService>(Strict),
                methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict),
                redirectsCacheService ?? Mock.Of<IRedirectsCacheService>(Strict),
                adminEventRaiserService ?? new AdminEventRaiserServiceMockBuilder().Build());
        }
    }
}
