import { abbreviations } from '../../common/abbreviations.bicep'
import { IpRange } from '../../common/types.bicep'
import { ResourceNames } from '../types.bicep'

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Location for all resources.')
param location string

@description('Name of the searchable documents container in the Search storage account.')
param searchableDocumentsContainerName string = 'searchable-documents'

@description('A list of IP network rules to allow access to the Search Service from specific public internet IP address ranges.')
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

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
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

module searchServiceModule '../components/searchService.bicep' = {
  name: 'searchServiceModuleDeploy'
  params: {
    name: searchServiceName
    location: location
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
    tagValues: tagValues
  }
}

module searchStorageAccountModule '../../public-api/components/storageAccount.bicep' = {
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
    containerNames: [searchableDocumentsContainerName]
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

output searchableDocumentsContainerName string = searchableDocumentsContainerName
output searchServiceEndpoint string = searchServiceModule.outputs.searchServiceEndpoint
output searchServiceName string = searchServiceModule.outputs.searchServiceName
output searchStorageAccountConnectionStringSecretName string = searchStorageAccountModule.outputs.connectionStringSecretName
output searchStorageAccountName string = searchStorageAccountModule.outputs.storageAccountName
