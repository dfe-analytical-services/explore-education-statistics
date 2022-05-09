#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface ISubjectMetaService
    {
        Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(Guid releaseId, Guid subjectId);

        Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(ReleaseSubject releaseSubject);

        Task<Either<ActionResult, SubjectMetaViewModel>> FilterSubjectMeta(Guid? releaseId,
            ObservationQueryContext query,
            CancellationToken cancellationToken);

        Task<Either<ActionResult, Unit>> UpdateSubjectFilters(Guid releaseId,
            Guid subjectId,
            List<FilterUpdateViewModel> request);

        Task<Either<ActionResult, Unit>> UpdateSubjectIndicators(Guid releaseId,
            Guid subjectId,
            List<IndicatorGroupUpdateViewModel> request);
    }
}
