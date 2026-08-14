import { abbreviations } from '../../../common/abbreviations.bicep'
import { staticTotalGreaterThanZero } from '../../../common/components/alerts/staticAlertConfig.bicep'

@description('Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string

@description('Name of the Key Vault instance to add as a linked service.')
param keyVaultName string

@description('Name of the Action Group to use for handling alerts raised for this Data Factory instance')
param alertsGroupName string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param deployAlerts bool

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

var dataFactoryName = '${subscription}-${abbreviations.dataFactoryFactories}-ees-release'

resource dataFactory 'Microsoft.DataFactory/factories@2018-06-01' = {
  name: dataFactoryName
  location: resourceGroup().location
  identity: {
    type: 'SystemAssigned'
  }
  tags: tagValues
}

resource managedVNet 'Microsoft.DataFactory/factories/managedVirtualNetworks@2018-06-01' = {
  parent: dataFactory
  name: 'default'
  properties: {}
}

resource vnetIntegrationRuntime 'Microsoft.DataFactory/factories/integrationRuntimes@2018-06-01' = {
  parent: dataFactory
  name: 'vnetIntegrationRuntime'
  properties: {
    type: 'Managed'
    typeProperties: {
      computeProperties: {
        location: resourceGroup().location
        dataFlowProperties: {
          computeType: 'General'
          coreCount: 8
          timeToLive: 10
          cleanup: false
        }
      }
    }
    managedVirtualNetwork: {
      type: 'ManagedVirtualNetworkReference'
      referenceName: managedVNet.name
    }
  }
}

resource keyVaultLinkedService 'Microsoft.DataFactory/factories/linkedServices@2018-06-01' = {
  name: 'AzureKeyVault'
  parent: dataFactory
  properties: {
    annotations: []
    type: 'AzureKeyVault'
    typeProperties: {
      baseUrl: 'https://${keyVaultName}.${environment().suffixes.keyvaultDns}/'
    }
  }
}

module activityFailuresAlertModule '../../../common/components/alerts/staticMetricAlert.bicep' = if (deployAlerts) {
  name: '${dataFactoryName}ActivityFailuresAlertModuleDeploy'
  params: {
    resourceName: dataFactoryName
    fullDescription: 'Data Factory - Activity Failures'
    resourceMetric: {
      resourceType: 'Microsoft.DataFactory/factories'
      metric: 'ActivityFailedRuns'
    }
    config: {
      ...staticTotalGreaterThanZero
      nameSuffix: 'DataFactoryActivityFailures'
      severity: 'Error'
    }
    alertsGroupName: alertsGroupName
    tagValues: tagValues
  }
  dependsOn: [
    dataFactory
  ]
}
