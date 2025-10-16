@export()
type PsqlFlexibleServerRole =
  | 'PostgreSQL Flexible Server Long Term Retention Backup Role'

@description('Specifies the name of the PostgreSQL Flexible Server.')
param psqlFlexibleServerName string

@description('Specifies the ids of the service principals that are being assigned the role')
param principalIds string[]

// A subset of allowed roles is supported here in accordance with role assignment conditions.
// See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles for possible
// roles to support here, in conjunction with the limited set of roles that the deploying service
// principal is allowed to assign.
@description('Specifies the role to assign to the service principals')
param role PsqlFlexibleServerRole

var rolesToRoleIds = {
  'PostgreSQL Flexible Server Long Term Retention Backup Role': 'c088a766-074b-43ba-90d4-1fb21feae531'
}

resource psqlFlexibleServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' existing = {
  name: psqlFlexibleServerName
}

// Create the role assignment. Note that this is dependent on the deploying service principal having
// "Microsoft.Authorization/roleAssignments/write" permissions on the PostgreSQL Flexible Server
// instance via an appropriate role.
resource psqlFlexibleServerRoleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in principalIds: {
  scope: psqlFlexibleServer
  name: guid(psqlFlexibleServer.id, principalId, rolesToRoleIds[role])
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', rolesToRoleIds[role])
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]
