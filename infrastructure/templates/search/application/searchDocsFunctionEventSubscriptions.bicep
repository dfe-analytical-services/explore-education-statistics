metadata description = '''
Creates Event Grid subscriptions for the Search Docs Function App, for a defined set of topics provisioned by the Core infrastructure.
 Each subscription delivers events to a storage queue within the Search Docs Function App storage account.
'''

import { ResourceNames } from '../../common/resource-names.bicep'
import { SearchStorageQueueNames } from '../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Queue names for the Search Docs Function App used as Event Grid subscription destinations.')
param storageQueueNames SearchStorageQueueNames

@description('The name of the Search Docs Function App storage account.')
param searchDocsFunctionAppStorageAccountName string

var topicNames = resourceNames.eventGrid.topics
var subscriptionNames = resourceNames.searchDocs.eventSubscriptions

// Define each of the topics and their associated subscriptions
var eventGridCustomTopicSubscriptions = [
  {
    topicName: topicNames.publicationChanged
    subscriptions: [
      {
        name: subscriptionNames.publicationArchived
        includedEventTypes: ['publication-archived']
        queueName: storageQueueNames.publicationArchived
      }
      {
        name: subscriptionNames.publicationChanged
        includedEventTypes: ['publication-changed']
        queueName: storageQueueNames.publicationChanged
      }
      {
        name: subscriptionNames.publicationDeleted
        includedEventTypes: ['publication-deleted']
        queueName: storageQueueNames.publicationDeleted
      }
      {
        name: subscriptionNames.publicationLatestPublishedReleaseReordered
        includedEventTypes: ['publication-latest-published-release-reordered']
        queueName: storageQueueNames.publicationLatestPublishedReleaseReordered
      }
      {
        name: subscriptionNames.publicationRestored
        includedEventTypes: ['publication-restored']
        queueName: storageQueueNames.publicationRestored
      }
    ]
  }
  {
    topicName: topicNames.releaseChanged
    subscriptions: [
      {
        name: subscriptionNames.releaseSlugChanged
        includedEventTypes: ['release-slug-changed']
        queueName: storageQueueNames.releaseSlugChanged
      }
    ]
  }
  {
    topicName: topicNames.releaseVersionChanged
    subscriptions: [
      {
        name: subscriptionNames.releaseVersionPublished
        includedEventTypes: ['release-version-published']
        queueName: storageQueueNames.releaseVersionPublished
      }
    ]
  }
  {
    topicName: topicNames.themeChanged
    subscriptions: [
      {
        name: subscriptionNames.themeChanged
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
      name: subscription.name
      topicName: subscription.topicName
      includedEventTypes: subscription.includedEventTypes
      storageAccountName: searchDocsFunctionAppStorageAccountName
      queueName: subscription.queueName
    }
  }
]
