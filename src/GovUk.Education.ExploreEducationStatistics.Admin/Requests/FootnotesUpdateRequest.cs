#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record FootnotesUpdateRequest
{
    public List<Guid> FootnoteIds { get; set; } = new();
}
