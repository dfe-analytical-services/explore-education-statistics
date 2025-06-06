#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query
{
    public record FullTableQuery
    {
        public Guid SubjectId { get; set; }

        public List<Guid> LocationIds { get; set; } = new();

        public TimePeriodQuery? TimePeriod { get; set; }

        public IEnumerable<Guid> Filters
        {
            [Obsolete("Use GetFilterItemIds() or GetNonHierarchicalFilterItemIds")]
            get; 
            set;
        } = new List<Guid>();

        public IEnumerable<Guid> Indicators { get; set; } = new List<Guid>();

        // FilterHierarchyOptions.Values are a list of a list of FilterItemIds, all associated with a particular filter hierarchy,
        // and each list of Guids associated with a particular filter hierarchy option/checkbox.
        // FilterHierarchyOptions Key is the leaf FilterId for the hierarchy the Value's FilterItemIds are associated with.
        public IDictionary<Guid, List<List<Guid>>>? FilterHierarchyOptions { get; set; } = null;

        public IEnumerable<Guid> GetNonHierarchicalFilterItemIds()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return Filters;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public List<Guid> GetFilterItemIds()
        {
            var filterItemIds = GetNonHierarchicalFilterItemIds().ToList();
            if (FilterHierarchyOptions != null)
            {
                filterItemIds.AddRange(FilterHierarchyOptions
                    .SelectMany(keyValue =>
                        keyValue.Value.SelectMany(hierarchyOption => hierarchyOption)));
            }

            // NOTE: We don't include FilterHierarchyOptions.Keys as they are filterIds, not filterItemIds

            return filterItemIds.Distinct().ToList();
        }
    }
}
