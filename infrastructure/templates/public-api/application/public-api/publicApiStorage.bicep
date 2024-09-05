@description('Specifies the location for all resources.')
param location string

@description('Specifies the Public API resource prefix')
param publicApiResourcePrefix string

@description('Specifies the subscription name.')
param subscription string

@description('Specifies the subnet id of the Public API Data Processor.')
param dataProcessorSubnetId string

@description('Specifies the subnet id of the Container App Environment.')
param containerAppEnvironmentSubnetId string

@description('Specifies the name of the Key Vault.')
param keyVaultName string

@description('Public API Storage : Size of the file share in GB.')
param publicApiDataFileShareQuota int = 1

@description('Public API Storage : Firewall rules.')
param storageFirewallRules {
  name: string
  cidr: string
}[] = []

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var publicApiDataFileShareName = '${publicApiResourcePrefix}-fs-data'
var publicApiStorageAccountName = '${subscription}eespapisa'

// TODO EES-5128 - add private endpoints to allow VNet traffic to go directly to Storage Account over the VNet.
// Currently supported by subnet whitelisting and Storage service endpoints being enabled on the whitelisted subnets.
module publicApiStorageAccountModule '../../components/storageAccount.bicep' = {
  name: 'publicApiStorageAccountDeploy'
  params: {
    location: location
    storageAccountName: publicApiStorageAccountName
    allowedSubnetIds: [
      dataProcessorSubnetId
      containerAppEnvironmentSubnetId
    ]
    firewallRules: storageFirewallRules
    skuStorageResource: 'Standard_LRS'
    keyVaultName: keyVaultName
    tagValues: tagValues
  }
}

module dataFilesFileShareModule '../../components/fileShare.bicep' = {
  name: 'fileShareDeploy'
  params: {
    fileShareName: publicApiDataFileShareName
    fileShareQuota: publicApiDataFileShareQuota
    storageAccountName: publicApiStorageAccountModule.outputs.storageAccountName
    fileShareAccessTier: 'TransactionOptimized'
  }
}

output storageAccountName string = publicApiStorageAccountModule.outputs.storageAccountName
output connectionStringSecretName string = publicApiStorageAccountModule.outputs.connectionStringSecretName
output accessKeySecretName string = publicApiStorageAccountModule.outputs.accessKeySecretName
output publicApiDataFileShareName string = publicApiDataFileShareName
output publicApiStorageAccountName string = publicApiStorageAccountName
