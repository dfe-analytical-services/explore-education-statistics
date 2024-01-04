@description('The name of the Key Vault to store the secret within')
param KeyVaultName string

@description('The name of the secret to store')
param SecretName string

@description('The value being stored in the Key Vault secret')
@secure()
param SecretValue string

@description('Optional: The type of content being stored')
param ContentType string = 'text/plain'

@description('Optional: Determines whether the secret is enabled')
param IsEnabled bool = true

resource KeyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: KeyVaultName
}

resource KeyVaultSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: replace(replace(SecretName, '.', '-'), ' ', '-')
  parent: KeyVault
  properties: {
    contentType: ContentType
    attributes: {
      enabled: IsEnabled 
    }
    value: SecretValue
  }
}

//The URI pointing at the created secret
output SecretUri string = KeyVaultSecret.properties.secretUri
