#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
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
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static Moq.MockBehavior;
using PublicationViewModel =
    GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels.PublicationViewModel;

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
                    .ListPublications(permissions: false, publication1.Topic.Id);

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

                Assert.Equal(publication1.Contact.Id, publicationViewModel.Contact.Id);
                Assert.Equal(publication1.Contact.ContactName, publicationViewModel.Contact.ContactName);
                Assert.Equal(publication1.Contact.ContactTelNo, publicationViewModel.Contact.ContactTelNo);
                Assert.Equal(publication1.Contact.TeamName, publicationViewModel.Contact.TeamName);
                Assert.Equal(publication1.Contact.TeamEmail, publicationViewModel.Contact.TeamEmail);

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
                    .ListPublications(permissions: false, topic.Id);

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
                    .ListPublications(permissions: false);

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
        public async Task ListPublications_CanViewAllPublications_Permissions()
        {
            var topic = new Topic
            {
                Title = "Topic title",
                Theme = new Theme
                {
                    Title = "Theme title",
                },
            };

            var publication = new Publication
            {
                Title = "Test Publication",
                Topic = topic,
            };

            var userService = new Mock<IUserService>(Strict);

            userService.Setup(s => s.GetUserId()).Returns(new Guid());
            userService.Setup(s => s.MatchesPolicy(CanAccessSystem)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanViewAllPublications)).ReturnsAsync(true);

            userService.Setup(s => s.MatchesPolicy(
                    It.Is<Publication>(p => p.Id == publication.Id),
                    CanUpdateSpecificPublication))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanUpdatePublicationTitles)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanUpdatePublicationSupersededBy)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(
                It.Is<Publication>(p => p.Id == publication.Id),
                CanCreateReleaseForSpecificPublication)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(
                It.Is<Publication>(p => p.Id == publication.Id),
                CanAdoptMethodologyForSpecificPublication)).ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(
                It.Is<Publication>(p => p.Id == publication.Id),
                CanCreateMethodologyForSpecificPublication)).ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(
                It.Is<Publication>(p => p.Id == publication.Id),
                CanManageExternalMethodologyForSpecificPublication)).ReturnsAsync(false);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext,
                    userService: userService.Object);

                var result = await publicationService
                    .ListPublications(permissions: true, topic.Id);

                var publicationViewModelList = result.AssertRight();

                var publicationViewModel = Assert.Single(publicationViewModelList);

                Assert.Equal(publication.Id, publicationViewModel.Id);

                Assert.False(publicationViewModel.IsSuperseded);

                Assert.NotNull(publicationViewModel.Permissions);
                Assert.True(publicationViewModel.Permissions!.CanUpdatePublication);
                Assert.True(publicationViewModel.Permissions.CanUpdatePublicationTitle);
                Assert.True(publicationViewModel.Permissions.CanUpdatePublicationSupersededBy);
                Assert.True(publicationViewModel.Permissions.CanCreateReleases);
                Assert.False(publicationViewModel.Permissions.CanAdoptMethodologies);
                Assert.False(publicationViewModel.Permissions.CanCreateMethodologies);
                Assert.False(publicationViewModel.Permissions.CanManageExternalMethodology);
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
                    .ListPublications(permissions: false, topic.Id);

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

                Assert.Equal(publication1.Contact.Id, publicationViewModel.Contact.Id);
                Assert.Equal(publication1.Contact.ContactName, publicationViewModel.Contact.ContactName);
                Assert.Equal(publication1.Contact.ContactTelNo, publicationViewModel.Contact.ContactTelNo);
                Assert.Equal(publication1.Contact.TeamName, publicationViewModel.Contact.TeamName);
                Assert.Equal(publication1.Contact.TeamEmail, publicationViewModel.Contact.TeamEmail);

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
                    .ListPublications(permissions: false, topic.Id);

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
                    .ListPublications(permissions: false);

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
        public async Task ListPublications_CannotViewAllPublications_Permissions()
        {
            var userId = Guid.NewGuid();

            var topic = new Topic
            {
                Title = "Topic title",
                Theme = new Theme
                {
                    Title = "Theme title",
                },
            };

            var publication = new Publication
            {
                Title = "Test Publication",
                Summary = "Test summary",
                Slug = "test-slug",
                Topic = topic,
                Contact = new Contact(),
                SupersededBy = null,
            };

            var userService = new Mock<IUserService>(Strict);

            userService.Setup(s => s.GetUserId()).Returns(userId);
            userService.Setup(s => s.MatchesPolicy(CanAccessSystem)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanViewAllPublications)).ReturnsAsync(false);

            userService.Setup(s => s.MatchesPolicy(
                    It.Is<Publication>(p => p.Id == publication.Id),
                    CanUpdateSpecificPublication))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanUpdatePublicationTitles)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanUpdatePublicationSupersededBy)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(
                It.Is<Publication>(p => p.Id == publication.Id),
                CanCreateReleaseForSpecificPublication)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(
                It.Is<Publication>(p => p.Id == publication.Id),
                CanAdoptMethodologyForSpecificPublication)).ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(
                It.Is<Publication>(p => p.Id == publication.Id),
                CanCreateMethodologyForSpecificPublication)).ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(
                It.Is<Publication>(p => p.Id == publication.Id),
                CanManageExternalMethodologyForSpecificPublication)).ReturnsAsync(false);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(
                    new UserPublicationRole
                    {
                        User = new User { Id = userId },
                        Publication = publication,
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
                    .ListPublications(permissions: true, topic.Id);

                var publicationViewModelList = result.AssertRight();

                var publicationViewModel = Assert.Single(publicationViewModelList);

                Assert.NotNull(publicationViewModel.Permissions);
                Assert.True(publicationViewModel.Permissions!.CanUpdatePublication);
                Assert.True(publicationViewModel.Permissions.CanUpdatePublicationTitle);
                Assert.True(publicationViewModel.Permissions.CanUpdatePublicationSupersededBy);
                Assert.True(publicationViewModel.Permissions.CanCreateReleases);
                Assert.False(publicationViewModel.Permissions.CanAdoptMethodologies);
                Assert.False(publicationViewModel.Permissions.CanCreateMethodologies);
                Assert.False(publicationViewModel.Permissions.CanManageExternalMethodology);
            }
        }

        [Fact]
        public async Task GetMyPublicationsAndReleasesByTopic_CanViewAllReleases_ReleaseOrder()
        {
            var topic = new Topic
            {
                Id = Guid.NewGuid(),
                Theme = new Theme(),
            };

            // The order they should appear in the result - ordered by descending Year then by descending TimeIdentifier
            var publication1Release1 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.Week1,
            };
            var publication1Release2 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.Week2,
            };
            var publication1Release3 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.Week1,
            };
            var publication1Release4 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "1999",
                TimePeriodCoverage = TimeIdentifier.Week1,
            };
            var publication1 = new Publication
            {
                Title = "publication1",
                Topic = topic,
                Releases = new List<Release>
                {
                    publication1Release1,
                    publication1Release2,
                    publication1Release3,
                    publication1Release4,
                },
            };

            var publication2Release1 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
            };
            var publication2Release2 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
            };
            var publication2 = new Publication
            {
                Title = "publication2",
                Topic = topic,
                Releases = ListOf(publication2Release1, publication2Release2),
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication1, publication2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext);

                var result = await publicationService.GetMyPublicationsAndReleasesByTopic(topic.Id);

                var publicationViewModels = result.AssertRight();

                // NOTE(mark): No ordering of publications anymore - but releases are still ordered
                // but all this is getting deleted in EES-3576, so going to leave this for now
                Assert.Equal(2, publicationViewModels.Count);
                Assert.Equal(publication1.Id, publicationViewModels[0].Id);

                var publication1Releases = publicationViewModels[0].Releases;
                Assert.Equal(4, publication1Releases.Count);
                Assert.Equal(publication1Release1.Id, publication1Releases[0].Id);
                Assert.Equal(publication1Release2.Id, publication1Releases[1].Id);
                Assert.Equal(publication1Release3.Id, publication1Releases[2].Id);
                Assert.Equal(publication1Release4.Id, publication1Releases[3].Id);

                Assert.Equal(publication2.Id, publicationViewModels[1].Id);
                var publication2Releases = publicationViewModels[1].Releases;
                Assert.Equal(2, publication2Releases.Count);
                Assert.Equal(publication2Release1.Id, publication2Releases[0].Id);
                Assert.Equal(publication2Release2.Id, publication2Releases[1].Id);
            }
        }

        [Fact]
        public async Task GetMyPublicationsAndReleasesByTopic_CanViewAllReleases_Methodologies()
        {
            var topic = new Topic
            {
                Id = Guid.NewGuid(),
                Theme = new Theme(),
            };

            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.Week1,
            };
            var methodology1Version1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                AlternativeTitle = "Methodology 1 Version 1",
                Version = 0,
                Status = Draft
            };

            var methodology2Version1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                AlternativeTitle = "Methodology 2 Version 1",
                Version = 0,
                Status = Approved
            };

            var methodology2Version2 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                AlternativeTitle = "Methodology 2 Version 2",
                Version = 1,
                Status = Draft,
                PreviousVersionId = methodology2Version1.Id
            };

            var methodology3Version1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                AlternativeTitle = "Methodology 3 Version 1",
                Version = 1,
                Status = Approved,
            };

            var publication1 = new Publication
            {
                Title = "publication1",
                Topic = topic,
                Releases = ListOf(release),
                Methodologies = ListOf(
                    new PublicationMethodology
                    {
                        Methodology = new Methodology
                        {
                            Versions = ListOf(methodology2Version2, methodology2Version1)
                        },
                        Owner = false
                    },
                    new PublicationMethodology
                    {
                        Methodology = new Methodology
                        {
                            Versions = ListOf(methodology3Version1)
                        },
                        Owner = false
                    },
                    new PublicationMethodology
                    {
                        Methodology = new Methodology
                        {
                            Versions = ListOf(methodology1Version1)
                        },
                        Owner = true
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext);

                var result = await publicationService.GetMyPublicationsAndReleasesByTopic(topic.Id);

                var publicationViewModels = result.AssertRight();

                var publicationViewModel = Assert.Single(publicationViewModels);
                Assert.Equal(publication1.Id, publicationViewModel.Id);

                var releases = publicationViewModel.Releases;
                Assert.Single(releases);
                Assert.Equal(release.Id, releases[0].Id);

                var methodologies = publicationViewModel.Methodologies;
                Assert.Equal(3, methodologies.Count);

                Assert.Equal(methodology1Version1.AlternativeTitle, methodologies[0].Title);
                Assert.True(methodologies[0].Owned);

                Assert.Equal(methodology2Version2.AlternativeTitle, methodologies[1].Title);
                Assert.False(methodologies[1].Owned);

                Assert.Equal(methodology3Version1.AlternativeTitle, methodologies[2].Title);
                Assert.False(methodologies[2].Owned);
            }
        }

        [Fact]
        public async Task GetMyPublicationsAndReleasesByTopic_CanViewAllReleases_LatestRelease()
        {
            var topic = new Topic
            {
                Id = Guid.NewGuid(),
                Theme = new Theme(),
            };

            // The order they should appear in result
            var publication1Release1 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.Week1,
                Published = DateTime.UtcNow,
            };
            var publication1Release2 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.Week2,
                Published = DateTime.UtcNow,
            };
            var publication1Release3 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.Week1,
                Published = DateTime.UtcNow,
            };
            var publication1Release4 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "1999",
                TimePeriodCoverage = TimeIdentifier.Week1,
                Published = DateTime.UtcNow,
            };
            var publication1 = new Publication
            {
                Title = "publication1",
                Topic = topic,
                Releases = ListOf(
                    publication1Release2,
                    publication1Release3,
                    publication1Release4,
                    publication1Release1),
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext);

                var result = await publicationService.GetMyPublicationsAndReleasesByTopic(topic.Id);
                var publicationViewModels = result.AssertRight();

                var publicationViewModel = Assert.Single(publicationViewModels);
                Assert.Equal(publication1.Id, publicationViewModel.Id);

                var publication1Releases = publicationViewModel.Releases;
                Assert.Equal(4, publication1Releases.Count);

                Assert.Equal(publication1Release1.Id, publication1Releases[0].Id);
                Assert.True(publication1Releases[0].LatestRelease);

                Assert.Equal(publication1Release2.Id, publication1Releases[1].Id);
                Assert.False(publication1Releases[1].LatestRelease);

                Assert.Equal(publication1Release3.Id, publication1Releases[2].Id);
                Assert.False(publication1Releases[2].LatestRelease);

                Assert.Equal(publication1Release4.Id, publication1Releases[3].Id);
                Assert.False(publication1Releases[3].LatestRelease);
            }
        }

        [Fact]
        public async Task GetMyPublicationsAndReleasesByTopic_CanViewAllReleases_ReleasePermissions()
        {
            var topic = new Topic
            {
                Id = Guid.NewGuid(),
                Theme = new Theme(),
            };

            var publication1Release1 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.Week1,
            };
            var publication1Release2 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.Week2,
            };
            var publication1 = new Publication
            {
                Title = "publication1",
                Topic = topic,
                Releases = ListOf(publication1Release1, publication1Release2),
            };

            var userService = new Mock<IUserService>(Strict);
            userService.Setup(s => s.MatchesPolicy(CanAccessSystem))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanViewAllReleases))
                .ReturnsAsync(true);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanUpdateSpecificRelease))
                .ReturnsAsync(true);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanDeleteSpecificRelease))
                .ReturnsAsync(true);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanAssignPrereleaseContactsToSpecificRelease))
                .ReturnsAsync(false);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanMakeAmendmentOfSpecificRelease))
                .ReturnsAsync(false);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext,
                    userService: userService.Object);

                var result = await publicationService.GetMyPublicationsAndReleasesByTopic(topic.Id);

                VerifyAllMocks(userService);

                var publicationViewModels = result.AssertRight();

                var publicationViewModel = Assert.Single(publicationViewModels);
                Assert.Equal(publication1.Id, publicationViewModel.Id);

                var publication1Releases = publicationViewModel.Releases;
                Assert.Equal(2, publication1Releases.Count);

                Assert.Equal(publication1Release1.Id, publication1Releases[0].Id);
                Assert.True(publication1Releases[0].Permissions!.CanUpdateRelease);
                Assert.True(publication1Releases[0].Permissions!.CanDeleteRelease);
                Assert.False(publication1Releases[0].Permissions!.CanAddPrereleaseUsers);
                Assert.False(publication1Releases[0].Permissions!.CanMakeAmendmentOfRelease);

                Assert.Equal(publication1Release2.Id, publication1Releases[1].Id);
                Assert.True(publication1Releases[1].Permissions!.CanUpdateRelease);
                Assert.True(publication1Releases[1].Permissions!.CanDeleteRelease);
                Assert.False(publication1Releases[1].Permissions!.CanAddPrereleaseUsers);
                Assert.False(publication1Releases[1].Permissions!.CanMakeAmendmentOfRelease);
            }
        }

        [Fact]
        public async Task GetMyPublicationsAndReleasesByTopic_NotViewAllReleases_ReleaseOrder()
        {
            var user = new User { Id = Guid.NewGuid(), };

            var topic = new Topic
            {
                Id = Guid.NewGuid(),
                Theme = new Theme(),
            };

            // The order they should appear in result
            var publication1Release1 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.Week1,
            };
            var publication1Release2 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.Week2,
            };
            var publication1Release3 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.Week1,
            };
            var publication1Release4 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "1999",
                TimePeriodCoverage = TimeIdentifier.Week1,
            };
            var publication1 = new Publication
            {
                Title = "publication1",
                Topic = topic,
                Releases = ListOf(
                    publication1Release2,
                    publication1Release3,
                    publication1Release4,
                    publication1Release1),
            };

            var publication2Release1 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
            };
            var publication2Release2 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
            };
            var publication2 = new Publication
            {
                Title = "publication2",
                Topic = topic,
                Releases = ListOf(publication2Release2, publication2Release1),
            };

            var userService = new Mock<IUserService>(Strict);
            userService.Setup(s => s.MatchesPolicy(CanAccessSystem))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanViewAllReleases))
                .ReturnsAsync(false);
            userService.Setup(s => s.GetUserId())
                .Returns(user.Id);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanUpdateSpecificRelease))
                .ReturnsAsync(true);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanDeleteSpecificRelease))
                .ReturnsAsync(true);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanAssignPrereleaseContactsToSpecificRelease))
                .ReturnsAsync(false);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanMakeAmendmentOfSpecificRelease))
                .ReturnsAsync(false);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication1, publication2);
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
                    });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext,
                    userService: userService.Object);

                var result = await publicationService
                    .GetMyPublicationsAndReleasesByTopic(topic.Id);

                VerifyAllMocks(userService);

                var publicationViewModels = result.AssertRight();

                // NOTE(mark): No ordering of publications anymore - but releases are still ordered
                // but all this is getting deleted in EES-3576, so going to leave this for now
                Assert.Equal(2, publicationViewModels.Count);
                Assert.Equal(publication1.Id, publicationViewModels[0].Id);

                var publication1Releases = publicationViewModels[0].Releases;
                Assert.Equal(4, publication1Releases.Count);

                Assert.Equal(publication1Release1.Id, publication1Releases[0].Id);
                Assert.Equal(publication1Release2.Id, publication1Releases[1].Id);
                Assert.Equal(publication1Release3.Id, publication1Releases[2].Id);
                Assert.Equal(publication1Release4.Id, publication1Releases[3].Id);

                Assert.Equal(publication2.Id, publicationViewModels[1].Id);
                var publication2Releases = publicationViewModels[1].Releases;
                Assert.Equal(2, publication2Releases.Count);

                Assert.Equal(publication2Release1.Id, publication2Releases[0].Id);
                Assert.Equal(publication2Release2.Id, publication2Releases[1].Id);
            }
        }

        [Fact]
        public async Task GetMyPublicationsAndReleasesByTopic_NotViewAllReleases_Methodologies()
        {
            var userId = Guid.NewGuid();

            var topic = new Topic
            {
                Id = Guid.NewGuid(),
                Theme = new Theme(),
            };

            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.Week1,
            };
            var methodology1Version1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                AlternativeTitle = "Methodology 1 Version 1",
                Version = 0,
                Status = Draft
            };

            var methodology2Version1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                AlternativeTitle = "Methodology 2 Version 1",
                Version = 0,
                Status = Approved
            };

            var methodology2Version2 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                AlternativeTitle = "Methodology 2 Version 2",
                Version = 1,
                Status = Draft,
                PreviousVersionId = methodology2Version1.Id
            };

            var methodology3Version1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                AlternativeTitle = "Methodology 3 Version 1",
                Version = 1,
                Status = Approved,
            };

            var publication1 = new Publication
            {
                Title = "publication1",
                Topic = topic,
                Releases = ListOf(release),
                Methodologies = ListOf(
                    new PublicationMethodology
                    {
                        Methodology = new Methodology
                        {
                            Versions = ListOf(methodology2Version2, methodology2Version1)
                        },
                        Owner = false
                    },
                    new PublicationMethodology
                    {
                        Methodology = new Methodology
                        {
                            Versions = ListOf(methodology3Version1)
                        },
                        Owner = false
                    },
                    new PublicationMethodology
                    {
                        Methodology = new Methodology
                        {
                            Versions = ListOf(methodology1Version1)
                        },
                        Owner = true
                    }
                )
            };

            var userService = new Mock<IUserService>(Strict);
            userService.Setup(s => s.MatchesPolicy(CanAccessSystem))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanViewAllReleases))
                .ReturnsAsync(false);
            userService.Setup(s => s.GetUserId())
                .Returns(userId);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<Release>(), CanUpdateSpecificRelease))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<Release>(), CanDeleteSpecificRelease))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<Release>(), CanAssignPrereleaseContactsToSpecificRelease))
                .ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<Release>(), CanMakeAmendmentOfSpecificRelease))
                .ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanDeleteSpecificMethodology))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanUpdateSpecificMethodology))
                .ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanApproveSpecificMethodology))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanMarkSpecificMethodologyAsDraft))
                .ReturnsAsync(true);
            userService.Setup(s =>
                    s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanMakeAmendmentOfSpecificMethodology))
                .ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<PublicationMethodology>(), CanDropMethodologyLink))
                .ReturnsAsync(true);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication1);
                await contentDbContext.UserPublicationRoles.AddRangeAsync(
                    new UserPublicationRole
                    {
                        User = new User { Id = userId, },
                        Publication = publication1,
                        Role = PublicationRole.Owner,
                    });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext,
                    userService: userService.Object);

                var result = await publicationService.GetMyPublicationsAndReleasesByTopic(topic.Id);

                VerifyAllMocks(userService);

                var publicationViewModels = result.AssertRight();

                var publicationViewModel = Assert.Single(publicationViewModels);
                Assert.Equal(publication1.Id, publicationViewModel.Id);

                var releases = publicationViewModel.Releases;
                Assert.Single(releases);
                Assert.Equal(release.Id, releases[0].Id);

                var methodologies = publicationViewModel.Methodologies;
                Assert.Equal(3, methodologies.Count);

                Assert.Equal(methodology1Version1.AlternativeTitle, methodologies[0].Title);
                Assert.True(methodologies[0].Owned);

                Assert.Equal(methodology2Version2.AlternativeTitle, methodologies[1].Title);
                Assert.False(methodologies[1].Owned);

                Assert.Equal(methodology3Version1.AlternativeTitle, methodologies[2].Title);
                Assert.False(methodologies[2].Owned);
            }
        }

        [Fact]
        public async Task GetMyPublicationsAndReleasesByTopic_NotViewAllReleases_LatestRelease()
        {
            var userId = Guid.NewGuid();

            var topic = new Topic
            {
                Id = Guid.NewGuid(),
            };

            // The order they should appear in result
            var publication1Release1 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.Week1,
                Published = DateTime.UtcNow,
            };
            var publication1Release2 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.Week2,
                Published = DateTime.UtcNow,
            };
            var publication1Release3 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.Week1,
                Published = DateTime.UtcNow,
            };
            var publication1Release4 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "1999",
                TimePeriodCoverage = TimeIdentifier.Week1,
                Published = DateTime.UtcNow,
            };
            var publication1 = new Publication
            {
                Title = "publication1",
                Releases = ListOf(
                    publication1Release1,
                    publication1Release2,
                    publication1Release3,
                    publication1Release4),
            };

            var userService = new Mock<IUserService>(Strict);
            userService.Setup(s => s.MatchesPolicy(CanAccessSystem))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanViewAllReleases))
                .ReturnsAsync(false);
            userService.Setup(s => s.GetUserId())
                .Returns(userId);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanUpdateSpecificRelease))
                .ReturnsAsync(true);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanDeleteSpecificRelease))
                .ReturnsAsync(true);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanAssignPrereleaseContactsToSpecificRelease))
                .ReturnsAsync(false);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanMakeAmendmentOfSpecificRelease))
                .ReturnsAsync(false);

            var publicationRepository = new Mock<IPublicationRepository>(Strict);

            publicationRepository.Setup(s => s.ListPublicationsForUser(userId, topic.Id))
                .ReturnsAsync(ListOf(publication1));

            publicationRepository.Setup(s => s.IsSuperseded(It.IsAny<Publication>()))
                .Returns(false);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddRangeAsync(publication1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext,
                    userService: userService.Object,
                    publicationRepository: publicationRepository.Object);

                var result = await publicationService.GetMyPublicationsAndReleasesByTopic(topic.Id);

                VerifyAllMocks(publicationRepository, userService);

                var publicationViewModels = result.AssertRight();

                var publicationViewModel = Assert.Single(publicationViewModels);
                Assert.Equal(publication1.Id, publicationViewModel.Id);

                var publication1Releases = publicationViewModel.Releases;
                Assert.Equal(4, publication1Releases.Count);

                Assert.Equal(publication1Release1.Id, publication1Releases[0].Id);
                Assert.True(publication1Releases[0].LatestRelease);

                Assert.Equal(publication1Release2.Id, publication1Releases[1].Id);
                Assert.False(publication1Releases[1].LatestRelease);

                Assert.Equal(publication1Release3.Id, publication1Releases[2].Id);
                Assert.False(publication1Releases[2].LatestRelease);

                Assert.Equal(publication1Release4.Id, publication1Releases[3].Id);
                Assert.False(publication1Releases[3].LatestRelease);
            }
        }

        [Fact]
        public async Task GetMyPublicationsAndReleasesByTopic_NotViewAllReleases_ReleasePermissionsSet()
        {
            var userId = Guid.NewGuid();

            var topic = new Topic
            {
                Id = Guid.NewGuid(),
            };

            var publication1Release1 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.Week1,
            };
            var publication1Release2 = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.Week2,
            };
            var publication1 = new Publication
            {
                Title = "publication1",
                Releases = ListOf(publication1Release1, publication1Release2),
            };

            var userService = new Mock<IUserService>(Strict);
            userService.Setup(s => s.MatchesPolicy(CanAccessSystem))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanViewAllReleases))
                .ReturnsAsync(false);
            userService.Setup(s => s.GetUserId())
                .Returns(userId);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanUpdateSpecificRelease))
                .ReturnsAsync(true);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanDeleteSpecificRelease))
                .ReturnsAsync(true);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanAssignPrereleaseContactsToSpecificRelease))
                .ReturnsAsync(false);
            userService
                .Setup(s => s.MatchesPolicy(
                    It.IsAny<Release>(), CanMakeAmendmentOfSpecificRelease))
                .ReturnsAsync(false);

            var publicationRepository = new Mock<IPublicationRepository>(Strict);
            publicationRepository.Setup(s => s.ListPublicationsForUser(userId, topic.Id))
                .ReturnsAsync(ListOf(publication1));
            publicationRepository.Setup(s => s.IsSuperseded(It.IsAny<Publication>()))
                .Returns(false);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationService = BuildPublicationService(
                    context: contentDbContext,
                    userService: userService.Object,
                    publicationRepository: publicationRepository.Object);

                var result = await publicationService.GetMyPublicationsAndReleasesByTopic(topic.Id);

                VerifyAllMocks(publicationRepository, userService);

                var publicationViewModels = result.AssertRight();

                var publicationViewModel = Assert.Single(publicationViewModels);
                Assert.Equal(publication1.Id, publicationViewModel.Id);

                var publication1Releases = publicationViewModel.Releases;
                Assert.Equal(2, publication1Releases.Count);

                Assert.Equal(publication1Release1.Id, publication1Releases[0].Id);
                Assert.True(publication1Releases[0].Permissions!.CanUpdateRelease);
                Assert.True(publication1Releases[0].Permissions!.CanDeleteRelease);
                Assert.False(publication1Releases[0].Permissions!.CanAddPrereleaseUsers);
                Assert.False(publication1Releases[0].Permissions!.CanMakeAmendmentOfRelease);

                Assert.Equal(publication1Release2.Id, publication1Releases[1].Id);
                Assert.True(publication1Releases[1].Permissions!.CanUpdateRelease);
                Assert.True(publication1Releases[1].Permissions!.CanDeleteRelease);
                Assert.False(publication1Releases[1].Permissions!.CanAddPrereleaseUsers);
                Assert.False(publication1Releases[1].Permissions!.CanMakeAmendmentOfRelease);
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
                Contact = new Contact
                {
                    ContactName = "Test contact",
                    ContactTelNo = "0123456789",
                    TeamName = "Test team",
                    TeamEmail = "team@test.com",
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

                Assert.Equal(publication.Contact.Id, result.Contact.Id);
                Assert.Equal(publication.Contact.ContactName, result.Contact.ContactName);
                Assert.Equal(publication.Contact.ContactTelNo, result.Contact.ContactTelNo);
                Assert.Equal(publication.Contact.TeamEmail, result.Contact.TeamEmail);
                Assert.Equal(publication.Contact.TeamName, result.Contact.TeamName);
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
        public async Task GetMyPublication_CanViewAllReleases()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                AlternativeTitle = "Methodology 1 Version 1",
                Version = 0,
                Status = Draft
            };
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
                Contact = new Contact
                {
                    ContactName = "Test contact",
                    ContactTelNo = "0123456789",
                    TeamName = "Test team",
                    TeamEmail = "team@test.com",
                },
                Methodologies = new List<PublicationMethodology>
                {
                    new()
                    {
                        Methodology = new Methodology
                        {
                            Versions = ListOf(methodologyVersion),
                        },
                        Owner = true
                    },
                },
                Releases = new List<Release>
                {
                    new Release
                    {
                        ReleaseName = "1999",
                        TimePeriodCoverage = TimeIdentifier.Week1,
                    },
                    new Release
                    {
                        ReleaseName = "2000",
                        TimePeriodCoverage = TimeIdentifier.Week1,
                    },
                    new Release
                    {
                        ReleaseName = "2000",
                        TimePeriodCoverage = TimeIdentifier.Week2,
                    },
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
                var publicationRepository = new PublicationRepository(context);
                var publicationService = BuildPublicationService(context, publicationRepository: publicationRepository);
                var result = await publicationService.GetMyPublication(publication.Id);
                var viewModel = result.AssertRight();

                Assert.Equal(publication.Id, viewModel.Id);
                Assert.Equal(publication.Title, viewModel.Title);
                Assert.Equal(publication.Summary, viewModel.Summary);

                Assert.Equal(publication.Topic.Id, viewModel.TopicId);
                Assert.Equal(publication.Topic.ThemeId, viewModel.ThemeId);

                Assert.Equal(publication.Contact.Id, viewModel.Contact.Id);
                Assert.Equal(publication.Contact.ContactName, viewModel.Contact.ContactName);
                Assert.Equal(publication.Contact.ContactTelNo, viewModel.Contact.ContactTelNo);
                Assert.Equal(publication.Contact.TeamEmail, viewModel.Contact.TeamEmail);
                Assert.Equal(publication.Contact.TeamName, viewModel.Contact.TeamName);

                Assert.Single(viewModel.Methodologies);
                Assert.Equal(methodologyVersion.Id, viewModel.Methodologies[0].Id);
                Assert.Equal(methodologyVersion.Title, viewModel.Methodologies[0].Title);

                Assert.Equal(3, viewModel.Releases.Count);
                var releases = viewModel.Releases;
                Assert.Equal("2000", releases[0].YearTitle);
                Assert.Equal(TimeIdentifier.Week2, releases[0].TimePeriodCoverage);
                Assert.Equal("2000", releases[1].YearTitle);
                Assert.Equal(TimeIdentifier.Week1, releases[1].TimePeriodCoverage);
                Assert.Equal("1999", releases[2].YearTitle);
                Assert.Equal(TimeIdentifier.Week1, releases[2].TimePeriodCoverage);
            }
        }

        [Fact]
        public async Task GetMyPublication_CanViewSpecificPublication()
        {
            var userId = Guid.NewGuid();
            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                AlternativeTitle = "Methodology 1 Version 1",
                Version = 0,
                Status = Draft
            };
            var release = new Release
            {
                Id = Guid.NewGuid(),
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
            };
            var userReleaseRole = new UserReleaseRole
            {
                UserId = userId,
                ReleaseId = release.Id,
            };
            var publication = new Publication
            {
                Title = "Test publication",
                Summary = "Test publication summary",
                Slug = "test-publication",
                Topic = new Topic
                {
                    Title = "Test topic",
                    Theme = new Theme
                    {
                        Title = "Test theme"
                    }
                },
                Contact = new Contact
                {
                    ContactName = "Test contact",
                    ContactTelNo = "0123456789",
                    TeamName = "Test team",
                    TeamEmail = "team@test.com",
                },
                Releases = ListOf(release),
                Methodologies = new List<PublicationMethodology>
                {
                    new()
                    {
                        Methodology = new Methodology
                        {
                            Versions = ListOf(methodologyVersion),
                        },
                        Owner = true
                    },
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication, userReleaseRole);
                await context.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(Strict);

            userService.Setup(s => s.GetUserId())
                .Returns(userId);
            userService.Setup(s => s.MatchesPolicy(CanAccessSystem))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanViewAllReleases))
                .ReturnsAsync(false);
            userService.Setup(s =>
                    s.MatchesPolicy(It.Is<Publication>(p => p.Id == publication.Id), CanViewSpecificPublication))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<Release>(), CanAssignPrereleaseContactsToSpecificRelease))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<Release>(), CanUpdateSpecificRelease))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<Release>(), CanDeleteSpecificRelease))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<Release>(), CanMakeAmendmentOfSpecificRelease))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanDeleteSpecificMethodology))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanUpdateSpecificMethodology))
                .ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanApproveSpecificMethodology))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanMarkSpecificMethodologyAsDraft))
                .ReturnsAsync(true);
            userService.Setup(s =>
                    s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanMakeAmendmentOfSpecificMethodology))
                .ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(It.IsAny<PublicationMethodology>(), CanDropMethodologyLink))
                .ReturnsAsync(true);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationRepository = new PublicationRepository(context);
                var publicationService = BuildPublicationService(context,
                    publicationRepository: publicationRepository,
                    userService: userService.Object);

                var result = await publicationService.GetMyPublication(publication.Id);

                VerifyAllMocks(userService);

                var viewModel = result.AssertRight();

                Assert.Equal(publication.Id, viewModel.Id);
                Assert.Equal(publication.Title, viewModel.Title);
                Assert.Equal(publication.Summary, viewModel.Summary);

                Assert.Equal(publication.Topic.Id, viewModel.TopicId);
                Assert.Equal(publication.Topic.ThemeId, viewModel.ThemeId);

                Assert.Equal(publication.Contact.Id, viewModel.Contact.Id);
                Assert.Equal(publication.Contact.ContactName, viewModel.Contact.ContactName);
                Assert.Equal(publication.Contact.ContactTelNo, viewModel.Contact.ContactTelNo);
                Assert.Equal(publication.Contact.TeamEmail, viewModel.Contact.TeamEmail);
                Assert.Equal(publication.Contact.TeamName, viewModel.Contact.TeamName);

                Assert.Single(viewModel.Releases);
                Assert.Equal(release.Id, viewModel.Releases[0].Id);

                Assert.Single(viewModel.Methodologies);
                Assert.Equal(methodologyVersion.Id, viewModel.Methodologies[0].Id);
                Assert.Equal(methodologyVersion.Title, viewModel.Methodologies[0].Title);
            }
        }

        [Fact]
        public async Task GetMyPublication_No_CanViewSpecificPublication()
        {
            var userId = Guid.NewGuid();
            var publication = new Publication();

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(Strict);

            userService
                .Setup(s => s.GetUserId())
                .Returns(userId);

            userService
                .Setup(s => s.MatchesPolicy(CanAccessSystem))
                .ReturnsAsync(true);

            userService
                .Setup(s => s.MatchesPolicy(CanViewAllReleases))
                .ReturnsAsync(false);

            userService
                .Setup(s =>
                    s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        CanViewSpecificPublication))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationRepository = new PublicationRepository(context);
                var publicationService = BuildPublicationService(context,
                    publicationRepository: publicationRepository,
                    userService: userService.Object);

                var result = await publicationService.GetMyPublication(publication.Id);

                VerifyAllMocks(userService);

                result.AssertForbidden();
            }
        }

        [Fact]
        public async Task GetMyPublication_NotFound()
        {
            await using var context = InMemoryApplicationDbContext();
            var publicationService = BuildPublicationService(context);

            var result = await publicationService.GetMyPublication(Guid.NewGuid());
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
                    new PublicationSaveRequest
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
                new PublicationSaveRequest
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
                    new PublicationSaveRequest
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
                        Contact = new ContactSaveViewModel
                        {
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789",
                            TeamName = "Test team",
                            TeamEmail = "john.smith@test.com",
                        },
                        TopicId = topic.Id,
                        SupersededById = newSupersededById,
                    }
                );

                VerifyAllMocks(methodologyVersionRepository);

                var viewModel = result.AssertRight();

                Assert.Equal("New title", viewModel.Title);
                Assert.Equal("New summary", viewModel.Summary);

                Assert.Equal("John Smith", viewModel.Contact.ContactName);
                Assert.Equal("0123456789", viewModel.Contact.ContactTelNo);
                Assert.Equal("Test team", viewModel.Contact.TeamName);
                Assert.Equal("john.smith@test.com", viewModel.Contact.TeamEmail);

                Assert.Equal(topic.Id, viewModel.Topic.Id);
                Assert.Equal(topic.Title, viewModel.Topic.Title);

                Assert.Equal(newSupersededById, viewModel.SupersededById);

                // Do an in depth check of the saved release
                var updatedPublication = await context.Publications.FindAsync(viewModel.Id);

                Assert.NotNull(updatedPublication);
                Assert.False(updatedPublication!.Live);
                Assert.True(updatedPublication.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedPublication.Updated!.Value).Milliseconds, 0, 1500);
                Assert.Equal("new-title", updatedPublication.Slug);
                Assert.Equal("New title", updatedPublication.Title);
                Assert.Equal(newSupersededById, updatedPublication.SupersededById);

                Assert.Equal("John Smith", updatedPublication.Contact.ContactName);
                Assert.Equal("0123456789", updatedPublication.Contact.ContactTelNo);
                Assert.Equal("Test team", updatedPublication.Contact.TeamName);
                Assert.Equal("john.smith@test.com", updatedPublication.Contact.TeamEmail);

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
                Published = new DateTime(2020, 8, 12),
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
                var themeCacheService = new Mock<IThemeCacheService>(Strict);
                var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
                var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

                publicationCacheService.Setup(mock => mock.UpdatePublication(publication.Slug))
                    .ReturnsAsync(new PublicationViewModel());

                publicationCacheService.Setup(mock => mock.UpdatePublication(supersededPublication.Slug))
                    .ReturnsAsync(new PublicationViewModel());

                themeCacheService.Setup(mock => mock.UpdatePublicationTree())
                    .ReturnsAsync(new List<ThemeTree<PublicationTreeNode>>());

                methodologyCacheService.Setup(mock => mock.UpdateSummariesTree())
                    .ReturnsAsync(new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(
                        new List<AllMethodologiesThemeViewModel>()));

                var publicationService = BuildPublicationService(context,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    publicationCacheService: publicationCacheService.Object,
                    themeCacheService: themeCacheService.Object,
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
                        Contact = new ContactSaveViewModel
                        {
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789",
                            TeamName = "Test team",
                            TeamEmail = "john.smith@test.com",
                        },
                        TopicId = topic.Id,
                        SupersededById = newSupersededById,
                    }
                );

                VerifyAllMocks(methodologyVersionRepository,
                    methodologyCacheService,
                    publicationCacheService,
                    themeCacheService);

                var viewModel = result.AssertRight();

                Assert.Equal("New title", viewModel.Title);
                Assert.Equal("New summary", viewModel.Summary);

                Assert.Equal("John Smith", viewModel.Contact.ContactName);
                Assert.Equal("0123456789", viewModel.Contact.ContactTelNo);
                Assert.Equal("Test team", viewModel.Contact.TeamName);
                Assert.Equal("john.smith@test.com", viewModel.Contact.TeamEmail);

                Assert.Equal(topic.Id, viewModel.Topic.Id);
                Assert.Equal(topic.Title, viewModel.Topic.Title);

                Assert.Equal(newSupersededById, viewModel.SupersededById);

                // Do an in depth check of the saved release
                var updatedPublication = await context.Publications.FindAsync(viewModel.Id);

                Assert.NotNull(updatedPublication);
                Assert.True(updatedPublication!.Live);
                Assert.True(updatedPublication.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedPublication.Updated!.Value).Milliseconds, 0, 1500);
                // Slug remains unchanged
                Assert.Equal("old-title", updatedPublication.Slug);
                Assert.Equal("New title", updatedPublication.Title);
                Assert.Equal("New summary", updatedPublication.Summary);

                Assert.Equal("John Smith", updatedPublication.Contact.ContactName);
                Assert.Equal("0123456789", updatedPublication.Contact.ContactTelNo);
                Assert.Equal("Test team", updatedPublication.Contact.TeamName);
                Assert.Equal("john.smith@test.com", updatedPublication.Contact.TeamEmail);

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
                        Contact = new ContactSaveViewModel
                        {
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789",
                            TeamName = "Test team",
                            TeamEmail = "john.smith@test.com",
                        },
                        TopicId = topic.Id,
                        SupersededById = publication.SupersededById,
                    }
                );

                VerifyAllMocks(methodologyVersionRepository);

                var viewModel = result.AssertRight();

                Assert.Equal("Old title", viewModel.Title);
                Assert.Equal("New summary", viewModel.Summary);
                Assert.Equal("John Smith", viewModel.Contact.ContactName);
                Assert.Equal(publication.SupersededById, viewModel.SupersededById);
            }
        }

        [Fact]
        public async Task UpdatePublication_SavesNewContact()
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
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

                methodologyVersionRepository.Setup(mock =>
                        mock.PublicationTitleChanged(
                            publication.Id,
                            publication.Slug,
                            "New title",
                            "new-slug"))
                    .Returns(Task.CompletedTask);

                var publicationService = BuildPublicationService(context,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveRequest
                    {
                        Title = "New title",
                        Slug = "new-slug",
                        Contact = new ContactSaveViewModel
                        {
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789",
                            TeamName = "Test team",
                            TeamEmail = "john.smith@test.com",
                        },
                        TopicId = publication.TopicId
                    }
                );

                VerifyAllMocks(methodologyVersionRepository);

                var viewModel = result.AssertRight();

                var updatedPublication = await context.Publications.FindAsync(viewModel.Id);

                Assert.NotNull(updatedPublication);
                Assert.Equal("John Smith", updatedPublication!.Contact.ContactName);
                Assert.Equal("0123456789", updatedPublication.Contact.ContactTelNo);
                Assert.Equal("Test team", updatedPublication.Contact.TeamName);
                Assert.Equal("john.smith@test.com", updatedPublication.Contact.TeamEmail);
            }
        }

        [Fact]
        public async Task UpdatePublication_SavesNewContactWhenSharedWithOtherPublication()
        {
            var sharedContact = new Contact
            {
                Id = Guid.NewGuid(),
                ContactName = "Old name",
                ContactTelNo = "0987654321",
                TeamName = "Old team",
                TeamEmail = "old.smith@test.com",
            };
            var publication = new Publication
            {
                Title = "Test publication",
                Slug = "test-publication",
                Topic = new Topic
                {
                    Title = "Test topic",
                    Theme = new Theme(),
                },
                Contact = sharedContact
            };
            var otherPublication = new Publication
            {
                Title = "Other publication",
                Summary = "Other publication summary",
                Contact = sharedContact
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication, otherPublication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

                methodologyVersionRepository.Setup(mock =>
                        mock.PublicationTitleChanged(
                            publication.Id,
                            publication.Slug,
                            "New title",
                            "new-title"))
                    .Returns(Task.CompletedTask);

                var publicationService = BuildPublicationService(context,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveRequest
                    {
                        Title = "New title",
                        Contact = new ContactSaveViewModel
                        {
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789",
                            TeamName = "Test team",
                            TeamEmail = "john.smith@test.com",
                        },
                        TopicId = publication.TopicId
                    }
                );

                VerifyAllMocks(methodologyVersionRepository);

                var viewModel = result.AssertRight();

                var updatedPublication = await context.Publications.FindAsync(viewModel.Id);

                Assert.NotNull(updatedPublication);
                Assert.NotEqual(sharedContact.Id, updatedPublication!.Contact.Id);
                Assert.Equal("John Smith", updatedPublication.Contact.ContactName);
                Assert.Equal("0123456789", updatedPublication.Contact.ContactTelNo);
                Assert.Equal("Test team", updatedPublication.Contact.TeamName);
                Assert.Equal("john.smith@test.com", updatedPublication.Contact.TeamEmail);
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
                Published = DateTime.UtcNow,
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
                var themeCacheService = new Mock<IThemeCacheService>(Strict);
                var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
                var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

                publicationCacheService.Setup(mock =>
                        mock.UpdatePublication(publication.Slug))
                    .ReturnsAsync(new PublicationViewModel());

                publicationCacheService.Setup(mock =>
                        mock.UpdatePublication(supersededPublication1.Slug))
                    .ReturnsAsync(new PublicationViewModel());

                publicationCacheService.Setup(mock =>
                        mock.UpdatePublication(supersededPublication2.Slug))
                    .ReturnsAsync(new PublicationViewModel());

                themeCacheService.Setup(mock => mock.UpdatePublicationTree())
                    .ReturnsAsync(new List<ThemeTree<PublicationTreeNode>>());

                methodologyCacheService.Setup(mock => mock.UpdateSummariesTree())
                    .ReturnsAsync(
                        new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(
                            new List<AllMethodologiesThemeViewModel>()));

                var publicationService = BuildPublicationService(context,
                    publicationCacheService: publicationCacheService.Object,
                    themeCacheService: themeCacheService.Object,
                    methodologyCacheService: methodologyCacheService.Object);

                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveRequest
                    {
                        Title = "Test title",
                        Slug = "test-slug",
                        Contact = new ContactSaveViewModel
                        {
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789",
                            TeamName = "Test team",
                            TeamEmail = "john.smith@test.com",
                        },
                        TopicId = publication.TopicId,
                    }
                );

                VerifyAllMocks(themeCacheService,
                    methodologyCacheService,
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
                Topic = topic
            };
            var otherPublication = new Publication
            {
                Title = "Duplicated title",
                Slug = "duplicated-title",
                Topic = topic
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
                    .ReturnsAsync(new PublicationViewModel());

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
                    .ReturnsAsync(new PublicationViewModel());

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
                var service = BuildPublicationService(context: contentDbContext);

                var result = await service.UpdateExternalMethodology(
                    publication.Id,
                    new ExternalMethodology
                    {
                        Title = "New external methodology",
                        Url = "http://test.external.methodology/new",
                    });
                var externalMethodology = result.AssertRight();

                Assert.Equal("New external methodology", externalMethodology.Title);
                Assert.Equal("http://test.external.methodology/new", externalMethodology.Url);

                var dbPublication = contentDbContext.Publications
                    .Single(p => p.Id == publication.Id);

                Assert.NotNull(dbPublication.ExternalMethodology);
                Assert.Equal("New external methodology", dbPublication!.ExternalMethodology!.Title);
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
                new ExternalMethodology
                {
                    Title = "New external methodology",
                    Url = "http://test.external.methodology/new",
                });

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
            IThemeCacheService? themeCacheService = null,
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
                methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict),
                themeCacheService ?? Mock.Of<IThemeCacheService>(Strict));
        }
    }
}
