#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

public interface IReleaseSubjectService
{
    Task<Either<ActionResult, ReleaseSubject>> Find(Guid subjectId, Guid? releaseId = null);

    Task<ReleaseSubject?> FindForLatestPublishedVersion(Guid subjectId);
}
