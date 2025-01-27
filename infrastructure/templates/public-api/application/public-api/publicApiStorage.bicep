import { ResourceNames, IpRange, PublicApiStorageAccountConfig } from '../../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Public API storage account and file share configuration.')
param config PublicApiStorageAccountConfig

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

module publicApiStorageAccountModule '../../components/storageAccount.bicep' = {
  name: 'publicApiStorageAccountDeploy'
  params: {
    location: location
    storageAccountName: resourceNames.publicApi.publicApiStorageAccount
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

module dataFilesFileShareModule '../../components/fileShare.bicep' = {
  name: 'fileShareDeploy'
  params: {
    fileShareName: resourceNames.publicApi.publicApiFileShare
    fileShareQuotaGbs: config.fileShare.quotaGbs
    storageAccountName: publicApiStorageAccountModule.outputs.storageAccountName
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

output storageAccountName string = publicApiStorageAccountModule.outputs.storageAccountName
output connectionStringSecretName string = publicApiStorageAccountModule.outputs.connectionStringSecretName
output accessKeySecretName string = publicApiStorageAccountModule.outputs.accessKeySecretName
output publicApiDataFileShareName string = resourceNames.publicApi.publicApiFileShare
output publicApiStorageAccountName string = resourceNames.publicApi.publicApiStorageAccount
