import { dynamicCountGreaterThan } from 'alerts/dynamicAlertConfig.bicep'

@description('Specifies the location for all resources.')
param location string

@description('Specifies the Application Insights name')
param appInsightsName string

@description('Whether to create or update Azure Monitor alerts during this deploy')
param alerts {
  exceptionCount: bool
  exceptionServerCount: bool
  failedRequests: bool
  alertsGroupName: string
}?

@description('Tags for the resources')
param tagValues object

var kind = 'web'

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: kind
  properties: {
    Application_Type: kind
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
  tags: tagValues
}

module exceptionCountAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.exceptionCount) {
  name: '${appInsightsName}ExceptionCountDeploy'
  params: {
    resourceName: appInsightsName
    resourceMetric: {
      resourceType: 'Microsoft.Insights/components'
      metric: 'exceptions/count'
    }
    config: {
      ...dynamicCountGreaterThan
      nameSuffix: 'exception-count'
      windowSize: 'PT30M'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
  dependsOn: [
    applicationInsights
  ]
}

module exceptionServerCountAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.exceptionServerCount) {
  name: '${appInsightsName}ExceptionServerCountDeploy'
  params: {
    resourceName: appInsightsName
    resourceMetric: {
      resourceType: 'Microsoft.Insights/components'
      metric: 'exceptions/server'
    }
    config: {
      ...dynamicCountGreaterThan
      nameSuffix: 'server-exception-count'
      windowSize: 'PT30M'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
  dependsOn: [
    applicationInsights
  ]
}

module failedRequestsAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.failedRequests) {
  name: '${appInsightsName}FailedRequestsDeploy'
  params: {
    resourceName: appInsightsName
    resourceMetric: {
      resourceType: 'Microsoft.Insights/components'
      metric: 'requests/failed'
    }
    config: {
      ...dynamicCountGreaterThan
      nameSuffix: 'failed-requests'
      windowSize: 'PT30M'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
  dependsOn: [
    applicationInsights
  ]
}

output applicationInsightsKey string = applicationInsights.properties.InstrumentationKey
output applicationInsightsConnectionString string = applicationInsights.properties.ConnectionString
output applicationInsightsName string = applicationInsights.name
