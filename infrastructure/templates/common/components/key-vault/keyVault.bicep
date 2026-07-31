@description('Specifies the name of the Key Vault')
param keyVaultName string

@description('Specifies the location for all resources. Defaults to the Resource Group location.')
param location string = resourceGroup().location

@description('Specifies the Azure Active Directory tenant ID that should be used for authenticating requests to the key vault. Defaults to the Resource Group tenant.')
param tenantId string = tenant().tenantId

@description('Specifies whether Azure Virtual Machines are permitted to retrieve certificates stored as secrets from the key vault.')
param enabledForDeployment bool = true

@description('Specifies whether Azure Disk Encryption is permitted to retrieve secrets from the vault and unwrap keys.')
param enabledForDiskEncryption bool = false

@description('Specifies whether Azure Resource Manager is permitted to retrieve secrets from the key vault.')
param enabledForTemplateDeployment bool = true

@description('Specifies whether the key vault is a standard vault or a premium vault.')
param skuName 'standard' | 'premium' = 'standard'

@description('The number of days to retain deleted secrets and certificates. Defaults to 7 days.')
param softDeleteRetentionInDays int = 7

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

resource keyvault 'Microsoft.KeyVault/vaults@2026-02-01' = {
  name: keyVaultName
  location: location
  properties: {
    enabledForDeployment: enabledForDeployment
    enabledForDiskEncryption: enabledForDiskEncryption
    enabledForTemplateDeployment: enabledForTemplateDeployment
    enablePurgeProtection: true
    tenantId: tenantId
    enableSoftDelete: true
    softDeleteRetentionInDays: softDeleteRetentionInDays
    accessPolicies: []
    sku: {
      name: skuName
      family: 'A'
    }
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
  tags: tagValues
}

output keyVaultName string = keyvault.name
output keyVaultId string = keyvault.id
