using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.ViewModels;

public class TableBuilderResultViewModel
{
    public SubjectResultMetaViewModel SubjectMeta { get; set; }

    public IEnumerable<ObservationViewModel> Results { get; set; }

    public TableBuilderResultViewModel()
    {
        Results = new List<ObservationViewModel>();
    }
}
