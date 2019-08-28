using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public class TableBuilderResultViewModel
    {
        public TableBuilderResultSubjectMetaViewModel SubjectMeta { get; set; }

        public IEnumerable<ObservationViewModel> Results { get; set; }

        public TableBuilderResultViewModel()
        {
            Results = new List<ObservationViewModel>();
        }
    }
}