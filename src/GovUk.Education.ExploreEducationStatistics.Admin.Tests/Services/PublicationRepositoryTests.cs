using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationRepositoryTests
    {
        [Fact]
        public async void LatestReleaseCorrectlyReportedInPublication()
        {
            var latestReleaseId = Guid.NewGuid();
            var notLatestReleaseId = Guid.NewGuid();
            var topicId = Guid.NewGuid();
            
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReportedInPublication"))
            {
                context.Add(new Topic
                {
                    Id = topicId,
                    Publications = new List<Publication>
                    {

                        new Publication
                        {
                            Id = Guid.NewGuid(),
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
                var publicationService = new PublicationRepository(context, AdminMapper());
                
                // Method under test - this return a list of publication for a user. The releases in the publication
                // should correctly report whether they are the latest or not. Note that this is dependent on the mapper
                // that we are passing in.
                var publications = await publicationService.GetAllPublicationsForTopicAsync(topicId);
                var releases = publications.Single().Releases;
                Assert.True(releases.Exists(r => r.Id == latestReleaseId && r.LatestRelease)); 
                Assert.True(releases.Exists(r => r.Id == notLatestReleaseId && !r.LatestRelease));
            }
        }
    }
}