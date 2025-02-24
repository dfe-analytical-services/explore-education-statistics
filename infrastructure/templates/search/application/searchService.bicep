import { IpRange } from '../../common/types.bicep'
import { ResourceNames } from '../types.bicep'

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Location for all resources.')
param location string

@description('Storage account firewall rules.')
param storageFirewallRules IpRange[]

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource searchDocsStoragePrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.searchDocsStoragePrivateEndpoints
  parent: vNet
}

module searchServiceModule '../components/searchService.bicep' = {
  name: 'searchServiceModuleDeploy'
  params: {
    name: resourceNames.search.searchService
    location: location
    tagValues: tagValues
    sku: 'free'
  }
}

module searchDocsStorageAccountModule '../../public-api/components/storageAccount.bicep' = {
  name: 'searchDocsStorageAccountModuleDeploy'
  params: {
    location: location
    storageAccountName: resourceNames.search.searchDocsStorageAccount
    publicNetworkAccessEnabled: false
    firewallRules: storageFirewallRules
    sku: 'Standard_LRS'
    kind: 'StorageV2'
    keyVaultName: resourceNames.existingResources.keyVault
    alerts: deployAlerts ? {
      availability: true
      latency: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    } : null
    privateEndpointSubnetIds: {
      blob: searchDocsStoragePrivateEndpointSubnet.id
    }
    tagValues: tagValues
  }
}

module blobServiceModule '../../common/components/blobService.bicep' = {
  name: 'blobServiceModuleDeploy'
  params: {
    storageAccountName: resourceNames.search.searchDocsStorageAccount
    containerNames: ['searchable-documents']
  }
}

output searchEndpoint string = searchServiceModule.outputs.searchEndpoint
