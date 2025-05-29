using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnThemeUpdated.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnThemeUpdated;

public class OnThemeUpdatedFunction(
    IEventGridEventHandler eventGridEventHandler,
    IContentApiClient contentApiClient,
    ILogger<OnThemeUpdatedFunction> logger)
{
    [Function(nameof(OnThemeUpdated))]
    [QueueOutput("%RefreshSearchableDocumentQueueName%")]
    public async Task<RefreshSearchableDocumentMessageDto[]> OnThemeUpdated(
        [QueueTrigger("%ThemeUpdatedQueueName%")]
        EventGridEvent eventDto,
        FunctionContext context) =>
        await eventGridEventHandler.Handle<ThemeUpdatedEventDto, RefreshSearchableDocumentMessageDto[]>(
            context, 
            eventDto,
            async (_, ct) =>
            {
                // Look up Publications for this Theme
                var themeId = Guid.Parse(eventDto.Subject);

                var publicationsForTheme = await contentApiClient.GetPublicationsForTheme(themeId, ct);
                
                logger.LogInformation(
                    "Refreshing Searchable Documents for Theme {ThemeId}: Publications {@PublicationInfos}",
                    themeId,
                    publicationsForTheme);
                
                return publicationsForTheme
                    .Select(p => new RefreshSearchableDocumentMessageDto { PublicationSlug = p.PublicationSlug })
                    .ToArray();
            });
}
