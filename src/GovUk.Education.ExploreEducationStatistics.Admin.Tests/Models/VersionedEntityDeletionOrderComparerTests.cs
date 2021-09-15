using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Models
{
    public class VersionedEntityDeletionOrderComparerTests
    {
        [Fact]
        public void VersionedEntityDeletionOrderComparer()
        {
            var version1Id = Guid.NewGuid();
            var version2Id = Guid.NewGuid();
            var version3Id = Guid.NewGuid();

            var version1 = new IdAndPreviousVersionIdPair(version1Id, null);
            var version2 = new IdAndPreviousVersionIdPair(version2Id, version1Id);
            var version3 = new IdAndPreviousVersionIdPair(version3Id, version2Id);

            var comparer = new VersionedEntityDeletionOrderComparer();

            Assert.Equal(-1, comparer.Compare(version2, version1));
            Assert.Equal(-1, comparer.Compare(version3, version2));

            Assert.Equal(1, comparer.Compare(version1, version2));
            Assert.Equal(1, comparer.Compare(version2, version3));

            Assert.Equal(1, comparer.Compare(version1, version3));
            Assert.Equal(-1, comparer.Compare(version3, version1));
        }

        [Fact]
        public void VersionedEntityDeletionOrderComparer_WithSequence()
        {
            var version1Id = Guid.NewGuid();
            var version4Id = Guid.NewGuid();
            var version3Id = Guid.NewGuid();
            var version2Id = Guid.NewGuid();
            var version5Id = Guid.NewGuid();

            var versions = new List<IdAndPreviousVersionIdPair>
            {
                new(version2Id, version1Id),
                new(version1Id, null),
                new(version5Id, version4Id),
                new(version4Id, version3Id),
                new(version3Id, version2Id)
            };

            var orderedByLatestVersionsFirst = versions
                .OrderBy(version => version, new VersionedEntityDeletionOrderComparer())
                .Select(version => version.Id)
                .ToList();

            var expectedVersionOrder = ListOf(version5Id, version4Id, version3Id, version2Id, version1Id);
            Assert.Equal(expectedVersionOrder, orderedByLatestVersionsFirst);
        }
    }
}
