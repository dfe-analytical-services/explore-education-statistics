@description('The name of the Function App to which the diagnostic setting will be applied.')
param functionAppName string

@description('The name of the diagnostic setting e.g. Send all logs and metrics to Log Analytics.')
param diagnosticSettingName string

@description('The id of the Log Analytics workspace to send logs and/or metrics to or null if Log Analytics is not a destination.')
param logAnalyticsWorkspaceId string?

@description('The categories of logs to enable.')
@allowed([
  'FunctionAppLogs'
  'AppServiceAuthenticationLogs'
])
param logCategoriesToEnable array = [
  'FunctionAppLogs'
  'AppServiceAuthenticationLogs'
]

@description('The categories of metrics to enable.')
@allowed([
  'AllMetrics'
])
param metricCategoriesToEnable array = [
  'AllMetrics'
]

var logSettings = [
  for category in logCategoriesToEnable: {
    category: category
    enabled: true
  }
]

var metricSettings = [
  for metric in metricCategoriesToEnable: {
    category: metric
    enabled: true
  }
]

resource functionApp 'Microsoft.Web/sites@2024-04-01' existing = {
  name: functionAppName
}

resource diagnosticSetting 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: diagnosticSettingName
  scope: functionApp
  properties: {
    logAnalyticsDestinationType: null
    logs: logSettings
    metrics: metricSettings
    workspaceId: logAnalyticsWorkspaceId
  }
}
