#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface IFootnoteRepository
{
    Task<Footnote> CreateFootnote(
        Guid releaseVersionId,
        string content,
        IReadOnlyDictionary<Guid, Guid> filterIdToSubjectIdMap,
        IReadOnlyDictionary<Guid, Guid> filterGroupIdToSubjectIdMap,
        IReadOnlyDictionary<Guid, Guid> filterItemIdToSubjectIdMap,
        IReadOnlyDictionary<Guid, Guid> indicatorIdToSubjectIdMap,
        IReadOnlySet<Guid> subjectIds,
        int order
    );

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
}
