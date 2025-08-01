import { abbreviations } from '../../common/abbreviations.bicep'
import { eventTopics } from '../../common/eventTopics.bicep'
import { buildFullyQualifiedTopicName } from '../../common/functions.bicep'
import { SearchStorageQueueNames } from '../types.bicep'

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Queue names for the Search Docs Function App used as Event Grid subscription destinations.')
param storageQueueNames SearchStorageQueueNames

@description('The name of the Search Docs Function App storage account.')
param searchDocsFunctionAppStorageAccountName string

// Define each of the topics and their associated subscriptions
var eventGridCustomTopicSubscriptions = [
  {
    topicName: eventTopics.publicationChanged
    subscriptions: [
      {
        name: 'publication-archived'
        includedEventTypes: ['publication-archived']
        queueName: storageQueueNames.publicationArchived
      }
      {
        name: 'publication-changed'
        includedEventTypes: ['publication-changed']
        queueName: storageQueueNames.publicationChanged
      }
      {
        name: 'publication-deleted'
        includedEventTypes: ['publication-deleted']
        queueName: storageQueueNames.publicationDeleted
      }
      {
        name: 'publication-latest-published-release-reordered'
        includedEventTypes: ['publication-latest-published-release-reordered']
        queueName: storageQueueNames.publicationLatestPublishedReleaseReordered
      }
      {
        name: 'publication-restored'
        includedEventTypes: ['publication-restored']
        queueName: storageQueueNames.publicationRestored
      }
    ]
  }
  {
    topicName: eventTopics.releaseChanged
    subscriptions: [
      {
        name: 'release-slug-changed'
        includedEventTypes: ['release-slug-changed']
        queueName: storageQueueNames.releaseSlugChanged
      }
    ]
  }
  {
    topicName: eventTopics.releaseVersionChanged
    subscriptions: [
      {
        name: 'release-version-published'
        includedEventTypes: ['release-version-published']
        queueName: storageQueueNames.releaseVersionPublished
      }
    ]
  }
  {
    topicName: eventTopics.themeChanged
    subscriptions: [
      {
        name: 'theme-changed'
        includedEventTypes: ['theme-changed']
        queueName: storageQueueNames.themeUpdated
      }
    ]
  }
]

var subscriptions = flatten(map(
  eventGridCustomTopicSubscriptions,
  item => map(item.subscriptions, subscription => { topicName: item.topicName, ...subscription })
))

// The functions have Queue trigger bindings rather than Event Grid trigger bindings,
// so Event Grid subscriptions are created using storage queues as destinations.
module eventGridQueueSubscriptionModuleDeploy '../../common/components/event-grid/eventGridCustomTopicQueueSubscription.bicep' = [
  for (subscription, index) in subscriptions: {
    name: 'eventGridQueueSubscriptionModuleDeploy-${index}'
    params: {
      name: '${resourcePrefix}-${abbreviations.eventGridSubscriptions}-${subscription.name}'
      topicName: buildFullyQualifiedTopicName(resourcePrefix, subscription.topicName)
      includedEventTypes: subscription.includedEventTypes
      storageAccountName: searchDocsFunctionAppStorageAccountName
      queueName: subscription.queueName
    }
  }
]
