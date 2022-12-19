#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFootnoteService
    {
        Task<Either<ActionResult, Footnote>> CreateFootnote(
            Guid releaseId,
            string content,
            IReadOnlySet<Guid> filterIds,
            IReadOnlySet<Guid> filterGroupIds,
            IReadOnlySet<Guid> filterItemIds,
            IReadOnlySet<Guid> indicatorIds,
            IReadOnlySet<Guid> subjectIds);

        Task<Either<ActionResult, List<Footnote>>> CopyFootnotes(Guid sourceReleaseId, Guid destinationReleaseId);

        Task<Either<ActionResult, Unit>> DeleteFootnote(Guid releaseId, Guid footnoteId);

        Task<Either<ActionResult, Footnote>> GetFootnote(Guid releaseId, Guid footnoteId);

        Task<Either<ActionResult, List<Footnote>>> GetFootnotes(Guid releaseId);

        Task<Either<ActionResult, Footnote>> UpdateFootnote(
            Guid releaseId,
            Guid footnoteId,
            string content,
            IReadOnlySet<Guid> filterIds,
            IReadOnlySet<Guid> filterGroupIds,
            IReadOnlySet<Guid> filterItemIds,
            IReadOnlySet<Guid> indicatorIds,
            IReadOnlySet<Guid> subjectIds);

        Task<Either<ActionResult, Unit>> UpdateFootnotes(Guid releaseId,
            FootnotesUpdateRequest request);
    }
}
