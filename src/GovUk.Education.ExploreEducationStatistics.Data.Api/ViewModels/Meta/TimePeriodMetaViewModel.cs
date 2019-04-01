using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class TimePeriodMetaViewModel
    {
        public string Hint { get; set; }
        public string Legend { get; set; }
        public IEnumerable<LabelValueViewModel> Options { get; set; }
    }
}