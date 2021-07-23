using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IFootnoteRepository
    {
        IEnumerable<Footnote> GetFilteredFootnotes(
            Guid releaseId,
            Guid subjectId,
            IQueryable<Observation> observations,
            IEnumerable<Guid> indicatorIds
        );

        Task<Footnote> GetFootnote(Guid id);

        IEnumerable<Footnote> GetFootnotes(Guid releaseId);

        IEnumerable<Footnote> GetFootnotes(Guid releaseId, Guid subjectId);

        IEnumerable<Footnote> GetFootnotes(Guid releaseId, IEnumerable<Guid> subjectIds);

        Task<IList<Subject>> GetSubjectsWithNoFootnotes(Guid releaseId);

        Task DeleteFootnote(Guid releaseId, Guid id);

        Task DeleteAllFootnotesBySubject(Guid releaseId, Guid subjectId);

        Task<bool> IsFootnoteExclusiveToReleaseAsync(Guid releaseId, Guid footnoteId);

        Task DeleteReleaseFootnoteLinkAsync(Guid releaseId, Guid footnoteId);
    }
}