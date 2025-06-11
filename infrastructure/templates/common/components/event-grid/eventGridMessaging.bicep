import { buildFullyQualifiedTopicName } from '../../functions.bicep'
import { IpRange } from '../../types.bicep'

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Location for all resources.')
param location string

@description('A list of IP network rules to allow access to the resource from specific public internet IP address ranges.')
param ipRules IpRange[]

@description('Specifies which custom topic alert rules to enable. If the optional alerts parameter is not provided, no alert rules will be created or updated.')
param customTopicAlerts {
  deadLetteredCount: bool
  deliveryAttemptFailCount: bool
  droppedEventCount: bool
  publishFailCount: bool
  unmatchedEventCount: bool
  alertsGroupName: string
}?

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

@description('A list of custom topic names to create.')
@minLength(1)
param customTopicNames string[]

module eventGridCustomTopicModule 'eventGridCustomTopic.bicep' = [
  for (topicName, index) in customTopicNames: {
    name: 'eventGridCustomTopicModuleDeploy-${index}'
    params: {
      name: buildFullyQualifiedTopicName(resourcePrefix, topicName)
      location: location
      ipRules: ipRules
      publicNetworkAccess: 'Enabled'
      systemAssignedIdentity: true
      alerts: customTopicAlerts
      tagValues: tagValues
    }
  }
]
