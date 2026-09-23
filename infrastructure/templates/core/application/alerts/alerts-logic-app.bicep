import { abbreviations } from '../../../common/abbreviations.bicep'

@description('Resource prefix for all resources.')
param subscription string

@description('Resource Id of the Log Analytics Workspace to link the logic app to.')
param logAnalyticsWorkspaceId string

@secure()
@description('''
JSON array of the Slack workspaces to post alerts to, each entry taking the form
`{ "channels": ["<channel id>"], "authToken": "<app token>" }`. Held in the `ees-alerts-slackconfig`
Key Vault secret so that each workspace's channels travel with the token that can post to them.
''')
param slackAlertsConfig string

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
      slackAlertsConfig: {
        type: 'securestring'
        value: slackAlertsConfig
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
