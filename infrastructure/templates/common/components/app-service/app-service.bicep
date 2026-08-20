import { ConnectionString } from 'types.bicep'

@description('Name of the App Service.')
param appServiceName string

@description('Name of the App Insights instance that this App Service is connected to.')
param appInsightsName string

@description('Minimum TLS version supported.')
param minTlsVersion string

@description('The owning App Service Plan id.')
param appServicePlanId string

@description('Subnet used to connect the App Service to a VNet.')
param subnetRef string

@description('Database connection strings.')
param connectionStrings ConnectionString[]?

@description('Application-specific appsettings. These will be merged with infrastructure appsettings.')
param applicationAppSettings object

@description('Whether or not to display detailed error messages in this environment.')
param detailedErrors bool

@description('Whether or not to enable autoscaling in this environment.')
param autoscaleEnabled bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var deploySlotName = 'deploy'

resource appService 'Microsoft.Web/sites@2019-08-01' = {
  name: appServiceName
  location: resourceGroup().location
  identity: {
    type: 'SystemAssigned'
  }
  tags: union(tagValues, {
    ServiceType: 'App Service'
  })
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    clientAffinityEnabled: true
    siteConfig: {
      http20Enabled: true
      minTlsVersion: minTlsVersion
      ftpsState: 'FtpsOnly'
      netFrameworkVersion: 'v10.0'
      alwaysOn: true
      webSocketsEnabled: false
      remoteDebuggingEnabled: false
      httpLoggingEnabled: true
      detailedErrorLoggingEnabled: true
      requestTracingEnabled: true
      use32BitWorkerProcess: false
      connectionStrings: connectionStrings
    }
  }
}

resource virtualNetworkLink 'Microsoft.Web/sites/config@2018-11-01' = if (subnetRef != null) {
  parent: appService
  name: 'virtualNetwork'
  location: resourceGroup().location
  properties: {
    subnetResourceId: subnetRef
    swiftSupported: true
  }
}

resource appSettings 'Microsoft.Web/sites/config@2019-08-01' = {
  parent: appService
  name: 'appsettings'
  location: resourceGroup().location
  properties: union(applicationAppSettings, {
    APPINSIGHTS_INSTRUMENTATIONKEY: reference(
      resourceId('Microsoft.Insights/components', appInsightsName),
      '2020-02-02'
    ).InstrumentationKey
    AppInsights__InstrumentationKey: reference(
      resourceId('Microsoft.Insights/components', appInsightsName),
      '2020-02-02'
    ).InstrumentationKey
    WEBSITE_NODE_DEFAULT_VERSION: '22.23.1'
    WEBSITE_RUN_FROM_PACKAGE: '1'
    WEBSITE_LOAD_CERTIFICATES: '*'
    ASPNETCORE_DETAILEDERRORS: detailedErrors
  })
}

resource stagingSlot 'Microsoft.Web/sites/slots@2018-11-01' = {
  parent: appService
  name: deploySlotName
  kind: 'app'
  location: resourceGroup().location
  tags: tagValues
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    siteConfig: {
      http20Enabled: true
      minTlsVersion: minTlsVersion
      ftpsState: 'FtpsOnly'
      netFrameworkVersion: 'v10.0'
      alwaysOn: false
      webSocketsEnabled: false
      remoteDebuggingEnabled: false
      httpLoggingEnabled: true
      detailedErrorLoggingEnabled: true
      requestTracingEnabled: true
      use32BitWorkerProcess: false
    }
  }
}

resource deploySlotVirttualNetworkLink 'Microsoft.Web/sites/slots/config@2018-11-01' = if (subnetRef != null) {
  parent: stagingSlot
  name: 'virtualNetwork'
  location: resourceGroup().location
  properties: {
    subnetResourceId: subnetRef
    swiftSupported: true
  }
}

resource autoscaleSettings 'Microsoft.Insights/autoscaleSettings@2014-04-01' = {
  name: '${appServiceName}-autoscale'
  location: resourceGroup().location
  tags: {}
  properties: {
    name: '${appServiceName}-autoscale'
    enabled: autoscaleEnabled
    targetResourceUri: appServicePlanId
    profiles: [
      {
        name: 'Auto created scale condition'
        capacity: {
          minimum: 2
          maximum: 10
          default: 2
        }
        rules: [
          {
            scaleAction: {
              direction: 'Increase'
              type: 'ChangeCount'
              value: 1
              cooldown: 'PT5M'
            }
            metricTrigger: {
              metricName: 'CpuPercentage'
              metricNamespace: 'microsoft.web/serverfarms'
              metricResourceUri: appServicePlanId
              operator: 'GreaterThan'
              statistic: 'Average'
              threshold: 70
              timeAggregation: 'Average'
              timeGrain: 'PT1M'
              timeWindow: 'PT10M'
              dimensions: []
              dividePerInstance: false
            }
          }
          {
            scaleAction: {
              direction: 'Decrease'
              type: 'ChangeCount'
              value: 1
              cooldown: 'PT5M'
            }
            metricTrigger: {
              metricName: 'CpuPercentage'
              metricNamespace: 'microsoft.web/serverfarms'
              metricResourceUri: appServicePlanId
              operator: 'LessThan'
              statistic: 'Average'
              threshold: 30
              timeAggregation: 'Average'
              timeGrain: 'PT1M'
              timeWindow: 'PT10M'
              dimensions: []
              dividePerInstance: false
            }
          }
        ]
      }
    ]
    notifications: []
    targetResourceLocation: ''
  }
}
