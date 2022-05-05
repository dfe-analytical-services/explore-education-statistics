#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface ISubjectMetaService
    {
        Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(Guid subjectId);

        Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(
            ObservationQueryContext query,
            CancellationToken cancellationToken);

        Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMetaRestricted(
            Guid releaseId,
            Guid subjectId);

        Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMetaRestricted(
            Guid releaseId,
            ObservationQueryContext query,
            CancellationToken cancellationToken);

        Task<Either<ActionResult, Unit>> UpdateSubjectFilters(
            Guid releaseId,
            Guid subjectId,
            List<FilterUpdateViewModel> request);

        Task<Either<ActionResult, Unit>> UpdateSubjectIndicators(
            Guid releaseId,
            Guid subjectId,
            List<IndicatorGroupUpdateViewModel> request);
    }
}
