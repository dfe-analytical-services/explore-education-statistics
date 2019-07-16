using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta.TableBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface ITableBuilderSubjectMetaService
    {
        TableBuilderSubjectMetaViewModel GetSubjectMeta(SubjectMetaQueryContext query);
    }
}