using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics
{
    public class FootnoteSubjectViewModel
    {
        public Dictionary<long, FootnoteFilterViewModel> Filters { get; set; }
        public Dictionary<long, FootnoteIndicatorGroupViewModel> IndicatorGroups { get; set; }
        public bool Selected { get; set; }
    }
}