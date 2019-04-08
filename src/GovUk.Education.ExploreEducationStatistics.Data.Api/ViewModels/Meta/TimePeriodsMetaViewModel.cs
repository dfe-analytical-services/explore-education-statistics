using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class TimePeriodsMetaViewModel
    {
        public string Hint { get; set; }
        public string Legend { get; set; }
        public IEnumerable<TimePeriodMetaViewModel> Options { get; set; }
    }
}