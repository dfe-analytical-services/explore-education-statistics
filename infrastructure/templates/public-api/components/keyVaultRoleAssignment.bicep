@description('Specifies the name of the Key Vault.')
param keyVaultName string

@description('Specifies the id of the service principals that should inherit this Key Vault policy')
param principalIds string[]

@description('Specifies the Key Vault role to assign to the service principals. See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles for possible roles to support here.')
@allowed([
  'Secrets User'
])
param role string

@description('See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles for possible roles to support here.')
var roleIds = {

  'Secrets User': '4633458b-17de-408a-b874-0445c86b69e6'
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

@description('Look up the built-in role definition')
resource roleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: roleIds[role]
}

@description('Grant the service principals the key vault role')
resource keyVaultSecretUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = [for principalId in principalIds: {
  scope: keyVault
  name: guid(resourceGroup().id, principalId, roleDefinition.id)
  properties: {
    roleDefinitionId: roleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]
