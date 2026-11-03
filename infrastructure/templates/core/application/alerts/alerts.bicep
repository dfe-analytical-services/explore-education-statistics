@description('Subscription name e.g. s101d01, passed through to the alerts logic app.')
param subscription string

@description('Name of the Action Group that receives alerts.')
param actionGroupName string

@description('Name of the logic app that forwards alerts to Slack and Teams.')
param alertsLogicAppName string

@description('The Key Vault instance that holds secrets necessary for alerts.')
param keyVaultName string

@description('Name of the Key Vault secret holding the Slack channels and app token that alerts are posted with.')
param slackAlertsConfigSecretName string

@description('Name of the Key Vault secret holding the Teams Power Automate webhook URL that alerts are posted to.')
param teamsWebhookUrlSecretName string

@description('Resource Id of the Log Analytics Workspace to link the logic app to.')
param logAnalyticsWorkspaceId string

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

module alertsLogicAppModule 'alerts-logic-app.bicep' = {
  name: 'alertsLogicAppModuleDeploy'
  params: {
    subscription: subscription
    alertsLogicAppName: alertsLogicAppName
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    slackAlertsConfig: keyVault.getSecret(slackAlertsConfigSecretName)
    teamsPowerAutomateWebhookUrl: keyVault.getSecret(teamsWebhookUrlSecretName)
  }
}

module alertsActionGroupModule 'alerts-action-group.bicep' = {
  name: 'alertsActionGroupModuleDeploy'
  params: {
    actionGroupName: actionGroupName
    alertsLogicAppName: alertsLogicAppModule.outputs.alertsLogicAppName
  }
}

output actionGroupName string = alertsActionGroupModule.outputs.actionGroupName
