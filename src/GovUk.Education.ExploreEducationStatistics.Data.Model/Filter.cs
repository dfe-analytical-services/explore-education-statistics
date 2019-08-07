using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Filter
    {
        public long Id { get; set; }
        public string Hint { get; set; }
        public string Label { get; set; }
        public string Name { get; set; }
        public Subject Subject { get; set; }
        public long SubjectId { get; set; }
        public ICollection<FilterGroup> FilterGroups { get; set; }
        public ICollection<FilterFootnote> Footnotes { get; set; }

        private Filter()
        {
        }

        public Filter(string hint, string label, string name, Subject subject)
        {
            Hint = hint;
            Label = label;
            Name = name;
            Subject = subject;
            FilterGroups = new List<FilterGroup>();
        }
    }
}