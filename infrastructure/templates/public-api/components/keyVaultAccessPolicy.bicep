//Specific parameters for the resources
@description('Specifies the name of the Key Vault.')
param keyVaultName string

@description('Specifies the principal ID of the resource.')
param principalId string

@description('Specifies the tenant ID of the resource.')
param tenantId string


// Variables and created data


//Resources
resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2021-11-01-preview' = {
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
