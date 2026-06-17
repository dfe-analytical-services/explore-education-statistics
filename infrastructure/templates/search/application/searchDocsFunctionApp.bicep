import { abbreviations } from '../../common/abbreviations.bicep'
import { IpRange, FirewallRule } from '../../common/types.bicep'
import { ResourceNames, SearchStorageQueueNames } from '../types.bicep'

@description('The URL of the Content API.')
param contentApiUrl string

@description('The IP address ranges that can access the Search Docs Function App endpoints.')
param functionAppFirewallRules FirewallRule[]

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@description('Name of the indexer associated with the \'Search\' index in Azure AI Search.')
param searchServiceSearchIndexerName string

@description('Specifies the Azure AI Search service name.')
param searchServiceName string

@description('Name of the Search storage account.')
param searchStorageAccountName string

@description('The connection string to the Search storage account.')
@secure()
param searchStorageAccountConnectionStringSecretName string

@description('Storage container name for search documents in the Search storage account.')
param searchDocumentsContainerName string

@description('The IP address ranges that can access the Search Docs Function App storage account.')
param storageFirewallRules IpRange[]

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

@description('Specifies the names of the storage queues that trigger functions within the Search Docs Function App.')
param storageQueueNames SearchStorageQueueNames = {
  publicationArchived: 'publication-archived-queue'
  publicationChanged: 'publication-changed-queue'
  publicationDeleted: 'publication-deleted-queue'
  publicationLatestPublishedReleaseReordered: 'publication-latest-published-release-reordered-queue'
  publicationRestored: 'publication-restored-queue'
  refreshSearchableDocument: 'refresh-searchable-document-queue'
  releaseSlugChanged: 'release-slug-changed-queue'
  releaseVersionPublished: 'release-version-published-queue'
  removePublicationSearchableDocuments: 'remove-publication-searchable-documents-queue'
  removeSearchableDocument: 'remove-searchable-document-queue'
  searchableDocumentCreated: 'search-document-created-queue'
  themeUpdated: 'theme-updated-queue'
}

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2026-02-01' existing = {
  name: resourceNames.existingResources.keyVault
}

resource vNet 'Microsoft.Network/virtualNetworks@2024-07-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource outboundVnetSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-07-01' existing = {
  name: resourceNames.existingResources.subnets.searchDocsFunctionApp
  parent: vNet
}

resource searchDocsFunctionAppPrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-07-01' existing = {
  name: resourceNames.existingResources.subnets.searchDocsFunctionAppPrivateEndpoints
  parent: vNet
}

resource searchStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: searchStorageAccountName
}

resource searchBlobStorage 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' existing = {
  name: 'default'
  parent: searchStorageAccount
}

resource searchDocumentsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' existing = {
  parent: searchBlobStorage
  name: searchDocumentsContainerName
}

resource searchService 'Microsoft.Search/searchServices@2025-05-01' existing = {
  name: searchServiceName
}

module functionAppModule '../../common/components/function-app/functionApp.bicep' = {
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
        name: 'App__SearchDocumentsContainerName'
        value: searchDocumentsContainer.name
      }
      {
        name: 'AzureSearch__SearchServiceEndpoint'
        value: searchService.properties.endpoint
      }
      {
        name: 'AzureSearch__IndexerName'
        value: searchServiceSearchIndexerName
      }
      {
        name: 'ContentApi__Url'
        value: contentApiUrl
      }
      {
        name: 'PublicationArchivedQueueName'
        value: storageQueueNames.publicationArchived
      }
      {
        name: 'PublicationChangedQueueName'
        value: storageQueueNames.publicationChanged
      }
      {
        name: 'PublicationDeletedQueueName'
        value: storageQueueNames.publicationDeleted
      }
      {
        name: 'PublicationLatestPublishedReleaseReorderedQueueName'
        value: storageQueueNames.publicationLatestPublishedReleaseReordered
      }
      {
        name: 'PublicationRestoredQueueName'
        value: storageQueueNames.publicationRestored
      }
      {
        name: 'RefreshSearchableDocumentQueueName'
        value: storageQueueNames.refreshSearchableDocument
      }
      {
        name: 'ReleaseSlugChangedQueueName'
        value: storageQueueNames.releaseSlugChanged
      }
      {
        name: 'ReleaseVersionPublishedQueueName'
        value: storageQueueNames.releaseVersionPublished
      }
      {
        name: 'RemovePublicationSearchableDocumentsQueueName'
        value: storageQueueNames.removePublicationSearchableDocuments
      }
      {
        name: 'RemoveSearchableDocumentQueueName'
        value: storageQueueNames.removeSearchableDocument
      }
      {
        name: 'SearchableDocumentCreatedQueueName'
        value: storageQueueNames.searchableDocumentCreated
      }
      {
        name: 'ThemeUpdatedQueueName'
        value: storageQueueNames.themeUpdated
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
    linuxFxVersion: 'DOTNET-ISOLATED|10.0'
    deployQueueRoleAssignment: true
    diagnosticSettingEnabled: true
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    storageAccountName: '${replace(resourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}searchdocsfn'
    storageAccountPublicNetworkAccessEnabled: false
    publicNetworkAccessEnabled: true
    functionAppFirewallRules: functionAppFirewallRules
    storageFirewallRules: storageFirewallRules
    privateEndpoints: {
      functionApp: searchDocsFunctionAppPrivateEndpointSubnet.id
      storageAccounts: searchDocsFunctionAppPrivateEndpointSubnet.id
    }
    outboundSubnetId: outboundVnetSubnet.id
    alerts: {
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
    }
    tagValues: tagValues
  }
}

module functionAppIdentityRoleAssignmentModule '../../common/components/search/searchServiceRoleAssignment.bicep' = {
  name: 'searchDocsFunctionAppRoleAssignmentModuleDeploy'
  params: {
    searchServiceName: searchService.name
    principalIds: [functionAppModule.outputs.functionAppIdentityPrincipalId]
    role: 'Search Service Contributor'
  }
}

module functionAppStorageAccountQueueServiceModule '../../common/components/queueService.bicep' = {
  name: 'searchDocsFunctionAppStorageAccountQueueServiceModuleDeploy'
  params: {
    storageAccountName: functionAppModule.outputs.storageAccountName
    queueNames: map(items(storageQueueNames), item => item.value)
  }
}

output functionAppName string = functionAppModule.outputs.name
output functionAppUrl string = functionAppModule.outputs.url
output functionAppStorageAccountName string = functionAppModule.outputs.storageAccountName
output storageQueueNames SearchStorageQueueNames = storageQueueNames
