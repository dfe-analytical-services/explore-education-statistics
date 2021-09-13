#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class IdAndPreviousVersionIdPair
    {
        public readonly Guid Id;
        public readonly Guid? PreviousVersionId;

        public IdAndPreviousVersionIdPair(Guid id, Guid? previousVersionId)
        {
            Id = id;
            PreviousVersionId = previousVersionId;
        }
    }

    /// <summary>
    /// An IComparer implementation that orders entities based upon having previous versions.  This IComparer will
    /// order the entities so that entities that have no previous versions will appear at the end of the list,
    /// entities that have that set of entities as previous versions will appear before them etc. 
    /// </summary>
    public class VersionedEntityDeletionOrderComparer : IComparer<IdAndPreviousVersionIdPair>
    {
        public int Compare(IdAndPreviousVersionIdPair? entity1Ids, IdAndPreviousVersionIdPair? entity2Ids)
        {
            if (entity1Ids == null)
            {
                return 1;
            }

            if (entity2Ids == null)
            {
                return -1;
            }

            if (entity1Ids.PreviousVersionId == null)
            {
                return 1;
            }

            if (entity2Ids.PreviousVersionId == null)
            {
                return -1;
            }

            if (entity1Ids.PreviousVersionId == entity2Ids.Id)
            {
                return -1;
            }

            return entity2Ids.PreviousVersionId == entity1Ids.Id ? 1 : -1;
        }
    }
}
