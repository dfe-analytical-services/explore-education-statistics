import { abbreviations } from '../common/abbreviations.bicep'

@description('Environment : Subscription name. Used as a prefix for created resources.')
param subscription string = ''

@description('Slack channel to post alerts to.')
param slackAlertsChannel string = ''

var resourceNames = {
  existingResources: {
    coreSqlServerName: '${subscription}-sqlsvr-ees-01'
    dataFactoryName: '${subscription}-${abbreviations.dataFactoryFactories}-ees-release'
    keyVaultName: '${subscription}-${abbreviations.keyVaultVaults}-ees-01'
  }
}

module linkedServicesModule 'application/configuration/linked-services.bicep' = {
  name: 'dataFactoryLinkedServicesModuleDeploy'
  params: {
    dataFactoryName: resourceNames.existingResources.dataFactoryName
  }
}

module privateEndpointsModule 'application/configuration/private-endpoints.bicep' = {
  name: 'dataFactoryPrivateEndpointsModuleDeploy'
  params: {
    dataFactoryName: resourceNames.existingResources.dataFactoryName
    coreSqlServerName: resourceNames.existingResources.coreSqlServerName
  }
}

module purgeSoftDeletedSubjectsPipelineModule 'application/pipelines/database-maintenance/purge-soft-deleted-subjects/pipeline-and-triggers.bicep' = {
  name: 'purgeSoftDeletedSubjectsPipelineAndTriggersModuleDeploy'
  params: {
    dataFactoryName: resourceNames.existingResources.dataFactoryName
    statisticsDbLinkedServiceName: linkedServicesModule.outputs.statisticsDbLinkedServiceName
    keyVaultName: resourceNames.existingResources.keyVaultName
    slackAlertsChannel: slackAlertsChannel
  }
}

module rebuildStatisticsIndexesPipelineModule 'application/pipelines/database-maintenance/rebuild-statistics-indexes/pipeline-and-triggers.bicep' = {
  name: 'rebuildStatisticsIndexesPipelineAndTriggersModuleDeploy'
  params: {
    dataFactoryName: resourceNames.existingResources.dataFactoryName
    statisticsDbLinkedServiceName: linkedServicesModule.outputs.statisticsDbLinkedServiceName
    keyVaultName: resourceNames.existingResources.keyVaultName
    slackAlertsChannel: slackAlertsChannel
  }
}
