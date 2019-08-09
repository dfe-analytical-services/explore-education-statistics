using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class TableResultViewModel
    {
        public IEnumerable<FootnoteViewModel> Footnotes { get; set; }
        
        public IEnumerable<TimePeriodMetaViewModel> TimePeriodRange { get; set; }

        public IEnumerable<ObservationViewModel> Result { get; set; }

        public TableResultViewModel()
        {
            Result = new List<ObservationViewModel>();
            TimePeriodRange = new List<TimePeriodMetaViewModel>();
        }
    }
}