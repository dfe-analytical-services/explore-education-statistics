import { abbreviations } from '../../common/abbreviations.bicep'
import { ResourceNames } from '../types.bicep'

@description('Specifies common resource naming variables.')
param resourceNames ResourceNames

@description('Resource prefix for all resources.')
param resourcePrefix string

// This is the built-in EventGrid Data Sender role. See https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#integration
var eventGridDataSenderRoleDefinitionId = 'd5a91429-5739-47e2-a06b-3470a27159e7'

resource adminAppService 'Microsoft.Web/sites@2024-04-01' existing = {
  name: resourceNames.existingResources.adminApp
}

var topicNames = [
  'publication-changed'
  'release-changed'
  'theme-changed'
]

module adminAppTopicRoleAssignmentModuleDeploy '../../common/components/event-grid/eventGridTopicRoleAssignment.bicep' = [
  for (topicName, index) in topicNames: {
    name: 'adminAppTopicRoleAssignmentModuleDeploy-${index}'
    params: {
      principalId: adminAppService.identity.principalId
      roleDefinitionId: eventGridDataSenderRoleDefinitionId
      topicName: '${resourcePrefix}-${abbreviations.eventGridTopics}-${topicName}'
    }
  }
]
