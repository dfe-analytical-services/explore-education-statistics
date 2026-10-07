@description('Name of the Azure Front Door profile.')
param frontDoorProfileName string

@description('Name of the Azure Front Door endpoint to grant purge access to.')
param frontDoorEndpointName string

@description('Principal ID of the service that purges the endpoint cache.')
param principalId string

@description('Resource ID used for the stable role-assignment name.')
param principalResourceId string

// CDN Endpoint Contributor only covers classic CDN endpoints, not afdEndpoints.
var cdnProfileContributorRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'ec156ff8-a8d1-4d15-830c-5b80698ca432'
)

resource frontDoor 'Microsoft.Cdn/profiles@2025-04-15' existing = {
  name: frontDoorProfileName
}

resource endpoint 'Microsoft.Cdn/profiles/afdendpoints@2025-04-15' existing = {
  parent: frontDoor
  name: frontDoorEndpointName
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(endpoint.id, principalResourceId, cdnProfileContributorRoleDefinitionId)
  scope: endpoint
  properties: {
    roleDefinitionId: cdnProfileContributorRoleDefinitionId
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
