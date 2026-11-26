using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshReleaseDocument.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullReset;

/// <summary>
/// The output of a full reset of the release documents. The function returns its HTTP response and queues its
/// messages from this one type, using <see cref="HttpResultAttribute"/> alongside <see cref="QueueOutputAttribute"/>.
/// </summary>
public record FullReleaseDocumentsResetOutput
{
    [QueueOutput("%RefreshReleaseDocumentQueueName%")]
    public RefreshReleaseDocumentMessageDto[] RefreshReleaseDocumentMessages { get; init; } = [];

    [HttpResult]
    public required IActionResult HttpResult { get; init; }
}
