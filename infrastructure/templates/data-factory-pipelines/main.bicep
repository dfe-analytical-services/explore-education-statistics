import { getResourceNamesForEnvironment } from '../common/resource-names.bicep'

@description('Environment : Subscription name. Used as a prefix for created resources.')
param subscription string = ''

@description('Environment : Name of this environment e.g. Development, Test.')
param environmentName string

@description('Whether to use the naming convention for the DfE Enterprise Landing Zone (ELZ) subscriptions. Set to true by the param files in "parameters/elz".')
param elz bool = false

var resourceNames = getResourceNamesForEnvironment({
  environmentIdentifier: subscription
  environmentName: environmentName
  elz: elz
})

module linkedServicesModule 'application/configuration/linked-services.bicep' = {
  name: 'dataFactoryLinkedServicesModuleDeploy'
  params: {
    dataFactoryName: resourceNames.dataFactory
  }
}

module privateEndpointsModule 'application/configuration/private-endpoints.bicep' = {
  name: 'dataFactoryPrivateEndpointsModuleDeploy'
  params: {
    dataFactoryName: resourceNames.dataFactory
    coreSqlServerName: resourceNames.databases.coreSqlServer
  }
}

module purgeSoftDeletedSubjectsPipelineModule 'application/pipelines/database-maintenance/purge-soft-deleted-subjects/pipeline-and-triggers.bicep' = {
  name: 'purgeSoftDeletedSubjectsPipelineAndTriggersModuleDeploy'
  params: {
    dataFactoryName: resourceNames.dataFactory
    statisticsDbLinkedServiceName: linkedServicesModule.outputs.statisticsDbLinkedServiceName
    keyVaultName: resourceNames.keyVault.keyVault
  }
}

module rebuildStatisticsIndexesPipelineModule 'application/pipelines/database-maintenance/rebuild-statistics-indexes/pipeline-and-triggers.bicep' = {
  name: 'rebuildStatisticsIndexesPipelineAndTriggersModuleDeploy'
  params: {
    dataFactoryName: resourceNames.dataFactory
    statisticsDbLinkedServiceName: linkedServicesModule.outputs.statisticsDbLinkedServiceName
    keyVaultName: resourceNames.keyVault.keyVault
  }
}
