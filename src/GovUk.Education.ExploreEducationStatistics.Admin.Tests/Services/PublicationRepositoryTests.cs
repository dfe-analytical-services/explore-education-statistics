using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PublicationRepositoryTests
    {
        [Fact]
        public async void ReleasesCorrectlyOrdered()
        {
            var topicId = Guid.NewGuid();
            var contextId = Guid.NewGuid().ToString();

            using (var context = InMemoryApplicationDbContext(contextId))
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
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2000",
                                    TimePeriodCoverage = TimeIdentifier.Week1,
                                    Published = DateTime.UtcNow
                                },
                                new Release
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2000",
                                    TimePeriodCoverage = TimeIdentifier.Week11,
                                    Published = DateTime.UtcNow
                                },
                                new Release
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2000",
                                    TimePeriodCoverage = TimeIdentifier.Week3,
                                    Published = DateTime.UtcNow
                                },
                                new Release
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2000",
                                    TimePeriodCoverage = TimeIdentifier.Week2,
                                    Published = DateTime.UtcNow
                                },
                                new Release
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "2001",
                                    TimePeriodCoverage = TimeIdentifier.Week1,
                                    Published = DateTime.UtcNow
                                },
                                new Release
                                {
                                    Id = Guid.NewGuid(),
                                    ReleaseName = "1999",
                                    TimePeriodCoverage = TimeIdentifier.Week1,
                                    Published = DateTime.UtcNow
                                }
                            }
                        }
                    }
                });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext(contextId))
            {
                var publicationService = new PublicationRepository(context, AdminMapper());
                var publications = await publicationService.GetAllPublicationsForTopicAsync(topicId);
                var releases = publications.Single().Releases;
                Assert.Equal("Week 1 2001", releases[0].Title);
                Assert.Equal("Week 11 2000", releases[1].Title);
                Assert.Equal("Week 3 2000", releases[2].Title);
                Assert.Equal("Week 2 2000", releases[3].Title);
                Assert.Equal("Week 1 2000", releases[4].Title);
                Assert.Equal("Week 1 1999", releases[5].Title);
            }
        }

        [Fact]
        public async void LatestReleaseCorrectlyReportedInPublication()
        {
            var latestReleaseId = Guid.NewGuid();
            var notLatestReleaseId = Guid.NewGuid();
            var topicId = Guid.NewGuid();
            var contextId = Guid.NewGuid().ToString();

            using (var context = InMemoryApplicationDbContext(contextId))
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
                                    ReleaseName = "2019",
                                    TimePeriodCoverage = TimeIdentifier.December,
                                    Published = DateTime.UtcNow
                                },
                                new Release
                                {
                                    Id = latestReleaseId,
                                    ReleaseName = "2020",
                                    TimePeriodCoverage = TimeIdentifier.June,
                                    Published = DateTime.UtcNow
                                }
                            }
                        }
                    }
                });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext(contextId))
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