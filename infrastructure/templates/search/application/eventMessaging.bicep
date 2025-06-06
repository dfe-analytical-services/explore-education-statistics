metadata description = '''
Sets up event messaging infrastructure using Azure Event Grid and ensures that both the Admin App Service and the
Publisher Function App have the necessary permissions to send events to the Event Grid topics.
'''

import { builtInRoleDefinitionIds } from '../../common/builtInRoles.bicep'
import { eventTopics } from '../../common/eventTopics.bicep'
import { buildFullyQualifiedTopicName } from '../../common/functions.bicep'
import { IpRange } from '../../common/types.bicep'
import { ResourceNames } from '../types.bicep'

@description('Location for all resources.')
param location string

@description('A list of IP network rules to allow access to Event Grid resources from specific public internet IP address ranges.')
param ipRules IpRange[] = []

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var topicNames = map(items(eventTopics), item => item.value)

resource adminAppService 'Microsoft.Web/sites@2024-04-01' existing = {
  name: resourceNames.existingResources.adminApp
}

resource publisherFunction 'Microsoft.Web/sites@2024-04-01' existing = {
  name: resourceNames.existingResources.publisherFunction
}

module eventGridMessagingModule '../../common/components/event-grid/eventGridMessaging.bicep' = {
  name: 'eventGridMessagingModuleDeploy'
  params: {
    location: location
    customTopicNames: topicNames
    ipRules: ipRules
    resourcePrefix: resourcePrefix
    customTopicAlerts: {
      deadLetteredCount: true
      deliveryAttemptFailCount: true
      droppedEventCount: true
      publishFailCount: true
      unmatchedEventCount: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    }
    tagValues: tagValues
  }
}

// Allow the Admin App Service to send events to Event Grid topics
module adminTopicRoleAssignmentModuleDeploy '../../common/components/event-grid/eventGridTopicRoleAssignment.bicep' = [
  for (topicName, index) in topicNames: {
    name: 'adminTopicRoleAssignmentModuleDeploy-${index}'
    params: {
      principalId: adminAppService.identity.principalId
      roleDefinitionId: builtInRoleDefinitionIds.EventGridDataSender
      topicName: buildFullyQualifiedTopicName(resourcePrefix, topicName)
    }
    dependsOn: [
      eventGridMessagingModule
    ]
  }
]

// Allow the Publisher Function App to send events to Event Grid topics
module publisherTopicRoleAssignmentModuleDeploy '../../common/components/event-grid/eventGridTopicRoleAssignment.bicep' = [
  for (topicName, index) in topicNames: {
    name: 'publisherTopicRoleAssignmentModuleDeploy-${index}'
    params: {
      principalId: publisherFunction.identity.principalId
      roleDefinitionId: builtInRoleDefinitionIds.EventGridDataSender
      topicName: buildFullyQualifiedTopicName(resourcePrefix, topicName)
    }
    dependsOn: [
      eventGridMessagingModule
    ]
  }
]
