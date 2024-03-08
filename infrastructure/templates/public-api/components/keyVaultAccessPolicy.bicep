@description('Specifies the name of the Key Vault.')
param keyVaultName string

@description('Specifies the principal ID of the resource that should have access to the Key Vault')
param principalId string

@description('Specifies the tenant ID of the resource that should have access to the Key Vault')
param tenantId string

resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  name: '${keyVaultName}/add'
  properties: {
    accessPolicies: [
      {
        objectId: principalId
        tenantId: tenantId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
    ]
  }
}
