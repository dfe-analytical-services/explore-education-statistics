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
  name: resourceNames.vnet.subnets.analyticsStoragePrivateEndpoints
  parent: vNet
}

var storageAccountName = resourceNames.analytics.storage.storageAccountName

var storageAccountConfig = {
  kind: 'FileStorage'
  sku: 'Premium_ZRS'
  fileShare: {
    quotaGbs: 100
    accessTier: 'Premium'
  }
}

var fileShareName = resourceNames.analytics.storage.fileShareName

module analyticsStorageAccountModule '../../common/components/storage/storage-account.bicep' = {
  name: 'analyticsStorageAccountDeploy'
  params: {
    storageAccountName: storageAccountName
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

module analyticsFileShareModule '../../common/components/storage/fileShare.bicep' = {
  name: 'analyticsFileShareDeploy'
  params: {
    fileShareName: fileShareName
    fileShareQuotaGbs: storageAccountConfig.fileShare.quotaGbs
    storageAccountName: analyticsStorageAccountModule.outputs.storageAccountName
    fileShareAccessTier: storageAccountConfig.fileShare.accessTier
    alerts: deployAlerts ? {
      availability: true
      latency: true
      capacity: true
      alertsGroupName: resourceNames.alertsGroup
    } : null
    tagValues: tagValues
  }
}

output storageAccountName string = analyticsStorageAccountModule.outputs.storageAccountName
output fileShareName string = fileShareName
