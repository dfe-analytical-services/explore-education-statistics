using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
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
        public async void CreatePublication_WithoutMethodology()
        {
            var (userService, repository, _) = Mocks();
            
            using (var context = InMemoryApplicationDbContext("Create"))
            {
                context.Add(new Topic {Id = new Guid("861517a2-5055-486c-b362-f971d9791943")});
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                var publicationService = new PublicationService(context, AdminMapper(),
                    userService.Object, repository.Object, new PersistenceHelper<ContentDbContext>(context));
                
                // Service method under test
                var result = await publicationService.CreatePublication(new CreatePublicationViewModel()
                {
                    Title = "Publication Title",
                    Contact = new SaveContactViewModel 
                    {
                        ContactName = "John Smith",
                        ContactTelNo = "0123456789",
                        TeamName = "Test team",
                        TeamEmail = "john.smith@test.com",
                    },
                    TopicId = new Guid("861517a2-5055-486c-b362-f971d9791943")
                });

                // Do an in depth check of the saved release
                var publication = context.Publications.Single(p => p.Id == result.Right.Id);
                
                Assert.Equal("Publication Title", publication.Title);
                Assert.Equal(new Guid("861517a2-5055-486c-b362-f971d9791943"), publication.TopicId);

                Assert.Equal("John Smith", publication.Contact.ContactName);
                Assert.Equal("0123456789", publication.Contact.ContactTelNo);
                Assert.Equal("Test team", publication.Contact.TeamName);
                Assert.Equal("john.smith@test.com", publication.Contact.TeamEmail);
            }
        }

        [Fact]
        public async void CreatePublication_WithMethodology()
        {
            var (userService, repository, _) = Mocks();
            
            using (var context = InMemoryApplicationDbContext("CreatePublication"))
            {
                context.Add(new Topic {Id = new Guid("b9ce9ddc-efdc-4853-b709-054dc7eed6e4")});
                context.Add(new Publication // An existing publication with a methodology
                {
                    Id = new Guid("7af5c874-a3cd-4a5a-873e-2564236a2bd1"),
                    Methodology = new Methodology
                    {
                        Id = new Guid("697fc9b8-4d44-45da-ae61-148dd9a31450")
                    }
                });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("CreatePublication"))
            {
                var publicationService = new PublicationService(context, AdminMapper(),
                    userService.Object, repository.Object, new PersistenceHelper<ContentDbContext>(context));
                
                // Service method under test
                var result = await publicationService.CreatePublication(new CreatePublicationViewModel()
                {
                    Title = "Publication Title",
                    Contact = new SaveContactViewModel
                    {
                        ContactName = "John Smith",
                        ContactTelNo = "0123456789",
                        TeamName = "Test team",
                        TeamEmail = "john.smith@test.com",
                    },
                    TopicId = new Guid("b9ce9ddc-efdc-4853-b709-054dc7eed6e4"),
                    MethodologyId = new Guid("697fc9b8-4d44-45da-ae61-148dd9a31450")
                });

                // Do an in depth check of the saved release
                var createdPublication = context.Publications.Single(p => p.Id == result.Right.Id);
                Assert.Equal("Publication Title", createdPublication.Title);

                Assert.Equal("John Smith", createdPublication.Contact.ContactName);
                Assert.Equal("0123456789", createdPublication.Contact.ContactTelNo);
                Assert.Equal("Test team", createdPublication.Contact.TeamName);
                Assert.Equal("john.smith@test.com", createdPublication.Contact.TeamEmail);

                Assert.Equal(new Guid("b9ce9ddc-efdc-4853-b709-054dc7eed6e4"), createdPublication.TopicId);
                Assert.Equal(new Guid("697fc9b8-4d44-45da-ae61-148dd9a31450"), createdPublication.MethodologyId);

                // Check that the already existing release hasn't been altered.
                var existingPublication =
                    context.Publications.Single(p => p.Id == new Guid("7af5c874-a3cd-4a5a-873e-2564236a2bd1"));
                Assert.Equal(new Guid("697fc9b8-4d44-45da-ae61-148dd9a31450"), existingPublication.MethodologyId);
            }
        }

        [Fact]
        public async void CreatePublication_FailsWithNonUniqueSlug()
        {
            var (userService, repository, persistenceHelper) = Mocks();

            const string titleToBeDuplicated = "A title to be duplicated";

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                var publicationService = new PublicationService(context, AdminMapper(),
                    userService.Object, repository.Object, persistenceHelper.Object);
                
                var result = await publicationService.CreatePublication(
                    new CreatePublicationViewModel
                    {
                        Title = titleToBeDuplicated,
                        Contact = new SaveContactViewModel
                        {
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789",
                            TeamName = "Test team",
                            TeamEmail = "john.smith@test.com",
                        },
                    });
                Assert.False(result.IsLeft); // First time should be ok
            }

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                var publicationService = new PublicationService(context, AdminMapper(),
                    userService.Object, repository.Object, persistenceHelper.Object);
                
                // Service method under test
                var result = await publicationService.CreatePublication(
                    new CreatePublicationViewModel()
                    {
                        Title = titleToBeDuplicated,
                        Contact = new SaveContactViewModel
                        {
                            ContactName = "John Smith",
                            ContactTelNo = "0123456789",
                            TeamName = "Test team",
                            TeamEmail = "john.smith@test.com",
                        },
                    });

                Assert.True(result.IsLeft); // Second time should be validation failure
                Assert.IsAssignableFrom<BadRequestObjectResult>(result.Left);

                var details = (ValidationProblemDetails) ((BadRequestObjectResult) result.Left).Value;
                Assert.Equal("SLUG_NOT_UNIQUE", details.Errors[""].First());
            }
        }
        
        [Fact]
        public async void UpdatePublicationMethodology_WithId()
        {
            var (userService, repository, _) = Mocks();
            var testPublicationId = new Guid("861517a2-5055-486c-b362-f971d9791943");
            var testMethodologyId = new Guid("1ad5f3dc-20f2-4baf-b715-8dd31ba58942");
            
            using (var context = InMemoryApplicationDbContext("UpdateMethodologyWithId"))
            {
                context.Add(new Publication { Id = testPublicationId});
                context.Add(new Methodology 
                { 
                    Id = testMethodologyId, 
                    Published = DateTime.UtcNow.AddDays(-1), 
                    Status = MethodologyStatus.Approved
                    
                });

                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("UpdateMethodologyWithId"))
            {
                var publicationService = new PublicationService(context, AdminMapper(),
                    userService.Object, repository.Object, new PersistenceHelper<ContentDbContext>(context));
                
                await publicationService.UpdatePublicationMethodology(testPublicationId, new UpdatePublicationMethodologyViewModel
                {
                    ExternalMethodology = null,
                    MethodologyId = testMethodologyId
                });

                var publication = context.Publications.Single(p => p.Id == testPublicationId);
                
                Assert.Equal(testMethodologyId, publication.MethodologyId);
            }
        }
        
        [Fact]
        public async void UpdatePublicationMethodology_WithId_Draft()
        {
            var (userService, repository, _) = Mocks();
            var testPublicationId = new Guid("861517a2-5055-486c-b362-f971d9791943");
            var testMethodologyId = new Guid("1ad5f3dc-20f2-4baf-b715-8dd31ba58942");
            
            using (var context = InMemoryApplicationDbContext("UpdateMethodologyWithId_Draft"))
            {
                context.Add(new Publication { Id = testPublicationId});
                context.Add(new Methodology { Id = testMethodologyId});

                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("UpdateMethodologyWithId_Draft"))
            {
                var publicationService = new PublicationService(context, AdminMapper(),
                    userService.Object, repository.Object, new PersistenceHelper<ContentDbContext>(context));
                
                var result = await publicationService.UpdatePublicationMethodology(testPublicationId, new UpdatePublicationMethodologyViewModel
                {
                    ExternalMethodology = null,
                    MethodologyId = testMethodologyId
                });

                var publication = context.Publications.Single(p => p.Id == testPublicationId);
                
                Assert.Null(publication.MethodologyId);
            }
        }
        
        [Fact]
        public async void UpdatePublicationMethodology_WithId_NotExists()
        {
            var (userService, repository, _) = Mocks();
            var testPublicationId = new Guid("861517a2-5055-486c-b362-f971d9791943");
            var testMethodologyId = new Guid("1ad5f3dc-20f2-4baf-b715-8dd31ba58942");
            
            using (var context = InMemoryApplicationDbContext("UpdateMethodologyWithId_NotExists"))
            {
                context.Add(new Publication { Id = testPublicationId});

                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("UpdateMethodologyWithId_NotExists"))
            {
                var publicationService = new PublicationService(context, AdminMapper(),
                    userService.Object, repository.Object, new PersistenceHelper<ContentDbContext>(context));
                
                var result = await publicationService.UpdatePublicationMethodology(testPublicationId, new UpdatePublicationMethodologyViewModel
                {
                    ExternalMethodology = null,
                    MethodologyId = testMethodologyId
                });

                var publication = context.Publications.Single(p => p.Id == testPublicationId);
                
                Assert.Null(publication.MethodologyId);
            }
        }
        
        [Fact]
        public async void UpdatePublicationMethodology_WithExternal()
        {
            var (userService, repository, _) = Mocks();
            var testPublicationId = new Guid("861517a2-5055-486c-b362-f971d9791943");
            
            using (var context = InMemoryApplicationDbContext("UpdateMethodologyWithExternal"))
            {
                context.Add(new Publication { Id = testPublicationId});

                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("UpdateMethodologyWithExternal"))
            {
                var publicationService = new PublicationService(context, AdminMapper(),
                    userService.Object, repository.Object, new PersistenceHelper<ContentDbContext>(context));
                
                var result = await publicationService.UpdatePublicationMethodology(testPublicationId, new UpdatePublicationMethodologyViewModel
                {
                    ExternalMethodology = new ExternalMethodology
                    {
                        Title = "title",
                        Url = "https://example.com"
                    },
                    MethodologyId = null
                });

                var publication = context.Publications.Single(p => p.Id == testPublicationId);
                
                Assert.Null(publication.MethodologyId);
            }
        }
        
        [Fact]
        public async void UpdatePublicationMethodology_InvalidRequest()
        {
            var (userService, repository, _) = Mocks();
            var testPublicationId = new Guid("861517a2-5055-486c-b362-f971d9791943");
            
            using (var context = InMemoryApplicationDbContext("UpdateMethodologyInvalidRequest"))
            {
                context.Add(new Publication { Id = testPublicationId});

                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("UpdateMethodologyInvalidRequest"))
            {
                var publicationService = new PublicationService(context, AdminMapper(),
                    userService.Object, repository.Object, new PersistenceHelper<ContentDbContext>(context));
                
                var result = await publicationService.UpdatePublicationMethodology(testPublicationId, new UpdatePublicationMethodologyViewModel
                {
                    ExternalMethodology = null,
                    MethodologyId = null
                });

                var publication = context.Publications.Single(p => p.Id == testPublicationId);
                
                Assert.Null(publication.MethodologyId);
                Assert.Null(publication.ExternalMethodology);

            }
        }

        [Fact]
        public async void PartialUpdateLegacyReleases_OnlyMatchingEntities()
        {
            var (userService, repository, _) = Mocks();
            var publicationId = Guid.NewGuid();

            var legacyRelease1Id = Guid.NewGuid();
            var legacyRelease2Id = Guid.NewGuid();
            
            using (var context = InMemoryApplicationDbContext("PartialUpdateLegacyReleases_OnlyMatchingEntities"))
            {
                context.Add(new Publication 
                { 
                    Id = publicationId,
                    LegacyReleases = new List<LegacyRelease>
                    {
                        new LegacyRelease 
                        {
                            Id = legacyRelease1Id,
                            Description = "Test description 1",
                            Url = "http://test1.com",
                            Order = 1,
                        },
                        new LegacyRelease 
                        {
                            Id = legacyRelease2Id,
                            Description = "Test description 2",
                            Url = "http://test2.com",
                            Order = 2,
                        },
                    }
                });

                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("PartialUpdateLegacyReleases_OnlyMatchingEntities"))
            {
                var publicationService = new PublicationService(
                    context, 
                    AdminMapper(),
                    userService.Object, 
                    repository.Object, 
                    new PersistenceHelper<ContentDbContext>(context)
                );
                
                var result = await publicationService.PartialUpdateLegacyReleases(
                    publicationId, 
                    new List<PartialUpdateLegacyReleaseViewModel>
                    {
                        new PartialUpdateLegacyReleaseViewModel
                        {
                            Id = legacyRelease1Id,
                            Description = "Updated description 1",
                            Url = "http://updated-test1.com",
                            Order = 3
                        }
                    });

                var legacyReleases = result.Right;

                Assert.Equal(legacyReleases.Count, 2);

                Assert.Equal(legacyRelease1Id, legacyReleases[0].Id);
                Assert.Equal("Updated description 1", legacyReleases[0].Description);
                Assert.Equal("http://updated-test1.com", legacyReleases[0].Url);
                Assert.Equal(3, legacyReleases[0].Order);

                Assert.Equal(legacyRelease2Id, legacyReleases[1].Id);
                Assert.Equal("Test description 2", legacyReleases[1].Description);
                Assert.Equal("http://test2.com", legacyReleases[1].Url);
                Assert.Equal(2, legacyReleases[1].Order);
            }   
        }
   
        [Fact]
        public async void PartialUpdateLegacyReleases_OnlyNonNullFields()
        {
            var (userService, repository, _) = Mocks();
            var publicationId = Guid.NewGuid();

            var legacyRelease1Id = Guid.NewGuid();
            
            using (var context = InMemoryApplicationDbContext("PartialUpdateLegacyReleases_OnlyNonNullFields"))
            {
                context.Add(new Publication 
                { 
                    Id = publicationId,
                    LegacyReleases = new List<LegacyRelease>
                    {
                        new LegacyRelease 
                        {
                            Id = legacyRelease1Id,
                            Description = "Test description 1",
                            Url = "http://test1.com",
                            Order = 1,
                        },
                    }
                });

                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("PartialUpdateLegacyReleases_OnlyNonNullFields"))
            {
                var publicationService = new PublicationService(
                    context, 
                    AdminMapper(),
                    userService.Object, 
                    repository.Object, 
                    new PersistenceHelper<ContentDbContext>(context)
                );
                
                var result = await publicationService.PartialUpdateLegacyReleases(
                    publicationId, 
                    new List<PartialUpdateLegacyReleaseViewModel>
                    {
                        new PartialUpdateLegacyReleaseViewModel
                        {
                            Id = legacyRelease1Id,
                            Description = "Updated description 1",
                        }
                    });

                var legacyReleases = result.Right;

                Assert.Equal(legacyReleases.Count, 1);

                Assert.Equal(legacyRelease1Id, legacyReleases[0].Id);
                Assert.Equal("Updated description 1", legacyReleases[0].Description);
                Assert.Equal("http://test1.com", legacyReleases[0].Url);
                Assert.Equal(1, legacyReleases[0].Order);
            }   
        }

        private (
            Mock<IUserService>, 
            Mock<IPublicationRepository>, 
            Mock<IPersistenceHelper<ContentDbContext>>) Mocks()
        {
            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext>();
            MockUtils.SetupCall<ContentDbContext, Topic>(persistenceHelper);
            MockUtils.SetupCall<ContentDbContext, Publication>(persistenceHelper);

            return (
                MockUtils.AlwaysTrueUserService(), 
                new Mock<IPublicationRepository>(), 
                persistenceHelper);
        }
    }
}