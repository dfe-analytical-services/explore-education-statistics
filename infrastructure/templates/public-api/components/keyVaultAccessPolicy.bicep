@description('Specifies the name of the Key Vault.')
param keyVaultName string

@description('Specifies the principal IDs of the resources that should inherit this Key Vault policy')
param principalIds string[]

@description('Specifies the permissions the principal should have in this Key Vault. Defaults to read-only access to secrets.')
param permissions object = {
 secrets: [
   'get'
   'list'
 ]
}

resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  name: '${keyVaultName}/add'
  properties: {
    accessPolicies: [for principalId in principalIds: {
      objectId: principalId
      tenantId: subscription().tenantId
      permissions: permissions
    }]
  }
}
