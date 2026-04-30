#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record UpdatePublicationDraftersRequest
{
    public HashSet<Guid> UserIds { get; init; } = [];
}
