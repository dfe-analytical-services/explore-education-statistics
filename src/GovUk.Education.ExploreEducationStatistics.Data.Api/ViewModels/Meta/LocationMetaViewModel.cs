using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class LocationMetaViewModel
    {
        public IEnumerable<LabelValueViewModel> LocalAuthority { get; set; }
        public IEnumerable<LabelValueViewModel> National { get; set; }
        public IEnumerable<LabelValueViewModel> Region { get; set; }
        public IEnumerable<LabelValueViewModel> School { get; set; }
    }
}