import { containerRegistryRoleType } from '../types.bicep'

@description('Specifies the name of an existing Container Registry to be use as the scope of the role assignment')
param containerRegistryName string

@description('Specifies the id of the service principals that should inherit this Key Vault policy')
param principalIds string[]

// A subset of allowed roles is supported here in accordance with role assignment conditions.
// See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles for possible
// roles to support here, in conjunction with the limited set of roles that the deploying service
// principal is allowed to assign.
@description('The Container Registry-specific role to assign')
param role containerRegistryRoleType

// See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#containers for Container-specific built in role ids.
var rolesToRoleIds = {
  AcrPull: '7f951dda-4ed3-4680-a7ca-43fe172d538d'
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: containerRegistryName
}

// Create the role assignment. Note that this is dependent on the deploying service principal having
// "Microsoft.Authorization/roleAssignments/write" permissions on the Container Registry via an appropriate
// role.
resource roleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in principalIds: {
  name: guid(containerRegistry.id, principalId, rolesToRoleIds[role])
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', rolesToRoleIds[role])
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]
