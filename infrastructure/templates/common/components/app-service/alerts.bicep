import { staticAverageLessThanHundred, staticMinGreaterThanZero } from '../alerts/staticAlertConfig.bicep'
import { dynamicAverageGreaterThan } from '../alerts/dynamicAlertConfig.bicep'

@description('Name of the App Service to connect these alerts to.')
param appServiceName string

@description('Azure Monitor alerts to add to the App Service.')
param alerts {
  appServiceHealth: bool
  httpErrors: bool
  alertsGroupName: string
}

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

module healthAlert '../alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.appServiceHealth) {
  name: '${appServiceName}HealthAlertModule'
  params: {
    resourceName: appServiceName
    resourceMetric: {
      resourceType: 'Microsoft.Web/sites'
      metric: 'HealthCheckStatus'
    }
    config: {
      ...staticAverageLessThanHundred
      nameSuffix: 'health'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
}

var unexpectedHttpStatusCodeMetrics = ['Http401', 'Http5xx']

module unexpectedHttpStatusCodeAlerts '../alerts/staticMetricAlert.bicep' = [
  for httpStatusCode in unexpectedHttpStatusCodeMetrics: if (alerts != null && alerts!.httpErrors) {
    name: '${appServiceName}${httpStatusCode}Module'
    params: {
      resourceName: appServiceName
      resourceMetric: {
        resourceType: 'Microsoft.Web/sites'
        metric: httpStatusCode
      }
      config: {
        ...staticMinGreaterThanZero
        nameSuffix: toLower(httpStatusCode)
      }
      alertsGroupName: alerts!.alertsGroupName
      tagValues: tagValues
    }
  }
]

var expectedHttpStatusCodeMetrics = ['Http403', 'Http4xx']

module expectedHttpStatusCodeAlerts '../alerts/dynamicMetricAlert.bicep' = [
  for httpStatusCode in expectedHttpStatusCodeMetrics: if (alerts != null && alerts!.httpErrors) {
    name: '${appServiceName}${httpStatusCode}Module'
    params: {
      resourceName: appServiceName
      resourceMetric: {
        resourceType: 'Microsoft.Web/sites'
        metric: httpStatusCode
      }
      config: {
        ...dynamicAverageGreaterThan
        nameSuffix: toLower(httpStatusCode)
        severity: 'Informational'
      }
      alertsGroupName: alerts!.alertsGroupName
      tagValues: tagValues
    }
  }
]
