@description('Name of the Data Factory instance that owns this pipeline.')
param dataFactoryName string

@description('Name of the Statistics database linked service.')
param statisticsDbLinkedServiceName string

@description('Name of the Key Vault instance that owns secrets for this pipeline.')
param keyVaultName string

@description('Name of the Key Vault secret holding the Slack app token that alerts are posted with.')
param slackAppTokenSecretName string

@description('Name of the Key Vault secret holding the Teams Power Automate webhook URL that alerts are posted to.')
param teamsWebhookUrlSecretName string

@description('Slack channel to post Azure alerts to.')
param slackAlertsChannel string

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

param fragmentationTables string = 'Observation,ObservationFilterItem'

module pipelineModule 'pipeline.bicep' = {
  name: 'rebuildStatisticsIndexesPipelineModuleDeploy'
  params: {
    dataFactoryName: dataFactoryName
    statisticsDbLinkedServiceName: statisticsDbLinkedServiceName
    fragmentationTables: fragmentationTables
    slackAlertsChannel: slackAlertsChannel
    slackAppToken: keyVault.getSecret(slackAppTokenSecretName)
    teamsPowerAutomateWebhookUrl: keyVault.getSecret(teamsWebhookUrlSecretName)
  }
}

module triggersModule 'triggers.bicep' = {
  name: 'rebuildStatisticsIndexesTriggersModuleDeploy'
  params: {
    dataFactoryName: dataFactoryName
    pipelineName: pipelineModule.outputs.pipelineName
    fragmentationTables: fragmentationTables
  }
}
