using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics
{
    public class FootnoteFilterViewModel
    {
        public Dictionary<Guid, FootnoteFilterGroupViewModel> FilterGroups { get; set; }
        public bool Selected { get; set; }
    }
}