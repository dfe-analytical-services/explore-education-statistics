import { abbreviations } from '../../../common/abbreviations.bicep'

@description('Resource prefix for all resources.')
param subscription string

@description('Resource Id of the Log Analytics Workspace to link the logic app to.')
param logAnalyticsWorkspaceId string

@description('Slack channels in the primary workspace to post Azure alerts to.')
param slackAlertsChannels array

@description('Slack channels in the secondary workspace to post Azure alerts to.')
param secondarySlackAlertsChannels array

@secure()
@description('Token to securely post to the primary workspace Slack channels.')
param slackAppToken string

@secure()
@description('Token to securely post to the secondary workspace Slack channels.')
param secondarySlackAppToken string

@secure()
@description('The Power Automate Webhook URL used to post messages to Teams.')
param teamsPowerAutomateWebhookUrl string

var alertsLogicAppName = '${subscription}-${abbreviations.logicWorkflows}-ees-slackwebhook'

resource alertsLogicApp 'Microsoft.Logic/workflows@2019-05-01' = {
  name: alertsLogicAppName
  location: resourceGroup().location
  properties: {
    state: 'Enabled'
    parameters: {
      subscription: {
        type: 'string'
        value: subscription
      }
      resourceGroup: {
        type: 'string'
        value: resourceGroup().name
      }
      slackAlertsChannels: {
        type: 'array'
        value: slackAlertsChannels
      }
      secondarySlackAlertsChannels: {
        type: 'array'
        value: secondarySlackAlertsChannels
      }
      slackAppToken: {
        type: 'securestring'
        value: slackAppToken
      }
      secondarySlackAppToken: {
        type: 'securestring'
        value: secondarySlackAppToken
      }
      teamsPowerAutomateWebhookUrl: {
        type: 'securestring'
        value: teamsPowerAutomateWebhookUrl
      }
    }
    definition: loadJsonContent('alerts-logic-app-definition.json')
  }
}

resource alertsLogicAppDiagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'Slack webhook logic app diagnostic setting'
  scope: alertsLogicApp
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [
      {
        category: 'WorkflowRuntime'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

output alertsLogicAppName string = alertsLogicAppName
