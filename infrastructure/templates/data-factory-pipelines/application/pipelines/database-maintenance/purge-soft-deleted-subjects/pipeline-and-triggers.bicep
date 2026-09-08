@description('Name of the Data Factory instance that owns this pipeline.')
param dataFactoryName string

@description('Name of the Key Vault instance that owns secrets for this pipeline.')
param keyVaultName string

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
    slackAppToken: keyVault.getSecret('ees-alerts-slackapptoken')
    teamsPowerAutomateWebhookUrl: keyVault.getSecret('ees-alerts-teamswebhookurl')
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
