import { buildFullyQualifiedTopicName } from '../../functions.bicep'
import { IpRange } from '../../types.bicep'

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Location for all resources.')
param location string

@description('A list of IP network rules to allow access to resources from specific public internet IP address ranges.')
param ipRules IpRange[] = []

@description('Specifies whether resources can be accessed from public networks, including the internet or are restricted to private endpoints only.')
param publicNetworkAccessEnabled bool = false

@description('Specifies a set of custom topics and configuration associated with them.')
param customTopics {
  @description('A list of custom topic names to create.')
  @minLength(1)
  names: string[]
  @description('The resource id of the subnet to be used for creating private endpoints for custom topics.')
  privateEndpointSubnetId: string?
  @description('Specifies which custom topic alert rules to enable. If the optional alerts parameter is not provided, no alert rules will be created or updated.')
  alerts: {
    deadLetteredCount: bool
    deliveryAttemptFailCount: bool
    droppedEventCount: bool
    publishFailCount: bool
    unmatchedEventCount: bool
    alertsGroupName: string
  }?
}?

@description('Specifies a set of tags with which to tag resources in Azure.')
param tagValues object

var customTopicNames string[] = customTopics.?names ?? []

module eventGridCustomTopicModule 'eventGridCustomTopic.bicep' = [
  for (topicName, index) in customTopicNames: {
    name: 'eventGridCustomTopicModuleDeploy-${index}'
    params: {
      name: buildFullyQualifiedTopicName(resourcePrefix, topicName)
      location: location
      ipRules: ipRules
      privateEndpointSubnetId: customTopics.?privateEndpointSubnetId
      publicNetworkAccessEnabled: publicNetworkAccessEnabled
      systemAssignedIdentity: true
      alerts: customTopics.?alerts
      tagValues: tagValues
    }
  }
]
