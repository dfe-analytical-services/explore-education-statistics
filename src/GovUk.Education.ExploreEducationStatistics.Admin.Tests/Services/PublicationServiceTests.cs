using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using Assert = Xunit.Assert;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationServiceTests
    {
        [Fact]
        public async void CreatePublicationWithoutMethodology()
        {
            using (var context = InMemoryApplicationDbContext("Create"))
            {
                context.Add(new Topic {Id = new Guid("861517a2-5055-486c-b362-f971d9791943")});
                context.Add(new Contact {Id = new Guid("1ad5f3dc-20f2-4baf-b715-8dd31ba58942")});
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                // Service method under test
                var result = await new PublicationService(context, MapperForProfile<MappingProfiles>()).CreatePublicationAsync(new CreatePublicationViewModel()
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
                // Service method under test
                var result = await new PublicationService(context, MapperForProfile<MappingProfiles>()).CreatePublicationAsync(new CreatePublicationViewModel()
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
            const string titleToBeDuplicated = "A title to be duplicated";

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                var result = await new PublicationService(context, MapperForProfile<MappingProfiles>()).CreatePublicationAsync(
                    new CreatePublicationViewModel
                    {
                        Title = titleToBeDuplicated
                    });
                Assert.False(result.IsLeft); // First time should be ok
            }

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                // Service method under test
                var result = await new PublicationService(context, MapperForProfile<MappingProfiles>()).CreatePublicationAsync(
                    new CreatePublicationViewModel()
                    {
                        Title = titleToBeDuplicated,
                    });

                Assert.True(result.IsLeft); // Second time should be validation failure
                CollectionAssert.AreEquivalent(new List<string> {"Slug"},
                    new List<string>(result.Left.MemberNames));
            }
        }
        
        
        [Fact]
        public async void LatestReleaseCorrectlyReportedInPublication()
        {

            var latestReleaseId = new Guid("4a01ee05-55b8-4764-83eb-6559108fac7e");
            var notLatestReleaseId = new Guid("e3aa5543-7993-4ea5-9827-730671a7729f");
            var topicId = new Guid("f63a51b3-fa86-4077-a3e5-3a2114ad006f");
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReportedInPublication"))
            {
                context.Add(new Topic
                {
                    Id = topicId,
                    Publications = new List<Publication>
                    {

                        new Publication
                        {
                            Id = new Guid("a61c8bd8-8b7b-4de8-b17c-caf21d7799b0"),
                            Title = "Publication",
                            TopicId = topicId,
                            Releases = new List<Release>
                            {
                                new Release
                                {
                                    Id = notLatestReleaseId,
                                    Order = 0,
                                    Published = DateTime.Now.AddDays(-2) // Is published but not the latest by order

                                },
                                new Release
                                {
                                    Id = latestReleaseId,
                                    Order = 1,
                                    Published = DateTime.Now.AddDays(-1) // Is published and the the latest by order
                                }
                            }
                        }
                    }
                });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReportedInPublication"))
            {
                // Method under test - this return a list of publication for a user. The releases in the publication
                // should correctly report whether they are the latest or not. Note that this is dependent on the mapper
                // that we are passing in.
                var publications = await new PublicationService(context, MapperForProfile<MappingProfiles>()).GetByTopicAndUserAsync(topicId, new Guid() /*currently not used*/);
                var releases = publications.Single().Releases;
                Assert.True(releases.Exists(r => r.Id == latestReleaseId && r.LatestRelease)); 
                Assert.True(releases.Exists(r => r.Id == notLatestReleaseId && !r.LatestRelease));
            }
        }
    }
}