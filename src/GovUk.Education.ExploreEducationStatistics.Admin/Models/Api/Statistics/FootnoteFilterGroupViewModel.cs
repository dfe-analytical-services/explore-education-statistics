using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics
{
    public class FootnoteFilterGroupViewModel
    {
        public IEnumerable<string> FilterItems { get; set; }
        public bool Selected { get; set; }
    }
}