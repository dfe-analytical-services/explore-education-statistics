#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface IFootnoteRepository
{
    Task<Footnote> CreateFootnote(
        Guid releaseVersionId,
        string content,
        List<FilterFootnote> filterLinks,
        List<FilterGroupFootnote> filterGroupLinks,
        List<FilterItemFootnote> filterItemLinks,
        List<IndicatorFootnote> indicatorLinks,
        IReadOnlySet<Guid> subjectIds,
        int order
    );

    Task<List<Footnote>> GetFilteredFootnotes(
        Guid releaseVersionId,
        Guid subjectId,
        IReadOnlyCollection<Guid> filterIds,
        IReadOnlyCollection<Guid> filterGroupIds,
        IReadOnlyCollection<Guid> filterItemIds,
        IReadOnlyCollection<Guid> indicatorIds
    );

    Task<Footnote> GetFootnote(Guid footnoteId);

    Task<List<Footnote>> GetFootnotes(Guid releaseVersionId, Guid? subjectId = null);

    Task<IList<Subject>> GetSubjectsWithNoFootnotes(Guid releaseVersionId);

    Task DeleteFootnote(Guid releaseVersionId, Guid footnoteId);

    Task DeleteFootnotesBySubject(Guid releaseVersionId, Guid subjectId);
}
