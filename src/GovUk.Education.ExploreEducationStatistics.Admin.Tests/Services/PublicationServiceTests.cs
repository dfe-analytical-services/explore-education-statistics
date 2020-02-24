using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
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
        public async void CreatePublicationWithoutMethodology()
        {
            var (userService, repository, _) = Mocks();
            
            using (var context = InMemoryApplicationDbContext("Create"))
            {
                context.Add(new Topic {Id = new Guid("861517a2-5055-486c-b362-f971d9791943")});
                context.Add(new Contact {Id = new Guid("1ad5f3dc-20f2-4baf-b715-8dd31ba58942")});
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                var publicationService = new PublicationService(context, AdminMapper(),
                    userService.Object, repository.Object, new PersistenceHelper<ContentDbContext>(context));
                
                // Service method under test
                var result = await publicationService.CreatePublicationAsync(new CreatePublicationViewModel()
                {
                    Title = "Publication Title",
                    ContactId = new Guid("1ad5f3dc-20f2-4baf-b715-8dd31ba58942"),
                    TopicId = new Guid("861517a2-5055-486c-b362-f971d9791943")
                });

                // Do an in depth check of the saved release
                var publication = context.Publications.Single(p => p.Id == result.Right.Id);
                Assert.Equal(new Guid("1ad5f3dc-20f2-4baf-b715-8dd31ba58942"), publication.ContactId);
                Assert.Equal("Publication Title", publication.Title);
                Assert.Equal(new Guid("861517a2-5055-486c-b362-f971d9791943"), publication.TopicId);
            }
        }

        [Fact]
        public async void CreatePublicationWithMethodology()
        {
            var (userService, repository, _) = Mocks();
            
            using (var context = InMemoryApplicationDbContext("CreatePublication"))
            {
                context.Add(new Topic {Id = new Guid("b9ce9ddc-efdc-4853-b709-054dc7eed6e4")});
                context.Add(new Contact {Id = new Guid("cd6c265b-7fbc-4c15-ab36-7c3e0ea216d5")});
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
                var result = await publicationService.CreatePublicationAsync(new CreatePublicationViewModel()
                {
                    Title = "Publication Title",
                    ContactId = new Guid("cd6c265b-7fbc-4c15-ab36-7c3e0ea216d5"),
                    TopicId = new Guid("b9ce9ddc-efdc-4853-b709-054dc7eed6e4"),
                    MethodologyId = new Guid("697fc9b8-4d44-45da-ae61-148dd9a31450")
                });

                // Do an in depth check of the saved release
                var createdPublication = context.Publications.Single(p => p.Id == result.Right.Id);
                Assert.Equal(new Guid("cd6c265b-7fbc-4c15-ab36-7c3e0ea216d5"), createdPublication.ContactId);
                Assert.Equal("Publication Title", createdPublication.Title);
                Assert.Equal(new Guid("b9ce9ddc-efdc-4853-b709-054dc7eed6e4"), createdPublication.TopicId);
                Assert.Equal(new Guid("697fc9b8-4d44-45da-ae61-148dd9a31450"), createdPublication.MethodologyId);

                // Check that the already existing release hasn't been altered.
                var existingPublication =
                    context.Publications.Single(p => p.Id == new Guid("7af5c874-a3cd-4a5a-873e-2564236a2bd1"));
                Assert.Equal(new Guid("697fc9b8-4d44-45da-ae61-148dd9a31450"), existingPublication.MethodologyId);
            }
        }

        [Fact]
        public async void CreatePublicationFailsWithNonUniqueSlug()
        {
            var (userService, repository, persistenceHelper) = Mocks();

            const string titleToBeDuplicated = "A title to be duplicated";

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                var publicationService = new PublicationService(context, AdminMapper(),
                    userService.Object, repository.Object, persistenceHelper.Object);
                
                var result = await publicationService.CreatePublicationAsync(
                    new CreatePublicationViewModel
                    {
                        Title = titleToBeDuplicated
                    });
                Assert.False(result.IsLeft); // First time should be ok
            }

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                var publicationService = new PublicationService(context, AdminMapper(),
                    userService.Object, repository.Object, persistenceHelper.Object);
                
                // Service method under test
                var result = await publicationService.CreatePublicationAsync(
                    new CreatePublicationViewModel()
                    {
                        Title = titleToBeDuplicated,
                    });

                Assert.True(result.IsLeft); // Second time should be validation failure
                Assert.IsAssignableFrom<BadRequestObjectResult>(result.Left);

                var details = (ValidationProblemDetails) ((BadRequestObjectResult) result.Left).Value;
                Assert.Equal("SLUG_NOT_UNIQUE", details.Errors[""].First());
            }
        }
        
        [Fact]
        public async void UpdatePublicationMethodologyWithId()
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
                
                await publicationService.UpdatePublicationMethodologyAsync(testPublicationId, new UpdatePublicationMethodologyViewModel
                {
                    ExternalMethodology = null,
                    MethodologyId = testMethodologyId
                });

                var publication = context.Publications.Single(p => p.Id == testPublicationId);
                
                Assert.Equal(testMethodologyId, publication.MethodologyId);
            }
        }
        
        [Fact]
        public async void UpdatePublicationMethodologyWithId_Draft()
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
                
                var result = await publicationService.UpdatePublicationMethodologyAsync(testPublicationId, new UpdatePublicationMethodologyViewModel
                {
                    ExternalMethodology = null,
                    MethodologyId = testMethodologyId
                });

                var publication = context.Publications.Single(p => p.Id == testPublicationId);
                
                Assert.Null(publication.MethodologyId);
            }
        }
        
        [Fact]
        public async void UpdatePublicationMethodologyWithId_NotExists()
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
                
                var result = await publicationService.UpdatePublicationMethodologyAsync(testPublicationId, new UpdatePublicationMethodologyViewModel
                {
                    ExternalMethodology = null,
                    MethodologyId = testMethodologyId
                });

                var publication = context.Publications.Single(p => p.Id == testPublicationId);
                
                Assert.Null(publication.MethodologyId);
            }
        }
        
        [Fact]
        public async void UpdatePublicationMethodologyWithExternal()
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
                
                var result = await publicationService.UpdatePublicationMethodologyAsync(testPublicationId, new UpdatePublicationMethodologyViewModel
                {
                    ExternalMethodology = new ExternalMethodology
                    {
                        Title = "title",
                        Url = new Uri("https://example.com")
                    },
                    MethodologyId = null
                });

                var publication = context.Publications.Single(p => p.Id == testPublicationId);
                
                Assert.Null(publication.MethodologyId);
            }
        }
        
        [Fact]
        public async void UpdatePublicationMethodologyInvalidRequest()
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
                
                var result = await publicationService.UpdatePublicationMethodologyAsync(testPublicationId, new UpdatePublicationMethodologyViewModel
                {
                    ExternalMethodology = null,
                    MethodologyId = null
                });

                var publication = context.Publications.Single(p => p.Id == testPublicationId);
                
                Assert.Null(publication.MethodologyId);
                Assert.Null(publication.ExternalMethodology);

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