#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

public interface ISubjectResultMetaService
{
    Task<Either<ActionResult, SubjectResultMetaViewModel>> GetSubjectMeta(
        Guid releaseVersionId,
        FullTableQuery query,
        IList<Observation> observations);
}
