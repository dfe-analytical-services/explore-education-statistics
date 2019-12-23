using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationServiceTests
    {
        [Fact]
        public async void CreatePublicationWithoutMethodology()
        {
            var (_, userService, repository) = Mocks();
            
            using (var context = InMemoryApplicationDbContext("Create"))
            {
                context.Add(new Topic {Id = new Guid("861517a2-5055-486c-b362-f971d9791943")});
                context.Add(new Contact {Id = new Guid("1ad5f3dc-20f2-4baf-b715-8dd31ba58942")});
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                var publicationService = new PublicationService(context, MapperForProfile<MappingProfiles>(),
                    userService.Object, repository.Object);
                
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
            var (_, userService, repository) = Mocks();
            
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
                var publicationService = new PublicationService(context, MapperForProfile<MappingProfiles>(),
                    userService.Object, repository.Object);
                
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
            var (_, userService, repository) = Mocks();

            const string titleToBeDuplicated = "A title to be duplicated";

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                var publicationService = new PublicationService(context, MapperForProfile<MappingProfiles>(),
                    userService.Object, repository.Object);
                
                var result = await publicationService.CreatePublicationAsync(
                    new CreatePublicationViewModel
                    {
                        Title = titleToBeDuplicated
                    });
                Assert.False(result.IsLeft); // First time should be ok
            }

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                var publicationService = new PublicationService(context, MapperForProfile<MappingProfiles>(),
                    userService.Object, repository.Object);
                
                // Service method under test
                var result = await publicationService.CreatePublicationAsync(
                    new CreatePublicationViewModel()
                    {
                        Title = titleToBeDuplicated,
                    });

                Assert.True(result.IsLeft); // Second time should be validation failure
                Assert.Equal(ValidationResult(SlugNotUnique).ErrorMessage,
                    result.Left.ErrorMessage);
            }
        }

        [Fact]
        public async void GetMyPublicationsAndReleasesByTopicAsync_CanViewAllReleases()
        {
            var (context, userService, repository) = Mocks();

            var topicId = Guid.NewGuid();
            
            var publicationService = new PublicationService(context.Object, MapperForProfile<MappingProfiles>(), 
                userService.Object, repository.Object);

            userService.
                Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases)).
                ReturnsAsync(true);

            var list = new List<PublicationViewModel>()
            {
                new PublicationViewModel
                {
                    Id = Guid.NewGuid()
                }
            };

            repository.
                Setup(s => s.GetAllPublicationsForTopicAsync(topicId)).
                ReturnsAsync(list);
            
            var result = await publicationService.GetMyPublicationsAndReleasesByTopicAsync(topicId);
            Assert.Equal(list, result);
            
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases));
            userService.VerifyNoOtherCalls();

            repository.Verify(s => s.GetAllPublicationsForTopicAsync(topicId));
            repository.VerifyNoOtherCalls();
        }
        
        [Fact]
        public async void GetMyPublicationsAndReleasesByTopicAsync_CanViewRelatedReleases()
        {
            var (context, userService, repository) = Mocks();

            var topicId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            var publicationService = new PublicationService(context.Object, MapperForProfile<MappingProfiles>(), 
                userService.Object, repository.Object);

            userService.
                Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases)).
                ReturnsAsync(false);

            userService.
                Setup(s => s.GetUserId()).
                Returns(userId);

            var list = new List<PublicationViewModel>()
            {
                new PublicationViewModel
                {
                    Id = Guid.NewGuid()
                }
            };

            repository.
                Setup(s => s.GetPublicationsForTopicRelatedToUserAsync(topicId, userId)).
                ReturnsAsync(list);
            
            var result = await publicationService.GetMyPublicationsAndReleasesByTopicAsync(topicId);
            Assert.Equal(list, result);
            
            userService.Verify(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases));
            userService.Verify(s => s.GetUserId());
            userService.VerifyNoOtherCalls();

            repository.Verify(s => s.GetPublicationsForTopicRelatedToUserAsync(topicId, userId));
            repository.VerifyNoOtherCalls();
        }
        
        private (Mock<ContentDbContext>, Mock<IUserService>, Mock<IPublicationRepository>) Mocks()
        {
            var userService = new Mock<IUserService>();

            userService
                .Setup(s => s.MatchesPolicy(SecurityPolicies.CanViewAllReleases))
                .ReturnsAsync(true);

            return (new Mock<ContentDbContext>(), userService, new Mock<IPublicationRepository>());
        }
    }
}