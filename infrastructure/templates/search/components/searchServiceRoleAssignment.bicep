import { SearchServiceRole } from '../types.bicep'

@description('Specifies the name of the Search Service.')
param searchServiceName string

@description('Specifies the ids of the service principals that are being assigned the role')
param principalIds string[]

// A subset of allowed roles is supported here in accordance with role assignment conditions.
// See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles for possible
// roles to support here, in conjunction with the limited set of roles that the deploying service
// principal is allowed to assign.
@description('Specifies the role to assign to the service principals')
param role SearchServiceRole

var rolesToRoleIds = {
  'Search Index Data Contributor': '8ebe5a00-799e-43f5-93ac-243d3dce84a7'
  'Search Index Data Reader': '1407120a-92aa-4202-b7e9-c0e197c71c8f'
  'Search Service Contributor': '7ca78c08-252a-4471-8644-bb5ff32d4ba0'
}

resource searchService 'Microsoft.Search/searchServices@2024-06-01-preview' existing = {
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
