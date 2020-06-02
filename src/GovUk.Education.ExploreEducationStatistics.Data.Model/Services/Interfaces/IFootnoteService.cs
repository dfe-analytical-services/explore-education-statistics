using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFootnoteService
    {
        IEnumerable<Footnote> GetFootnotes(
            Guid releaseId,
            Guid subjectId,
            IQueryable<Observation> observations,
            IEnumerable<Guid> indicators
        );

        Task<Either<ActionResult, bool>> DeleteFootnote(Guid releaseId, Guid id);

        Task<Either<ActionResult, bool>> DeleteFootnotes(Guid releaseId, List<Footnote> footnotes);

        Task<bool> IsFootnoteExclusiveToReleaseAsync(Guid releaseId, Guid footnoteId);
    }
}