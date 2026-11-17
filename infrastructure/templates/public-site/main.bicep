import { HttpStatusAlertConfig } from '../common/components/alerts/httpStatusAlertConfig.bicep'
import { ResourceNames } from '../common/resource-names.bicep'
import { AppServicePlanSku } from '../common/components/app-service-plan/types.bicep'
import { builtInRoleDefinitionIds } from '../common/builtInRoles.bicep'

@description('Names of resources in this deploy.')
param resourceNames ResourceNames

@description('Minimum TLS version supported.')
param minTlsVersion string

@description('App Service Plan SKU.')
param appServiceSku AppServicePlanSku

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@description('Whether to display detailed error messages in this environment or not.')
param detailedErrors bool

@description('Whether or not to enable autoscaling of App Services in this environment.')
param autoscaleAppServices bool

@description('Whether to spread the App Service plan\'s instances across availability zones.')
param zoneRedundant bool

@description('The origins supported for CORS calls to this App Service.')
param allowedOrigins string[]

@description('Whether or not to deploy Azure Metric alerts.')
param deployAlerts bool

@description('Optional override of the existing Http401 rule. Omit to preserve current behaviour.')
param http401Alert HttpStatusAlertConfig?

@description('Optional override of the existing Http5xx rule. Omit to preserve current behaviour.')
param http5xxAlert HttpStatusAlertConfig?

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

@secure()
@description('''
The existing appsettings for the production slot, fetched by the pipeline before deployment. Used to
prevent infrastructure deploys from overriding application-specific appsettings back to their original values.
''')
param existingProdAppSettings object = {}

module appServicePlanModule '../common/components/app-service-plan/app-service-plan.bicep' = {
  name: 'publicSiteAppServicePlanModule'
  params: {
    planName: resourceNames.publicSite.appServicePlan
    sku: appServiceSku
    zoneRedundant: zoneRedundant
    kind: 'app,linux,container'
    operatingSystem: 'Linux'
    alerts: deployAlerts ? {
      alertsGroupName: resourceNames.alertsGroup
      cpuPercentage: true
      memoryPercentage: true
    } : null
    tagValues: tagValues
  }
}

module appInsightsModule '../common/components/monitoring/appInsights.bicep' = {
  name: 'publicSiteAppInsightsModuleDeploy'
  params: {
    appInsightsName: resourceNames.publicSite.appInsights
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    alerts: deployAlerts ? {
      alertsGroupName: resourceNames.alertsGroup
      exceptionCount: true
      exceptionServerCount: true
      failedRequests: true
    } : null
    tagValues: tagValues
  }
}

module appServiceModule '../common/components/app-service/app-service.bicep' = {
  name: 'publicSiteAppServiceModuleDeploy'
  params: {
    appServiceName: resourceNames.publicSite.appService
    kind: 'app,linux,container'
    operatingSystem: 'Linux'
    minTlsVersion: minTlsVersion
    appServicePlanId: appServicePlanModule.outputs.planId
    keyVaultRoles: {
      keyVaultName: resourceNames.keyVault.keyVault
      secretsUser: true
      certificateUser: true
      legacyKeyVaultRoleAssignmentName: true
    }
    appInsightsName: appInsightsModule.outputs.applicationInsightsName
    detailedErrors: detailedErrors
    autoscaleEnabled: autoscaleAppServices
    swapSlotEnabled: false
    healthCheckPath: '/health'
    allowedOrigins: allowedOrigins
    alerts: deployAlerts ? {
      appServiceHealth: true
      httpErrors: true
      http401: http401Alert
      http5xx: http5xxAlert
      responseTimeSeconds: 12
      alertsGroupName: resourceNames.alertsGroup
    } : null
    websitePort: 3000
    applicationAppSettings: {}
    existingProdAppSettings: existingProdAppSettings
    tagValues: tagValues
  }
}

module searchIndexDataReaderRoleAssignmentModule '../common/components/search/searchServiceRoleAssignment.bicep' = {
  name: 'publicSiteSearchIndexDataReaderRoleAssignmentModuleDeploy'
  params: {
    searchServiceName: resourceNames.search.service
    principalIds: [appServiceModule.outputs.appServiceSystemIdentityId]
    roleAssignmentNameOverride: guid(
      resourceId('Microsoft.Search/searchServices', resourceNames.search.service),
      resourceId('Microsoft.Web/sites', appServiceModule.outputs.appServiceName),
      subscriptionResourceId('Microsoft.Authorization/roleDefinitions', builtInRoleDefinitionIds.SearchIndexDataReader)
    )
    role: 'Search Index Data Reader'
  }
}
