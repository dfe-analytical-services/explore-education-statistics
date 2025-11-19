#nullable enable
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Releases.Dtos;

public record ReleaseUpdatesUpdateRequest
{
    [FromRoute]
    public required Guid ReleaseVersionId { get; init; }

    [FromRoute]
    public required Guid ReleaseUpdateId { get; init; }

    public DateTime? On { get; init; }

    public required string Reason { get; init; }
};
