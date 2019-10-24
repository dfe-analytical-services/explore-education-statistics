using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class FilterItem : IEquatable<FilterItem>
    {
        public long Id { get; set; }
        public string Label { get; set; }
        public FilterGroup FilterGroup { get; set; }
        public long FilterGroupId { get; set; }
        public ICollection<FilterItemFootnote> Footnotes { get; set; }

        private FilterItem()
        {
        }

        public FilterItem(string label, FilterGroup filterGroup)
        {
            Label = label;
            FilterGroup = filterGroup;
            FilterGroupId = filterGroup.Id;
        }

        public bool Equals(FilterItem other)
        {
            return other?.Id == Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FilterItem) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}