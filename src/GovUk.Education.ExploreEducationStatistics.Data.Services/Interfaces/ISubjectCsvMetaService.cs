#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

public interface ISubjectCsvMetaService
{
    Task<Either<ActionResult, SubjectCsvMetaViewModel>> GetSubjectCsvMeta(
        ReleaseFile releaseFile,
        FullTableQuery query,
        CancellationToken cancellationToken = default
    );
}
