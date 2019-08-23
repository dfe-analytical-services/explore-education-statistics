using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface ISubjectMetaService
    {
        SubjectMetaViewModel GetSubjectMeta(SubjectMetaQueryContext query, IQueryable<Observation> observations);
    }
}