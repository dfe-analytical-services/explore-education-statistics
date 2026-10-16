metadata description = '''
Sets up event messaging infrastructure using Azure Event Grid and ensures that both the Admin App Service and the
Publisher Function App have the necessary permissions to send events to the Event Grid topics.
'''

import { builtInRoleDefinitionIds } from '../../../common/builtInRoles.bicep'
import { ResourceNames } from '../../../common/resource-names.bicep'
import { IpRange } from '../../../common/types.bicep'

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

@description('Names of the Event Grid resources.')
param names ResourceNames.eventGrid

@description('Whether to create or update Azure Monitor alerts during this deploy.')
param deployAlerts bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var topicNamesList = map(items(names.topics), item => item.value)

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

module eventGridMessagingModule '../../../common/components/event-grid/eventGridMessaging.bicep' = {
  name: 'eventGridMessagingModuleDeploy'
  params: {
    location: location
    ipRules: ipRules
    publicNetworkAccessEnabled: publicNetworkAccessEnabled
    customTopics: {
      names: topicNamesList
      privateEndpointSubnetId: eventGridCustomTopicPrivateEndpointsSubnet.id
      alerts: deployAlerts ? {
        deadLetteredCount: true
        deliveryAttemptFailCount: true
        droppedEventCount: true
        publishFailCount: true
        unmatchedEventCount: true
        alertsGroupName: resourceNames.alertsGroup
      } : null
    }
    tagValues: tagValues
  }
}

// Allow the Admin App Service to send events to Event Grid topics
module adminTopicRoleAssignmentModuleDeploy '../../../common/components/event-grid/eventGridTopicRoleAssignment.bicep' = [
  for (topicName, index) in topicNamesList: {
    name: 'adminTopicRoleAssignmentModuleDeploy-${index}'
    params: {
      principalId: adminAppService.identity.principalId
      roleDefinitionId: builtInRoleDefinitionIds.EventGridDataSender
      topicName: topicName
    }
    dependsOn: [
      eventGridMessagingModule
    ]
  }
]

// Allow the Publisher Function App to send events to Event Grid topics
module publisherTopicRoleAssignmentModuleDeploy '../../../common/components/event-grid/eventGridTopicRoleAssignment.bicep' = [
  for (topicName, index) in topicNamesList: {
    name: 'publisherTopicRoleAssignmentModuleDeploy-${index}'
    params: {
      principalId: publisherFunction.identity.principalId
      roleDefinitionId: builtInRoleDefinitionIds.EventGridDataSender
      topicName: topicName
    }
    dependsOn: [
      eventGridMessagingModule
    ]
  }
]
