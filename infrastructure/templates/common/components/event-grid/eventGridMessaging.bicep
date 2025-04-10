import { abbreviations } from '../../abbreviations.bicep'
import { IpRange } from '../../types.bicep'

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Location for all resources.')
param location string

@description('A list of IP network rules to allow access to the resource from specific public internet IP address ranges.')
param ipRules IpRange[]

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

@description('A list of custom topic names to create.')
@minLength(1)
param customTopicNames string[]

module eventGridCustomTopicModule 'eventGridCustomTopic.bicep' = [
  for (topicName, index) in customTopicNames: {
    name: 'eventGridCustomTopicModuleDeploy-${index}'
    params: {
      name: '${resourcePrefix}-${abbreviations.eventGridTopics}-${topicName}'
      location: location
      ipRules: ipRules
      publicNetworkAccess: 'Enabled'
      systemAssignedIdentity: true
      tagValues: tagValues
    }
  }
]
