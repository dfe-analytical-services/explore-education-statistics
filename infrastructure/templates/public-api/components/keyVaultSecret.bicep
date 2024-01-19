@description('The name of the Key Vault to store the secret within')
param keyVaultName string

@description('The name of the secret to store')
param secretName string

@description('The value being stored in the Key Vault secret')
@secure()
param secretValue string

@description('Optional: The type of content being stored')
param contentType string = 'text/plain'

@description('Optional: Determines whether the secret is enabled')
param isEnabled bool = true

resource KeyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

resource KeyVaultSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: replace(replace(secretName, '.', '-'), ' ', '-')
  parent: KeyVault
  properties: {
    contentType: contentType
    attributes: {
      enabled: isEnabled 
    }
    value: secretValue
  }
}

//The URI pointing at the created secret
output keyVaultSecretUri string = KeyVaultSecret.properties.secretUri
