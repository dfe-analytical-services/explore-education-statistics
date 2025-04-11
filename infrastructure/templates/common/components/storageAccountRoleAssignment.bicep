import { StorageAccountRole } from '../types.bicep'

@description('Specifies the name of the Storage Account.')
param storageAccountName string

@description('Specifies the ids of the service principals that are being assigned the role')
param principalIds string[]

// A subset of allowed roles is supported here in accordance with role assignment conditions.
// See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles for possible
// roles to support here, in conjunction with the limited set of roles that the deploying service
// principal is allowed to assign.
@description('Specifies the role to assign to the service principals')
param role StorageAccountRole

var rolesToRoleIds = {
  'Storage Blob Data Contributor': 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
  'Storage Blob Data Owner': 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
  'Storage Blob Data Reader': '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
  'Storage Queue Data Contributor': '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

// Create the role assignment. Note that this is dependent on the deploying service principal having
// "Microsoft.Authorization/roleAssignments/write" permissions on the Storage Account via an appropriate
// role.
resource storageAccountRoleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in principalIds: {
  scope: storageAccount
  name: guid(storageAccount.id, principalId, rolesToRoleIds[role])
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', rolesToRoleIds[role])
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]
