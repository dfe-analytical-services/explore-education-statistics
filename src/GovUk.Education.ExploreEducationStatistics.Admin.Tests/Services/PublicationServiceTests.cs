#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
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
                },
                LegacyReleases = new List<LegacyRelease>
                {
                    new LegacyRelease
                    {
                        Description = "Legacy Release A",
                        Url = "legacy-release-a-url",
                        Order = 3,
                    },
                    new LegacyRelease
                    {
                        Description = "Legacy Release C",
                        Url = "legacy-release-c-url",
                        Order = 1,
                    },
                    new LegacyRelease
                    {
                        Description = "Legacy Release B",
                        Url = "legacy-release-b-url",
                        Order = 2,
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

                Assert.Equal(3, viewModel.LegacyReleases.Count);
                var legacyReleases = publication.LegacyReleases
                    .OrderByDescending(lr => lr.Order)
                    .ToList();
                Assert.Equal(legacyReleases[0].Id, viewModel.LegacyReleases[0].Id); // Legacy Release A
                Assert.Equal(legacyReleases[1].Id, viewModel.LegacyReleases[1].Id); // Legacy Release B
                Assert.Equal(legacyReleases[2].Id, viewModel.LegacyReleases[2].Id); // Legacy Release C
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
                },
                LegacyReleases = new List<LegacyRelease>
                {
                    // In result, should appear in order A, B, C
                    new LegacyRelease
                    {
                        Description = "Legacy Release A",
                        Url = "legacy-release-a-url",
                        Order = 3,
                    },
                    new LegacyRelease
                    {
                        Description = "Legacy Release C",
                        Url = "legacy-release-c-url",
                        Order = 1,
                    },
                    new LegacyRelease
                    {
                        Description = "Legacy Release B",
                        Url = "legacy-release-b-url",
                        Order = 2,
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

                Assert.Equal(3, viewModel.LegacyReleases.Count);
                var legacyReleases = publication.LegacyReleases
                    .OrderByDescending(lr => lr.Order)
                    .ToList();
                Assert.Equal(legacyReleases[0].Id, viewModel.LegacyReleases[0].Id); // Legacy Release A
                Assert.Equal(legacyReleases[1].Id, viewModel.LegacyReleases[1].Id); // Legacy Release B
                Assert.Equal(legacyReleases[2].Id, viewModel.LegacyReleases[2].Id); // Legacy Release C
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

                var publicationViewModel = result.Right;
                Assert.Equal("Test publication", publicationViewModel.Title);

                Assert.Equal("John Smith", publicationViewModel.Contact.ContactName);
                Assert.Equal("0123456789", publicationViewModel.Contact.ContactTelNo);
                Assert.Equal("Test team", publicationViewModel.Contact.TeamName);
                Assert.Equal("john.smith@test.com", publicationViewModel.Contact.TeamEmail);

                Assert.Equal(topic.Id, publicationViewModel.TopicId);

                // Do an in depth check of the saved release
                var createdPublication = await context.Publications.FindAsync(publicationViewModel.Id);
                Assert.False(createdPublication.Live);
                Assert.Equal("test-publication", createdPublication.Slug);
                Assert.False(createdPublication.Updated.HasValue);
                Assert.Equal("Test publication", createdPublication.Title);

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

                var publicationService = BuildPublicationService(context, methodologyVersionRepository: methodologyVersionRepository.Object);

                methodologyVersionRepository
                    .Setup(s => s.PublicationTitleChanged(publication.Id, publication.Slug, "New title", "new-title"))
                    .Returns(Task.CompletedTask);

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
                        TopicId = topic.Id
                    }
                );
                
                VerifyAllMocks(methodologyVersionRepository);

                Assert.Equal("New title", result.Right.Title);

                Assert.Equal("John Smith", result.Right.Contact.ContactName);
                Assert.Equal("0123456789", result.Right.Contact.ContactTelNo);
                Assert.Equal("Test team", result.Right.Contact.TeamName);
                Assert.Equal("john.smith@test.com", result.Right.Contact.TeamEmail);

                Assert.Equal(topic.Id, result.Right.TopicId);

                // Do an in depth check of the saved release
                var updatedPublication = await context.Publications.FindAsync(result.Right.Id);
                Assert.False(updatedPublication.Live);
                Assert.True(updatedPublication.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedPublication.Updated.Value).Milliseconds, 0, 1500);
                Assert.Equal("new-title", updatedPublication.Slug);
                Assert.Equal("New title", updatedPublication.Title);

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

            var publication = new Publication
            {
                Slug = "old-title",
                Title = "Old title",
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
                
                var publicationService = BuildPublicationService(context, methodologyVersionRepository: methodologyVersionRepository.Object);

                // Expect the title to change but not the slug, as the Publication is already published. 
                methodologyVersionRepository
                    .Setup(s => s.PublicationTitleChanged(publication.Id, publication.Slug, "New title", "old-title"))
                    .Returns(Task.CompletedTask);
                
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
                        TopicId = topic.Id
                    }
                );
                
                VerifyAllMocks(methodologyVersionRepository);

                Assert.Equal("New title", result.Right.Title);

                Assert.Equal("John Smith", result.Right.Contact.ContactName);
                Assert.Equal("0123456789", result.Right.Contact.ContactTelNo);
                Assert.Equal("Test team", result.Right.Contact.TeamName);
                Assert.Equal("john.smith@test.com", result.Right.Contact.TeamEmail);

                Assert.Equal(topic.Id, result.Right.TopicId);

                // Do an in depth check of the saved release
                var updatedPublication = await context.Publications.FindAsync(result.Right.Id);
                Assert.True(updatedPublication.Live);
                Assert.True(updatedPublication.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedPublication.Updated.Value).Milliseconds, 0, 1500);
                // Slug remains unchanged
                Assert.Equal("old-title", updatedPublication.Slug);
                Assert.Equal("New title", updatedPublication.Title);

                Assert.Equal("John Smith", updatedPublication.Contact.ContactName);
                Assert.Equal("0123456789", updatedPublication.Contact.ContactTelNo);
                Assert.Equal("Test team", updatedPublication.Contact.TeamName);
                Assert.Equal("john.smith@test.com", updatedPublication.Contact.TeamEmail);

                Assert.Equal(topic.Id, updatedPublication.TopicId);
                Assert.Equal("New topic", updatedPublication.Topic.Title);
            }
        }

                [Fact]
        public async void UpdatePublication_NoTitleChange()
        {
            var topic = new Topic
            {
                Title = "New topic"
            };

            var publication = new Publication
            {
                Title = "Old title",
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
                
                var publicationService = BuildPublicationService(context, methodologyVersionRepository: methodologyVersionRepository.Object);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel
                    {
                        Title = "Old title",
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
                
                VerifyAllMocks(methodologyVersionRepository);

                Assert.Equal("Old title", result.Right.Title);
                Assert.Equal("John Smith", result.Right.Contact.ContactName);
            }
        }

        [Fact]
        public async Task UpdatePublication_SavesNewContact()
        {
            var publication = new Publication
            {
                Title = "Test title",
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

                var updatedPublication = await context.Publications.FindAsync(result.Right.Id);

                Assert.Equal("John Smith", updatedPublication.Contact.ContactName);
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
                Topic = new Topic
                {
                    Title = "Test topic"
                },
                Contact = sharedContact
            };
            var otherPublication = new Publication
            {
                Title = "Other publication",
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
                var publicationService = BuildPublicationService(context);

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

                var updatedPublication = await context.Publications.FindAsync(result.Right.Id);

                Assert.NotEqual(sharedContact.Id, updatedPublication.Contact.Id);
                Assert.Equal("John Smith", updatedPublication.Contact.ContactName);
                Assert.Equal("0123456789", updatedPublication.Contact.ContactTelNo);
                Assert.Equal("Test team", updatedPublication.Contact.TeamName);
                Assert.Equal("john.smith@test.com", updatedPublication.Contact.TeamEmail);
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
        public async Task UpdatePublication_NoPermissionToChangeTitle()
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
                        SecurityPolicies.CanUpdateSpecificPublication))
                .ReturnsAsync(true);
            userService
                .Setup(s =>
                    s.MatchesPolicy(SecurityPolicies.CanUpdatePublicationTitles))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context, userService: userService.Object);

                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel
                    {
                        Title = "New publication title",
                    }
                );

                var actionResult = result.AssertLeft();
                Assert.IsType<ForbidResult>(actionResult);
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
                        SecurityPolicies.CanUpdateSpecificPublication))
                .ReturnsAsync(true);

            userService
                .Setup(s =>
                    s.MatchesPolicy(
                        It.Is<Topic>(t => t.Id == newTopic.Id),
                        SecurityPolicies.CanCreatePublicationForSpecificTopic))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context, userService: userService.Object);

                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel
                    {
                        Title = "Publication title",
                        TopicId = newTopic.Id,
                    }
                );

                var actionResult = result.AssertLeft();
                Assert.IsType<ForbidResult>(actionResult);
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
                var publicationService = BuildPublicationService(context);

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

                var legacyReleases = result.Right;

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
                var publicationService = BuildPublicationService(context);

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

                var legacyReleases = result.Right;

                Assert.Single(legacyReleases);

                Assert.Equal(publication.LegacyReleases[0].Id, legacyReleases[0].Id);
                Assert.Equal("Updated description 1", legacyReleases[0].Description);
                Assert.Equal("https://test1.com", legacyReleases[0].Url);
                Assert.Equal(1, legacyReleases[0].Order);
            }
        }

        [Fact]
        public async Task GetLatestReleaseVersions()
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

                var result = await publicationService.GetLatestReleaseVersions(
                    publication.Id);

                var releases = result.AssertRight();

                Assert.Equal(3, releases.Count);

                Assert.Equal(release3Amendment.Id, releases[0].Id);
                Assert.Equal(release2.Id, releases[1].Id);
                Assert.Equal(release1Amendment.Id, releases[2].Id);
            }
        }

        private static PublicationService BuildPublicationService(ContentDbContext context,
            IUserService? userService = null,
            IPublicationRepository? publicationRepository = null,
            IPublishingService? publishingService = null,
            IMethodologyVersionRepository? methodologyVersionRepository = null)
        {
            return new(
                context,
                AdminMapper(),
                new PersistenceHelper<ContentDbContext>(context),
                userService ?? AlwaysTrueUserService().Object, 
                publicationRepository ?? new Mock<IPublicationRepository>().Object,
                publishingService ?? new Mock<IPublishingService>().Object, 
                methodologyVersionRepository ?? new Mock<IMethodologyVersionRepository>().Object);
        }
    }
}
