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
    adminApp: '${subscription}-as-ees-admin'
    logAnalyticsWorkspace: '${resourcePrefix}-${abbreviations.operationalInsightsWorkspaces}'
    publisherFunction: '${subscription}-fa-ees-publisher'
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

module monitoringModule 'application/monitoring.bicep' = {
  name: 'monitoringModule'
  params: {
    location: location
    resourcePrefix: resourcePrefix
    resourceNames: resourceNames
    tagValues: tagValues
  }
}

// Provision the event messaging infrastructure utilised by EES for communication between services.
// While not exclusively part of the Search service infrastructure, it is included here to support
// other services that publish events but are not yet defined in Bicep such as the Admin App Service
// and the Publisher Function App.
// The Search Service relies on this infrastructure to subscribe to events.
module eventMessagingModule 'application/eventMessaging.bicep' = {
  name: 'eventMessagingModule'
  params: {
    location: location
    ipRules: [] // TODO EES-6036 Should be maintenanceIpRanges
    resourcePrefix: resourcePrefix
    resourceNames: resourceNames
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
    logAnalyticsWorkspaceId: monitoringModule.outputs.logAnalyticsWorkspaceId
    searchServiceEndpoint: searchServiceModule.outputs.searchServiceEndpoint
    searchServiceIndexerName: searchServiceModule.outputs.searchServiceIndexerName
    searchServiceName: searchServiceModule.outputs.searchServiceName
    searchStorageAccountName: searchServiceModule.outputs.searchStorageAccountName
    searchStorageAccountConnectionStringSecretName: searchServiceModule.outputs.searchStorageAccountConnectionStringSecretName
    searchableDocumentsContainerName: searchServiceModule.outputs.searchableDocumentsContainerName
    storageFirewallRules: maintenanceIpRanges
    applicationInsightsConnectionString: monitoringModule.outputs.applicationInsightsConnectionString
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
  dependsOn: [
    eventMessagingModule
  ]
}

module searchServiceModule 'application/searchService.bicep' = {
  name: 'searchServiceModule'
  params: {
    location: location
    githubSourceRef: githubSourceRef
    indexName: 'index-1'
    resourceNames: resourceNames
    resourcePrefix: resourcePrefix
    searchServiceIpRules: []
    storageIpRules: maintenanceIpRanges
    deployAlerts: deployAlerts
    deploySearchConfig: deploySearchConfig
    tagValues: tagValues
  }
}

output searchDocsFunctionAppUrl string = searchDocsFunctionModule.outputs.functionAppUrl
output searchServiceEndpoint string = searchServiceModule.outputs.searchServiceEndpoint
