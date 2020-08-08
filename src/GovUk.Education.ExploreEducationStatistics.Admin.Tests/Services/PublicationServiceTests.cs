using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationServiceTests
    {
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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                // Service method under test
                var result = await publicationService.CreatePublication(
                    new SavePublicationViewModel()
                    {
                        Title = "Test publication",
                        Contact = new SaveContactViewModel
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
                var createdPublication = context.Publications.Single(p => p.Id == publicationViewModel.Id);
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

            var mocks = Mocks();

            var publicationService = BuildPublicationService(context, mocks);

            // Service method under test
            var result = await publicationService.CreatePublication(
                new SavePublicationViewModel()
                {
                    Title = "Test publication",
                    TopicId = Guid.NewGuid(),
                });

            Assert.True(result.IsLeft);

            var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
            var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);

            Assert.Equal("TOPIC_DOES_NOT_EXIST", details.Errors[""].First());
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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                // Service method under test
                var result = await publicationService.CreatePublication(
                    new SavePublicationViewModel()
                    {
                        Title = "Test title",
                        TopicId = topic.Id,
                        MethodologyId = Guid.NewGuid(),
                    }
                );

                Assert.True(result.IsLeft);

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);

                Assert.Equal("METHODOLOGY_DOES_NOT_EXIST", details.Errors[""].First());
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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                // Service method under test
                var result = await publicationService.CreatePublication(
                    new SavePublicationViewModel()
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

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);

                Assert.Equal("CANNOT_SPECIFY_METHODOLOGY_AND_EXTERNAL_METHODOLOGY", details.Errors[""].First());
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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                // Service method under test
                var result = await publicationService.CreatePublication(
                    new SavePublicationViewModel()
                    {
                        Title = "Test title",
                        TopicId = topic.Id,
                        MethodologyId = methodology.Id,
                    }
                );

                Assert.True(result.IsLeft);

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);

                Assert.Equal("METHODOLOGY_MUST_BE_APPROVED_OR_PUBLISHED", details.Errors[""].First());
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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                // Service method under test
                var result = await publicationService.CreatePublication(
                    new SavePublicationViewModel()
                    {
                        Title = "Test publication",
                        TopicId = topic.Id
                    }
                );

                Assert.True(result.IsLeft);

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);

                Assert.Equal("SLUG_NOT_UNIQUE", details.Errors[""].First());
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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new SavePublicationViewModel()
                    {
                        Title = "New title",
                        Contact = new SaveContactViewModel
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
                var updatedPublication = context.Publications.Single(p => p.Id == result.Right.Id);
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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new SavePublicationViewModel()
                    {
                        Title = "New title",
                        Contact = new SaveContactViewModel
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

                var updatedPublication = context.Publications.Single(p => p.Id == result.Right.Id);

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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new SavePublicationViewModel()
                    {
                        Title = "New title",
                        Contact = new SaveContactViewModel
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

                var updatedPublication = context.Publications.Single(p => p.Id == result.Right.Id);

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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new SavePublicationViewModel()
                    {
                        Title = "Test publication",
                        TopicId = Guid.NewGuid(),
                    }
                );

                Assert.True(result.IsLeft);

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);

                Assert.Equal("TOPIC_DOES_NOT_EXIST", details.Errors[""].First());
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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new SavePublicationViewModel()
                    {
                        Title = "Test publication",
                        TopicId = publication.TopicId,
                        MethodologyId = Guid.NewGuid(),
                    }
                );

                Assert.True(result.IsLeft);

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);

                Assert.Equal("METHODOLOGY_DOES_NOT_EXIST", details.Errors[""].First());
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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new SavePublicationViewModel()
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

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);

                Assert.Equal("CANNOT_SPECIFY_METHODOLOGY_AND_EXTERNAL_METHODOLOGY", details.Errors[""].First());
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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new SavePublicationViewModel()
                    {
                        Title = "Test publication",
                        TopicId = publication.TopicId,
                        MethodologyId = methodology.Id,
                    }
                );

                Assert.True(result.IsLeft);

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);

                Assert.Equal("METHODOLOGY_MUST_BE_APPROVED_OR_PUBLISHED", details.Errors[""].First());
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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                // Service method under test
                var result = await publicationService.UpdatePublication(
                    publication.Id,
                    new SavePublicationViewModel()
                    {
                        Title = "Duplicated title",
                        TopicId = topic.Id,
                    }
                );

                Assert.True(result.IsLeft);

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);

                Assert.Equal("SLUG_NOT_UNIQUE", details.Errors[""].First());
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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                var result = await publicationService.PartialUpdateLegacyReleases(
                    publication.Id,
                    new List<PartialUpdateLegacyReleaseViewModel>
                    {
                        new PartialUpdateLegacyReleaseViewModel
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
                var mocks = Mocks();

                var publicationService = BuildPublicationService(context, mocks);

                var result = await publicationService.PartialUpdateLegacyReleases(
                    publication.Id,
                    new List<PartialUpdateLegacyReleaseViewModel>
                    {
                        new PartialUpdateLegacyReleaseViewModel
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