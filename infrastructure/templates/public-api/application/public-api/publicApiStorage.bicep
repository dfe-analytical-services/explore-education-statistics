import { ResourceNames, IpRange } from '../../types.bicep'

param resourceNames ResourceNames

@description('Specifies the location for all resources.')
param location string

@description('Public API Storage : Size of the file share in GB.')
param publicApiDataFileShareQuota int

@description('Public API Storage : Firewall rules.')
param storageFirewallRules IpRange[]

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.existingResources.vNet
}

resource dataProcessorSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.dataProcessor
  parent: vNet
}

resource containerAppEnvironmentSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.containerAppEnvironment
  parent: vNet
}

resource publisherSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.existingResources.subnets.publisherFunction
  parent: vNet
}

// TODO EES-5128 - add private endpoints to allow VNet traffic to go directly to Storage Account over the VNet.
// Currently supported by subnet whitelisting and Storage service endpoints being enabled on the whitelisted subnets.
module publicApiStorageAccountModule '../../components/storageAccount.bicep' = {
  name: 'publicApiStorageAccountDeploy'
  params: {
    location: location
    storageAccountName: resourceNames.publicApi.publicApiStorageAccount
    allowedSubnetIds: [
      dataProcessorSubnet.id
      containerAppEnvironmentSubnet.id
      publisherSubnet.id
    ]
    firewallRules: storageFirewallRules
    skuStorageResource: 'Standard_LRS'
    keyVaultName: resourceNames.existingResources.keyVault
    alerts: deployAlerts ? {
      availability: true
      latency: true
      alertsGroupName: resourceNames.existingResources.alertsGroup
    } : null
    tagValues: tagValues
  }
}

module dataFilesFileShareModule '../../components/fileShare.bicep' = {
  name: 'fileShareDeploy'
  params: {
    fileShareName: resourceNames.publicApi.publicApiFileShare
    fileShareQuota: publicApiDataFileShareQuota
    storageAccountName: publicApiStorageAccountModule.outputs.storageAccountName
    fileShareAccessTier: 'TransactionOptimized'
    alerts: deployAlerts ? {
      availability: true
      latency: true
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
