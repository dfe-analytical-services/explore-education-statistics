import { abbreviations } from '../../common/abbreviations.bicep'
import { IpRange, FirewallRule } from '../../common/types.bicep'
import { ResourceNames } from '../types.bicep'

@description('The URL of the Content API.')
param contentApiUrl string

@description('The IP address ranges that can access the Search Docs Function App endpoints.')
param functionAppFirewallRules FirewallRule[]

@description('Specifies the base URL of the Search Service endpoint for interacting with the data plane REST API. For example: https://[search-service-name].search.windows.net')
param searchServiceEndpoint string

@description('Specifies the Search Service indexer name.')
param searchServiceIndexerName string

@description('Specifies the Search Service name.')
param searchServiceName string

@description('Name of the Search storage account.')
param searchStorageAccountName string

@description('The connection string to the Search storage account.')
@secure()
param searchStorageAccountConnectionStringSecretName string

@description('Name of the searchable documents container in the storage account.')
param searchableDocumentsContainerName string

@description('The IP address ranges that can access the Search Docs Function App storage accounts.')
param storageFirewallRules IpRange[]

@description('Whether to create/update Azure Monitor alerts during this deploy.')
param deployAlerts bool

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Location for all resources.')
param location string

@description('The Application Insights connection string that is associated with this resource.')
param applicationInsightsConnectionString string = ''

@description('Specifies whether or not the Search Docs Function App already exists.')
param functionAppExists bool

@description('The name of the queue that is used when a searchable document requires a refresh.')
param refreshSearchableDocumentQueueName string = 'refresh-searchable-document-queue'

@description('The name of the queue that is used when a release slug is changed.')
param releaseSlugChangedQueueName string = 'release-slug-changed-queue'

@description('The name of the queue that is used when a release version is published.')
param releaseVersionPublishedQueueName string = 'release-version-published-queue'

@description('The name of the queue that is used when a searchable document is created.')
param searchableDocumentCreatedQueueName string = 'search-document-created-queue'

@description('The name of the queue that is used when a theme is updated.')
param themeUpdatedQueueName string = 'theme-updated-queue'

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.existingResources.keyVault
}

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource outboundVnetSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.searchDocsFunction
  parent: vNet
}

resource searchDocsFunctionPrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.searchDocsFunctionPrivateEndpoints
  parent: vNet
}

resource searchStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: searchStorageAccountName
}

resource searchBlobStorage 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' existing = {
  name: 'default'
  parent: searchStorageAccount
}

resource searchableDocumentsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' existing = {
  parent: searchBlobStorage
  name: searchableDocumentsContainerName
}

resource searchService 'Microsoft.Search/searchServices@2024-06-01-preview' existing = {
    name: searchServiceName
}

module functionAppModule '../../common/components/functionApp.bicep' = {
  name: 'searchDocsFunctionAppModuleDeploy'
  params: {
    functionAppName: '${resourcePrefix}-${abbreviations.webSitesFunctions}-searchdocs'
    location: location
    applicationInsightsConnectionString: applicationInsightsConnectionString
    appServicePlanName: '${resourcePrefix}-${abbreviations.webServerFarms}-searchdocs'
    appSettings: [
      {
        name: 'App__SearchStorageConnectionString'
        value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${searchStorageAccountConnectionStringSecretName})'
      }
      {
        name: 'App__SearchableDocumentsContainerName'
        value: searchableDocumentsContainer.name
      }
      {
        name: 'AzureSearch__SearchServiceEndpoint'
        // Should be able to replace this with searchService.properties.endpoint in future using API version 2025-02-01-preview or later
        value: searchServiceEndpoint
      }
      {
        name: 'AzureSearch__IndexerName'
        value: searchServiceIndexerName
      }
      {
        name: 'ContentApi__Url'
        value: contentApiUrl
      }
      {
        name: 'RefreshSearchableDocumentQueueName'
        value: refreshSearchableDocumentQueueName
      }
      {
        name: 'ReleaseSlugChangedQueueName'
        value: releaseSlugChangedQueueName
      }
      {
        name: 'ReleaseVersionPublishedQueueName'
        value: releaseVersionPublishedQueueName
      }
      {
        name: 'SearchableDocumentCreatedQueueName'
        value: searchableDocumentCreatedQueueName
      }
      {
        name: 'ThemeUpdatedQueueName'
        value: themeUpdatedQueueName
      }
    ]
    functionAppExists: functionAppExists
    keyVaultName: keyVault.name
    sku: {
      name: 'EP1'
      tier: 'ElasticPremium'
      family: 'EP'
    }
    healthCheckPath: '/api/HealthCheck'
    operatingSystem: 'Linux'
    functionAppRuntime: 'dotnet-isolated'
    functionAppRuntimeVersion: '8.0'
    deployQueueRoleAssignment: true
    storageAccountName: '${replace(resourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}searchdocsfn'
    storageAccountPublicNetworkAccessEnabled: false
    publicNetworkAccessEnabled: true
    functionAppFirewallRules: functionAppFirewallRules
    storageFirewallRules: storageFirewallRules
    privateEndpoints: {
      functionApp: searchDocsFunctionPrivateEndpointSubnet.id
      storageAccounts: searchDocsFunctionPrivateEndpointSubnet.id
    }
    outboundSubnetId: outboundVnetSubnet.id
    alerts: deployAlerts ? {
      cpuPercentage: true
      functionAppHealth: true
      httpErrors: true
      memoryPercentage: true
      storageAccountAvailability: true
      storageLatency: false
      fileServiceAvailability: true
      fileServiceLatency: false
      fileServiceCapacity: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    } : null
    tagValues: tagValues
  }
}

module functionAppIdentityRoleAssignmentModule '../components/searchServiceRoleAssignment.bicep' = {
  name: 'searchDocsFunctionAppRoleAssignmentModuleDeploy'
  params: {
    searchServiceName: searchService.name
    principalIds: [functionAppModule.outputs.functionAppIdentityPrincipalId]
    role: 'Search Service Contributor'
  }
}

output functionAppName string = functionAppModule.outputs.name
output functionAppUrl string = functionAppModule.outputs.url
