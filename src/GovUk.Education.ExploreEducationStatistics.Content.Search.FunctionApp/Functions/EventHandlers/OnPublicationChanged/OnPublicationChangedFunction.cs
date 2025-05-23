﻿using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.RefreshSearchableDocument.Dto;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationChanged.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.EventHandlers.OnPublicationChanged;

public class OnPublicationChangedFunction(IEventGridEventHandler eventGridEventHandler)
{
    [Function(nameof(OnPublicationChanged))]
    [QueueOutput("%RefreshSearchableDocumentQueueName%")]
    public async Task<RefreshSearchableDocumentMessageDto[]> OnPublicationChanged(
        [QueueTrigger("%PublicationChangedQueueName%")]
        EventGridEvent eventDto,
        FunctionContext context) =>
        await eventGridEventHandler.Handle<PublicationChangedEventDto, RefreshSearchableDocumentMessageDto[]>(
            context, 
            eventDto,
            (payload, ct) => 
                Task.FromResult<RefreshSearchableDocumentMessageDto[]>(
                    string.IsNullOrEmpty(payload.Slug)
                    ? []
                    : [ new RefreshSearchableDocumentMessageDto { PublicationSlug = payload.Slug } ]));
}
