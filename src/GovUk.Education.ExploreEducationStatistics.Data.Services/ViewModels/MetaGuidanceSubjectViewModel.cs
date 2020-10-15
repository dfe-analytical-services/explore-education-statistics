using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public class MetaGuidanceSubjectViewModel
    {
        public Guid Id { get; set; }
        public string Filename { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public MetaGuidanceSubjectTimePeriodsViewModel TimePeriods { get; set; }
        public List<string> GeographicLevels { get; set; }
        public List<LabelValue> Variables { get; set; }
    }

    public class MetaGuidanceSubjectTimePeriodsViewModel
    {
        public string From { get; }
        public string To { get; }

        public MetaGuidanceSubjectTimePeriodsViewModel()
        {
        }

        public MetaGuidanceSubjectTimePeriodsViewModel(string from, string to)
        {
            From = from;
            To = to;
        }
    }
}