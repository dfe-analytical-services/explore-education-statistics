@description('Subscription name e.g. s101d01, passed to the logic app as a workflow parameter.')
param subscription string

@description('Name of the logic app.')
param alertsLogicAppName string

@description('Resource Id of the Log Analytics Workspace to link the logic app to.')
param logAnalyticsWorkspaceId string

@secure()
@description('The Power Automate Webhook URL used to post messages to Teams.')
param teamsPowerAutomateWebhookUrl string

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
      teamsPowerAutomateWebhookUrl: {
        type: 'securestring'
        value: teamsPowerAutomateWebhookUrl
      }
    }
    definition: loadJsonContent('alerts-logic-app-definition.json')
  }
}

resource alertsLogicAppDiagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  // Legacy name kept: renaming would add a second diagnostic setting rather than replace this one.
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
