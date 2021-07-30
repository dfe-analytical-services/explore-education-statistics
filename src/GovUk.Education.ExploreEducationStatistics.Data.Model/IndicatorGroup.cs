using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class IndicatorGroup
    {
        public Guid Id { get; set; }
        public string Label { get; set; }
        public Subject Subject { get; set; }
        public Guid SubjectId { get; set; }
        public IList<Indicator> Indicators { get; set; }

        public IndicatorGroup()
        {
        }

        public IndicatorGroup(string label, Subject subject)
        {
            Id = Guid.NewGuid();
            Label = label;
            Subject = subject;
            Indicators = new List<Indicator>();
        }
    }
}