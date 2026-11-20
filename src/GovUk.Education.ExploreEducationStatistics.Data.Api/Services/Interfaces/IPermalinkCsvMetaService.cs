#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;

public interface IPermalinkCsvMetaService
{
    Task<Either<ActionResult, PermalinkCsvMetaViewModel>> GetCsvMeta(
        Guid subjectId,
        SubjectResultMetaViewModel tableResultMeta,
        IReadOnlyList<Location> locations,
        CancellationToken cancellationToken = default
    );
}
