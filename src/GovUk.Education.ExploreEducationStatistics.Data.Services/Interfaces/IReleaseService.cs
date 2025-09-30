#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

public interface IReleaseService
{
    Task<Either<ActionResult, List<SubjectViewModel>>> ListSubjects(Guid releaseVersionId);

    Task<Either<ActionResult, List<FeaturedTableViewModel>>> ListFeaturedTables(
        Guid releaseVersionId
    );
}
