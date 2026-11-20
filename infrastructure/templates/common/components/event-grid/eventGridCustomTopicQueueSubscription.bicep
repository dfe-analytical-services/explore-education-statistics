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

@description('Whether to deliver events with the topic\'s system-assigned identity, granting it permission to send messages to the queue\'s storage account. Needed when the storage account doesn\'t allow shared key access.')
param deliverWithTopicIdentity bool = false

resource topic 'Microsoft.EventGrid/topics@2025-02-15' existing = {
  name: topicName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' existing = {
  name: storageAccountName
}

var queueDestination = {
  properties: {
    resourceId: storageAccount.id
    queueName: queueName
    queueMessageTimeToLiveInSeconds: 604800
  }
  endpointType: 'StorageQueue'
}

// Allow the topic to send messages to the queue's storage account with its identity.
module topicQueueRoleAssignmentModule '../storageAccountRoleAssignment.bicep' = if (deliverWithTopicIdentity) {
  name: '${uniqueString(name)}QueueRoleAssignmentDeploy'
  params: {
    storageAccountName: storageAccount.name
    principalIds: [topic.identity!.principalId]
    role: 'Storage Queue Data Message Sender'
  }
}

resource eventSubscription 'Microsoft.EventGrid/topics/eventSubscriptions@2025-02-15' = {
  parent: topic
  name: name
  properties: union(deliverWithTopicIdentity ? {
    deliveryWithResourceIdentity: {
      identity: {
        type: 'SystemAssigned'
      }
      destination: queueDestination
    }
  } : {
    destination: queueDestination
  }, {
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
  })
  dependsOn: [
    topicQueueRoleAssignmentModule
  ]
}
