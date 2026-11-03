@description('Name of the Data Factory instance that owns this pipeline.')
param dataFactoryName string

@description('Name of the Statistics database linked service.')
param statisticsDbLinkedServiceName string

@description('Name of the Key Vault instance that owns secrets for this pipeline.')
param keyVaultName string

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
    teamsPowerAutomateWebhookUrl: keyVault.getSecret('ees-alerts-teamswebhookurl')
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
