using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
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