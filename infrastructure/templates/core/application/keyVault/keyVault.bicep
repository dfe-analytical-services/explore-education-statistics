import { abbreviations } from '../../../common/abbreviations.bicep'

@description('Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

var keyVaultName = '${subscription}-${abbreviations.keyVaultVaults}-ees-01'

module keyVaultModule '../../../common/components/key-vault/keyVault.bicep' = {
  name: '${keyVaultName}ModuleDeploy'
  params: {
    keyVaultName: keyVaultName
    enabledForDeployment: true
    enabledForDiskEncryption: true
    enabledForTemplateDeployment: true
    skuName: 'standard'
    tagValues: tagValues
  }
}

output keyVaultName string = keyVaultName 
