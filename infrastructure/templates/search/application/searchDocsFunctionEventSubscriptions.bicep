import { abbreviations } from '../../common/abbreviations.bicep'
import { ResourceNames, SearchStorageQueueNames } from '../types.bicep'

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Queue names for the Search Docs Function App used as Event Grid subscription destinations.')
param storageQueueNames SearchStorageQueueNames

@description('The name of the Search Docs Function App storage account.')
param searchDocsFunctionAppStorageAccountName string

// Define each of the topics and their associated subscriptions
var eventGridCustomTopicSubscriptions = [
  {
    topic: 'publication-changed'
    subscriptions: [
      {
        name: 'publication-changed'
        includedEventTypes: ['publication-changed']
        queueName: storageQueueNames.publicationChangedQueueName
      }
      {
        name: 'publication-latest-published-release-reordered'
        includedEventTypes: ['publication-latest-published-release-reordered']
        queueName: storageQueueNames.publicationLatestPublishedReleaseVersionChangedQueueName
      }
    ]
  }
  {
    topic: 'release-changed'
    subscriptions: [
      {
        name: 'release-slug-changed'
        includedEventTypes: ['release-slug-changed']
        queueName: storageQueueNames.releaseSlugChangedQueueName
      }
    ]
  }
  {
    topic: 'release-version-changed'
    subscriptions: [
      {
        name: 'release-version-published'
        includedEventTypes: ['release-version-published']
        queueName: storageQueueNames.releaseVersionPublishedQueueName
      }
    ]
  }
  {
    topic: 'theme-changed'
    subscriptions: [
      {
        name: 'theme-changed'
        includedEventTypes: ['theme-changed']
        queueName: storageQueueNames.themeUpdatedQueueName
      }
    ]
  }
]

var subscriptions = flatten(map(
  eventGridCustomTopicSubscriptions,
  item => map(item.subscriptions, subscription => { topic: item.topic, ...subscription })
))

// The functions have Queue trigger bindings rather than Event Grid trigger bindings,
// so Event Grid subscriptions are created using storage queues as destinations.
module eventGridQueueSubscriptionModuleDeploy '../../common/components/event-grid/eventGridCustomTopicQueueSubscription.bicep' = [
  for (subscription, index) in subscriptions: {
    name: '${index}eventGridQueueSubscriptionModuleDeploy'
    params: {
      name: '${resourcePrefix}-${abbreviations.eventGridSubscriptions}-${subscription.name}'
      topicName: '${resourcePrefix}-${abbreviations.eventGridTopics}-${subscription.topic}'
      includedEventTypes: subscription.includedEventTypes
      storageAccountName: searchDocsFunctionAppStorageAccountName
      queueName: subscription.queueName
    }
  }
]
