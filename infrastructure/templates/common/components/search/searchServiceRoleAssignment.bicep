import { builtInRoleDefinitionIds } from '../../builtInRoles.bicep'

@export()
type SearchServiceRole = 'Search Index Data Contributor' | 'Search Index Data Reader' | 'Search Service Contributor'

@description('Specifies the name of the Search Service.')
param searchServiceName string

@description('Specifies the ids of the service principals that are being assigned the role')
param principalIds string[]

// A subset of allowed roles is supported here in accordance with role assignment conditions.
// See https://learn.microsoft.com/en-gb/azure/role-based-access-control/built-in-roles/ai-machine-learning for possible
// roles to support here, in conjunction with the limited set of roles that the deploying service
// principal is allowed to assign.
@description('Specifies the role to assign to the service principals')
param role SearchServiceRole

var rolesToRoleIds = {
  'Search Index Data Contributor': builtInRoleDefinitionIds.SearchIndexDataContributor
  'Search Index Data Reader': builtInRoleDefinitionIds.SearchIndexDataReader
  'Search Service Contributor': builtInRoleDefinitionIds.SearchServiceContributor
}

resource searchService 'Microsoft.Search/searchServices@2025-05-01' existing = {
  name: searchServiceName
}

// Create the role assignment. Note that this is dependent on the deploying service principal having
// "Microsoft.Authorization/roleAssignments/write" permissions on the Search Service via an appropriate
// role.
resource searchServiceRoleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in principalIds: {
  scope: searchService
  name: guid(searchService.id, principalId, rolesToRoleIds[role])
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', rolesToRoleIds[role])
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]
