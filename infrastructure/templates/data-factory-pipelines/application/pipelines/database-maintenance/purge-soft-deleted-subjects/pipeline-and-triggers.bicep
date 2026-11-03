@description('Name of the Data Factory instance that owns this pipeline.')
param dataFactoryName string

@description('Name of the Key Vault instance that owns secrets for this pipeline.')
param keyVaultName string

@description('Name of the Key Vault secret holding the Slack app token that alerts are posted with.')
param slackAppTokenSecretName string

@description('Name of the Key Vault secret holding the Teams Power Automate webhook URL that alerts are posted to.')
param teamsWebhookUrlSecretName string

@description('Name of the Statistics database linked service.')
param statisticsDbLinkedServiceName string

@description('Slack channel to post Azure alerts to.')
param slackAlertsChannel string

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

var removeSoftDeletedSubjectsObservationLimit = 20000000
var removeSoftDeletedSubjectsObservationCommitBatchSize = 500000
var removeSoftDeletedSubjectsObservationFilterItemCommitBatchSize = 500000

module pipelineModule 'pipeline.bicep' = {
  name: 'purgeSoftDeletedSubjectsPipelineModuleDeploy'
  params: {
    dataFactoryName: dataFactoryName
    statisticsDbLinkedServiceName: statisticsDbLinkedServiceName
    slackAlertsChannel: slackAlertsChannel
    slackAppToken: keyVault.getSecret(slackAppTokenSecretName)
    teamsPowerAutomateWebhookUrl: keyVault.getSecret(teamsWebhookUrlSecretName)
    removeSoftDeletedSubjectsObservationLimit: removeSoftDeletedSubjectsObservationLimit
    removeSoftDeletedSubjectsObservationCommitBatchSize: removeSoftDeletedSubjectsObservationCommitBatchSize
    removeSoftDeletedSubjectsObservationFilterItemCommitBatchSize: removeSoftDeletedSubjectsObservationFilterItemCommitBatchSize
  }
}

module triggersModule 'triggers.bicep' = {
  name: 'purgeSoftDeletedSubjectsTriggersModuleDeploy'
  params: {
    dataFactoryName: dataFactoryName
    pipelineName: pipelineModule.outputs.pipelineName
    removeSoftDeletedSubjectsObservationLimit: removeSoftDeletedSubjectsObservationLimit
    removeSoftDeletedSubjectsObservationCommitBatchSize: removeSoftDeletedSubjectsObservationCommitBatchSize
    removeSoftDeletedSubjectsObservationFilterItemCommitBatchSize: removeSoftDeletedSubjectsObservationFilterItemCommitBatchSize
  }
}
