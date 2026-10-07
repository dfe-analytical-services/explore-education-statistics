import { ResourceNames } from '../bicep-main-infrastructure-release/resource-names.bicep'
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

@description('The origins supported for CORS calls to this App Service.')
param allowedOrigins string[]

@description('Whether or not to deploy Azure Metric alerts.')
param deployAlerts bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

@description('URL for the ACR hosting Docker images for this App Service.')
param dockerRegistryUrl string

@secure()
@description('Username for the user pulling Docker images for this App Service.')
param dockerPullUsername string

@secure()
@description('Password for the user pulling Docker images for this App Service.')
param dockerPullPassword string

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
    allowedOrigins: allowedOrigins
    alerts: deployAlerts ? {
      appServiceHealth: true
      httpErrors: true
      responseTimeSeconds: 12
      alertsGroupName: resourceNames.alertsGroup
    } : null
    websitePort: 3000
    // Application-specific appsettings (APP_ENV, AZURE_SEARCH_*, BASIC_AUTH*, *_API_BASE_URL,
    // GA_TRACKING_ID, PUBLIC_URL, DEFAULT_CACHE_MAX_AGE_SECONDS) are no longer seeded from here
    // - the app-release pipeline applies them ahead of each code deploy via
    // public-site-bicep-config.bicep. See existingProdAppSettings below for how they're
    // preserved across infrastructure deploys. What remains here is deployment/platform
    // plumbing (which registry/credentials to pull the image from, fixed container runtime
    // mode) rather than application behaviour, so it stays infrastructure-controlled.
    applicationAppSettings: {
      DOCKER_REGISTRY_SERVER_URL: dockerRegistryUrl
      DOCKER_REGISTRY_SERVER_USERNAME: dockerPullUsername
      DOCKER_REGISTRY_SERVER_PASSWORD: dockerPullPassword
      NEXT_CONFIG_MODE: 'server'
      NODE_ENV: 'production'
      WEBSITES_DISABLE_CONTENT_COMPRESSION: true
    }
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
