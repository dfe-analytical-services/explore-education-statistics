@description('The resource name.')
@minLength(3)
@maxLength(64)
param name string

@description('The Event Grid topic name associated with the subscription.')
param topicName string

@description('The name of the storage account that contains the queue that is the destination of the subscription.')
param storageAccountName string

@description('The name of the storage queue under the storage account that is the destination of the subscription.')
param queueName string

@description('A list of event types to include in the subscription. If not specified, all event types are included.')
param includedEventTypes array = []

resource topic 'Microsoft.EventGrid/topics@2025-02-15' existing = {
  name: topicName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' existing = {
  name: storageAccountName
}

resource eventSubscription 'Microsoft.EventGrid/topics/eventSubscriptions@2025-02-15' = {
  parent: topic
  name: name
  properties: {
    destination: {
      properties: {
        resourceId: storageAccount.id
        queueName: queueName
        queueMessageTimeToLiveInSeconds: 604800
      }
      endpointType: 'StorageQueue'
    }
    filter: {
      enableAdvancedFilteringOnArrays: true
      includedEventTypes: length(includedEventTypes) > 0 ? includedEventTypes : null
    }
    labels: []
    eventDeliverySchema: 'EventGridSchema'
    retryPolicy: {
      maxDeliveryAttempts: 30
      eventTimeToLiveInMinutes: 1440
    }
  }
}
