using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshPublicationDataSetDocuments.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullDataSetDocumentsReset;

/// <summary>
/// The output of a full reset of the data set documents. The function returns its HTTP response and queues its
/// messages from this one type, using <see cref="HttpResultAttribute"/> alongside <see cref="QueueOutputAttribute"/>.
/// </summary>
public record FullDataSetDocumentsResetOutput
{
    [QueueOutput("%RefreshPublicationDataSetDocumentsQueueName%")]
    public RefreshPublicationDataSetDocumentsMessageDto[] RefreshPublicationDataSetDocumentsMessages { get; init; } =
    [];

    [HttpResult]
    public required IActionResult HttpResult { get; init; }
}
