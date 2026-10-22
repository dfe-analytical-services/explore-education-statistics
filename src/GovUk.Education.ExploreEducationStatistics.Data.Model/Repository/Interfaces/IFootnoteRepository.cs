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

    /// <summary>
    /// Gets the release version's footnotes that apply to a selection of the subject's filters, filter groups, filter
    /// items and indicators. A footnote applies if it is linked directly to the subject, or if it is linked to the
    /// subject's data and the selection includes one of its filter, filter group or filter item links for the subject
    /// (if it has any) and one of its indicator links for the subject (if it has any).
    /// </summary>
    Task<List<Footnote>> GetFootnotesForSelection(
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
