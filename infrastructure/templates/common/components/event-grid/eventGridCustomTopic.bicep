import { IpRange } from '../../types.bicep'
import { staticTotalGreaterThanZero } from '../../../public-api/components/alerts/staticAlertConfig.bicep'

@description('The resource name.')
@minLength(3)
@maxLength(50)
param name string

@description('Location for all resources.')
param location string

@description('A list IP network rules to allow access to the Search Service from specific public internet IP address ranges. These rules are applied only when \'publicNetworkAccess\' is \'Enabled\'.')
param ipRules IpRange[] = []

@description('Specifies whether to enable local authentication. Microsoft Entra access authentication is always enabled.')
param localAuthenticationEnabled bool = false

@description('Specifies whether traffic is allowed over the public interface.')
@allowed([
  'Disabled'
  'Enabled'
])
param publicNetworkAccess string = 'Disabled'

@description('Indicates whether the resource should have a system-assigned managed identity.')
param systemAssignedIdentity bool = false

@description('The name of a user-assigned managed identity to assign to the resource.')
param userAssignedIdentityName string = ''

@description('Specifies which alert rules to enable. If the optional alerts parameter is not provided, no alert rules will be created or updated.')
param alerts {
  deadLetteredCount: bool
  deliveryAttemptFailCount: bool
  droppedEventCount: bool
  publishFailCount: bool
  unmatchedEventCount: bool
  alertsGroupName: string
}?

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

var identityType = systemAssignedIdentity
  ? (!empty(userAssignedIdentityName) ? 'SystemAssigned, UserAssigned' : 'SystemAssigned')
  : (!empty(userAssignedIdentityName) ? 'UserAssigned' : 'None')

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = if (!empty(userAssignedIdentityName)) {
  name: userAssignedIdentityName
}

resource topic 'Microsoft.EventGrid/topics@2025-02-15' = {
  name: name
  location: location
  identity: {
    type: identityType
    userAssignedIdentities: !empty(userAssignedIdentityName) ? { '${userAssignedIdentity.id}': {} } : null
  }
  properties: {
    minimumTlsVersionAllowed: '1.2'
    inputSchema: 'EventGridSchema'
    publicNetworkAccess: publicNetworkAccess
    inboundIpRules: [
      for ipRule in ipRules: {
        action: 'Allow'
        ipMask: ipRule.cidr
      }
    ]
    disableLocalAuth: !localAuthenticationEnabled
    dataResidencyBoundary: 'WithinRegion'
  }
  tags: tagValues
}

module deadLetteredCountAlert '../../../public-api/components/alerts/staticMetricAlert.bicep' = if (alerts != null) {
  name: '${name}DeadLttrDeploy'
  params: {
    enabled: alerts!.deadLetteredCount
    resourceName: topic.name
    resourceMetric: {
      resourceType: 'Microsoft.EventGrid/topics'
      metric: 'DeadLetteredCount'
      dimensions: []
    }
    config: {
      ...staticTotalGreaterThanZero
      nameSuffix: 'dead-lettered-count'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module droppedEventCount '../../../public-api/components/alerts/staticMetricAlert.bicep' = if (alerts != null) {
  name: '${name}DropEvntDeploy'
  params: {
    enabled: alerts!.droppedEventCount
    resourceName: topic.name
    resourceMetric: {
      resourceType: 'Microsoft.EventGrid/topics'
      metric: 'DroppedEventCount'
      dimensions: []
    }
    config: {
      ...staticTotalGreaterThanZero
      nameSuffix: 'dropped-event-count'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module deliveryAttemptFailCountAlert '../../../public-api/components/alerts/staticMetricAlert.bicep' = if (alerts != null) {
  name: '${name}DlvAttFlDeploy'
  params: {
    enabled: alerts!.deliveryAttemptFailCount
    resourceName: topic.name
    resourceMetric: {
      resourceType: 'Microsoft.EventGrid/topics'
      metric: 'DeliveryAttemptFailCount'
      dimensions: []
    }
    config: {
      ...staticTotalGreaterThanZero
      nameSuffix: 'delivery-attempt-fail-count'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module publishFailCountAlert '../../../public-api/components/alerts/staticMetricAlert.bicep' = if (alerts != null) {
  name: '${name}PubFailDeploy'
  params: {
    enabled: alerts!.publishFailCount
    resourceName: topic.name
    resourceMetric: {
      resourceType: 'Microsoft.EventGrid/topics'
      metric: 'PublishFailCount'
      dimensions: []
    }
    config: {
      ...staticTotalGreaterThanZero
      nameSuffix: 'publish-fail-count'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module unmatchedEventCountAlert '../../../public-api/components/alerts/staticMetricAlert.bicep' = if (alerts != null) {
  name: '${name}UnmtEvtDeploy'
  params: {
    enabled: alerts!.unmatchedEventCount
    resourceName: topic.name
    resourceMetric: {
      resourceType: 'Microsoft.EventGrid/topics'
      metric: 'UnmatchedEventCount'
      dimensions: []
    }
    config: {
      ...staticTotalGreaterThanZero
      nameSuffix: 'unmatched-event-count'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

output name string = topic.name
