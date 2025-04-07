import { abbreviations } from '../../common/abbreviations.bicep'
import { IpRange } from '../../common/types.bicep'

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Location for all resources.')
param location string

@description('A list of IP network rules to allow access to the resource from specific public internet IP address ranges.')
param ipRules IpRange[]

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var customTopicNames = [
  'publication-changed'
  'release-changed'
  'release-version-changed'
  'theme-changed'
]

module eventGridCustomTopicsModule '../../common/components/eventGridCustomTopic.bicep' = [for topicName in customTopicNames: {
  name: '${topicName}EventGridCustomTopicModuleDeploy'
  params: {
    name: '${resourcePrefix}-${abbreviations.eventGridTopics}-${topicName}'
    location: location
    ipRules: ipRules
    publicNetworkAccess: 'Enabled'
    systemAssignedIdentity: true
    tagValues: tagValues
  }
}]
