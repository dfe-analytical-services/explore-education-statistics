import { ResourceNames } from '../../common/resource-names.bicep'
import { IpRange } from '../../common/types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Firewall rules.')
param storageFirewallRules IpRange[]

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.vnet.vnet
}

resource storagePrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.vnet.subnets.screenerStoragePrivateEndpoints
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

module screenerLogsStorageAccountModule '../../common/components/storage/storage-account.bicep' = {
  name: 'screenerLogsStorageAccountDeploy'
  params: {
    storageAccountName: resourceNames.screener.logsStorageAccount
    publicNetworkAccessEnabled: true
    firewallRules: storageFirewallRules
    sku: storageAccountConfig.sku
    kind: storageAccountConfig.kind
    keyVaultName: resourceNames.keyVault.keyVault
    alerts: deployAlerts ? {
      availability: true
      latency: true
      alertsGroupName: resourceNames.alertsGroup
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
    fileShareName: resourceNames.screener.logsFileShare
    fileShareQuotaGbs: storageAccountConfig.fileShare.quotaGbs
    fileShareAccessTier: storageAccountConfig.fileShare.accessTier
    storageAccountName: screenerLogsStorageAccountModule.outputs.storageAccountName
    alerts: deployAlerts ? {
      availability: true
      latency: true
      capacity: true
      alertsGroupName: resourceNames.alertsGroup
    } : null
    tagValues: tagValues
  }
}

output storageAccountName string = screenerLogsStorageAccountModule.outputs.storageAccountName
output fileShareName string = resourceNames.screener.logsFileShare
