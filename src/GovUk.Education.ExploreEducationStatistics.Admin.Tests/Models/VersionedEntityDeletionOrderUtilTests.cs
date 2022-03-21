using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Models
{
    public class VersionedEntityDeletionOrderUtilTests
    {
        [Fact]
        public void Sort()
        {
            var entity1Version1Id = "entity-1-version-1";
            var entity1Version2Id = "entity-1-version-2";
            var entity1Version3Id = "entity-1-version-3";
            var entity1Version4Id = "entity-1-version-4";
            var entity1Version5Id = "entity-1-version-5";
            var entity1Version6Id = "entity-1-version-6";
            var entity1Version7Id = "entity-1-version-7";
            var entity1Version8Id = "entity-1-version-8";
            var entity2Version1Id = "entity-2-version-1";
            var entity2Version2Id = "entity-2-version-2";
            var entity2Version3Id = "entity-2-version-3";
            var entity3Version1Id = "entity-3-version-1";
            var entity3Version2Id = "entity-3-version-2";
            var entity4Version3Id = "entity-4-version-3";
            var entity4Version6Id = "entity-4-version-6";

            var versions = new List<IdAndPreviousVersionIdPair<string>>
            {
                new(entity1Version6Id, entity1Version5Id),
                new(entity3Version2Id, entity3Version1Id),
                null,
                new(entity1Version2Id, entity1Version1Id),
                new(entity4Version3Id, "entity-4-version-2"),
                new(entity2Version3Id, entity2Version2Id),
                new(entity1Version1Id, null),
                new(entity1Version5Id, entity1Version4Id),
                new(entity1Version7Id, entity1Version6Id),
                new(entity3Version1Id, null),
                new(entity1Version8Id, entity1Version7Id),
                new(entity4Version6Id, "entity-4-version-5"),
                new(entity2Version2Id, entity2Version1Id),
                new(entity1Version4Id, entity1Version3Id),
                new(entity1Version3Id, entity1Version2Id),
                new(entity2Version1Id, null)
            };

            var ordered = VersionedEntityDeletionOrderUtil
                .Sort(versions)
                .Select(version => version.Id)
                .ToList();

            var expectedVersionOrder = ListOf(
                entity1Version8Id,
                entity1Version7Id,
                entity1Version6Id,
                entity1Version5Id, 
                entity1Version4Id, 
                entity1Version3Id, 
                entity1Version2Id, 
                entity1Version1Id,
                entity2Version3Id,
                entity2Version2Id,
                entity2Version1Id,
                entity3Version2Id,
                entity3Version1Id,
                entity4Version3Id,
                entity4Version6Id);
            
            Assert.Equal(expectedVersionOrder, ordered);
        }
    }
}
