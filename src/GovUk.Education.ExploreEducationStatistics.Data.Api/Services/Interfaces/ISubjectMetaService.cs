using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface ISubjectMetaService
    {
        SubjectMetaViewModel GetSubjectMeta(SubjectMetaQueryContext query, IEnumerable<Observation> observations);
    }
}