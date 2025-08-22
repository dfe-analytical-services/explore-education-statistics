#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public class ReplacementRequest
{
    public List<Guid> OriginalFileIds { get; set; } = [];
}
