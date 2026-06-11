import { abbreviations } from '../../common/abbreviations.bicep'
import { IpRange, FirewallRule } from '../../common/types.bicep'
import { ResourceNames } from '../types.bicep'

@description('The IP address ranges that can access the Natural Language Search Function App endpoints.')
param functionAppFirewallRules FirewallRule[]

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@description('Specifies the Azure AI Search service name.')
param searchServiceName string

@description('Name of the \'Natural language search filter\' index in Azure AI Search.')
param searchServiceNLSearchFilterIndexName string

@description('Name of the \'Natural language search dataset\' index in Azure AI Search.')
param searchServiceNLSearchDatasetIndexName string

@description('Name of the Search storage account.')
param searchStorageAccountName string

@description('The connection string to the Search storage account.')
@secure()
param searchStorageAccountConnectionStringSecretName string

@description('Name of the storage container in the Search storage account that stores the locations dictionary.')
param locationsDictionaryContainerName string

@description('The IP address ranges that can access the Natural Language Search Function App storage account.')
param storageFirewallRules IpRange[]

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Location for all resources.')
param location string

@description('The Application Insights connection string that is associated with this resource.')
param applicationInsightsConnectionString string = ''

@description('Specifies whether or not the Natural Language Search Function App already exists.')
param functionAppExists bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2026-02-01' existing = {
  name: resourceNames.existingResources.keyVault
}

resource vNet 'Microsoft.Network/virtualNetworks@2024-07-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource outboundVnetSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-07-01' existing = {
  name: resourceNames.existingResources.subnets.nlSearchFunctionApp
  parent: vNet
}

resource nlSearchFunctionAppPrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-07-01' existing = {
  name: resourceNames.existingResources.subnets.nlSearchFunctionAppPrivateEndpoints
  parent: vNet
}

resource searchStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: searchStorageAccountName
}

resource searchBlobStorage 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' existing = {
  name: 'default'
  parent: searchStorageAccount
}

resource locationsDictionaryContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' existing = {
  parent: searchBlobStorage
  name: locationsDictionaryContainerName
}

resource searchService 'Microsoft.Search/searchServices@2025-05-01' existing = {
  name: searchServiceName
}

var azureOpenAIApiBaseUrlSecretName = 'nlsearch-azure-openai-api-base-url'
var azureOpenAIApiKeySecretName = 'nlsearch-azure-openai-api-subscription-key'

module functionAppModule '../../common/components/function-app/functionApp.bicep' = {
  name: 'nlSearchFunctionAppModuleDeploy'
  params: {
    functionAppName: '${resourcePrefix}-${abbreviations.webSitesFunctions}-nlsearch'
    location: location
    applicationInsightsConnectionString: applicationInsightsConnectionString
    appServicePlanName: '${resourcePrefix}-${abbreviations.webServerFarms}-nlsearch'
    appSettings: [
      {
        name: 'AZURE_SEARCH_ENDPOINT'
        value: searchService.properties.endpoint
      }
      {
        name: 'AZURE_SEARCH_FILTER_INDEX'
        value: searchServiceNLSearchFilterIndexName
      }
      {
        name: 'AZURE_SEARCH_DATASET_INDEX'
        value: searchServiceNLSearchDatasetIndexName
      }
      {
        name: 'AZURE_OPENAI_ENDPOINT'
        value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${azureOpenAIApiBaseUrlSecretName})'
      }
      {
        name: 'AZURE_OPENAI_API_KEY'
        value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${azureOpenAIApiKeySecretName})'
      }
      {
        name: 'AZURE_OPENAI_DEPLOYMENT'
        value: 'gpt-4.1-mini'
      }
      {
        name: 'AZURE_OPENAI_EMBEDDING_DEPLOYMENT'
        value: 'text-embedding-3-large'
      }
      {
        name: 'AZURE_OPENAI_API_VERSION'
        value: '2024-10-21'
      }
      {
        name: 'LOCATIONS_DICT_CONTAINER_NAME'
        value: locationsDictionaryContainer.name
      }
      {
        name: 'SEARCH_STORAGE_CONN_STRING'
        value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${searchStorageAccountConnectionStringSecretName})'
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
    functionAppRuntime: 'python'
    linuxFxVersion: 'Python|3.14'
    diagnosticSettingEnabled: true
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    storageAccountName: '${replace(resourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}nlsearchfn'
    storageAccountPublicNetworkAccessEnabled: false
    publicNetworkAccessEnabled: true
    functionAppFirewallRules: functionAppFirewallRules
    storageFirewallRules: storageFirewallRules
    privateEndpoints: {
      functionApp: nlSearchFunctionAppPrivateEndpointSubnet.id
      storageAccounts: nlSearchFunctionAppPrivateEndpointSubnet.id
    }
    outboundSubnetId: outboundVnetSubnet.id
    alerts: {
      cpuPercentage: true
      // TODO EES-7164 - Enable function app health alert once the health check endpoint is deployed
      functionAppHealth: false
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
  name: 'nlSearchFunctionAppRoleAssignmentModuleDeploy'
  params: {
    searchServiceName: searchService.name
    principalIds: [functionAppModule.outputs.functionAppIdentityPrincipalId]
    role: 'Search Index Data Reader'
  }
}

output functionAppName string = functionAppModule.outputs.name
output functionAppUrl string = functionAppModule.outputs.url
output functionAppStorageAccountName string = functionAppModule.outputs.storageAccountName
