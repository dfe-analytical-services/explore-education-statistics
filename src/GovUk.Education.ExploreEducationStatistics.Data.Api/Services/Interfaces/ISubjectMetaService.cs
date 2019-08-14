using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface ISubjectMetaService
    {
        SubjectMetaViewModel GetSubjectMeta(SubjectMetaQueryContext query, IQueryable<Observation> observations);
    }
}