import { abbreviations } from '../../common/abbreviations.bicep'
import { IpRange } from '../../common/types.bicep'
import { ResourceNames } from '../types.bicep'

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Resource prefix for all resources.')
param resourcePrefix string

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Location for all resources.')
param location string

@description('Name of the searchable documents container in the Search storage account.')
param searchableDocumentsContainerName string = 'searchable-documents'

@description('Storage account firewall rules.')
param storageFirewallRules IpRange[]

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: resourceNames.existingResources.keyVault
}

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource searchStoragePrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.searchStoragePrivateEndpoints
  parent: vNet
}

module searchServiceModule '../components/searchService.bicep' = {
  name: 'searchServiceModuleDeploy'
  params: {
    name: '${resourcePrefix}-${abbreviations.searchSearchServices}'
    location: location
    sku: 'free'
    tagValues: tagValues
  }
}

module searchStorageAccountModule '../../public-api/components/storageAccount.bicep' = {
  name: 'searchStorageAccountModuleDeploy'
  params: {
    location: location
    storageAccountName: '${replace(resourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}search'
    publicNetworkAccessEnabled: false
    firewallRules: storageFirewallRules
    sku: 'Standard_LRS'
    kind: 'StorageV2'
    keyVaultName: keyVault.name
    alerts: deployAlerts ? {
      availability: true
      latency: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    } : null
    privateEndpointSubnetIds: {
      blob: searchStoragePrivateEndpointSubnet.id
    }
    tagValues: tagValues
  }
}

module blobServiceModule '../../common/components/blobService.bicep' = {
  name: 'blobServiceModuleDeploy'
  params: {
    storageAccountName: searchStorageAccountModule.outputs.storageAccountName
    containerNames: [searchableDocumentsContainerName]
  }
}

output searchableDocumentsContainerName string = searchableDocumentsContainerName
output searchServiceEndpoint string = searchServiceModule.outputs.searchServiceEndpoint
output searchServiceName string = searchServiceModule.outputs.searchServiceName
output searchStorageAccountConnectionStringSecretName string = searchStorageAccountModule.outputs.connectionStringSecretName
output searchStorageAccountName string = searchStorageAccountModule.outputs.storageAccountName
