#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class FilterGroup
    {
        public Guid Id { get; set; }
        public Filter Filter { get; set; } = null!;
        public Guid FilterId { get; set; }
        public string Label { get; set; } = string.Empty;
        public List<FilterItem> FilterItems { get; set; } = new();
        public List<FilterGroupFootnote> Footnotes { get; set; } = new();

        public static IEqualityComparer<FilterGroup> IdComparer { get; } = new IdEqualityComparer();

        public FilterGroup()
        {
        }

        public FilterGroup(Guid filterId, string label)
        {
            Id = Guid.NewGuid();
            FilterId = filterId;
            Label = label;
            FilterItems = new List<FilterItem>();
        }

        public FilterGroup Clone()
        {
            return (FilterGroup) MemberwiseClone();
        }

        private sealed class IdEqualityComparer : IEqualityComparer<FilterGroup>
        {
            public bool Equals(FilterGroup x, FilterGroup y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id.Equals(y.Id);
            }

            public int GetHashCode(FilterGroup obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}
