metadata description = '''
Sets up event messaging infrastructure using Azure Event Grid and ensures that both the Admin App Service and the
Publisher Function App have the necessary permissions to send events to the Event Grid topics.
'''

import { builtInRoleDefinitionIds } from '../builtInRoles.bicep'
import { eventTopics } from '../eventTopics.bicep'
import { buildFullyQualifiedTopicName } from '../functions.bicep'
import { IpRange } from '../types.bicep'

@description('Location for all resources.')
param location string

@description('A list of IP network rules to allow access to Event Grid resources from specific public internet IP address ranges.')
param ipRules IpRange[] = []

@description('Specifies whether Event Grid resources can be accessed from public networks, including the internet or are restricted to private endpoints only.')
param publicNetworkAccessEnabled bool = false

@description('Specifies resource naming variables.')
@sealed()
param resourceNames {
  adminApp: string
  alertsGroup: string
  publisherFunction: string
  vNet: string
  subnets: {
    eventGridCustomTopicPrivateEndpoints: string
  }
}

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var topicNames = map(items(eventTopics), item => item.value)

resource adminAppService 'Microsoft.Web/sites@2024-04-01' existing = {
  name: resourceNames.adminApp
}

resource publisherFunction 'Microsoft.Web/sites@2024-04-01' existing = {
  name: resourceNames.publisherFunction
}

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.vNet
}

resource eventGridCustomTopicPrivateEndpointsSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-07-01' existing = {
  name: resourceNames.subnets.eventGridCustomTopicPrivateEndpoints
  parent: vNet
}

module eventGridMessagingModule '../components/event-grid/eventGridMessaging.bicep' = {
  name: 'eventGridMessagingModuleDeploy'
  params: {
    location: location
    ipRules: ipRules
    publicNetworkAccessEnabled: publicNetworkAccessEnabled
    resourcePrefix: resourcePrefix
    customTopics: {
      names: topicNames
      privateEndpointSubnetId: eventGridCustomTopicPrivateEndpointsSubnet.id
      alerts: {
        deadLetteredCount: true
        deliveryAttemptFailCount: true
        droppedEventCount: true
        publishFailCount: true
        unmatchedEventCount: true
        alertsGroupName: resourceNames.alertsGroup
      }
    }
    tagValues: tagValues
  }
}

// Allow the Admin App Service to send events to Event Grid topics
module adminTopicRoleAssignmentModuleDeploy '../components/event-grid/eventGridTopicRoleAssignment.bicep' = [
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
module publisherTopicRoleAssignmentModuleDeploy '../components/event-grid/eventGridTopicRoleAssignment.bicep' = [
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
