@description('Resource prefix for all resources.')
param subscription string

@description('The Key Vault instance that holds secrets necessary for alerts.')
param keyVaultName string

@description('Resource Id of the Log Analytics Workspace to link the logic app to.')
param logAnalyticsWorkspaceId string

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

module alertsLogicAppModule 'alerts-logic-app.bicep' = {
  name: 'alertsLogicAppModuleDeploy'
  params: {
    subscription: subscription
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    slackAlertsConfig: keyVault.getSecret('ees-alerts-slackconfig')
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

output actionGroupName string = alertsActionGroupModule.outputs.actionGroupName
