import { IpRange, FirewallRule } from '../../common/types.bicep'
import { ResourceNames } from '../../common/resource-names.bicep'
import { SearchStorageQueueNames } from '../types.bicep'

@description('The URL of the Content API.')
param contentApiUrl string

@description('Storage container name for dataset documents in the Search storage account.')
param datasetDocumentsContainerName string

@description('Name of the indexer associated with the \'Dataset\' index in Azure AI Search.')
param datasetIndexerName string

@description('The IP address ranges that can access the Search Docs Function App endpoints.')
param functionAppFirewallRules FirewallRule[]

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@description('Name of the indexer associated with the \'Release\' index in Azure AI Search.')
param releaseIndexerName string

@description('Specifies the Azure AI Search service name.')
param searchServiceName string

@description('Name of the Search storage account.')
param searchStorageAccountName string

@description('The connection string to the Search storage account.')
@secure()
param searchStorageAccountConnectionStringSecretName string

@description('Storage container name for release documents in the Search storage account.')
param releaseDocumentsContainerName string

@description('The IP address ranges that can access the Search Docs Function App storage account.')
param storageFirewallRules IpRange[]

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('The Application Insights connection string that is associated with this resource.')
param applicationInsightsConnectionString string = ''

@description('Specifies the names of the storage queues that trigger functions within the Search Docs Function App.')
param storageQueueNames SearchStorageQueueNames = {
  publicationArchived: 'publication-archived-queue'
  publicationChanged: 'publication-changed-queue'
  publicationDeleted: 'publication-deleted-queue'
  publicationLatestPublishedReleaseReordered: 'publication-latest-published-release-reordered-queue'
  publicationRestored: 'publication-restored-queue'
  refreshReleaseDocument: 'refresh-release-document-queue'
  releaseDocumentCreated: 'release-document-created-queue'
  releaseSlugChanged: 'release-slug-changed-queue'
  releaseVersionPublished: 'release-version-published-queue'
  removePublicationReleaseDocuments: 'remove-publication-release-documents-queue'
  removeReleaseDocument: 'remove-release-document-queue'
  themeUpdated: 'theme-updated-queue'
}

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2026-02-01' existing = {
  name: resourceNames.keyVault.keyVault
}

resource vNet 'Microsoft.Network/virtualNetworks@2024-07-01' existing = {
  name: resourceNames.vnet.vnet
}

resource outboundVnetSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-07-01' existing = {
  name: resourceNames.vnet.subnets.searchDocsFunctionApp
  parent: vNet
}

resource searchDocsFunctionAppPrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-07-01' existing = {
  name: resourceNames.vnet.subnets.searchDocsFunctionAppPrivateEndpoints
  parent: vNet
}

resource searchStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: searchStorageAccountName
}

resource searchBlobStorage 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' existing = {
  name: 'default'
  parent: searchStorageAccount
}

resource releaseDocumentsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' existing = {
  parent: searchBlobStorage
  name: releaseDocumentsContainerName
}

resource datasetDocumentsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' existing = {
  parent: searchBlobStorage
  name: datasetDocumentsContainerName
}

resource searchService 'Microsoft.Search/searchServices@2025-05-01' existing = {
  name: searchServiceName
}

module functionAppModule '../../common/components/function-app/function-app.bicep' = {
  name: 'searchDocsFunctionAppModuleDeploy'
  params: {
    functionAppName: resourceNames.searchDocs.functionApp
    applicationInsightsConnectionString: applicationInsightsConnectionString
    appServicePlanName: resourceNames.searchDocs.appServicePlan
    appSettings: [
      {
        name: 'App__SearchStorageConnectionString'
        value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${searchStorageAccountConnectionStringSecretName})'
      }
      {
        name: 'App__ReleaseDocumentsContainerName'
        value: releaseDocumentsContainer.name
      }
      {
        name: 'App__DatasetDocumentsContainerName'
        value: datasetDocumentsContainer.name
      }
      {
        name: 'AzureSearch__SearchServiceEndpoint'
        value: searchService.properties.endpoint
      }
      {
        name: 'AzureSearch__ReleaseIndexerName'
        value: releaseIndexerName
      }
      {
        name: 'AzureSearch__DatasetIndexerName'
        value: datasetIndexerName
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
        name: 'RefreshReleaseDocumentQueueName'
        value: storageQueueNames.refreshReleaseDocument
      }
      {
        name: 'ReleaseDocumentCreatedQueueName'
        value: storageQueueNames.releaseDocumentCreated
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
        name: 'RemovePublicationReleaseDocumentsQueueName'
        value: storageQueueNames.removePublicationReleaseDocuments
      }
      {
        name: 'RemoveReleaseDocumentQueueName'
        value: storageQueueNames.removeReleaseDocument
      }
      {
        name: 'ThemeUpdatedQueueName'
        value: storageQueueNames.themeUpdated
      }
    ]
    keyVaultName: keyVault.name
    keyVaultRoles: {
      secretsUser: true
      legacyKeyVaultRoleAssignmentName: false
    }
    sku: {
      name: 'EP1'
      tier: 'ElasticPremium'
      family: 'EP'
    }
    elasticCapacity: {
      minimumInstanceCount: 1
      maximumInstanceCount: 1
      instanceMemoryMB: 2048
    }
    healthCheckPath: '/api/HealthCheck'
    operatingSystem: 'Linux'
    functionAppRuntime: 'dotnet-isolated'
    linuxFxVersion: 'DOTNET-ISOLATED|10.0'
    deployQueueRoleAssignment: true
    diagnosticSettingsLogAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    storageAccountName: resourceNames.searchDocs.functionStorageAccount
    storageAccountConnectionStringSecretName: resourceNames.keyVault.secrets.searchDocsStorageAccountConnectionString
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
      alertsGroupName: resourceNames.alertsGroup
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
