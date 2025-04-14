import { abbreviations } from '../../common/abbreviations.bicep'
import { eventTopics } from '../../common/eventTopics.bicep'
import { IpRange } from '../../common/types.bicep'

@description('Location for all resources.')
param location string

@description('A list of IP network rules to allow access to Event Grid resources from specific public internet IP address ranges.')
param ipRules IpRange[] = []

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var topicNames = map(items(eventTopics), item => item.value)

module eventGridMessagingModule '../../common/components/event-grid/eventGridMessaging.bicep' = {
  name: 'eventGridMessagingModuleDeploy'
  params: {
    location: location
    customTopicNames: topicNames
    ipRules: ipRules
    resourcePrefix: resourcePrefix
    tagValues: tagValues
  }
}
