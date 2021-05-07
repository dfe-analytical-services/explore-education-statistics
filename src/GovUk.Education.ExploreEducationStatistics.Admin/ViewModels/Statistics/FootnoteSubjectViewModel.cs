using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics
{
    public class FootnoteSubjectViewModel
    {
        public Dictionary<Guid, FootnoteFilterViewModel> Filters { get; set; }
        public Dictionary<Guid, FootnoteIndicatorGroupViewModel> IndicatorGroups { get; set; }
        public Dictionary<GeographicLevel, FootnotesLocationsMetaViewModel> Locations { get; set; }
        public List<(int, TimeIdentifier)> TimePeriods { get; set; }
        public bool Selected { get; set; }
    }
}
