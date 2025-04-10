import { abbreviations } from '../../common/abbreviations.bicep'
import { ResourceNames } from '../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Resource prefix for all resources.')
param resourcePrefix string

// This is the built-in EventGrid Data Sender role. See https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#integration
var eventGridDataSenderRoleDefinitionId = 'd5a91429-5739-47e2-a06b-3470a27159e7'

resource publisherFunction 'Microsoft.Web/sites@2024-04-01' existing = {
  name: resourceNames.existingResources.publisherFunction
}

var topicNames = [
  'release-version-changed'
]

module publisherTopicRoleAssignmentModuleDeploy '../../common/components/event-grid/eventGridTopicRoleAssignment.bicep' = [
  for (topicName, index) in topicNames: {
    name: 'publisherTopicRoleAssignmentModuleDeploy-${index}'
    params: {
      principalId: publisherFunction.identity.principalId
      roleDefinitionId: eventGridDataSenderRoleDefinitionId
      topicName: '${resourcePrefix}-${abbreviations.eventGridTopics}-${topicName}'
    }
  }
]
