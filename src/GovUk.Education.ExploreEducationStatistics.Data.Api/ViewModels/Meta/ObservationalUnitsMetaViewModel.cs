using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class ObservationalUnitsViewModel
    {
        public IEnumerable<NameLabelViewModel> Country { get; set; }
        public IEnumerable<NameLabelViewModel> LocalAuthority { get; set; }
        public IEnumerable<NameLabelViewModel> Region { get; set; }
        public TimePeriodMetaViewModel TimePeriod { get; set; }
    }
}