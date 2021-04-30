using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics
{
    public class FootnoteSubjectViewModel
    {
        public Dictionary<Guid, FootnoteFilterViewModel> Filters { get; set; }
        public Dictionary<Guid, FootnoteIndicatorGroupViewModel> IndicatorGroups { get; set; }
        public bool Selected { get; set; }
    }
}
