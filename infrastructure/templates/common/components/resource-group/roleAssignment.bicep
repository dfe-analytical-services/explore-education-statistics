@export()
type ResourceGroupRole =
  | 'Reader'

@description('Specifies the ids of the service principals that are being assigned the role')
param principalIds string[]

// A subset of allowed roles is supported here in accordance with role assignment conditions.
// See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles for possible
// roles to support here, in conjunction with the limited set of roles that the deploying service
// principal is allowed to assign.
@description('Specifies the role to assign to the service principals')
param role ResourceGroupRole

var rolesToRoleIds = {
  Reader: 'acdd72a7-3385-48ef-bd42-f606fba81ae7'
}

// Create the role assignment. Note that this is dependent on the deploying service principal having
// "Microsoft.Authorization/roleAssignments/write" permissions on the Resource Group via an appropriate role.
resource psqlFlexibleServerRoleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in principalIds: {
  scope: resourceGroup()
  name: guid(resourceGroup().id, principalId, rolesToRoleIds[role])
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', rolesToRoleIds[role])
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]
