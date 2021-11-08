#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class IdAndPreviousVersionIdPair<TId> where TId : class, IComparable
    {
        public readonly TId Id;
        public readonly TId? PreviousVersionId;

        public IdAndPreviousVersionIdPair(TId id, TId? previousVersionId)
        {
            Id = id;
            PreviousVersionId = previousVersionId;
        }

        public bool FirstVersion => PreviousVersionId == null;

        public bool DirectDescendantOf(IdAndPreviousVersionIdPair<TId> entityIds) => entityIds.Id.Equals(PreviousVersionId);

        protected bool Equals(IdAndPreviousVersionIdPair<TId> other)
        {
            return Id.Equals(other.Id) && Nullable.Equals(PreviousVersionId, other.PreviousVersionId);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((IdAndPreviousVersionIdPair<TId>)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, PreviousVersionId);
        }
    }

    /// <summary>
    /// A utility class to order a set of versioned entity ids in correct deletion order, with most recent descendants first and 
    /// ancestors last, in order to prevent the deletion of an ancestor prior to one of its referencing descendants. 
    /// </summary>
    public static class VersionedEntityDeletionOrderUtil
    {
        public static List<IdAndPreviousVersionIdPair<TId>> Sort<TId>(List<IdAndPreviousVersionIdPair<TId>> entities)
            where TId : class, IComparable
        {
            var sorted = new List<IdAndPreviousVersionIdPair<TId>>();
            var unsorted = entities
                .Where(entity => entity != null)
                .ToList();

            // Identify the deepest entity ancestors in the list.  These are the first versions in the ancestry (no
            // previous version) or ones where their previous version is not present in the unsorted list.
            //
            // Add them as the first entries in the sorted list. 
            var entityIds = unsorted.Select(entity => entity.Id);
            
            var deepestAncestors = unsorted
                .Where(entity => entity.FirstVersion || !entityIds.Contains(entity.PreviousVersionId))
                .ToList();
            
            sorted.AddRange(deepestAncestors.OrderBy(entity => entity.Id).ToList());
            unsorted.RemoveAll(entity => deepestAncestors.Contains(entity));
            
            // Now repeatedly select the immediate descendents of items in the sorted list and add them before their
            // direct ancestors in the sorted list.
            //
            // Repeat this until all of the descendants in the unsorted list have been transferred into the sorted list.
            while (unsorted.Count > 0)
            {
                foreach (var descendant in unsorted.ToList())
                {
                    var directAncestor =
                        sorted.FirstOrDefault(ancestor => descendant.DirectDescendantOf(ancestor));

                    if (directAncestor != null)
                    {
                        sorted.Insert(sorted.IndexOf(directAncestor), descendant);
                        unsorted.Remove(descendant);
                    }
                }
            }

            return sorted;
        }
    }
}
