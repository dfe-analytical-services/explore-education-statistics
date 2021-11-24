#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IResultSubjectMetaService
    {
        Task<Either<ActionResult, ResultSubjectMetaViewModel>> GetSubjectMeta(Guid releaseId,
            SubjectMetaQueryContext query,
            IQueryable<Observation> observations);
    }
}
