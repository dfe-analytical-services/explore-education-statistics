#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

public interface IReleaseSubjectService
{
    Task<Either<ActionResult, ReleaseFile>> Find(Guid subjectId, Guid? releaseVersionId = null);

    Task<ReleaseFile?> FindForLatestPublishedVersion(Guid subjectId);
}
