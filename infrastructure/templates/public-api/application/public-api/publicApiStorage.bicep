import { resourceNamesType, firewallRuleType } from '../../types.bicep'

param resourceNames resourceNamesType

@description('Specifies the location for all resources.')
param location string

@description('Public API Storage : Size of the file share in GB.')
param publicApiDataFileShareQuota int = 1

@description('Public API Storage : Firewall rules.')
param storageFirewallRules firewallRuleType[] = []

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

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
    ]
    firewallRules: storageFirewallRules
    skuStorageResource: 'Standard_LRS'
    keyVaultName: resourceNames.existingResources.keyVault
    tagValues: tagValues
  }
}

module dataFilesFileShareModule '../../components/fileShare.bicep' = {
  name: 'fileShareDeploy'
  params: {
    fileShareName: resourceNames.publicApi.publicApiFileshare
    fileShareQuota: publicApiDataFileShareQuota
    storageAccountName: publicApiStorageAccountModule.outputs.storageAccountName
    fileShareAccessTier: 'TransactionOptimized'
  }
}

output storageAccountName string = publicApiStorageAccountModule.outputs.storageAccountName
output connectionStringSecretName string = publicApiStorageAccountModule.outputs.connectionStringSecretName
output accessKeySecretName string = publicApiStorageAccountModule.outputs.accessKeySecretName
output publicApiDataFileShareName string = resourceNames.publicApi.publicApiFileshare
output publicApiStorageAccountName string = resourceNames.publicApi.publicApiStorageAccount
