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
  kind: 'StorageV2'
  sku: 'Standard_LRS'
  fileShare: {
    quotaGbs: 1
    accessTier: 'Hot'
  }
}

module screenerLogsStorageAccountModule '../../common/components/storage/storageAccount.bicep' = {
  name: 'screenerLogsStorageAccountDeploy'
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

module screenerLogsFileShareModule '../../common/components/storage/fileShare.bicep' = {
  name: 'screenerLogsFileShareDeploy'
  params: {
    fileShareName: resourceNames.screener.screenerLogsFileShare
    fileShareQuotaGbs: storageAccountConfig.fileShare.quotaGbs
    fileShareAccessTier: storageAccountConfig.fileShare.accessTier
    storageAccountName: screenerLogsStorageAccountModule.outputs.storageAccountName
    alerts: deployAlerts ? {
      availability: true
      latency: true
      capacity: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    } : null
    tagValues: tagValues
  }
}

output storageAccountName string = screenerLogsStorageAccountModule.outputs.storageAccountName
output fileShareName string = resourceNames.screener.screenerLogsFileShare
