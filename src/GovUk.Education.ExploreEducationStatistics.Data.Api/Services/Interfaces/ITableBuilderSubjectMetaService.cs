using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface ITableBuilderSubjectMetaService
    {
        TableBuilderSubjectMetaViewModel GetSubjectMeta(SubjectMetaQueryContext query);
    }
}