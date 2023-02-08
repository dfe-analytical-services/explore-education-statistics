#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

public interface ISubjectCsvMetaService
{
    Task<Either<ActionResult, SubjectCsvMetaViewModel>> GetSubjectCsvMeta(
        ReleaseSubject releaseSubject,
        ObservationQueryContext query,
        IList<Observation> observations,
        CancellationToken cancellationToken = default);
}
