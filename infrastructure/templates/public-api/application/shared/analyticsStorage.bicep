import { ResourceNames, IpRange, StorageAccountConfig } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Public API storage account and file share configuration.')
param config StorageAccountConfig

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
  name: resourceNames.existingResources.subnets.storagePrivateEndpoints
  parent: vNet
}

module analyticsStorageAccountModule '../../components/storageAccount.bicep' = {
  name: 'analyticsStorageAccountDeploy'
  params: {
    location: location
    storageAccountName: resourceNames.sharedResources.analyticsStorageAccount
    publicNetworkAccessEnabled: false
    firewallRules: storageFirewallRules
    sku: config.sku
    kind: config.kind
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

module analyticsFileShareModule '../../components/fileShare.bicep' = {
  name: 'analyticsFileShareDeploy'
  params: {
    fileShareName: resourceNames.sharedResources.analyticsFileShare
    fileShareQuotaGbs: config.fileShare.quotaGbs
    storageAccountName: analyticsStorageAccountModule.outputs.storageAccountName
    fileShareAccessTier: config.fileShare.accessTier
    alerts: deployAlerts ? {
      availability: true
      latency: true
      capacity: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    } : null
    tagValues: tagValues
  }
}

output storageAccountName string = analyticsStorageAccountModule.outputs.storageAccountName
output connectionStringSecretName string = analyticsStorageAccountModule.outputs.connectionStringSecretName
output accessKeySecretName string = analyticsStorageAccountModule.outputs.accessKeySecretName
