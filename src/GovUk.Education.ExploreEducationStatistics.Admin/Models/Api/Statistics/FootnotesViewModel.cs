using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics
{
    public class FootnotesViewModel
    {
        public IEnumerable<FootnoteViewModel> Footnotes { get; set; }
        public Dictionary<Guid, FootnotesSubjectMetaViewModel> Meta { get; set; }
    }
}