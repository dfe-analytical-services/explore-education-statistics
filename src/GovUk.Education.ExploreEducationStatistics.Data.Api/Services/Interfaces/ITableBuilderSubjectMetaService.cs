using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface ITableBuilderSubjectMetaService
    {
        TableBuilderSubjectMetaViewModel GetSubjectMeta(SubjectMetaQueryContext query);
    }
}