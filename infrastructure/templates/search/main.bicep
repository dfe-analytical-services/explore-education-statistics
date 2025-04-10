import { abbreviations } from '../common/abbreviations.bicep'
import { IpRange } from '../common/types.bicep'

@description('Environment : Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string = ''

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Tagging : Environment name e.g. Development. Used for tagging resources created by this infrastructure pipeline.')
param environmentName string

@description('Tagging : Used for tagging resources created by this infrastructure pipeline.')
param resourceTags {
  CostCentre: string
  Department: string
  Solution: string
  ServiceOwner: string
  CreatedBy: string
  DeploymentRepoUrl: string
}?

@description('Tagging : Date Provisioned. Used for tagging resources created by this infrastructure pipeline.')
param dateProvisioned string = utcNow('u')

@description('Whether to create/update Azure Monitor alerts during this deploy.')
param deployAlerts bool = false

@description('Whether to deploy the Search service configuration to create/update the data source, index and indexer.')
param deploySearchConfig bool = false

@description('The URL of the Content API.')
param contentApiUrl string

@description('Specifies whether or not the Search Docs Function App already exists.')
param searchDocsFunctionAppExists bool = true

@description('Provides access to resources for specific IP address ranges used for service maintenance.')
param maintenanceIpRanges IpRange[] = []

@description('The branch, tag or commit of the source code from which the deployment is triggered.')
param githubSourceRef string = 'refs/heads/dev'

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

// Custom Event Grid topics used by Admin and Publisher to publish events
var eventGridCustomTopicNames = [
  'publication-changed'
  'release-changed'
  'release-version-changed'
  'theme-changed'
]

var maintenanceFirewallRules = [
  for maintenanceIpRange in maintenanceIpRanges: {
    name: maintenanceIpRange.name
    cidr: maintenanceIpRange.cidr
    tag: 'Default'
    priority: 100
  }
]

var resourcePrefix = '${subscription}-ees'

var resourceNames = {
  existingResources: {
    keyVault: '${subscription}-kv-ees-01'
    vNet: '${subscription}-vnet-ees'
    alertsGroup: '${subscription}-ag-ees-alertedusers'
    subnets: {
      searchDocsFunction: '${resourcePrefix}-snet-${abbreviations.webSitesFunctions}-searchdocs'
      searchDocsFunctionPrivateEndpoints: '${resourcePrefix}-snet-${abbreviations.webSitesFunctions}-searchdocs-pep'
      searchStoragePrivateEndpoints: '${resourcePrefix}-snet-${abbreviations.storageStorageAccounts}-search-pep'
    }
  }
}

// Create a shared Application Insights resource for Search resources to use.
module applicationInsightsModule 'application/searchApplicationInsights.bicep' = {
  name: 'searchApplicationInsightsModule'
  params: {
    location: location
    resourcePrefix: resourcePrefix
    resourceNames: resourceNames
    tagValues: tagValues
  }
}

module eventGridMessagingModule '../common/components/event-grid/eventGridMessaging.bicep' = {
  name: 'eventGridMessagingModule'
  params: {
    location: location
    resourcePrefix: resourcePrefix
    customTopicNames: eventGridCustomTopicNames
    ipRules: maintenanceIpRanges
    tagValues: tagValues
  }
}

module searchDocsFunctionModule 'application/searchDocsFunction.bicep' = {
  name: 'searchDocsFunctionModule'
  params: {
    location: location
    resourceNames: resourceNames
    resourcePrefix: resourcePrefix
    contentApiUrl: contentApiUrl
    functionAppExists: searchDocsFunctionAppExists
    functionAppFirewallRules: union(
      [
        {
          cidr: 'AzureCloud'
          tag: 'ServiceTag'
          priority: 101
          name: 'AzureCloud'
        }
      ],
      maintenanceFirewallRules
    )
    searchServiceEndpoint: searchServiceModule.outputs.searchServiceEndpoint
    searchServiceIndexerName: searchServiceModule.outputs.searchServiceIndexerName
    searchServiceName: searchServiceModule.outputs.searchServiceName
    searchStorageAccountName: searchServiceModule.outputs.searchStorageAccountName
    searchStorageAccountConnectionStringSecretName: searchServiceModule.outputs.searchStorageAccountConnectionStringSecretName
    searchableDocumentsContainerName: searchServiceModule.outputs.searchableDocumentsContainerName
    storageFirewallRules: maintenanceIpRanges
    applicationInsightsConnectionString: applicationInsightsModule.outputs.applicationInsightsConnectionString
    tagValues: tagValues
    deployAlerts: deployAlerts
  }
}

module searchDocsFunctionEventSubscriptionsModule 'application/searchDocsFunctionEventSubscriptions.bicep' = {
  name: 'searchDocsFunctionEventSubscriptionsModule'
  params: {
    resourcePrefix: resourcePrefix
    searchDocsFunctionAppStorageAccountName: searchDocsFunctionModule.outputs.functionAppStorageAccountName
    storageQueueNames: searchDocsFunctionModule.outputs.storageQueueNames
  }
}

module searchServiceModule 'application/searchService.bicep' = {
  name: 'searchServiceModule'
  params: {
    location: location
    githubSourceRef: githubSourceRef
    indexName: 'index-1'
    resourceNames: resourceNames
    resourcePrefix: resourcePrefix
    searchServiceIpRules: maintenanceIpRanges
    storageIpRules: maintenanceIpRanges
    deployAlerts: deployAlerts
    deploySearchConfig: deploySearchConfig
    tagValues: tagValues
  }
}

output searchDocsFunctionAppUrl string = searchDocsFunctionModule.outputs.functionAppUrl
output searchServiceEndpoint string = searchServiceModule.outputs.searchServiceEndpoint
