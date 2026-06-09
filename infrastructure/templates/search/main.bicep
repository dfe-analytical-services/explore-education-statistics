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

@description('The URL of the Content API.')
param contentApiUrl string

@description('Specifies whether or not the Natural Language Search Function App already exists.')
param naturalLanguageSearchFunctionAppExists bool = true

@description('Specifies whether or not the Search Docs Function App already exists.')
param searchDocsFunctionAppExists bool = true

@description('Name of the \'Natural language search filter\' index in Azure AI Search.')
param searchServiceNLSearchFilterIndexName string = ''

@description('Name of the \'Natural language search dataset\' index in Azure AI Search.')
param searchServiceNLSearchDatasetIndexName string = ''

@description('Name of the indexer associated with the \'Search\' index in Azure AI Search.')
param searchServiceSearchIndexerName string = ''

@description('Controls the availability of semantic ranking for all indexes. Set to \'free\' for limited query volume on the free plan, \'standard\' for unlimited volume on the standard pricing plan, or \'disabled\' to turn it off.')
@allowed([
  'disabled'
  'free'
  'standard'
])
param searchServiceSemanticRankerAvailability string

@description('Provides access to resources for specific IP address ranges used for service maintenance.')
param maintenanceIpRanges IpRange[] = []

@description('Indicates whether to deploy the Natural Language Search Function App as part of this deployment.')
param deployNaturalLanguageSearchFunctionApp bool = false

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
    publisherFunction: '${subscription}-${abbreviations.webSitesFunctions}-ees-publisher'
    keyVault: '${subscription}-${abbreviations.keyVaultVaults}-ees-01'
    vNet: '${subscription}-${abbreviations.networkVirtualNetworks}-ees'
    alertsGroup: '${subscription}-${abbreviations.insightsActionGroups}-ees-alertedusers'
    subnets: {
      eventGridCustomTopicPrivateEndpoints: '${resourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-${abbreviations.eventGridTopics}-pep'
      nlSearchFunctionApp: '${resourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-${abbreviations.webSitesFunctions}-nlsearch'
      nlSearchFunctionAppPrivateEndpoints: '${resourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-${abbreviations.webSitesFunctions}-nlsearch-pep'
      searchDocsFunctionApp: '${resourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-${abbreviations.webSitesFunctions}-searchdocs'
      searchDocsFunctionAppPrivateEndpoints: '${resourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-${abbreviations.webSitesFunctions}-searchdocs-pep'
      searchStoragePrivateEndpoints: '${resourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-${abbreviations.storageStorageAccounts}-search-pep'
    }
  }
}

module monitoringModule 'application/monitoring.bicep' = {
  name: 'monitoringModuleDeploy'
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
module eventMessagingModule '../common/application/eventMessaging.bicep' = {
  name: 'eventMessagingModuleDeploy'
  params: {
    location: location
    resourcePrefix: resourcePrefix
    resourceNames: {
      adminApp: resourceNames.existingResources.adminApp
      alertsGroup: resourceNames.existingResources.alertsGroup
      publisherFunction: resourceNames.existingResources.publisherFunction
      vNet: resourceNames.existingResources.vNet
      subnets: {
        eventGridCustomTopicPrivateEndpoints: resourceNames.existingResources.subnets.eventGridCustomTopicPrivateEndpoints
      }
    }
    tagValues: tagValues
  }
}

module nlSearchFunctionAppModule 'application/nlSearchFunctionApp.bicep' = if (deployNaturalLanguageSearchFunctionApp) {
  name: 'nlSearchFunctionAppApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    resourcePrefix: resourcePrefix
    functionAppExists: naturalLanguageSearchFunctionAppExists
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
    searchServiceName: searchServiceModule.outputs.searchServiceName
    searchStorageAccountName: searchServiceModule.outputs.searchStorageAccountName
    searchStorageAccountConnectionStringSecretName: searchServiceModule.outputs.searchStorageAccountConnectionStringSecretName
    locationsDictionaryContainerName: searchServiceModule.outputs.searchStorageDocumentContainers.nlSearchLocationsDictionary
    searchServiceNLSearchFilterIndexName: searchServiceNLSearchFilterIndexName
    searchServiceNLSearchDatasetIndexName: searchServiceNLSearchDatasetIndexName
    storageFirewallRules: maintenanceIpRanges
    applicationInsightsConnectionString: monitoringModule.outputs.applicationInsightsConnectionString
    tagValues: tagValues
  }
}

module searchDocsFunctionAppModule 'application/searchDocsFunctionApp.bicep' = {
  name: 'searchDocsFunctionAppApplicationModuleDeploy'
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
    searchServiceSearchIndexerName: searchServiceSearchIndexerName
    searchServiceName: searchServiceModule.outputs.searchServiceName
    searchStorageAccountName: searchServiceModule.outputs.searchStorageAccountName
    searchStorageAccountConnectionStringSecretName: searchServiceModule.outputs.searchStorageAccountConnectionStringSecretName
    searchDocumentsContainerName: searchServiceModule.outputs.searchStorageDocumentContainers.searchDocuments
    storageFirewallRules: maintenanceIpRanges
    applicationInsightsConnectionString: monitoringModule.outputs.applicationInsightsConnectionString
    tagValues: tagValues
  }
}

module searchDocsFunctionEventSubscriptionsModule 'application/searchDocsFunctionEventSubscriptions.bicep' = {
  name: 'searchDocsFunctionEventSubscriptionsModuleDeploy'
  params: {
    resourcePrefix: resourcePrefix
    searchDocsFunctionAppStorageAccountName: searchDocsFunctionAppModule.outputs.functionAppStorageAccountName
    storageQueueNames: searchDocsFunctionAppModule.outputs.storageQueueNames
  }
  dependsOn: [
    eventMessagingModule
  ]
}

module searchServiceModule 'application/searchService.bicep' = {
  name: 'searchServiceApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    resourcePrefix: resourcePrefix
    searchServiceIpRules: [] // No restrictions applied as the resource is intended to be publicly accessible.
    semanticRankerAvailability: searchServiceSemanticRankerAvailability
    storageIpRules: maintenanceIpRanges
    logAnalyticsWorkspaceId: monitoringModule.outputs.logAnalyticsWorkspaceId
    tagValues: tagValues
  }
}

output searchDocsFunctionAppUrl string = searchDocsFunctionAppModule.outputs.functionAppUrl
output searchServiceEndpoint string = searchServiceModule.outputs.searchServiceEndpoint
output searchStorageAccountManagedIdentityConnectionString string = searchServiceModule.outputs.searchStorageAccountManagedIdentityConnectionString
output searchDocumentsContainerName string = searchServiceModule.outputs.searchStorageDocumentContainers.searchDocuments
output nlSearchFunctionAppUrl string = deployNaturalLanguageSearchFunctionApp ? nlSearchFunctionAppModule.outputs.functionAppUrl : ''
output nlSearchDatasetDocumentsContainerName string = searchServiceModule.outputs.searchStorageDocumentContainers.nlSearchDatasetDocuments
output nlSearchFilterDocumentsContainerName string = searchServiceModule.outputs.searchStorageDocumentContainers.nlSearchFilterDocuments
