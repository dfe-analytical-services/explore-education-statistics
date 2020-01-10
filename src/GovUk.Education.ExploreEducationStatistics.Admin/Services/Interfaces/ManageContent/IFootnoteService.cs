using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent
{
    public interface IFootnoteService
    {
        Task<Either<ActionResult, Footnote>> CreateFootnote(string content,
            IEnumerable<Guid> filterIds,
            IEnumerable<Guid> filterGroupIds,
            IEnumerable<Guid> filterItemIds,
            IEnumerable<Guid> indicatorIds,
            IEnumerable<Guid> subjectIds);

        Task<Either<ActionResult, bool>> DeleteFootnote(Guid id);

        Task<Either<ActionResult, IEnumerable<Footnote>>> GetFootnotesAsync(Guid releaseId);

        Task<Either<ActionResult, Footnote>> UpdateFootnote(Guid id,
            string content,
            IEnumerable<Guid> filterIds,
            IEnumerable<Guid> filterGroupIds,
            IEnumerable<Guid> filterItemIds,
            IEnumerable<Guid> indicatorIds,
            IEnumerable<Guid> subjectIds);
    }
}