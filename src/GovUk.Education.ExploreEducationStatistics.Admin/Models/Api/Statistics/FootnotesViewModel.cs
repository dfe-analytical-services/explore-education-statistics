using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics
{
    public class FootnotesViewModel
    {
        public IEnumerable<FootnoteViewModel> Footnotes { get; set; }
        public Dictionary<long, FootnotesSubjectMetaViewModel> Meta { get; set; }
    }
}