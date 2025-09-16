// Originally sourced from https://github.com/Azure/azure-quickstart-templates/blob/master/quickstarts/microsoft.search/azure-search-create/main.bicep.

import { IpRange } from '../../common/types.bicep'
import { dynamicAverageGreaterThan, dynamicTotalGreaterThan } from '../../public-api/components/alerts/dynamicAlertConfig.bicep'

@description('Service name must only contain lowercase letters, digits or dashes, cannot use dash as the first two or last one characters, cannot contain consecutive dashes, and is limited between 2 and 60 characters in length.')
@minLength(2)
@maxLength(60)
param name string

@description('A list IP network rules to allow access to the Search Service from specific public internet IP address ranges. These rules are applied only when \'publicNetworkAccess\' is \'Enabled\'.')
param ipRules IpRange[] = []

@allowed([
  'free'
  'basic'
  'standard'
  'standard2'
  'standard3'
  'storage_optimized_l1'
  'storage_optimized_l2'
])
@description('The pricing tier of the Search Service you want to create (for example, basic or standard).')
param sku string = 'standard'

@description('Replicas distribute search workloads across the service. You need at least two replicas to support high availability of query workloads (not applicable to the free tier).')
@minValue(1)
@maxValue(12)
param replicaCount int = 1

@description('Partitions allow for scaling of document count as well as faster indexing by sharding your index over multiple search units.')
@allowed([
  1
  2
  3
  4
  6
  12
])
param partitionCount int = 1

@description('Applicable only for SKUs set to standard3. You can set this property to enable a single, high density partition that allows up to 1000 indexes, which is much higher than the maximum indexes allowed for any other SKU.')
@allowed([
  'default'
  'highDensity'
])
param hostingMode string = 'default'

@description('Specifies whether traffic is allowed over the public interface.')
@allowed([
  'Disabled'
  'Enabled'
])
param publicNetworkAccess string = 'Disabled'

@description('Controls the availability of semantic ranking for all indexes. Set to \'free\' for limited query volume on the free plan, \'standard\' for unlimited volume on the standard pricing plan, or \'disabled\' to turn it off.')
@allowed([
  'disabled'
  'free'
  'standard'
])
param semanticRankerAvailability string = 'free'

@description('Indicates whether the resource should have a system-assigned managed identity.')
param systemAssignedIdentity bool = false

@description('The name of a user-assigned managed identity to assign to the resource.')
param userAssignedIdentityName string = ''

@description('Location for all resources.')
param location string

@description('Specifies which alert rules to enable. If the optional alerts parameter is not provided, no alert rules will be created or updated.')
param alerts {
  searchLatency: bool
  searchQueriesPerSecond: bool
  throttledSearchQueriesPercentage: bool
  alertsGroupName: string
}?

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

var identityType = systemAssignedIdentity
  ? (!empty(userAssignedIdentityName) ? 'SystemAssigned, UserAssigned' : 'SystemAssigned')
  : (!empty(userAssignedIdentityName) ? 'UserAssigned' : 'None')

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = if (!empty(userAssignedIdentityName)) {
  name: userAssignedIdentityName
}

resource searchService 'Microsoft.Search/searchServices@2025-02-01-preview' = {
  name: name
  location: location
  sku: {
    name: sku
  }
  identity: {
    type: identityType
    userAssignedIdentities: !empty(userAssignedIdentityName) ? { '${userAssignedIdentity.id}': {} } : null
  }
  properties: {
    authOptions: {
      aadOrApiKey: {
        aadAuthFailureMode: 'http403'
      }
    }
    replicaCount: replicaCount
    networkRuleSet: {
      bypass: length(ipRules) > 0 ? 'AzureServices' : 'None'
      ipRules: [
        for ipRule in ipRules: {
          value: ipRule.cidr
        }
      ]
    }
    partitionCount: partitionCount
    publicNetworkAccess: publicNetworkAccess
    semanticSearch: semanticRankerAvailability
    hostingMode: hostingMode
  }
  tags: tagValues
}

resource searchDiagnosticSetting 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${name}-diagnostic'
  properties: {
    logAnalyticsDestinationType: null
    logs: [
      {
        categoryGroup: 'allLogs'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
    workspaceId: logAnalyticsWorkspaceId
  }
}

module searchLatencyAlert '../../public-api/components/alerts/dynamicMetricAlert.bicep' = if (alerts != null) {
  name: '${name}SrchLatDeploy'
  params: {
    enabled: alerts!.searchLatency
    resourceName: searchService.name
    resourceMetric: {
      resourceType: 'Microsoft.Search/searchServices'
      metric: 'SearchLatency'
    }
    config: {
      ...dynamicAverageGreaterThan
      nameSuffix: 'latency'
      sensitivity: 'Medium'
      evaluationFrequency: 'PT1M'
      evaluationPeriods: 4
      minFailingEvaluationPeriods: 4
      windowSize: 'PT5M'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module searchQueriesPerSecondAlert '../../public-api/components/alerts/dynamicMetricAlert.bicep' = if (alerts != null) {
  name: '${name}SearchQPSDeploy'
  params: {
    enabled: alerts!.searchQueriesPerSecond
    resourceName: searchService.name
    resourceMetric: {
      resourceType: 'Microsoft.Search/searchServices'
      metric: 'SearchQueriesPerSecond'
    }
    config: {
      ...dynamicAverageGreaterThan
      nameSuffix: 'queries-per-second'
      sensitivity: 'Low'
      evaluationFrequency: 'PT1M'
      evaluationPeriods: 4
      minFailingEvaluationPeriods: 4
      windowSize: 'PT5M'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module throttledSearchQueriesPercentageAlert '../../public-api/components/alerts/dynamicMetricAlert.bicep' = if (alerts != null) {
  name: '${name}ThrSQPctDeploy'
  params: {
    enabled: alerts!.throttledSearchQueriesPercentage
    resourceName: searchService.name
    resourceMetric: {
      resourceType: 'Microsoft.Search/searchServices'
      metric: 'ThrottledSearchQueriesPercentage'
    }
    config: {
      ...dynamicTotalGreaterThan
      nameSuffix: 'throttled-queries-percentage'
      sensitivity: 'Medium'
      evaluationFrequency: 'PT1M'
      evaluationPeriods: 4
      minFailingEvaluationPeriods: 4
      windowSize: 'PT5M'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

output searchServiceEndpoint string = searchService.properties.endpoint
output searchServiceIdentityPrincipalId string = systemAssignedIdentity ? searchService.identity.principalId : ''
output searchServiceName string = searchService.name
