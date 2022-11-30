#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IFootnoteRepository
    {
        Task<Footnote> CreateFootnote(
            Guid releaseId,
            string content,
            IReadOnlyCollection<Guid> filterIds,
            IReadOnlyCollection<Guid> filterGroupIds,
            IReadOnlyCollection<Guid> filterItemIds,
            IReadOnlyCollection<Guid> indicatorIds,
            IReadOnlyCollection<Guid> subjectIds);

        Task<List<Footnote>> GetFilteredFootnotes(
            Guid releaseId,
            Guid subjectId,
            IEnumerable<Guid> filterItemIds,
            IEnumerable<Guid> indicatorIds
        );

        Task<Footnote> GetFootnote(Guid footnoteId);

        Task<List<Footnote>> GetFootnotes(Guid releaseId, Guid? subjectId = null);

        Task<IList<Subject>> GetSubjectsWithNoFootnotes(Guid releaseId);

        Task DeleteFootnote(Guid releaseId, Guid id);

        Task DeleteAllFootnotesBySubject(Guid releaseId, Guid subjectId);

        Task<bool> IsFootnoteExclusiveToReleaseAsync(Guid releaseId, Guid footnoteId);

        Task DeleteReleaseFootnoteLinkAsync(Guid releaseId, Guid footnoteId);
    }
}
