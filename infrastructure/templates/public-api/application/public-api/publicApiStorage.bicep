import { IpRange } from '../../../common/types.bicep'
import { ResourceNames } from '../../../common/resource-names.bicep'
import { StorageAccountConfig } from '../../../common/components/storage/types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Public API storage account and file share configuration.')
param config StorageAccountConfig

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
  name: resourceNames.vnet.subnets.publicApiStoragePrivateEndpoints
  parent: vNet
}

module publicApiStorageAccountModule '../../../common/components/storage/storage-account.bicep' = {
  name: 'publicApiStorageAccountDeploy'
  params: {
    storageAccountName: resourceNames.publicApi.storage.storageAccount
    publicNetworkAccessEnabled: false
    firewallRules: storageFirewallRules
    sku: config.sku
    kind: config.kind
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

module dataFilesFileShareModule '../../../common/components/storage/fileShare.bicep' = {
  name: 'publicApiFileShareDeploy'
  params: {
    fileShareName: resourceNames.publicApi.storage.fileShare
    fileShareQuotaGbs: config.fileShare.quotaGbs
    storageAccountName: publicApiStorageAccountModule.outputs.storageAccountName
    fileShareAccessTier: config.fileShare.accessTier
    alerts: deployAlerts ? {
      availability: true
      latency: true
      capacity: true
      alertsGroupName: resourceNames.alertsGroup
    } : null
    tagValues: tagValues
  }
}

module publicApiStorageBackupModule '../../../common/components/recovery-services-vault/file-share-registration.bicep' = {
  name: 'publicApiStorageBackupModuleDeploy'
  params: {
    vaultName: resourceNames.backups.recoveryServicesVault
    backupPolicyName: 'DailyPolicy'
    storageAccountName: publicApiStorageAccountModule.outputs.storageAccountName
    fileShareName: dataFilesFileShareModule.outputs.fileShareName
    tagValues: tagValues
  }
}

output storageAccountName string = publicApiStorageAccountModule.outputs.storageAccountName
