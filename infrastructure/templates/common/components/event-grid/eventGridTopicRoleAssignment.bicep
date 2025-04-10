@description('Specifies the id of the principal to which the role is assigned.')
param principalId string

@description('Specifies whether the principal is a user, a group, or a service principal.')
@allowed([
  'User'
  'Group'
  'ServicePrincipal'
])
param principalType string = 'ServicePrincipal'

@description('Specifies the role definition id\'s to assign to the principal.')
param roleDefinitionId string

@description('Specifies the name of the Event Grid topic to which the role assignment is scoped.')
param topicName string

resource topic 'Microsoft.EventGrid/topics@2025-02-15' existing = {
  name: topicName
}

resource roleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: roleDefinitionId
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: topic
  name: guid(topic.id, principalId, roleDefinition.id)
  properties: {
    principalId: principalId
    principalType: principalType
    roleDefinitionId: roleDefinition.id
  }
}
