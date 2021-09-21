#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public class MetaGuidanceSubjectViewModel
    {
        public Guid Id { get; set; }

        public string Filename { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public TimePeriodRangeLabels TimePeriodsRange { get; set; } = new TimePeriodRangeLabels();

        public List<string> GeographicLevels { get; set; } = new List<string>();

        public List<LabelValue> Variables { get; set; } = new List<LabelValue>();

        public List<FootnoteViewModel> Footnotes { get; set; } = new List<FootnoteViewModel>();
    }
}
