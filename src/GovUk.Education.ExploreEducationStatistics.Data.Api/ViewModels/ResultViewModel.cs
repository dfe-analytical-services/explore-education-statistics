using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class ResultViewModel
    {
        public IEnumerable<FootnoteViewModel> Footnotes { get; set; }
        
        public IEnumerable<TimePeriodMetaViewModel> TimePeriodRange { get; set; }

        public IEnumerable<ObservationViewModel> Result { get; set; }

        public ResultViewModel()
        {
            Result = new List<ObservationViewModel>();
            TimePeriodRange = new List<TimePeriodMetaViewModel>();
        }
    }
}