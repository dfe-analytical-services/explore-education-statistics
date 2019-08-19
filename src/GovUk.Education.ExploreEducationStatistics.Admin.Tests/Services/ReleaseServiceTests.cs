using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseServiceTests
    {

        [Fact]
        public async void LatestReleaseCorrectlyReported()
        {
            var latestReleaseId = new Guid("274d4621-7d21-431b-80de-77b62a4374d2");
            var notLatestReleaseId = new Guid("49b73c2f-141a-4dc7-a2d4-69316aef8bbc");
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                context.Add(new Publication
                {
                    Id = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"),
                    Title = "Publication",
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
                });

                context.SaveChanges();
            }

            // Note that we use different contexts for each method call - this is to avoid misleadingly optimistic
            // loading of the entity graph as we go.
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                // Method under test
                var notLatest =
                    await new ReleaseService(context, MapperForProfile<MappingProfiles>()).GetReleaseForIdAsync(
                        notLatestReleaseId);
                Assert.False(notLatest.LatestRelease);
            }

            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                // Method under test
                var notLatest =
                    await new ReleaseService(context, MapperForProfile<MappingProfiles>()).GetReleaseForIdAsync(
                        notLatestReleaseId);
                Assert.False(notLatest.LatestRelease);
            }
        }
    }
}