@description('A Container Registry-specific role to assign')
@allowed([
  'AcrPull'
])
param role string

@description('The name of an existing Container Registry to be use as the scope of the role assignment')
param containerRegistryName string

@description('The name of the Managed Identity that the role will be assigned to')
param managedIdentityName string

// See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#containers for Container-specific built in role ids.
var rolesToRoleIds = {
  AcrPull: '7f951dda-4ed3-4680-a7ca-43fe172d538d'
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: containerRegistryName
}

resource managedIdentitty 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: managedIdentityName
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, rolesToRoleIds[role], managedIdentitty.name)
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', rolesToRoleIds[role])
    principalId: managedIdentitty.properties.principalId
    principalType: 'ServicePrincipal'
  }
}
