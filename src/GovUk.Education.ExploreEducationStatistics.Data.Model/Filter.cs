using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Filter
    {
        public Guid Id { get; set; }
        public string Hint { get; set; }
        public string Label { get; set; }
        public string Name { get; set; }
        public Subject Subject { get; set; }
        public Guid SubjectId { get; set; }
        public ICollection<FilterGroup> FilterGroups { get; set; }
        public ICollection<FilterFootnote> Footnotes { get; set; }

        public static IEqualityComparer<Filter> IdComparer { get; } = new IdEqualityComparer();

        public Filter()
        {
        }

        public Filter(string hint, string label, string name, Subject subject)
        {
            Id = Guid.NewGuid();
            Hint = hint;
            Label = label;
            Name = name;
            Subject = subject;
            FilterGroups = new List<FilterGroup>();
        }

        private sealed class IdEqualityComparer : IEqualityComparer<Filter>
        {
            public bool Equals(Filter x, Filter y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id.Equals(y.Id);
            }

            public int GetHashCode(Filter obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}