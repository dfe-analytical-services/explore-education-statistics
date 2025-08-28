#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

public interface ISubjectMetaService
{
    Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(Guid releaseVersionId,
        Guid subjectId);

    Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(ReleaseSubject releaseSubject);

    Task<Either<ActionResult, SubjectMetaViewModel>> FilterSubjectMeta(Guid? releaseVersionId,
        LocationsOrTimePeriodsQueryRequest request,
        CancellationToken cancellationToken);

    Task<Either<ActionResult, Unit>> UpdateSubjectFilters(Guid releaseVersionId,
        Guid subjectId,
        List<FilterUpdateViewModel> request);

    Task<Either<ActionResult, Unit>> UpdateSubjectIndicators(Guid releaseVersionId,
        Guid subjectId,
        List<IndicatorGroupUpdateViewModel> request);
}
