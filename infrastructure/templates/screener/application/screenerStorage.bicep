import { ResourceNames } from '../types.bicep'
import { IpRange } from '../../common/types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Firewall rules.')
param storageFirewallRules IpRange[]

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource storagePrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.screenerFunctionPrivateEndpoints
  parent: vNet
}

var storageAccountConfig = {
  kind: 'FileStorage'
  sku: 'Standard_LRS'
  fileShare: {
    quotaGbs: 1
    accessTier: 'Hot'
  }
}

module storageStorageAccountModule '../../common/components/storage/storageAccount.bicep' = {
  name: 'storageStorageAccountDeploy'
  params: {
    location: location
    storageAccountName: resourceNames.screener.screenerLogsStorageAccount
    publicNetworkAccessEnabled: true
    firewallRules: storageFirewallRules
    sku: storageAccountConfig.sku
    kind: storageAccountConfig.kind
    keyVaultName: resourceNames.existingResources.keyVault
    alerts: deployAlerts ? {
      availability: true
      latency: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    } : null
    privateEndpointSubnetIds: {
      file: storagePrivateEndpointSubnet.id
    }
    tagValues: tagValues
  }
}

module storageFileShareModule '../../common/components/storage/fileShare.bicep' = {
  name: 'storageFileShareDeploy'
  params: {
    fileShareName: resourceNames.screener.screenerLogsFileShare
    fileShareQuotaGbs: storageAccountConfig.fileShare.quotaGbs
    storageAccountName: storageStorageAccountModule.outputs.storageAccountName
    fileShareAccessTier: storageAccountConfig.fileShare.accessTier
    alerts: deployAlerts ? {
      availability: true
      latency: true
      capacity: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    } : null
    tagValues: tagValues
  }
}

output storageAccountName string = storageStorageAccountModule.outputs.storageAccountName
output fileShareName string = resourceNames.screener.screenerLogsFileShare
