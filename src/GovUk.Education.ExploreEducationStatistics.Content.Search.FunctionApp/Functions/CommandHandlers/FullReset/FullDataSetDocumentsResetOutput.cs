using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshPublicationDataSetDocuments.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.FullReset;

public record FullDataSetDocumentsResetOutput
{
    [QueueOutput("%RefreshPublicationDataSetDocumentsQueueName%")]
    public RefreshPublicationDataSetDocumentsMessageDto[] RefreshPublicationDataSetDocumentsMessages { get; init; } =
    [];

    [HttpResult]
    public required IActionResult HttpResult { get; init; }
}
