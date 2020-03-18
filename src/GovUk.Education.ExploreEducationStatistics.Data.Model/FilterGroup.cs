using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class FilterGroup
    {
        public Guid Id { get; set; }
        public Filter Filter { get; set; }
        public Guid FilterId { get; set; }
        public string Label { get; set; }
        public ICollection<FilterItem> FilterItems { get; set; }
        public ICollection<FilterGroupFootnote> Footnotes { get; set; }

        public static IEqualityComparer<FilterGroup> IdComparer { get; } = new IdEqualityComparer();

        public FilterGroup()
        {
        }

        public FilterGroup(Filter filter, string label)
        {
            Id = Guid.NewGuid();
            Filter = filter;
            Label = label;
            FilterItems = new List<FilterItem>();
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