using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class ResultWithMetaViewModel : ResultViewModel
    {
        public SubjectMetaViewModel MetaData { get; set; }
    }
}