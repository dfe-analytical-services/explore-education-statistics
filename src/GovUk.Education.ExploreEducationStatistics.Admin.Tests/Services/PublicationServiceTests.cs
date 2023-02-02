#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using IPublicationRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationServiceTests
    {
        [Fact]
        public async Task ListPublications_CanViewAllPublications_Topic()
        {
            var topic = new Topic
            {
                Title = "Topic title",
                Theme = new Theme
                {
                    Title = "Theme title",
                },
            };

            var publication1 = new Publication
            {
                Title = "Test Publication",
                Summary = "Test summary",
                Slug = "test-slug",
                Topic = topic,
                Contact = new Contact
                {
                    ContactName = "contact name",
                    ContactTelNo = "1234",
                    TeamName = "team name",
                    TeamEmail = "team@email",
                },
                SupersededBy = null,
            };

            var publication2 = new Publication
            {
                Topic = new Topic(),
            };

            var publication3 = new Publication
            {
                Topic = new Topic(),
            };

            var userService = new Mock<IUserService>(Strict);

            userService.Setup(s => s.GetUserId()).Returns(new Guid());
            userService.Setup(s => s.MatchesPolicy(CanAccessSystem)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanViewAllPublications)).ReturnsAsync(true);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication1, publication2, publication3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext,
                    userService: userService.Object);

                var result = await publicationService
                    .ListPublications(publication1.Topic.Id);

                var publicationViewModelList = result.AssertRight();

                var publicationViewModel = Assert.Single(publicationViewModelList);

                Assert.Equal(publication1.Id, publicationViewModel.Id);
                Assert.Equal(publication1.Title, publicationViewModel.Title);
                Assert.Equal(publication1.Summary, publicationViewModel.Summary);
                Assert.Equal(publication1.Slug, publicationViewModel.Slug);
                Assert.Equal(publication1.Topic.Id, publicationViewModel.Topic.Id);
                Assert.Equal(publication1.Topic.Title, publicationViewModel.Topic.Title);
                Assert.Equal(publication1.Topic.Theme.Id, publicationViewModel.Theme.Id);
                Assert.Equal(publication1.Topic.Theme.Title, publicationViewModel.Theme.Title);

                Assert.Null(publicationViewModel.SupersededById);
                Assert.False(publicationViewModel.IsSuperseded);

                Assert.Null(publicationViewModel.Permissions);
            }
        }

        [Fact]
        public async Task ListPublications_CanViewAllPublications_Order()
        {
            var topic = new Topic
            {
                Theme = new Theme(),
            };

            var publication1 = new Publication
            {
                Title = "A",
                Topic = topic,
            };

            var publication2 = new Publication
            {
                Title = "B",
                Topic = topic,
            };

            var publication3 = new Publication
            {
                Title = "C",
                Topic = topic,
            };

            var userService = new Mock<IUserService>(Strict);

            userService.Setup(s => s.GetUserId()).Returns(new Guid());
            userService.Setup(s => s.MatchesPolicy(CanAccessSystem)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanViewAllPublications)).ReturnsAsync(true);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(publication2, publication3, publication1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext,
                    userService: userService.Object);

                var result = await publicationService
                    .ListPublications(topic.Id);

                var publicationViewModelList = result.AssertRight();

                Assert.Equal(3, publicationViewModelList.Count);
                Assert.Equal(publication1.Id, publicationViewModelList[0].Id);
                Assert.Equal(publication1.Title, publicationViewModelList[0].Title);

                Assert.Equal(publication2.Id, publicationViewModelList[1].Id);
                Assert.Equal(publication2.Title, publicationViewModelList[1].Title);

                Assert.Equal(publication3.Id, publicationViewModelList[2].Id);
                Assert.Equal(publication3.Title, publicationViewModelList[2].Title);
            }
        }

        [Fact]
        public async Task ListPublications_CanViewAllPublications_NoTopic()
        {
            var publication1 = new Publication
            {
                Title = "publication1",
                Topic = new Topic { Theme = new Theme(), },
            };

            var publication2 = new Publication
            {
                Title = "publication2",
                Topic = new Topic { Theme = new Theme(), },
            };

            var publication3 = new Publication
            {
                Title = "publication3",
                Topic = new Topic { Theme = new Theme(), },
            };

            var userService = new Mock<IUserService>(Strict);

            userService.Setup(s => s.GetUserId()).Returns(new Guid());
            userService.Setup(s => s.MatchesPolicy(CanAccessSystem)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanViewAllPublications)).ReturnsAsync(true);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(publication1, publication2, publication3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext,
                    userService: userService.Object);

                var result = await publicationService
                    .ListPublications();

                var publicationViewModelList = result.AssertRight();

                Assert.Equal(3, publicationViewModelList.Count);
                Assert.Equal(publication1.Id, publicationViewModelList[0].Id);
                Assert.Equal(publication1.Title, publicationViewModelList[0].Title);
                Assert.Equal(publication1.TopicId, publicationViewModelList[0].Topic.Id);

                Assert.Equal(publication2.Id, publicationViewModelList[1].Id);
                Assert.Equal(publication2.Title, publicationViewModelList[1].Title);
                Assert.Equal(publication2.TopicId, publicationViewModelList[1].Topic.Id);

                Assert.Equal(publication3.Id, publicationViewModelList[2].Id);
                Assert.Equal(publication3.Title, publicationViewModelList[2].Title);
                Assert.Equal(publication3.TopicId, publicationViewModelList[2].Topic.Id);
            }
        }

        [Fact]
        public async Task ListPublications_CannotViewAllPublications()
        {
            var user = new User { Id = Guid.NewGuid(), };

            var topic = new Topic
            {
                Title = "Topic title",
                Theme = new Theme
                {
                    Title = "Theme title",
                },
            };

            var publication1 = new Publication
            {
                Title = "Test Publication",
                Summary = "Test summary",
                Slug = "test-slug",
                Topic = topic,
                SupersededBy = null,
            };

            var publication2 = new Publication
            {
                Topic = new Topic(),
            };

            var publication3 = new Publication
            {
                Topic = new Topic(),
            };

            var userService = new Mock<IUserService>(Strict);

            userService.Setup(s => s.GetUserId()).Returns(user.Id);
            userService.Setup(s => s.MatchesPolicy(CanAccessSystem)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanViewAllPublications)).ReturnsAsync(false);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication1, publication2, publication3);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(
                    new UserPublicationRole
                    {
                        User = user,
                        Publication = publication1,
                        Role = PublicationRole.Owner,
                    },
                    new UserPublicationRole
                    {
                        User = user,
                        Publication = publication2,
                        Role = PublicationRole.Owner,
                    },
                    new UserPublicationRole
                    {
                        User = user,
                        Publication = publication3,
                        Role = PublicationRole.Owner,
                    });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext,
                    userService: userService.Object);

                var result = await publicationService
                    .ListPublications(topic.Id);

                var publicationViewModelList = result.AssertRight();

                var publicationViewModel = Assert.Single(publicationViewModelList);

                Assert.Equal(publication1.Id, publicationViewModel.Id);
                Assert.Equal(publication1.Title, publicationViewModel.Title);
                Assert.Equal(publication1.Summary, publicationViewModel.Summary);
                Assert.Equal(publication1.Slug, publicationViewModel.Slug);
                Assert.Equal(publication1.Topic.Id, publicationViewModel.Topic.Id);
                Assert.Equal(publication1.Topic.Title, publicationViewModel.Topic.Title);
                Assert.Equal(publication1.Topic.Theme.Id, publicationViewModel.Theme.Id);
                Assert.Equal(publication1.Topic.Theme.Title, publicationViewModel.Theme.Title);

                Assert.Null(publicationViewModel.SupersededById);
                Assert.False(publicationViewModel.IsSuperseded);

                Assert.Null(publicationViewModel.Permissions);
            }
        }

        [Fact]
        public async Task ListPublications_CannotViewAllPublications_Order()
        {
            var user = new User { Id = Guid.NewGuid(), };

            var topic = new Topic { Theme = new Theme(), };

            var publication1 = new Publication
            {
                Title = "A",
                Topic = topic,
            };

            var publication2 = new Publication
            {
                Title = "B",
                Topic = topic,
            };

            var publication3 = new Publication
            {
                Title = "C",
                Topic = topic,
            };

            var userService = new Mock<IUserService>(Strict);

            userService.Setup(s => s.GetUserId()).Returns(user.Id);
            userService.Setup(s => s.MatchesPolicy(CanAccessSystem)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanViewAllPublications)).ReturnsAsync(false);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication2, publication3, publication1);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(
                    new UserPublicationRole
                    {
                        User = user,
                        Publication = publication1,
                        Role = PublicationRole.Owner,
                    },
                    new UserPublicationRole
                    {
                        User = user,
                        Publication = publication2,
                        Role = PublicationRole.Owner,
                    },
                    new UserPublicationRole
                    {
                        User = user,
                        Publication = publication3,
                        Role = PublicationRole.Owner,
                    });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext,
                    userService: userService.Object);

                var result = await publicationService
                    .ListPublications(topic.Id);

                var publicationViewModelList = result.AssertRight();

                Assert.Equal(3, publicationViewModelList.Count);

                Assert.Equal(publication1.Id, publicationViewModelList[0].Id);
                Assert.Equal(publication1.Title, publicationViewModelList[0].Title);

                Assert.Equal(publication2.Id, publicationViewModelList[1].Id);
                Assert.Equal(publication2.Title, publicationViewModelList[1].Title);

                Assert.Equal(publication3.Id, publicationViewModelList[2].Id);
                Assert.Equal(publication3.Title, publicationViewModelList[2].Title);
            }
        }

        [Fact]
        public async Task ListPublications_CannotViewAllPublications_NoTopic()
        {
            var user = new User { Id = Guid.NewGuid(), };

            var publication1 = new Publication
            {
                Title = "publication1",
                Topic = new Topic { Theme = new Theme(), },
            };

            var publication2 = new Publication
            {
                Title = "publication2",
                Topic = new Topic { Theme = new Theme(), },
            };

            var publication3 = new Publication
            {
                Title = "publication3",
                Topic = new Topic { Theme = new Theme(), },
            };

            var publication4 = new Publication
            {
                Title = "publication4",
                Topic = new Topic { Theme = new Theme(), },
            };

            var userService = new Mock<IUserService>(Strict);

            userService.Setup(s => s.GetUserId()).Returns(user.Id);
            userService.Setup(s => s.MatchesPolicy(CanAccessSystem)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanViewAllPublications)).ReturnsAsync(false);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(
                    publication1,
                    publication2,
                    publication3,
                    publication4);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(
                    new UserPublicationRole
                    {
                        User = user,
                        Publication = publication1,
                        Role = PublicationRole.Owner,
                    },
                    new UserPublicationRole
                    {
                        User = user,
                        Publication = publication2,
                        Role = PublicationRole.Owner,
                    },
                    new UserPublicationRole
                    {
                        User = user,
                        Publication = publication3,
                        Role = PublicationRole.Owner,
                    });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext,
                    userService: userService.Object);

                var result = await publicationService
                    .ListPublications();

                var publicationViewModelList = result.AssertRight();

                Assert.Equal(3, publicationViewModelList.Count);

                Assert.Equal(publication1.Id, publicationViewModelList[0].Id);
                Assert.Equal(publication1.Title, publicationViewModelList[0].Title);
                Assert.Equal(publication1.TopicId, publicationViewModelList[0].Topic.Id);

                Assert.Equal(publication2.Id, publicationViewModelList[1].Id);
                Assert.Equal(publication2.Title, publicationViewModelList[1].Title);
                Assert.Equal(publication2.TopicId, publicationViewModelList[1].Topic.Id);

                Assert.Equal(publication3.Id, publicationViewModelList[2].Id);
                Assert.Equal(publication3.Title, publicationViewModelList[2].Title);
                Assert.Equal(publication3.TopicId, publicationViewModelList[2].Topic.Id);
            }
        }

        [Fact]
        public async Task GetPublication()
        {
            var publication = new Publication
            {
                Title = "Test publication",
                Summary = "Test summary",
                Slug = "test-publication",
                Topic = new Topic
                {
                    Title = "Test topic",
                    Theme = new Theme
                    {
                        Title = "Test theme"
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context);

                var result = (await publicationService.GetPublication(publication.Id)).AssertRight();

                Assert.Equal(publication.Id, result.Id);
                Assert.Equal(publication.Title, result.Title);
                Assert.Equal(publication.Summary, result.Summary);
                Assert.Equal(publication.Slug, result.Slug);

                Assert.Equal(publication.Topic.Id, result.Topic.Id);
                Assert.Equal(publication.Topic.Title, result.Topic.Title);

                Assert.Equal(publication.Topic.ThemeId, result.Theme.Id);
                Assert.Equal(publication.Topic.Theme.Title, result.Theme.Title);

                Assert.Null(result.SupersededById);
                Assert.False(result.IsSuperseded);

                Assert.Null(result.Permissions);
            }
        }

        [Fact]
        public async Task GetPublication_Permissions()
        {
            var publication = new Publication
            {
                Topic = new Topic
                {
                    Theme = new Theme(),
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var userService = new Mock<IUserService>(Strict);

                userService.Setup(s => s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        CanViewSpecificPublication))
                    .ReturnsAsync(true);
                userService.Setup(s => s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        CanUpdateSpecificPublicationSummary))
                    .ReturnsAsync(true);
                userService.Setup(s => s.MatchesPolicy(CanUpdatePublication))
                    .ReturnsAsync(true);
                userService.Setup(s => s.MatchesPolicy(CanUpdateContact))
                    .ReturnsAsync(true);
                userService.Setup(s => s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        CanCreateReleaseForSpecificPublication))
                    .ReturnsAsync(false);
                userService.Setup(s => s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        CanAdoptMethodologyForSpecificPublication))
                    .ReturnsAsync(false);
                userService.Setup(s => s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        CanCreateMethodologyForSpecificPublication))
                    .ReturnsAsync(false);
                userService.Setup(s => s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        CanManageExternalMethodologyForSpecificPublication))
                    .ReturnsAsync(false);
                userService.Setup(s => s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        CanManageLegacyReleases))
                    .ReturnsAsync(true);
                userService.Setup(s => s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        CanUpdateContact))
                    .ReturnsAsync(true);
                userService.Setup(s => s.MatchesPolicy(
                        It.Is<Tuple<Publication, ReleaseRole>>(tuple =>
                            tuple.Item1.Id == publication.Id && tuple.Item2 == ReleaseRole.Contributor),
                        CanUpdateSpecificReleaseRole))
                    .ReturnsAsync(false);
                userService.Setup(s => s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        CanViewReleaseTeamAccess))
                    .ReturnsAsync(false);

                var publicationService = BuildPublicationService(context,
                    userService: userService.Object);

                var result = (await publicationService.GetPublication(publication.Id, includePermissions: true)).AssertRight();

                Assert.Equal(publication.Id, result.Id);

                Assert.NotNull(result.Permissions);
                Assert.True(result.Permissions!.CanUpdatePublication);
                Assert.True(result.Permissions.CanUpdatePublicationSummary);
                Assert.False(result.Permissions.CanCreateReleases);
                Assert.False(result.Permissions.CanAdoptMethodologies);
                Assert.False(result.Permissions.CanCreateMethodologies);
                Assert.False(result.Permissions.CanManageExternalMethodology);
                Assert.True(result.Permissions.CanManageLegacyReleases);
                Assert.True(result.Permissions.CanUpdateContact);
                Assert.False(result.Permissions.CanUpdateContributorReleaseRole);
            }
        }

        [Fact]
        public async Task GetPublication_IsSuperseded()
        {
            var publication = new Publication
            {
                Title = "Test publication",
                Topic = new Topic
                {
                    Theme = new Theme(),
                },
                SupersededBy = new Publication
                {
                    LatestPublishedReleaseId = Guid.NewGuid()
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context);

                var result = (await publicationService.GetPublication(publication.Id, includePermissions: true))
                    .AssertRight();

                Assert.Equal(publication.Id, result.Id);

                Assert.Equal(publication.SupersededById, result.SupersededById);
                Assert.True(result.IsSuperseded);
            }
        }

        [Fact]
        public async Task GetPublication_IsSuperseded_False()
        {
            var publication = new Publication
            {
                Title = "Test publication",
                Topic = new Topic
                {
                    Theme = new Theme(),
                },
                SupersededBy = new Publication
                {
                    // Superseding publication doesn't have a published release
                    LatestPublishedReleaseId = null
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context);

                var result = (await publicationService.GetPublication(publication.Id, includePermissions: true)).AssertRight();

                Assert.Equal(publication.Id, result.Id);

                Assert.Equal(publication.SupersededById, result.SupersededById);
                Assert.False(result.IsSuperseded);
            }
        }

        [Fact]
        public async Task GetPublication_NotFound()
        {
            await using var context = InMemoryApplicationDbContext();

            var publicationService = BuildPublicationService(context);

            var result = await publicationService.GetPublication(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task ListPublicationSummaries()
        {
            var publication1 = new Publication
            {
                Title = "Test Publication 1"
            };

            var publication2 = new Publication
            {
                Title = "Test Publication 2"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication1, publication2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildPublicationService(contentDbContext);

                var result = await service.ListPublicationSummaries();

                var publicationViewModels = result.AssertRight();
                Assert.Equal(2, publicationViewModels.Count);

                Assert.Equal(publication1.Id, publicationViewModels[0].Id);
                Assert.Equal(publication1.Title, publicationViewModels[0].Title);

                Assert.Equal(publication2.Id, publicationViewModels[1].Id);
                Assert.Equal(publication2.Title, publicationViewModels[1].Title);
            }
        }

        [Fact]
        public async Task ListPublicationSummaries_NoPublications()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var service = BuildPublicationService(contentDbContext);

            var result = await service.ListPublicationSummaries();

            var publicationViewModels = result.AssertRight();
            Assert.Empty(publicationViewModels);
        }

        [Fact]
        public async Task CreatePublication()
        {
            var topic = new Topic
            {
                Title = "Test topic",
                Theme = new Theme
                {
                    Title = "Test theme",
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(topic);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context);

                // Service method under test
                var result = await publicationService.CreatePublication(
                    new PublicationCreateRequest
                    {
                        Title = "Test publication",
                        Summary = "Test summary",
                        Contact = new ContactSaveViewModel
                        {
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789",
                            TeamName = "Test team",
                            TeamEmail = "john.smith@test.com",
                        },
                        TopicId = topic.Id
                    }
                );

                var publicationViewModel = result.AssertRight();
                Assert.Equal("Test publication", publicationViewModel.Title);
                Assert.Equal("Test summary", publicationViewModel.Summary);

                Assert.Equal("John Smith", publicationViewModel.Contact.ContactName);
                Assert.Equal("0123456789", publicationViewModel.Contact.ContactTelNo);
                Assert.Equal("Test team", publicationViewModel.Contact.TeamName);
                Assert.Equal("john.smith@test.com", publicationViewModel.Contact.TeamEmail);

                Assert.Equal(topic.Id, publicationViewModel.Topic.Id);
                Assert.Equal(topic.Title, publicationViewModel.Topic.Title);

                Assert.Equal(topic.Theme.Id, publicationViewModel.Theme.Id);
                Assert.Equal(topic.Theme.Title, publicationViewModel.Theme.Title);

                // Do an in depth check of the saved release
                var createdPublication = await context.Publications.FindAsync(publicationViewModel.Id);

                Assert.NotNull(createdPublication);
                Assert.False(createdPublication!.Live);
                Assert.Equal("test-publication", createdPublication.Slug);
                Assert.False(createdPublication.Updated.HasValue);
                Assert.Equal("Test publication", createdPublication.Title);
                Assert.Equal("Test summary", createdPublication.Summary);

                Assert.Equal("John Smith", createdPublication.Contact.ContactName);
                Assert.Equal("0123456789", createdPublication.Contact.ContactTelNo);
                Assert.Equal("Test team", createdPublication.Contact.TeamName);
                Assert.Equal("john.smith@test.com", createdPublication.Contact.TeamEmail);

                Assert.Equal(topic.Id, createdPublication.TopicId);
                Assert.Equal("Test topic", createdPublication.Topic.Title);
            }
        }

        [Fact]
        public async Task CreatePublication_FailsWithNonExistingTopic()
        {
            await using var context = InMemoryApplicationDbContext();

            var publicationService = BuildPublicationService(context);

            // Service method under test
            var result = await publicationService.CreatePublication(
                new PublicationCreateRequest
                {
                    Title = "Test publication",
                    TopicId = Guid.NewGuid()
                });

            result.AssertBadRequest(TopicDoesNotExist);
        }

        [Fact]
        public async Task CreatePublication_FailsWithNonUniqueSlug()
        {
            var topic = new Topic
            {
                Title = "Test topic"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(topic);
                context.Add(
                    new Publication
                    {
                        Title = "Test publication",
                        Slug = "test-publication"
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context);

                // Service method under test
                var result = await publicationService.CreatePublication(
                    new PublicationCreateRequest
                    {
                        Title = "Test publication",
                        TopicId = topic.Id
                    }
                );

                result.AssertBadRequest(SlugNotUnique);
            }
        }

        [Fact]
        public async Task UpdatePublication_NotPublished()
        {
            var topic = new Topic
            {
                Title = "New topic",
                Theme = new Theme(),
            };

            var publication = new Publication
            {
                Title = "Old title",
                Summary = "Old summary",
                Slug = "old-slug",
                Topic = new Topic
                {
                    Title = "Old topic"
                },
                Contact = new Contact
                {
                    ContactName = "Old name",
                    ContactTelNo = "0987654321",
                    TeamName = "Old team",
                    TeamEmail = "old.smith@test.com",
                },
                SupersededById = Guid.NewGuid(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(topic);
                context.Add(publication);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

                var publicationService = BuildPublicationService(context,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                methodologyVersionRepository
                    .Setup(s => s.PublicationTitleChanged(
                        publication.Id,
                        publication.Slug,
                        "New title",
                        "new-title"))
                    .Returns(Task.CompletedTask);

                var newSupersededById = Guid.NewGuid();

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveRequest
                    {
                        Title = "New title",
                        Summary = "New summary",
                        TopicId = topic.Id,
                        SupersededById = newSupersededById,
                    }
                );

                VerifyAllMocks(methodologyVersionRepository);

                var viewModel = result.AssertRight();

                Assert.Equal("New title", viewModel.Title);
                Assert.Equal("New summary", viewModel.Summary);

                Assert.Equal(topic.Id, viewModel.Topic.Id);
                Assert.Equal(topic.Title, viewModel.Topic.Title);

                Assert.Equal(newSupersededById, viewModel.SupersededById);

                // Do an in depth check of the saved release
                var updatedPublication = await context.Publications
                    .Include(p => p.Contact)
                    .SingleAsync(p => p.Id == viewModel.Id);

                Assert.False(updatedPublication.Live);
                Assert.True(updatedPublication.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedPublication.Updated!.Value).Milliseconds, 0, 1500);
                Assert.Equal("new-title", updatedPublication.Slug);
                Assert.Equal("New title", updatedPublication.Title);
                Assert.Equal(newSupersededById, updatedPublication.SupersededById);

                Assert.Equal("Old name", updatedPublication.Contact.ContactName);
                Assert.Equal("0987654321", updatedPublication.Contact.ContactTelNo);
                Assert.Equal("Old team", updatedPublication.Contact.TeamName);
                Assert.Equal("old.smith@test.com", updatedPublication.Contact.TeamEmail);


                Assert.Equal(topic.Id, updatedPublication.TopicId);
                Assert.Equal("New topic", updatedPublication.Topic.Title);
            }
        }

        [Fact]
        public async Task UpdatePublication_AlreadyPublished()
        {
            var topic = new Topic
            {
                Title = "New topic",
                Theme = new Theme(),
            };

            var supersedingPublicationToRemove = new Publication
            {
                Slug = "superseding-to-remove-slug",
            };

            var publication = new Publication
            {
                Slug = "old-title",
                Title = "Old title",
                Summary = "Old summary",
                Topic = new Topic
                {
                    Title = "Old topic"
                },
                Contact = new Contact
                {
                    ContactName = "Old name",
                    ContactTelNo = "0987654321",
                    TeamName = "Old team",
                    TeamEmail = "old.smith@test.com",
                },
                LatestPublishedRelease = new Release(),
                SupersededBy = supersedingPublicationToRemove,
            };

            var supersededPublication = new Publication
            {
                Slug = "superseded-slug",
                SupersededBy = publication,
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.AddRange(topic, publication, supersedingPublicationToRemove, supersededPublication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
                var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
                var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

                publicationCacheService.Setup(mock => mock.UpdatePublication(publication.Slug))
                    .ReturnsAsync(new PublicationCacheViewModel());

                publicationCacheService.Setup(mock => mock.UpdatePublication(supersededPublication.Slug))
                    .ReturnsAsync(new PublicationCacheViewModel());

                publicationCacheService.Setup(mock => mock.UpdatePublicationTree())
                    .ReturnsAsync(new List<PublicationTreeThemeViewModel>());

                methodologyCacheService.Setup(mock => mock.UpdateSummariesTree())
                    .ReturnsAsync(new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(
                        new List<AllMethodologiesThemeViewModel>()));

                var publicationService = BuildPublicationService(context,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    publicationCacheService: publicationCacheService.Object,
                    methodologyCacheService: methodologyCacheService.Object);

                // Expect the title to change but not the slug, as the Publication is already published.
                methodologyVersionRepository
                    .Setup(s => s.PublicationTitleChanged(
                        publication.Id,
                        publication.Slug,
                        "New title",
                        "old-title"))
                    .Returns(Task.CompletedTask);

                var newSupersededById = Guid.NewGuid();

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveRequest
                    {
                        Title = "New title",
                        Summary = "New summary",
                        TopicId = topic.Id,
                        SupersededById = newSupersededById,
                    }
                );

                VerifyAllMocks(methodologyVersionRepository,
                    methodologyCacheService,
                    publicationCacheService);

                var viewModel = result.AssertRight();

                Assert.Equal("New title", viewModel.Title);
                Assert.Equal("New summary", viewModel.Summary);

                Assert.Equal(topic.Id, viewModel.Topic.Id);
                Assert.Equal(topic.Title, viewModel.Topic.Title);

                Assert.Equal(newSupersededById, viewModel.SupersededById);

                // Do an in depth check of the saved release
                var updatedPublication = await context.Publications
                    .Include(p => p.Contact)
                    .SingleAsync(p => p.Id == viewModel.Id);

                Assert.True(updatedPublication.Live);
                Assert.True(updatedPublication.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedPublication.Updated!.Value).Milliseconds, 0, 1500);
                // Slug remains unchanged
                Assert.Equal("old-title", updatedPublication.Slug);
                Assert.Equal("New title", updatedPublication.Title);
                Assert.Equal("New summary", updatedPublication.Summary);

                Assert.Equal("Old name", updatedPublication.Contact.ContactName);
                Assert.Equal("0987654321", updatedPublication.Contact.ContactTelNo);
                Assert.Equal("Old team", updatedPublication.Contact.TeamName);
                Assert.Equal("old.smith@test.com", updatedPublication.Contact.TeamEmail);

                Assert.Equal(topic.Id, updatedPublication.TopicId);
                Assert.Equal("New topic", updatedPublication.Topic.Title);

                Assert.Equal(newSupersededById, updatedPublication.SupersededById);
            }
        }

        [Fact]
        public async void UpdatePublication_NoTitleOrSupersededByChange()
        {
            var topic = new Topic
            {
                Title = "New topic",
                Theme = new Theme(),
            };

            var publication = new Publication
            {
                Title = "Old title",
                Summary = "Old summary",
                Slug = "old-slug",
                Topic = new Topic
                {
                    Title = "Old topic"
                },
                Contact = new Contact
                {
                    ContactName = "Old name",
                    ContactTelNo = "0987654321",
                    TeamName = "Old team",
                    TeamEmail = "old.smith@test.com",
                },
                SupersededById = Guid.NewGuid(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(topic);
                context.Add(publication);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                // Expect no calls to be made on this Mock as the Publication's Title hasn't changed.
                var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

                var publicationService = BuildPublicationService(context,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveRequest
                    {
                        Title = "Old title",
                        Summary = "New summary",
                        TopicId = topic.Id,
                        SupersededById = publication.SupersededById,
                    }
                );

                VerifyAllMocks(methodologyVersionRepository);

                var viewModel = result.AssertRight();

                Assert.Equal("Old title", viewModel.Title);
                Assert.Equal("New summary", viewModel.Summary);
                Assert.Equal(publication.SupersededById, viewModel.SupersededById);
            }
        }

        [Fact]
        public async Task UpdatePublication_RemovesSupersededPublicationCacheBlobs()
        {
            var publication = new Publication
            {
                Title = "Test title",
                Slug = "test-slug",
                Topic = new Topic
                {
                    Title = "Test topic",
                    Theme = new Theme(),
                },
                LatestPublishedRelease = new Release()
            };

            var supersededPublication1 = new Publication
            {
                Title = "Superseded title 1",
                Slug = "superseded-slug-1",
                SupersededBy = publication,
            };

            var supersededPublication2 = new Publication
            {
                Title = "Superseded title 2",
                Slug = "superseded-slug-2",
                SupersededBy = publication,
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.AddRange(
                    publication,
                    supersededPublication1,
                    supersededPublication2);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
                var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

                publicationCacheService.Setup(mock =>
                        mock.UpdatePublication(publication.Slug))
                    .ReturnsAsync(new PublicationCacheViewModel());

                publicationCacheService.Setup(mock =>
                        mock.UpdatePublication(supersededPublication1.Slug))
                    .ReturnsAsync(new PublicationCacheViewModel());

                publicationCacheService.Setup(mock =>
                        mock.UpdatePublication(supersededPublication2.Slug))
                    .ReturnsAsync(new PublicationCacheViewModel());

                publicationCacheService.Setup(mock => mock.UpdatePublicationTree())
                    .ReturnsAsync(new List<PublicationTreeThemeViewModel>());

                methodologyCacheService.Setup(mock => mock.UpdateSummariesTree())
                    .ReturnsAsync(
                        new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(
                            new List<AllMethodologiesThemeViewModel>()));

                var publicationService = BuildPublicationService(context,
                    publicationCacheService: publicationCacheService.Object,
                    methodologyCacheService: methodologyCacheService.Object);

                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveRequest
                    {
                        Title = "Test title",
                        Slug = "test-slug",
                        TopicId = publication.TopicId,
                    }
                );

                VerifyAllMocks(methodologyCacheService,
                    publicationCacheService,
                    publicationCacheService);

                var viewModel = result.AssertRight();

                var updatedPublication = await context.Publications.FindAsync(viewModel.Id);
                Assert.NotNull(updatedPublication);
            }
        }

        [Fact]
        public async Task UpdatePublication_FailsWithNonExistingTopic()
        {
            var publication = new Publication
            {
                Title = "Test publication",
                Topic = new Topic
                {
                    Title = "Test topic"
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveRequest
                    {
                        Title = "Test publication",
                        TopicId = Guid.NewGuid(),
                    }
                );

                result.AssertBadRequest(TopicDoesNotExist);
            }
        }

        [Fact]
        public async Task UpdatePublication_FailsWithNonUniqueSlug()
        {
            var topic = new Topic
            {
                Title = "Topic title"
            };
            var publication = new Publication
            {
                Title = "Test publication",
                Slug = "test-publication",
                Topic = topic,
            };
            var otherPublication = new Publication
            {
                Title = "Duplicated title",
                Slug = "duplicated-title",
                Topic = topic,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(publication);
                context.Add(otherPublication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveRequest
                    {
                        Title = "Duplicated title",
                        TopicId = topic.Id,
                    }
                );

                result.AssertBadRequest(SlugNotUnique);
            }
        }

        [Fact]
        public async Task PartialUpdateLegacyReleases_OnlyMatchingEntities()
        {
            var publication = new Publication
            {
                Slug = "test-publication",
                LegacyReleases = new List<LegacyRelease>
                {
                    new()
                    {
                        Description = "Test description 1",
                        Url = "https://test1.com",
                        Order = 1,
                    },
                    new()
                    {
                        Description = "Test description 2",
                        Url = "https://test2.com",
                        Order = 2,
                    },
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

                publicationCacheService.Setup(mock => mock.UpdatePublication(publication.Slug))
                    .ReturnsAsync(new PublicationCacheViewModel());

                var publicationService = BuildPublicationService(context,
                    publicationCacheService: publicationCacheService.Object);

                var result = await publicationService.PartialUpdateLegacyReleases(
                    publication.Id,
                    new List<LegacyReleasePartialUpdateViewModel>
                    {
                        new()
                        {
                            Id = publication.LegacyReleases[0].Id,
                            Description = "Updated description 1",
                            Url = "https://updated-test1.com",
                            Order = 3
                        }
                    }
                );

                VerifyAllMocks(publicationCacheService);

                var legacyReleases = result.AssertRight();

                Assert.Equal(2, legacyReleases.Count);

                Assert.Equal(publication.LegacyReleases[0].Id, legacyReleases[0].Id);
                Assert.Equal("Updated description 1", legacyReleases[0].Description);
                Assert.Equal("https://updated-test1.com", legacyReleases[0].Url);
                Assert.Equal(3, legacyReleases[0].Order);

                Assert.Equal(publication.LegacyReleases[1].Id, legacyReleases[1].Id);
                Assert.Equal("Test description 2", legacyReleases[1].Description);
                Assert.Equal("https://test2.com", legacyReleases[1].Url);
                Assert.Equal(2, legacyReleases[1].Order);
            }
        }

        [Fact]
        public async Task PartialUpdateLegacyReleases_OnlyNonNullFields()
        {
            var publication = new Publication
            {
                Slug = "test-publication",
                LegacyReleases = new List<LegacyRelease>
                {
                    new()
                    {
                        Description = "Test description 1",
                        Url = "https://test1.com",
                        Order = 1,
                    },
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

                publicationCacheService.Setup(mock => mock.UpdatePublication(publication.Slug))
                    .ReturnsAsync(new PublicationCacheViewModel());

                var publicationService = BuildPublicationService(context,
                    publicationCacheService: publicationCacheService.Object);

                var result = await publicationService.PartialUpdateLegacyReleases(
                    publication.Id,
                    new List<LegacyReleasePartialUpdateViewModel>
                    {
                        new LegacyReleasePartialUpdateViewModel
                        {
                            Id = publication.LegacyReleases[0].Id,
                            Description = "Updated description 1",
                        }
                    }
                );

                VerifyAllMocks(publicationCacheService);

                var legacyReleases = result.AssertRight();

                Assert.Single(legacyReleases);

                Assert.Equal(publication.LegacyReleases[0].Id, legacyReleases[0].Id);
                Assert.Equal("Updated description 1", legacyReleases[0].Description);
                Assert.Equal("https://test1.com", legacyReleases[0].Url);
                Assert.Equal(1, legacyReleases[0].Order);
            }
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

            var contentDbContext = InMemoryApplicationDbContext();
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();

            var service = BuildPublicationService(context: contentDbContext);

            var result = await service.GetExternalMethodology(publication.Id);
            var externalMethodology = result.AssertRight();

            Assert.Equal(publication.ExternalMethodology.Title, externalMethodology.Title);
            Assert.Equal(publication.ExternalMethodology.Url, externalMethodology.Url);
        }

        [Fact]
        public async Task GetExternalMethodology_NoPublication()
        {
            var contentDbContext = InMemoryApplicationDbContext();

            var service = BuildPublicationService(context: contentDbContext);

            var result = await service.GetExternalMethodology(Guid.NewGuid());
            result.AssertNotFound();
        }

        [Fact]
        public async Task GetExternalMethodology_NoExternalMethodology()
        {
            var publication = new Publication
            {
                ExternalMethodology = null,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildPublicationService(context: contentDbContext);

                var result = await service.GetExternalMethodology(publication.Id);
                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task UpdateExternalMethodology()
        {
            var publication = new Publication
            {
                ExternalMethodology = new ExternalMethodology
                {
                    Title = "Original external methodology",
                    Url = "http://test.external.methodology/original",
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
                publicationCacheService.Setup(s => s.UpdatePublication(publication.Slug))
                    .ReturnsAsync(new PublicationCacheViewModel());

                var service = BuildPublicationService(context: contentDbContext,
                    publicationCacheService: publicationCacheService.Object);

                var result = await service.UpdateExternalMethodology(
                    publication.Id,
                    new ExternalMethodologySaveRequest
                    {
                        Title = "New external methodology",
                        Url = "http://test.external.methodology/new",
                    });

                VerifyAllMocks(publicationCacheService);

                var externalMethodology = result.AssertRight();

                Assert.Equal("New external methodology", externalMethodology.Title);
                Assert.Equal("http://test.external.methodology/new", externalMethodology.Url);

                var dbPublication = contentDbContext.Publications
                    .Single(p => p.Id == publication.Id);

                Assert.NotNull(dbPublication.ExternalMethodology);
                Assert.Equal("New external methodology", dbPublication.ExternalMethodology!.Title);
                Assert.Equal("http://test.external.methodology/new", dbPublication.ExternalMethodology.Url);
            }
        }

        [Fact]
        public async Task UpdateExternalMethodology_NoPublication()
        {
            var contentDbContext = InMemoryApplicationDbContext();
            var service = BuildPublicationService(context: contentDbContext);

            var result = await service.UpdateExternalMethodology(
                publicationId: Guid.NewGuid(),
                new ExternalMethodologySaveRequest
                {
                    Title = "New external methodology",
                    Url = "http://test.external.methodology/new",
                });

            result.AssertNotFound();
        }

        [Fact]
        public async Task RemoveExternalMethodology()
        {
            var publication = new Publication
            {
                ExternalMethodology = new ExternalMethodology
                {
                    Title = "Original external methodology",
                    Url = "http://test.external.methodology/original",
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
                publicationCacheService.Setup(s => s.UpdatePublication(publication.Slug))
                    .ReturnsAsync(new PublicationCacheViewModel());
                var service = BuildPublicationService(context: contentDbContext,
                    publicationCacheService: publicationCacheService.Object);

                var result = await service.RemoveExternalMethodology(publication.Id);

                VerifyAllMocks(publicationCacheService);

                result.AssertRight();

                var dbPublication = contentDbContext.Publications
                    .Single(p => p.Id == publication.Id);

                Assert.Null(dbPublication.ExternalMethodology);
            }
        }

        [Fact]
        public async Task RemoveExternalMethodology_NoPublication()
        {
            var contentDbContext = InMemoryApplicationDbContext();
            var service = BuildPublicationService(context: contentDbContext);

            var result = await service.RemoveExternalMethodology(publicationId: Guid.NewGuid());

            result.AssertNotFound();
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

            var contentDbContext = InMemoryApplicationDbContext();
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();

            var service = BuildPublicationService(context: contentDbContext);

            var result = await service.GetContact(publication.Id);
            var contact = result.AssertRight();

            Assert.Equal(publication.Contact.ContactName, contact.ContactName);
            Assert.Equal(publication.Contact.ContactTelNo, contact.ContactTelNo);
            Assert.Equal(publication.Contact.TeamName, contact.TeamName);
            Assert.Equal(publication.Contact.TeamEmail, contact.TeamEmail);
        }

        [Fact]
        public async Task GetContact_NoContact()
        {
            var publication = new Publication();

            var contentDbContext = InMemoryApplicationDbContext();
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();

            var service = BuildPublicationService(context: contentDbContext);

            var result = await service.GetContact(publication.Id);
            result.AssertNotFound();
        }

        [Fact]
        public async Task GetContact_NoPublication()
        {
            var contentDbContext = InMemoryApplicationDbContext();

            var service = BuildPublicationService(context: contentDbContext);

            var result = await service.GetContact(Guid.NewGuid());
            result.AssertNotFound();
        }

        [Fact]
        public async Task UpdateContact()
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

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
                publicationCacheService.Setup(s => s.UpdatePublication(publication.Slug))
                    .ReturnsAsync(new PublicationCacheViewModel());

                var service = BuildPublicationService(context: contentDbContext,
                    publicationCacheService: publicationCacheService.Object);

                var updatedContact = new Contact
                {
                    ContactName = "new contact name",
                    ContactTelNo = "12345 6789",
                    TeamName = "new team name",
                    TeamEmail = "new_team@email.com",
                };

                var result = await service.UpdateContact(publication.Id, updatedContact);
                var contact = result.AssertRight();

                Assert.Equal(updatedContact.ContactName, contact.ContactName);
                Assert.Equal(updatedContact.ContactTelNo, contact.ContactTelNo);
                Assert.Equal(updatedContact.TeamName, contact.TeamName);
                Assert.Equal(updatedContact.TeamEmail, contact.TeamEmail);

                var dbPublication = await contentDbContext.Publications
                    .Include(p => p.Contact)
                    .SingleAsync(p => p.Id == publication.Id);

                Assert.Equal(updatedContact.ContactName, dbPublication.Contact.ContactName);
                Assert.Equal(updatedContact.ContactTelNo, dbPublication.Contact.ContactTelNo);
                Assert.Equal(updatedContact.TeamName, dbPublication.Contact.TeamName);
                Assert.Equal(updatedContact.TeamEmail, dbPublication.Contact.TeamEmail);
            }
        }

        [Fact]
        public async Task UpdateContact_SharedContact()
        {
            var sharedContact = new Contact
            {
                ContactName = "contact name",
                ContactTelNo = "12345",
                TeamName = "team name",
                TeamEmail = "team@email.com",
            };

            var publication1 = new Publication
            {
                Contact = sharedContact,
            };

            var publication2 = new Publication
            {
                Contact = sharedContact,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication1, publication2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
                publicationCacheService.Setup(s => s.UpdatePublication(publication1.Slug))
                    .ReturnsAsync(new PublicationCacheViewModel());

                var service = BuildPublicationService(context: contentDbContext,
                    publicationCacheService: publicationCacheService.Object);

                var updatedContact = new Contact
                {
                    ContactName = "new contact name",
                    ContactTelNo = "12345 6789",
                    TeamName = "new team name",
                    TeamEmail = "new_team@email.com",
                };

                var result = await service.UpdateContact(publication1.Id, updatedContact);
                var contact = result.AssertRight();

                Assert.Equal(updatedContact.ContactName, contact.ContactName);
                Assert.Equal(updatedContact.ContactTelNo, contact.ContactTelNo);
                Assert.Equal(updatedContact.TeamName, contact.TeamName);
                Assert.Equal(updatedContact.TeamEmail, contact.TeamEmail);

                var dbPublication1 = contentDbContext.Publications
                    .Include(p => p.Contact)
                    .Single(p => p.Id == publication1.Id);

                Assert.Equal(updatedContact.ContactName, dbPublication1.Contact.ContactName);
                Assert.Equal(updatedContact.ContactTelNo, dbPublication1.Contact.ContactTelNo);
                Assert.Equal(updatedContact.TeamName, dbPublication1.Contact.TeamName);
                Assert.Equal(updatedContact.TeamEmail, dbPublication1.Contact.TeamEmail);

                var dbPublication2 = contentDbContext.Publications
                    .Include(p => p.Contact)
                    .Single(p => p.Id == publication2.Id);

                Assert.Equal(sharedContact.ContactName, dbPublication2.Contact.ContactName);
                Assert.Equal(sharedContact.ContactTelNo, dbPublication2.Contact.ContactTelNo);
                Assert.Equal(sharedContact.TeamName, dbPublication2.Contact.TeamName);
                Assert.Equal(sharedContact.TeamEmail, dbPublication2.Contact.TeamEmail);
            }
        }

        [Fact]
        public async Task UpdateContact_NoPublication()
        {
            var contentDbContext = InMemoryApplicationDbContext();

            var service = BuildPublicationService(context: contentDbContext);

            var result = await service.UpdateContact(
                Guid.NewGuid(), new Contact());
            result.AssertNotFound();
        }

        [Fact]
        public async Task ListActiveReleases()
        {
            var release1Original = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
            };
            var release1Amendment = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
                PreviousVersion = release1Original,
            };
            var release2 = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2001",
            };
            var release3Original = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2002",
            };
            var release3Amendment = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2002",
                PreviousVersion = release3Original,
            };
            var publication = new Publication
            {
                Releases = new List<Release>
                {
                    release1Original,
                    release1Amendment,
                    release2,
                    release3Original,
                    release3Amendment,
                }
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context);

                var result = await publicationService.ListActiveReleases(
                    publication.Id);

                var releases = result.AssertRight();

                Assert.Equal(3, releases.Count);

                Assert.Equal(release3Amendment.Id, releases[0].Id);
                Assert.Equal(release2.Id, releases[1].Id);
                Assert.Equal(release1Amendment.Id, releases[2].Id);
            }
        }

        [Fact]
        public async Task ListActiveReleases_SingleRelease()
        {
            var release = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
                Type = ReleaseType.AdHocStatistics,
                Published = DateTime.UtcNow,
                PublishScheduled = null,
                NextReleaseDate = new PartialDate { Year = "2030" },
                ApprovalStatus = ReleaseApprovalStatus.Approved,
                ReleaseStatuses = ListOf(
                    new ReleaseStatus { InternalReleaseNote = "Internal note" }),
                PreviousVersionId = Guid.NewGuid(),
                Publication = new Publication
                {
                    Title = "Publication title",
                },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context);

                var result = await publicationService.ListActiveReleases(
                    release.Publication.Id);

                var releases = result.AssertRight();

                var resultRelease = Assert.Single(releases);

                Assert.Equal(release.Id, resultRelease.Id);
                Assert.Equal(release.Title, resultRelease.Title);
                Assert.Equal(release.Slug, resultRelease.Slug);
                Assert.Equal(release.Type, resultRelease.Type);
                Assert.Equal(release.Year, resultRelease.Year);
                Assert.Equal(release.YearTitle, resultRelease.YearTitle);
                Assert.Equal(release.TimePeriodCoverage, resultRelease.TimePeriodCoverage);
                Assert.Equal(release.Published, resultRelease.Published);
                Assert.Equal(release.Live, resultRelease.Live);
                Assert.Equal(release.PublishScheduled, resultRelease.PublishScheduled);
                release.NextReleaseDate.AssertDeepEqualTo(resultRelease.NextReleaseDate);
                Assert.Equal(release.ApprovalStatus, resultRelease.ApprovalStatus);
                Assert.Equal(release.Amendment, resultRelease.Amendment);
                Assert.Equal(release.PreviousVersionId, resultRelease.PreviousVersionId);
                Assert.Null(resultRelease.Permissions);
            }
        }

        [Fact]
        public async Task ListActiveReleases_Live_True()
        {
            var release1Original = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
                Published = DateTime.UtcNow,
            };
            var release1Amendment = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
                PreviousVersion = release1Original,
            };
            var release2 = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2001",
            };
            var release3Original = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2002",
                Published = DateTime.UtcNow,
            };
            var release3Amendment = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2002",
                PreviousVersion = release3Original,
                Published = DateTime.UtcNow,
            };
            var publication = new Publication
            {
                Releases = new List<Release>
                {
                    release1Original,
                    release1Amendment,
                    release2,
                    release3Original,
                    release3Amendment,
                }
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication, release1Original, release1Amendment);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context);

                var result = await publicationService.ListActiveReleases(
                    publication.Id, live: true);

                var releases = result.AssertRight();

                var release = Assert.Single(releases);
                Assert.Equal(release3Amendment.Id, release.Id);
            }
        }

        [Fact]
        public async Task ListActiveReleases_Live_False()
        {
            var release1Original = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
                Published = DateTime.UtcNow,
            };
            var release1Amendment = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
                PreviousVersion = release1Original,
            };
            var release2 = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2001",
            };
            var release3Original = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2002",
                Published = DateTime.UtcNow,
            };
            var release3Amendment = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2002",
                PreviousVersion = release3Original,
                Published = DateTime.UtcNow,
            };
            var publication = new Publication
            {
                Releases = new List<Release>
                {
                    release1Original,
                    release1Amendment,
                    release2,
                    release3Original,
                    release3Amendment,
                }
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication, release1Original, release1Amendment);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context);

                var result = await publicationService.ListActiveReleases(
                    publication.Id, live: false);

                var releases = result.AssertRight();
                Assert.Equal(2, releases.Count);
                Assert.Equal(release2.Id, releases[0].Id);
                Assert.Equal(release1Amendment.Id, releases[1].Id);
            }
        }

        [Fact]
        public async Task ListActiveReleases_IncludePermissions_True()
        {
            var release = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
                Type = ReleaseType.AdHocStatistics,
                ApprovalStatus = ReleaseApprovalStatus.Draft,
                Publication = new Publication
                {
                    Title = "Publication title",
                },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context);

                var result = await publicationService.ListActiveReleases(
                    release.PublicationId, includePermissions: true);

                var releases = result.AssertRight();

                var resultRelease = Assert.Single(releases);

                Assert.NotNull(resultRelease.Permissions);
                Assert.True(resultRelease.Permissions!.CanDeleteRelease);
                Assert.True(resultRelease.Permissions!.CanUpdateRelease);
                Assert.True(resultRelease.Permissions!.CanAddPrereleaseUsers);
                Assert.True(resultRelease.Permissions!.CanMakeAmendmentOfRelease);
            }
        }

        [Fact]
        public async Task ListActiveReleases_IncludePermissions_False()
        {
            var release = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
                Type = ReleaseType.AdHocStatistics,
                ApprovalStatus = ReleaseApprovalStatus.Draft,
                Publication = new Publication
                {
                    Title = "Publication title",
                },
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context);

                var result = await publicationService.ListActiveReleases(
                    release.PublicationId, includePermissions: false);

                var releases = result.AssertRight();

                var resultRelease = Assert.Single(releases);

                Assert.Null(resultRelease.Permissions);
            }
        }

        [Fact]
        public async Task ListActiveReleasesPaginated()
        {
            var release1 = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
            };
            var release2 = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2001",
            };
            var release3 = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2003",
            };
            var release4 = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2004",
            };
            var publication = new Publication
            {
                Releases = new List<Release>
                {
                    release1,
                    release2,
                    release3,
                    release4,
                }
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context);

                var result = await publicationService.ListActiveReleasesPaginated(
                    publication.Id,
                    page: 1,
                    pageSize: 2
                );

                var pagedResult = result.AssertRight();

                var releases = pagedResult.Results;
                Assert.Equal(2, releases.Count);

                Assert.Equal(release4.Id, releases[0].Id);
                Assert.Equal(release3.Id, releases[1].Id);

                Assert.Equal(1, pagedResult.Paging.Page);
                Assert.Equal(2, pagedResult.Paging.PageSize);
                Assert.Equal(2, pagedResult.Paging.TotalPages);
                Assert.Equal(4, pagedResult.Paging.TotalResults);
            }
        }

        private static PublicationService BuildPublicationService(
            ContentDbContext context,
            IUserService? userService = null,
            IPublicationRepository? publicationRepository = null,
            IMethodologyVersionRepository? methodologyVersionRepository = null,
            IPublicationCacheService? publicationCacheService = null,
            IMethodologyCacheService? methodologyCacheService = null)
        {
            return new(
                context,
                AdminMapper(),
                new PersistenceHelper<ContentDbContext>(context),
                userService ?? AlwaysTrueUserService().Object,
                publicationRepository ?? new PublicationRepository(context),
                methodologyVersionRepository ?? Mock.Of<IMethodologyVersionRepository>(Strict),
                publicationCacheService ?? Mock.Of<IPublicationCacheService>(Strict),
                methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict));
        }
    }
}
