using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface ITableBuilderSubjectMetaService
    {
        TableBuilderSubjectMetaViewModel GetSubjectMeta(SubjectMetaQueryContext query);
    }
}