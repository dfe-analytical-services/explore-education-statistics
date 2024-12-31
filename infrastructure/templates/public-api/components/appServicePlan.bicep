@description('Specifies the App Service plan name')
param planName string

@description('Specifies the location for all resources.')
param location string

@description('The SKU for the plan')
param sku object

@description('The kind of plan to deploy. Impacted by application type and desired OS. See https://github.com/Azure/app-service-linux-docs/blob/master/Things_You_Should_Know/kind_property.md#app-service-resource-kind-reference.')
param kind 
  | 'app'
  | 'functionapp'

@description('Function App Plan : operating system')
param operatingSystem 'Windows' | 'Linux' = 'Linux'

@description('Whether to create or update Azure Monitor alerts during this deploy')
param alerts {
  cpuPercentage: bool
  memoryPercentage: bool
  alertsGroupName: string
}?

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: planName
  location: location
  kind: kind
  sku: sku
  properties: {
    reserved: operatingSystem == 'Linux'
  }
  tags: tagValues
}

module cpuPercentageAlert 'alerts/appServicePlans/cpuPercentageAlert.bicep' = if (alerts != null && alerts!.cpuPercentage) {
  name: '${planName}CpuPercentageDeploy'
  params: {
    resourceName: planName
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

module memoryPercentageAlert 'alerts/appServicePlans/memoryPercentageAlert.bicep' = if (alerts != null && alerts!.memoryPercentage) {
  name: '${planName}MemoryPercentageDeploy'
  params: {
    resourceName: planName
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

output planId string = appServicePlan.id
