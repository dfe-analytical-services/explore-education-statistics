using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ValidationTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationServiceTests
    {
        [Fact]
        public async void GetPublication()
        {
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
                Methodology = new Methodology
                {
                    Title = "Test methodology",
                    Status = MethodologyStatus.Approved,
                },
                Contact = new Contact
                {
                    ContactName = "Test contact",
                    ContactTelNo = "0123456789",
                    TeamName = "Test team",
                    TeamEmail = "team@test.com",
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                await context.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context, Mocks());

                var result = await publicationService.GetPublication(publication.Id);

                Assert.True(result.IsRight);

                Assert.Equal(publication.Id, result.Right.Id);
                Assert.Equal(publication.Title, result.Right.Title);
                Assert.Equal(publication.Slug, result.Right.Slug);

                Assert.Equal(publication.Topic.Id, result.Right.TopicId);
                Assert.Equal(publication.Topic.ThemeId, result.Right.ThemeId);

                Assert.Equal(publication.Methodology.Id, result.Right.Methodology.Id);
                Assert.Equal(publication.Methodology.Title, result.Right.Methodology.Title);

                Assert.Equal(publication.Contact.Id, result.Right.Contact.Id);
                Assert.Equal(publication.Contact.ContactName, result.Right.Contact.ContactName);
                Assert.Equal(publication.Contact.ContactTelNo, result.Right.Contact.ContactTelNo);
                Assert.Equal(publication.Contact.TeamEmail, result.Right.Contact.TeamEmail);
                Assert.Equal(publication.Contact.TeamName, result.Right.Contact.TeamName);
            }
        }

        [Fact]
        public async void GetPublication_NotFound()
        {
            await using var context = InMemoryApplicationDbContext();
            var publicationService = BuildPublicationService(context, Mocks());

            var result = await publicationService.GetPublication(Guid.NewGuid());

            Assert.True(result.IsLeft);
            Assert.IsType<NotFoundResult>(result.Left);
        }

        [Fact]
        public async void CreatePublication()
        {
            var topic = new Topic
            {
                Title = "Test topic"
            };
            var methodology = new Methodology
            {
                Title = "Test methodology",
                Status = MethodologyStatus.Approved,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(topic);
                context.Add(methodology);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context, Mocks());

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
                        TopicId = topic.Id,
                        MethodologyId = methodology.Id
                    }
                );

                var publicationViewModel = result.Right;
                Assert.Equal("Test publication", publicationViewModel.Title);

                Assert.Equal("John Smith", publicationViewModel.Contact.ContactName);
                Assert.Equal("0123456789", publicationViewModel.Contact.ContactTelNo);
                Assert.Equal("Test team", publicationViewModel.Contact.TeamName);
                Assert.Equal("john.smith@test.com", publicationViewModel.Contact.TeamEmail);

                Assert.Equal(topic.Id, publicationViewModel.TopicId);

                Assert.Equal(methodology.Id, publicationViewModel.Methodology.Id);
                Assert.Equal("Test methodology", publicationViewModel.Methodology.Title);

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

                Assert.Equal(methodology.Id, createdPublication.Methodology.Id);
                Assert.Equal("Test methodology", createdPublication.Methodology.Title);
            }
        }

        [Fact]
        public async void CreatePublication_FailsWithNonExistingTopic()
        {
            await using var context = InMemoryApplicationDbContext();

            var publicationService = BuildPublicationService(context, Mocks());

            // Service method under test
            var result = await publicationService.CreatePublication(
                new PublicationSaveViewModel
                {
                    Title = "Test publication",
                    TopicId = Guid.NewGuid()
                });

            Assert.True(result.IsLeft);
            AssertValidationProblem(result.Left, TopicDoesNotExist);
        }

        [Fact]
        public async void CreatePublication_FailsWithNonExistingMethodology()
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
                var publicationService = BuildPublicationService(context, Mocks());

                // Service method under test
                var result = await publicationService.CreatePublication(
                    new PublicationSaveViewModel()
                    {
                        Title = "Test title",
                        TopicId = topic.Id,
                        MethodologyId = Guid.NewGuid(),
                    }
                );

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, MethodologyDoesNotExist);
            }
        }

        [Fact]
        public async void CreatePublication_FailsWithMethodologyAndExternalMethodology()
        {
            var topic = new Topic
            {
                Title = "Test topic"
            };
            var methodology = new Methodology
            {
                Title = "Test methodology",
                Status = MethodologyStatus.Approved,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(topic);
                context.Add(methodology);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context, Mocks());

                // Service method under test
                var result = await publicationService.CreatePublication(
                    new PublicationSaveViewModel()
                    {
                        Title = "Test title",
                        TopicId = topic.Id,
                        MethodologyId = methodology.Id,
                        ExternalMethodology = new ExternalMethodology
                        {
                            Title = "Test external",
                            Url = "http://test.com"
                        }
                    }
                );

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, CannotSpecifyMethodologyAndExternalMethodology);
            }
        }


        [Fact]
        public async void CreatePublication_FailsWithUnapprovedMethodology()
        {
            var topic = new Topic
            {
                Title = "Test topic"
            };
            var methodology = new Methodology
            {
                Title = "Test methodology",
                Status = MethodologyStatus.Draft,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(topic);
                context.Add(methodology);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context, Mocks());

                // Service method under test
                var result = await publicationService.CreatePublication(
                    new PublicationSaveViewModel()
                    {
                        Title = "Test title",
                        TopicId = topic.Id,
                        MethodologyId = methodology.Id,
                    }
                );

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, MethodologyMustBeApproved);
            }
        }

        [Fact]
        public async void CreatePublication_FailsWithNonUniqueSlug()
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
                var publicationService = BuildPublicationService(context, Mocks());

                // Service method under test
                var result = await publicationService.CreatePublication(
                    new PublicationSaveViewModel()
                    {
                        Title = "Test publication",
                        TopicId = topic.Id
                    }
                );

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, SlugNotUnique);
            }
        }

        [Fact]
        public async void UpdatePublication()
        {
            var topic = new Topic
            {
                Title = "New topic"
            };
            var methodology = new Methodology
            {
                Title = "New methodology",
                Status = MethodologyStatus.Approved,
            };
            var publication = new Publication
            {
                Title = "Old title",
                Topic = new Topic
                {
                    Title = "Old topic"
                },
                Methodology = new Methodology
                {
                    Title = "Old methodology"
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
                context.Add(methodology);
                context.Add(publication);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context, Mocks());

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel()
                    {
                        Title = "New title",
                        Contact = new ContactSaveViewModel
                        {
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789",
                            TeamName = "Test team",
                            TeamEmail = "john.smith@test.com",
                        },
                        TopicId = topic.Id,
                        MethodologyId = methodology.Id,
                    }
                );

                Assert.Equal("New title", result.Right.Title);

                Assert.Equal("John Smith", result.Right.Contact.ContactName);
                Assert.Equal("0123456789", result.Right.Contact.ContactTelNo);
                Assert.Equal("Test team", result.Right.Contact.TeamName);
                Assert.Equal("john.smith@test.com", result.Right.Contact.TeamEmail);

                Assert.Equal(topic.Id, result.Right.TopicId);

                Assert.Equal(methodology.Id, result.Right.Methodology.Id);
                Assert.Equal("New methodology", result.Right.Methodology.Title);

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

                Assert.Equal(methodology.Id, updatedPublication.MethodologyId);
                Assert.Equal("New methodology", updatedPublication.Methodology.Title);
            }
        }

        [Fact]
        public async void UpdatePublication_AlreadyPublished()
        {
            var topic = new Topic
            {
                Title = "New topic"
            };
            var methodology = new Methodology
            {
                Title = "New methodology",
                Status = MethodologyStatus.Approved,
            };
            var publication = new Publication
            {
                Slug = "old-title",
                Title = "Old title",
                Topic = new Topic
                {
                    Title = "Old topic"
                },
                Methodology = new Methodology
                {
                    Title = "Old methodology"
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
                context.Add(methodology);
                context.Add(publication);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = BuildPublicationService(context, Mocks());

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel()
                    {
                        Title = "New title",
                        Contact = new ContactSaveViewModel
                        {
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789",
                            TeamName = "Test team",
                            TeamEmail = "john.smith@test.com",
                        },
                        TopicId = topic.Id,
                        MethodologyId = methodology.Id,
                    }
                );

                Assert.Equal("New title", result.Right.Title);

                Assert.Equal("John Smith", result.Right.Contact.ContactName);
                Assert.Equal("0123456789", result.Right.Contact.ContactTelNo);
                Assert.Equal("Test team", result.Right.Contact.TeamName);
                Assert.Equal("john.smith@test.com", result.Right.Contact.TeamEmail);

                Assert.Equal(topic.Id, result.Right.TopicId);

                Assert.Equal(methodology.Id, result.Right.Methodology.Id);
                Assert.Equal("New methodology", result.Right.Methodology.Title);

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

                Assert.Equal(methodology.Id, updatedPublication.MethodologyId);
                Assert.Equal("New methodology", updatedPublication.Methodology.Title);
            }
        }

        [Fact]
        public async void UpdatePublication_SavesNewContact()
        {
            var publication = new Publication
            {
                Title = "Test title",
                Topic = new Topic
                {
                    Title = "Test topic"
                },
                Methodology = new Methodology
                {
                    Title = "Test methodology",
                    Status = MethodologyStatus.Approved
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
                var publicationService = BuildPublicationService(context, Mocks());

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel()
                    {
                        Title = "New title",
                        Contact = new ContactSaveViewModel
                        {
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789",
                            TeamName = "Test team",
                            TeamEmail = "john.smith@test.com",
                        },
                        TopicId = publication.TopicId,
                        MethodologyId = publication.MethodologyId,
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
        public async void UpdatePublication_SavesNewContactWhenSharedWithOtherPublication()
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
                Methodology = new Methodology
                {
                    Title = "Test methodology",
                    Status = MethodologyStatus.Approved
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
                var publicationService = BuildPublicationService(context, Mocks());

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel()
                    {
                        Title = "New title",
                        Contact = new ContactSaveViewModel
                        {
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789",
                            TeamName = "Test team",
                            TeamEmail = "john.smith@test.com",
                        },
                        TopicId = publication.TopicId,
                        MethodologyId = publication.MethodologyId,
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
        public async void UpdatePublication_FailsWithNonExistingTopic()
        {
            var publication = new Publication
            {
                Title = "Test publication",
                Topic = new Topic
                {
                    Title = "Test topic"
                },
                Methodology = new Methodology
                {
                    Title = "Test methodology"
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
                var publicationService = BuildPublicationService(context, Mocks());

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel()
                    {
                        Title = "Test publication",
                        TopicId = Guid.NewGuid(),
                    }
                );

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, TopicDoesNotExist);
            }
        }

        [Fact]
        public async void UpdatePublication_FailsWithNonExistingMethodology()
        {
            var publication = new Publication
            {
                Topic = new Topic
                {
                    Title = "Old topic"
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
                var publicationService = BuildPublicationService(context, Mocks());

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel()
                    {
                        Title = "Test publication",
                        TopicId = publication.TopicId,
                        MethodologyId = Guid.NewGuid(),
                    }
                );

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, MethodologyDoesNotExist);
            }
        }

        [Fact]
        public async void UpdatePublication_FailsWithMethodologyAndExternalMethodology()
        {
            var publication = new Publication
            {
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
                var publicationService = BuildPublicationService(context, Mocks());

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel()
                    {
                        Title = "Test publication",
                        TopicId = publication.TopicId,
                        MethodologyId = Guid.NewGuid(),
                        ExternalMethodology = new ExternalMethodology
                        {
                            Title = "Test external",
                            Url = "http://test.com"
                        }
                    }
                );

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, CannotSpecifyMethodologyAndExternalMethodology);
            }
        }

        [Fact]
        public async void UpdatePublication_FailsWithUnapprovedMethodology()
        {
            var methodology = new Methodology
            {
                Title = "Test methodology",
                Status = MethodologyStatus.Draft,
            };
            var publication = new Publication
            {
                Topic = new Topic
                {
                    Title = "Test topic"
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Add(methodology);
                context.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {

                var publicationService = BuildPublicationService(context, Mocks());

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel()
                    {
                        Title = "Test publication",
                        TopicId = publication.TopicId,
                        MethodologyId = methodology.Id,
                    }
                );

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, MethodologyMustBeApproved);
            }
        }

        [Fact]
        public async void UpdatePublication_FailsWithNonUniqueSlug()
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
                var publicationService = BuildPublicationService(context, Mocks());

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new PublicationSaveViewModel()
                    {
                        Title = "Duplicated title",
                        TopicId = topic.Id,
                    }
                );

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, SlugNotUnique);
            }
        }

        [Fact]
        public async void PartialUpdateLegacyReleases_OnlyMatchingEntities()
        {
            var publication = new Publication
            {
                LegacyReleases = new List<LegacyRelease>
                {
                    new LegacyRelease
                    {
                        Description = "Test description 1",
                        Url = "http://test1.com",
                        Order = 1,
                    },
                    new LegacyRelease
                    {
                        Description = "Test description 2",
                        Url = "http://test2.com",
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
                var publicationService = BuildPublicationService(context, Mocks());

                var result = await publicationService.PartialUpdateLegacyReleases(
                    publication.Id,
                    new List<LegacyReleasePartialUpdateViewModel>
                    {
                        new LegacyReleasePartialUpdateViewModel
                        {
                            Id = publication.LegacyReleases[0].Id,
                            Description = "Updated description 1",
                            Url = "http://updated-test1.com",
                            Order = 3
                        }
                    }
                );

                var legacyReleases = result.Right;

                Assert.Equal(2, legacyReleases.Count);

                Assert.Equal(publication.LegacyReleases[0].Id, legacyReleases[0].Id);
                Assert.Equal("Updated description 1", legacyReleases[0].Description);
                Assert.Equal("http://updated-test1.com", legacyReleases[0].Url);
                Assert.Equal(3, legacyReleases[0].Order);

                Assert.Equal(publication.LegacyReleases[1].Id, legacyReleases[1].Id);
                Assert.Equal("Test description 2", legacyReleases[1].Description);
                Assert.Equal("http://test2.com", legacyReleases[1].Url);
                Assert.Equal(2, legacyReleases[1].Order);
            }
        }

        [Fact]
        public async void PartialUpdateLegacyReleases_OnlyNonNullFields()
        {
            var publication = new Publication
            {
                LegacyReleases = new List<LegacyRelease>
                {
                    new LegacyRelease
                    {
                        Description = "Test description 1",
                        Url = "http://test1.com",
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
                var publicationService = BuildPublicationService(context, Mocks());

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
                Assert.Equal("http://test1.com", legacyReleases[0].Url);
                Assert.Equal(1, legacyReleases[0].Order);
            }
        }

        private static PublicationService BuildPublicationService(ContentDbContext context,
            (Mock<IUserService> userService,
                Mock<IPublicationRepository> publicationRepository,
                Mock<IPublishingService> publishingService) mocks)
        {
            var (userService, publicationRepository, publishingService) = mocks;

            return new PublicationService(
                context,
                AdminMapper(),
                userService.Object,
                publicationRepository.Object,
                publishingService.Object,
                new PersistenceHelper<ContentDbContext>(context));
        }

        private static (Mock<IUserService> UserService,
            Mock<IPublicationRepository> PublicationRepository,
            Mock<IPublishingService> PublishingService) Mocks()
        {
            return (
                MockUtils.AlwaysTrueUserService(),
                new Mock<IPublicationRepository>(),
                new Mock<IPublishingService>());
        }
    }
}