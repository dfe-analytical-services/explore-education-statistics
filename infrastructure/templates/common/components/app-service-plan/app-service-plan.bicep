import { cpuPercentageConfig, memoryPercentageConfig } from '../alerts/dynamicAlertConfig.bicep'
import { AppServicePlanSku } from 'types.bicep'

@description('Specifies the App Service plan name')
param planName string

@description('Specifies the location for all resources.')
param location string = resourceGroup().location

@description('The SKU for the plan')
param sku AppServicePlanSku

@description('The operating system to use to host App Services.')
param operatingSystem 'Windows' | 'Linux' = 'Linux'

@description('The kind of plan to create. Use "app,linux,container" and "Linux" for the "operatingSystem" param for App Services for Docker.')
param kind 'app' | 'app,linux,container' = 'app'

@description('Whether to create or update Azure Monitor alerts during this deploy')
param alerts {
  cpuPercentage: bool
  memoryPercentage: bool
  alertsGroupName: string
}?

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  sku: sku
  name: planName
  kind: kind
  location: location
  tags: union(
    tagValues,
    {
      ServiceType: 'App Service plan'
    }
  ) 
  properties: {
    reserved: operatingSystem == 'Linux'
  }
}

module cpuPercentageAlert '../alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.cpuPercentage) {
  name: '${planName}CpuPercentageDeploy'
  params: {
    resourceName: planName
    resourceMetric: {
      resourceType: 'Microsoft.Web/serverfarms'
      metric: 'CpuPercentage'
    }
    config: cpuPercentageConfig
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
  dependsOn: [
    appServicePlan
  ]
}

module memoryPercentageAlert '../alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.memoryPercentage) {
  name: '${planName}MemoryPercentageDeploy'
  params: {
    resourceName: planName
    resourceMetric: {
      resourceType: 'Microsoft.Web/serverfarms'
      metric: 'MemoryPercentage'
    }
    config: memoryPercentageConfig
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
  dependsOn: [
    appServicePlan
  ]
}

output planId string = appServicePlan.id
