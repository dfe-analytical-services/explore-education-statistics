using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public class TableBuilderResultViewModel
    {
        public SubjectResultMetaViewModel SubjectMeta { get; set; }

        public IEnumerable<ObservationViewModel> Results { get; set; }

        public TableBuilderResultViewModel()
        {
            Results = new List<ObservationViewModel>();
        }
    }
}
