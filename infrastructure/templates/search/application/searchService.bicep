import { abbreviations } from '../../common/abbreviations.bicep'
import { IpRange } from '../../common/types.bicep'
import { ResourceNames } from '../types.bicep'

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Location for all resources.')
param location string

@description('Storage container names for search documents. A container for each value will be created in the Search storage account.')
param searchStorageDocumentContainers object = {
  searchDocuments: 'searchable-documents'
  nlSearchDatasetDocuments: 'nl-search-dataset-documents'
  nlSearchFilterDocuments: 'nl-search-filter-documents'
}

@description('Indicates whether API keys are permitted for authentication to the Azure AI Search service in addition to role-based access control (RBAC). Disabling API keys forces all clients to use RBAC only.')
param searchServiceLocalAuthEnabled bool = false

@description('A list of IP network rules to allow access to the Azure AI Search service from specific public internet IP address ranges.')
param searchServiceIpRules IpRange[]

@description('Controls the availability of semantic ranking for all indexes. Set to \'free\' for limited query volume on the free plan, \'standard\' for unlimited volume on the standard pricing plan, or \'disabled\' to turn it off.')
@allowed([
  'disabled'
  'free'
  'standard'
])
param semanticRankerAvailability string

@description('A list of IP network rules to allow access to the Search storage account from specific public internet IP address ranges.')
param storageIpRules IpRange[]

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2026-02-01' existing = {
  name: resourceNames.existingResources.keyVault
}

resource vNet 'Microsoft.Network/virtualNetworks@2024-07-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource searchStoragePrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2024-07-01' existing = {
  name: resourceNames.existingResources.subnets.searchStoragePrivateEndpoints
  parent: vNet
}

var searchServiceName = '${resourcePrefix}-${abbreviations.searchSearchServices}'
var searchStorageDocumentContainerNames = map(items(searchStorageDocumentContainers), item => item.value)

module searchServiceModule '../components/searchService.bicep' = {
  name: 'searchServiceModuleDeploy'
  params: {
    name: searchServiceName
    location: location
    searchServiceLocalAuthEnabled: searchServiceLocalAuthEnabled
    ipRules: searchServiceIpRules
    publicNetworkAccess: 'Enabled'
    semanticRankerAvailability: semanticRankerAvailability
    sku: 'basic'
    systemAssignedIdentity: true
    alerts: {
      searchLatency: true
      searchQueriesPerSecond: true
      throttledSearchQueriesPercentage: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    }
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    tagValues: tagValues
  }
}

module searchStorageAccountModule '../../common/components/storage/storageAccount.bicep' = {
  name: 'searchStorageAccountModuleDeploy'
  params: {
    location: location
    storageAccountName: '${replace(resourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}search'
    publicNetworkAccessEnabled: true
    firewallRules: storageIpRules
    sku: 'Standard_LRS'
    kind: 'StorageV2'
    keyVaultName: keyVault.name
    alerts: {
      availability: true
      latency: false
      alertsGroupName: resourceNames.existingResources.alertsGroup
    }
    privateEndpointSubnetIds: {
      blob: searchStoragePrivateEndpointSubnet.id
    }
    tagValues: tagValues
  }
}

module searchStorageAccountBlobServiceModule '../../common/components/blobService.bicep' = {
  name: 'searchStorageAccountBlobServiceModuleDeploy'
  params: {
    storageAccountName: searchStorageAccountModule.outputs.storageAccountName
    containerNames: searchStorageDocumentContainerNames
  }
}

module searchServiceBlobRoleAssignmentModule '../../common/components/storageAccountRoleAssignment.bicep' = {
  name: '${searchServiceName}BlobRoleAssignmentModuleDeploy'
  params: {
    principalIds: [searchServiceModule.outputs.searchServiceIdentityPrincipalId]
    storageAccountName: searchStorageAccountModule.outputs.storageAccountName
    role: 'Storage Blob Data Reader'
  }
}

output searchServiceEndpoint string = searchServiceModule.outputs.searchServiceEndpoint
output searchServiceName string = searchServiceModule.outputs.searchServiceName
output searchStorageAccountConnectionStringSecretName string = searchStorageAccountModule.outputs.connectionStringSecretName
output searchStorageAccountManagedIdentityConnectionString string = 'ResourceId=${searchStorageAccountModule.outputs.storageAccountId};'
output searchStorageAccountName string = searchStorageAccountModule.outputs.storageAccountName
output searchStorageDocumentContainers object = searchStorageDocumentContainers
