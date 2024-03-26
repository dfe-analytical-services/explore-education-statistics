#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface IFootnoteRepository
{
    Task<Footnote> CreateFootnote(
        Guid releaseVersionId,
        string content,
        IReadOnlySet<Guid> filterIds,
        IReadOnlySet<Guid> filterGroupIds,
        IReadOnlySet<Guid> filterItemIds,
        IReadOnlySet<Guid> indicatorIds,
        IReadOnlySet<Guid> subjectIds,
        int order);

    Task<List<Footnote>> GetFilteredFootnotes(
        Guid releaseVersionId,
        Guid subjectId,
        IEnumerable<Guid> filterItemIds,
        IEnumerable<Guid> indicatorIds
    );

    Task<Footnote> GetFootnote(Guid footnoteId);

    Task<List<Footnote>> GetFootnotes(Guid releaseVersionId, Guid? subjectId = null);

    Task<IList<Subject>> GetSubjectsWithNoFootnotes(Guid releaseVersionId);

    Task DeleteFootnote(Guid releaseVersionId, Guid footnoteId);

    Task DeleteFootnotesBySubject(Guid releaseVersionId, Guid subjectId);

    Task<bool> IsFootnoteExclusiveToRelease(Guid releaseVersionId, Guid footnoteId);

    Task DeleteReleaseFootnoteLink(Guid releaseVersionId, Guid footnoteId);
}
