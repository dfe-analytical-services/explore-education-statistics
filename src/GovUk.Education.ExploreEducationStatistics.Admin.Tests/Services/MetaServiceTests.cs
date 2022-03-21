#nullable enable
using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class MetaServiceTests
    {
        [Fact]
        public void GetTimeIdentifiers()
        {
            var service = new MetaService();

            var timeIdentifiersRetrieved = service.GetTimeIdentifiersByCategory();
            foreach (var category in (TimeIdentifierCategory[]) Enum.GetValues(typeof(TimeIdentifierCategory)))
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
