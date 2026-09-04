import { abbreviations } from '../../../common/abbreviations.bicep'

@description('Resource prefix for all resources.')
param subscription string

@description('Resource Id of the Log Analytics Workspace to link the logic app to.')
param logAnalyticsWorkspaceId string

@description('Slack channel to post Azure alerts to.')
param slackAlertsChannel string

@secure()
@description('Token to securely post to the Slack channel.')
param slackAppToken string

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
      slackAlertsChannel: {
        type: 'string'
        value: slackAlertsChannel
      }
      slackAppToken: {
        type: 'securestring'
        value: slackAppToken
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
