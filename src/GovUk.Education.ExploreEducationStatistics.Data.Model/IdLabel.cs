using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class IdLabel
    {
        public IdLabel(Guid id, string label)
        {
            Id = id;
            Label = label;
        }

        public IdLabel()
        {
        }

        public Guid Id { get; set; }
        public string Label { get; set; }
    }
}