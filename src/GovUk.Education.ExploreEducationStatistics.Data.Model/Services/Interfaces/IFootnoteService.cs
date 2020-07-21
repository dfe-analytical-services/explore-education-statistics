using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFootnoteService
    {
        IEnumerable<Footnote> GetFilteredFootnotes(
            Guid releaseId,
            Guid subjectId,
            IQueryable<Observation> observations,
            IEnumerable<Guid> indicators
        );
        
        IEnumerable<Footnote> GetFootnotes(Guid releaseId);

        IEnumerable<Footnote> GetFootnotes(Guid releaseId, List<Guid> subjects);

        Task DeleteFootnote(Guid releaseId, Guid id);

        Task DeleteAllFootnotesBySubject(Guid releaseId, Guid subjectId);
        
        Task<bool> IsFootnoteExclusiveToReleaseAsync(Guid releaseId, Guid footnoteId);

        Task DeleteReleaseFootnoteLinkAsync(Guid releaseId, Guid footnoteId);

    }
}