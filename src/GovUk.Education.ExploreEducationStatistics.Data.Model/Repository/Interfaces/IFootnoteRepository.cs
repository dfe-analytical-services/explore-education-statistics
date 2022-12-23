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
            IReadOnlySet<Guid> filterIds,
            IReadOnlySet<Guid> filterGroupIds,
            IReadOnlySet<Guid> filterItemIds,
            IReadOnlySet<Guid> indicatorIds,
            IReadOnlySet<Guid> subjectIds,
            int order);

        Task<List<Footnote>> GetFilteredFootnotes(
            Guid releaseId,
            Guid subjectId,
            IEnumerable<Guid> filterItemIds,
            IEnumerable<Guid> indicatorIds
        );

        Task<Footnote> GetFootnote(Guid footnoteId);

        Task<List<Footnote>> GetFootnotes(Guid releaseId, Guid? subjectId = null);

        Task<IList<Subject>> GetSubjectsWithNoFootnotes(Guid releaseId);

        Task DeleteFootnote(Guid releaseId, Guid footnoteId);

        Task DeleteFootnotesBySubject(Guid releaseId, Guid subjectId);

        Task<bool> IsFootnoteExclusiveToRelease(Guid releaseId, Guid footnoteId);

        Task DeleteReleaseFootnoteLink(Guid releaseId, Guid footnoteId);
    }
}
