using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class MetaServiceTests
    {
        [Fact]
        public void GetReleaseTypes()
        {
            using (var context = DbUtils.InMemoryApplicationDbContext("Find"))
            {
                var releaseTypesToSave = new List<ReleaseType>
                {
                    new ReleaseType()
                    {
                        Id = Guid.NewGuid(),
                        Title = "Ad Hoc Statistics",
                    },
                    new ReleaseType()
                    {
                        Id = Guid.NewGuid(),
                        Title = "Official Statistics",
                    }
                };


                context.AddRange(releaseTypesToSave);
                context.SaveChanges();

                var service = new MetaService(context);
                // Method under test
                var retrievedReleaseTypes = service.GetReleaseTypes();
                Assert.True(retrievedReleaseTypes.Exists(rt => rt.Title == "Ad Hoc Statistics"));
                Assert.True(retrievedReleaseTypes.Exists(rt => rt.Title == "Official Statistics"));
            }
        }

        [Fact]
        public void GetTimeIdentifiers()
        {
            var service = new MetaService(null /* Do not need */);
            // Method under test
            var timeIdentifiersRetrieved = service.GetTimeIdentifiersByCategory();
            foreach (TimeIdentifierCategory category in Enum.GetValues(typeof(TimeIdentifierCategory)))
            {
                // Check each Category is accounted for
                Assert.True(timeIdentifiersRetrieved.Exists(ti => ti.Category == category));
                foreach (var identifier in category.GetTimeIdentifiers())
                {
                    // Check each Identifier is accounted for
                    Assert.True(timeIdentifiersRetrieved.Single(ti => ti.Category == category).TimeIdentifiers
                        .Exists(i => i.Identifier == identifier));
                }
            }
        }
    }
}