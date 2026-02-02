import { KeyVaultRole } from 'types.bicep'

@description('Specifies the name of the Key Vault.')
param keyVaultName string

@description('Specifies the ids of the service principals that are being assigned the role')
param principalIds string[]

// A subset of allowed roles is supported here in accordance with role assignment conditions.
// See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles for possible
// roles to support here, in conjunction with the limited set of roles that the deploying service
// principal is allowed to assign.
@description('Specifies the Key Vault role to assign to the service principals')
param role KeyVaultRole

var rolesToRoleIds = {
  'Secrets User': '4633458b-17de-408a-b874-0445c86b69e6'
  'Certificate User': 'db79e9a7-68ee-4b58-9aeb-b90e7c24fcba'
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Create the role assignment. Note that this is dependent on the deploying service principal having
// "Microsoft.Authorization/roleAssignments/write" permissions on the Key Vault via an appropriate
// role.
resource keyVaultRoleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in principalIds: {
  scope: keyVault
  name: guid(resourceGroup().id, principalId, rolesToRoleIds[role])
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', rolesToRoleIds[role])
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]
