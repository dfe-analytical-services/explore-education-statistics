using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics
{
    public class FootnoteFilterViewModel
    {
        public Dictionary<long, FootnoteFilterGroupViewModel> FilterGroups { get; set; }
        public bool Selected { get; set; }
    }
}