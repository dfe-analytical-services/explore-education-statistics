#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class FilterItem : IEquatable<FilterItem>
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public FilterGroup FilterGroup { get; set; } = null!;
        public Guid FilterGroupId { get; set; }
        public List<FilterItemFootnote> Footnotes { get; set; } = new();

        public FilterItem()
        {
        }

        public FilterItem(string label, Guid filterGroupId)
        {
            Id = Guid.NewGuid();
            Label = label;
            FilterGroupId = filterGroupId;
        }

        public bool Equals(FilterItem? other)
        {
            return other?.Id == Id;
        }

        public override bool Equals(object? obj)
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
