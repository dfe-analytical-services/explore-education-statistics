@description('Resource prefix for all resources.')
param subscription string

@description('The Key Vault instance that holds secrets necessary for alerts.')
param keyVaultName string

@description('Resource Id of the Log Analytics Workspace to link the logic app to.')
param logAnalyticsWorkspaceId string

@description('Slack channel to post Azure alerts to.')
param slackAlertsChannel string

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

module alertsLogicAppModule 'alerts-logic-app.bicep' = {
  name: 'alertsLogicAppModuleDeploy'
  params: {
    subscription: subscription
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    slackAlertsChannel: slackAlertsChannel
    slackAppToken: keyVault.getSecret('ees-alerts-slackapptoken')
    teamsPowerAutomateWebhookUrl: keyVault.getSecret('ees-alerts-teamswebhookurl')
  }
}

module alertsActionGroupModule 'alerts-action-group.bicep' = {
  name: 'alertsActionGroupModuleDeploy'
  params: {
    subscription: subscription
    alertsLogicAppName: alertsLogicAppModule.outputs.alertsLogicAppName
  }
}
