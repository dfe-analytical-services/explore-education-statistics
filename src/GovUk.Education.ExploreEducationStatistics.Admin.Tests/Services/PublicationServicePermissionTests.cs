#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using IPublicationRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationServicePermissionTests
    {
        private readonly Topic _topic = new()
        {
            Id = Guid.NewGuid(),
            Title = "Test topic"
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
                .ExpectCheckToFail(SecurityPolicies.CanAccessSystem)
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
            context.Add(_topic);
            await context.SaveChangesAsync();

            var userService = AlwaysTrueUserService();
            var publicationService = BuildPublicationService(
                userService: userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service =>
                    await service.CreatePublication(new PublicationCreateRequest
                    {
                        TopicId = _topic.Id,
                    }),
                _topic,
                userService,
                publicationService,
                SecurityPolicies.CanCreatePublicationForSpecificTopic);
        }

        [Fact]
        public async Task UpdatePublication_CanUpdatePublicationTitles()
        {
            await using var context = InMemoryApplicationDbContext();
            context.Add(_topic);
            context.Add(_publication);
            await context.SaveChangesAsync();

            var userService = AlwaysTrueUserService();
            var publicationService = BuildPublicationService(
                userService: userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service =>
                    await service.UpdatePublication(_publication.Id, new PublicationSaveRequest
                    {
                        TopicId = _topic.Id,
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
            context.Add(_topic);
            context.Add(_publication);
            await context.SaveChangesAsync();

            var userService = AlwaysTrueUserService();
            var publicationService = BuildPublicationService(
                userService: userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service =>
                    await service.UpdatePublication(_publication.Id, new PublicationSaveRequest
                    {
                        TopicId = _topic.Id,
                        Title = "Updated publication",
                    }),
                _publication,
                userService,
                publicationService,
                SecurityPolicies.CanUpdateSpecificPublicationSummary);
        }

        [Fact]
        public async Task UpdatePublication_CanCreatePublicationForSpecificTopic()
        {
            await using var context = InMemoryApplicationDbContext();
            context.Add(_topic);
            context.Add(_publication);
            await context.SaveChangesAsync();

            var userService = AlwaysTrueUserService();
            var publicationService = BuildPublicationService(
                userService: userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service =>
                    await service.UpdatePublication(_publication.Id, new PublicationSaveRequest
                    {
                        TopicId = _topic.Id,
                        Title = "Updated publication",
                    }),
                _topic,
                userService,
                publicationService,
                SecurityPolicies.CanCreatePublicationForSpecificTopic);
        }

        [Fact]
        public async Task UpdatePublication_NoPermissionToChangeSummary()
        {
            var publication = new Publication
            {
                Title = "Old publication title",
                Slug = "publication-slug",
                Topic = new Topic { Title = "Old topic title" },
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
                Topic = new Topic { Title = "Old topic title" },
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
        public async Task UpdatePublication_NoPermissionToChangeTopic()
        {
            var newTopic = new Topic
            {
                Title = "New topic title"
            };
            var publication = new Publication
            {
                Title = "Publication title",
                Slug = "publication-slug",
                Topic = new Topic { Title = "Old topic title" },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication, newTopic);
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
                        It.Is<Topic>(t => t.Id == newTopic.Id),
                        SecurityPolicies.CanCreatePublicationForSpecificTopic))
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
                        TopicId = newTopic.Id,
                    }
                );

                result.AssertForbidden();
            }
        }

        [Fact]
        public async Task UpdatePublication_NoPermissionToChangeSupersededById()
        {
            var newTopic = new Topic
            {
                Title = "New topic title"
            };
            var publication = new Publication
            {
                Title = "Publication title",
                Slug = "publication-slug",
                Topic = new Topic { Title = "Old topic title" },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication, newTopic);
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
                        It.Is<Topic>(t => t.Id == newTopic.Id),
                        SecurityPolicies.CanCreatePublicationForSpecificTopic))
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
                        TopicId = publication.Topic.Id,
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
                .SetupResourceCheckToFail(publication, SecurityPolicies.CanManageExternalMethodologyForSpecificPublication)
                .AssertForbidden(async userService =>
                {
                    var contentDbContext = InMemoryApplicationDbContext();
                    await contentDbContext.Publications.AddAsync(publication);
                    await contentDbContext.SaveChangesAsync();

                    var service = BuildPublicationService(
                        context: contentDbContext,
                        userService: userService.Object);
                    return await service.UpdateExternalMethodology(publication.Id, new ExternalMethodologySaveRequest());
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
                .SetupResourceCheckToFail(publication, SecurityPolicies.CanManageExternalMethodologyForSpecificPublication)
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
                    return await service.UpdateContact(publication.Id, new Contact());
                });
        }

        [Fact]
        public void ListActiveReleases()
        {
            var userService = AlwaysTrueUserService();
            var publicationService = BuildPublicationService(
                userService: userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service => await service.ListActiveReleases(_publication.Id),
                _publication,
                userService,
                publicationService,
                SecurityPolicies.CanViewSpecificPublication);
        }

        [Fact]
        public void PartialUpdateLegacyReleases()
        {
            var userService = AlwaysTrueUserService();
            var publicationService = BuildPublicationService(
                userService: userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(
                async service => await service.PartialUpdateLegacyReleases(_publication.Id, new List<LegacyReleasePartialUpdateViewModel>()),
                _publication,
                userService,
                publicationService,
                SecurityPolicies.CanManageLegacyReleases);
        }

        private static PublicationService BuildPublicationService(
            ContentDbContext? context = null,
            IUserService? userService = null,
            IPublicationRepository? publicationRepository = null,
            IMethodologyVersionRepository? methodologyVersionRepository = null,
            IPublicationCacheService? publicationCacheService = null,
            IMethodologyCacheService? methodologyCacheService = null)
        {
            context ??= Mock.Of<ContentDbContext>();

            return new(
                context,
                AdminMapper(),
                new PersistenceHelper<ContentDbContext>(context),
                userService ?? AlwaysTrueUserService().Object,
                publicationRepository ?? Mock.Of<IPublicationRepository>(Strict),
                methodologyVersionRepository ?? Mock.Of<IMethodologyVersionRepository>(Strict),
                publicationCacheService ?? Mock.Of<IPublicationCacheService>(Strict),
                methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict));
        }
    }
}
