@description('Name of the Key Vault.')
param keyVaultName string

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

module keyVaultModule '../../../common/components/key-vault/keyVault.bicep' = {
  name: '${keyVaultName}ModuleDeploy'
  params: {
    keyVaultName: keyVaultName
    enabledForDiskEncryption: true
    softDeleteRetentionInDays: 90
    tagValues: tagValues
  }
}

output keyVaultName string = keyVaultName 
