using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class ResultWithMetaViewModel
    {
        public SubjectMetaViewModel MetaData { get; set; }
        public IEnumerable<ObservationViewModel> Result { get; set; }

        public ResultWithMetaViewModel()
        {
            Result = new List<ObservationViewModel>();
        }
    }
}