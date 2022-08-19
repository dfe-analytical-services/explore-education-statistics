#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationServiceTests
    {
        [Fact]
        public async Task GetPublication()
        {
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
                Methodologies = AsList(
                    new PublicationMethodology
                    {
                        Methodology = new Methodology
                        {
                            Slug = "methodology-1-slug",
                            Versions = AsList(methodology1Version1)
                        },
                        Owner = true
                    },
                    new PublicationMethodology
                    {
                        Methodology = new Methodology
                        {
                            Slug = "methodology-2-slug",
                            Versions = AsList(methodology2Version1, methodology2Version2)
                        },
                        Owner = false
                    }
                )
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

                Assert.Equal(publication.Topic.Id, result.TopicId);
                Assert.Equal(publication.Topic.ThemeId, result.ThemeId);

                Assert.Equal(publication.Contact.Id, result.Contact.Id);
                Assert.Equal(publication.Contact.ContactName, result.Contact.ContactName);
                Assert.Equal(publication.Contact.ContactTelNo, result.Contact.ContactTelNo);
                Assert.Equal(publication.Contact.TeamEmail, result.Contact.TeamEmail);
                Assert.Equal(publication.Contact.TeamName, result.Contact.TeamName);

                Assert.Equal(2, result.Methodologies.Count);
                
                Assert.Equal(methodology1Version1.Id, result.Methodologies[0].Id);
                Assert.Equal(methodology1Version1.Title, result.Methodologies[0].Title);
                
                Assert.Equal(methodology2Version2.Id, result.Methodologies[1].Id);
                Assert.Equal(methodology2Version2.Title, result.Methodologies[1].Title);
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
                    new PublicationMethodology
                    {
                        Methodology = new Methodology
                        {
                            Slug = "methodology-1-slug",
                            Versions = new List<MethodologyVersion>
                            {
                                methodologyVersion,
                            },
                        },
                        Owner = true
                    },
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
                var publicationRepository = new PublicationRepository(context, AdminMapper());
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
                Assert.Equal(methodologyVersion.Id, viewModel.Methodologies[0].Methodology.Id);
                Assert.Equal(methodologyVersion.Title, viewModel.Methodologies[0].Methodology.Title);
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
                Releases = new List<Release>
                {
                    release,
                },
                Methodologies = new List<PublicationMethodology>
                {
                    new PublicationMethodology
                    {
                        Methodology = new Methodology
                        {
                            Slug = "methodology-1-slug",
                            Versions = new List<MethodologyVersion>
                            {
                                methodologyVersion,
                            },
                        },
                        Owner = true
                    },
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(publication, userReleaseRole);
                await context.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(Strict);

            userService
                .Setup(s => s.GetUserId())
                .Returns(userId);

            userService
                .Setup(s => s.MatchesPolicy(SecurityPolicies.CanAccessSystem))
                .ReturnsAsync(true);

            userService
                .Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases))
                .ReturnsAsync(false);

            userService
                .Setup(s =>
                    s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        SecurityPolicies.CanViewSpecificPublication))
                .ReturnsAsync(true);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationRepository = new PublicationRepository(context, AdminMapper());
                var publicationService = BuildPublicationService(context,
                    publicationRepository: publicationRepository,
                    userService: userService.Object);
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

                Assert.Single(viewModel.Releases);
                Assert.Equal(release.Id, viewModel.Releases[0].Id);

                Assert.Single(viewModel.Methodologies);
                Assert.Equal(methodologyVersion.Id, viewModel.Methodologies[0].Methodology.Id);
                Assert.Equal(methodologyVersion.Title, viewModel.Methodologies[0].Methodology.Title);
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
                .Setup(s => s.MatchesPolicy(SecurityPolicies.CanAccessSystem))
                .ReturnsAsync(true);

            userService
                .Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases))
                .ReturnsAsync(false);

            userService
                .Setup(s =>
                    s.MatchesPolicy(
                        It.Is<Publication>(p => p.Id == publication.Id),
                        SecurityPolicies.CanViewSpecificPublication))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationRepository = new PublicationRepository(context, AdminMapper());
                var publicationService = BuildPublicationService(context,
                    publicationRepository: publicationRepository,
                    userService: userService.Object);
                var result = await publicationService.GetMyPublication(publication.Id);
                var actionResult = result.AssertLeft();
                Assert.IsType<ForbidResult>(actionResult);
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

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
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
                Title = "Test topic"
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
                    new PublicationSaveViewModel
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

                Assert.Equal(topic.Id, publicationViewModel.TopicId);

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
                new PublicationSaveViewModel
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
                    new PublicationSaveViewModel
                    {
                        Title = "Test publication",
                        TopicId = topic.Id
                    }
                );

                result.AssertBadRequest(SlugNotUnique);
            }
        }

        [Fact]
        public async Task UpdatePublication()
        {
            var topic = new Topic
            {
                Title = "New topic"
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
                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                // NOTE: No publicBlobCacheService Setup required for DeleteItem, because publication.Live is false

                var publicationService = BuildPublicationService(context,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    publicBlobCacheService: publicBlobCacheService.Object);

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
                    new PublicationSaveViewModel
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

                VerifyAllMocks(methodologyVersionRepository, publicBlobCacheService);

                var viewModel = result.AssertRight();

                Assert.Equal("New title", viewModel.Title);
                Assert.Equal("New summary", viewModel.Summary);

                Assert.Equal("John Smith", viewModel.Contact.ContactName);
                Assert.Equal("0123456789", viewModel.Contact.ContactTelNo);
                Assert.Equal("Test team", viewModel.Contact.TeamName);
                Assert.Equal("john.smith@test.com", viewModel.Contact.TeamEmail);

                Assert.Equal(topic.Id, viewModel.TopicId);

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
                Title = "New topic"
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
                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                publicBlobCacheService.Setup(mock => mock.DeleteItem(It.IsAny<AllMethodologiesCacheKey>()))
                    .Returns(Task.CompletedTask);
                publicBlobCacheService.Setup(mock => mock.DeleteItem(new PublicationTreeCacheKey()))
                    .Returns(Task.CompletedTask);
                publicBlobCacheService.Setup(mock => mock.DeleteItem(new PublicationCacheKey(publication.Slug)))
                    .Returns(Task.CompletedTask);
                publicBlobCacheService.Setup(mock => mock.DeleteItem(new PublicationCacheKey(supersededPublication.Slug)))
                    .Returns(Task.CompletedTask);

                var publicationService = BuildPublicationService(context,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    publicBlobCacheService: publicBlobCacheService.Object);

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
                    new PublicationSaveViewModel
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

                VerifyAllMocks(methodologyVersionRepository, publicBlobCacheService);

                var viewModel = result.AssertRight();

                Assert.Equal("New title", viewModel.Title);
                Assert.Equal("New summary", viewModel.Summary);

                Assert.Equal("John Smith", viewModel.Contact.ContactName);
                Assert.Equal("0123456789", viewModel.Contact.ContactTelNo);
                Assert.Equal("Test team", viewModel.Contact.TeamName);
                Assert.Equal("john.smith@test.com", viewModel.Contact.TeamEmail);

                Assert.Equal(topic.Id, viewModel.TopicId);

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
                Title = "New topic"
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
                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                // NOTE: No publicBlobCacheService Setup for DeleteItem, as blob not deleted as publication.Live is false

                var publicationService = BuildPublicationService(context,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    publicBlobCacheService: publicBlobCacheService.Object);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel
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

                VerifyAllMocks(methodologyVersionRepository, publicBlobCacheService);

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
                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                // NOTE: No setup for publicBlobCacheService for DeleteItem, as blob not deleted as publication.Live is false

                var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

                methodologyVersionRepository.Setup(mock =>
                    mock.PublicationTitleChanged(
                        publication.Id,
                        publication.Slug,
                        "New title",
                        "new-slug"))
                    .Returns(Task.CompletedTask);

                var publicationService = BuildPublicationService(context,
                    publicBlobCacheService: publicBlobCacheService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel
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
                    Title = "Test topic"
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
                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                // NOTE: No publicBlobCacheService Setup for DeleteItem, as blob not deleted as publication.Live is false

                var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

                methodologyVersionRepository.Setup(mock =>
                    mock.PublicationTitleChanged(
                        publication.Id,
                        publication.Slug,
                        "New title",
                        "new-title"))
                    .Returns(Task.CompletedTask);

                var publicationService = BuildPublicationService(context,
                    publicBlobCacheService: publicBlobCacheService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel
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
                    Title = "Test topic"
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
                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                publicBlobCacheService.Setup(mock =>
                        mock.DeleteItem(new AllMethodologiesCacheKey()))
                    .Returns(Task.CompletedTask);

                publicBlobCacheService.Setup(mock =>
                        mock.DeleteItem(new PublicationTreeCacheKey()))
                    .Returns(Task.CompletedTask);

                publicBlobCacheService.Setup(mock =>
                        mock.DeleteItem(new PublicationCacheKey(publication.Slug)))
                    .Returns(Task.CompletedTask);

                publicBlobCacheService.Setup(mock =>
                        mock.DeleteItem(new PublicationCacheKey(supersededPublication1.Slug)))
                    .Returns(Task.CompletedTask);

                publicBlobCacheService.Setup(mock =>
                        mock.DeleteItem(new PublicationCacheKey(supersededPublication2.Slug)))
                    .Returns(Task.CompletedTask);

                var publicationService = BuildPublicationService(context,
                    publicBlobCacheService: publicBlobCacheService.Object);

                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel
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
                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                var publicationService = BuildPublicationService(context,
                    publicBlobCacheService: publicBlobCacheService.Object);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel
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
                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                var publicationService = BuildPublicationService(context,
                    publicBlobCacheService: publicBlobCacheService.Object);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel
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
                LegacyReleases = new List<LegacyRelease>
                {
                    new LegacyRelease
                    {
                        Description = "Test description 1",
                        Url = "https://test1.com",
                        Order = 1,
                    },
                    new LegacyRelease
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
                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                publicBlobCacheService.Setup(mock => mock.DeleteItem(It.IsAny<PublicationCacheKey>()))
                    .Returns(Task.CompletedTask);

                var publicationService = BuildPublicationService(context,
                    publicBlobCacheService: publicBlobCacheService.Object);

                var result = await publicationService.PartialUpdateLegacyReleases(
                    publication.Id,
                    new List<LegacyReleasePartialUpdateViewModel>
                    {
                        new LegacyReleasePartialUpdateViewModel
                        {
                            Id = publication.LegacyReleases[0].Id,
                            Description = "Updated description 1",
                            Url = "https://updated-test1.com",
                            Order = 3
                        }
                    }
                );

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
                LegacyReleases = new List<LegacyRelease>
                {
                    new LegacyRelease
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
                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                publicBlobCacheService.Setup(mock => mock.DeleteItem(It.IsAny<PublicationCacheKey>()))
                    .Returns(Task.CompletedTask);

                var publicationService = BuildPublicationService(context,
                    publicBlobCacheService: publicBlobCacheService.Object);

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

                var legacyReleases = result.AssertRight();

                Assert.Single(legacyReleases);

                Assert.Equal(publication.LegacyReleases[0].Id, legacyReleases[0].Id);
                Assert.Equal("Updated description 1", legacyReleases[0].Description);
                Assert.Equal("https://test1.com", legacyReleases[0].Url);
                Assert.Equal(1, legacyReleases[0].Order);
            }
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
                await context.AddRangeAsync(publication, release1Original, release1Amendment);
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

        private static PublicationService BuildPublicationService(
            ContentDbContext context,
            IUserService? userService = null,
            IPublicationRepository? publicationRepository = null,
            IMethodologyVersionRepository? methodologyVersionRepository = null,
            IBlobCacheService? publicBlobCacheService = null)
        {
            return new(
                context,
                AdminMapper(),
                new PersistenceHelper<ContentDbContext>(context),
                userService ?? AlwaysTrueUserService().Object, 
                publicationRepository ?? Mock.Of<IPublicationRepository>(Strict),
                methodologyVersionRepository ?? Mock.Of<IMethodologyVersionRepository>(Strict),
                publicBlobCacheService ?? Mock.Of<IBlobCacheService>(Strict));
        }
    }
}
