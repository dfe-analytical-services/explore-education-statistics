#nullable enable
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
        public TimePeriodLabels TimePeriods { get; set; }
        public List<string> GeographicLevels { get; set; }
        public List<LabelValue> Variables { get; set; }
    }
}
