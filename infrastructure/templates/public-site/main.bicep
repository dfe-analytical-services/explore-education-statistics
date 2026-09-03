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

@description('Public URL of the public site.')
param publicAppUrl string

@description('Enables Basic Auth on the public application, the purpose of this is prevent accidential access to the application before it is publically avaliable (following GDS guidance)')
param publicAppBasicAuth bool

@description('Username protecting the public app, no requirement to be secret, the purpose of this is prevent accidential access to the application before it is publically avaliable (following GDS guidance)')
param publicAppBasicAuthUsername string

@secure()
@description('Password protecting the public app, no requirement to be secret, the purpose of this is prevent accidential access to the application before it is publically avaliable (following GDS guidance)')
param publicAppBasicAuthPassword string

@description('The origins supported for CORS calls to this App Service.')
param allowedOrigins string[]

@description('Whether or not to deploy Azure Metric alerts.')
param deployAlerts bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

@description('Name fo the environment being deployed to e.g. Development, Test.')
param environmentName string

@description('Public hostname of the Content API.')
param contentApiPublicHostname string

@description('Public hostname of the Data API.')
param dataApiPublicHostname string

@description('Public hostname of the public API.')
param publicApiPublicHostname string

@description('URL for the ACR hosting Docker images for this App Service.')
param dockerRegistryUrl string

@secure()
@description('Username for the user pulling Docker images for this App Service.')
param dockerPullUsername string

@secure()
@description('Password for the user pulling Docker images for this App Service.')
param dockerPullPassword string

@description('GA tracking ID.')
param googleAnalyticsTrackingId string

@description('The default max age in seconds for content to be cached by Azure Front Door.')
param defaultCacheMaxAgeSeconds int

module appServicePlanModule '../common/components/app-service-plan/app-service-plan.bicep' = {
  name: 'publicSiteAppServicePlanModule'
  params: {
    planName: resourceNames.publicSite.appServicePlan
    sku: appServiceSku
    operatingSystem: 'Linux'
    kind: 'app,linux,container'
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

resource searchService 'Microsoft.Search/searchServices@2025-05-01' existing = {
  name: resourceNames.search.service
}

resource nlSearchFunctionApp 'Microsoft.Web/sites@2025-03-01' existing = {
  name: resourceNames.nlSearch.functionApp
}

module appServiceModule '../common/components/app-service/app-service.bicep' = {
  name: 'publicSiteAppServiceModuleDeploy'
  params: {
    appServiceName: resourceNames.publicSite.appService
    operatingSystem: 'Linux'
    kind: 'app,linux,container'
    minTlsVersion: minTlsVersion
    appServicePlanId: appServicePlanModule.outputs.planId
    keyVaultRoles: {
      keyVaultName: resourceNames.keyVault.keyVault
      secretsUser: true
      certificateUser: true
    }
    legacyKeyVaultRoleAssignmentName: true
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
    applicationAppSettings: {
      APP_ENV: environmentName
      AZURE_SEARCH_ENDPOINT: searchService.properties.endpoint
      AZURE_SEARCH_INDEX: 'index-1'
      AZURE_DATASETS_SEARCH_INDEX: 'nl-search-dataset-index'
      AZURE_TABLE_TOOL_SEARCH_ENDPOINT: 'https://${nlSearchFunctionApp.properties.defaultHostName}/api/natural_language_search_function'
      BASIC_AUTH: publicAppBasicAuth
      BASIC_AUTH_USERNAME: publicAppBasicAuthUsername
      BASIC_AUTH_PASSWORD: publicAppBasicAuthPassword
      CONTENT_API_BASE_URL: 'https://${contentApiPublicHostname}/api'
      DATA_API_BASE_URL: 'https://${dataApiPublicHostname}/api'
      DOCKER_REGISTRY_SERVER_URL: dockerRegistryUrl
      DOCKER_REGISTRY_SERVER_USERNAME: dockerPullUsername
      DOCKER_REGISTRY_SERVER_PASSWORD: dockerPullPassword
      NOTIFICATION_API_BASE_URL: 'https://${resourceNames.notifier.functionApp}.azurewebsites.net/api'
      GA_TRACKING_ID: googleAnalyticsTrackingId
      NEXT_CONFIG_MODE: 'server'
      NODE_ENV: 'production'
      PUBLIC_URL: '${publicAppUrl}/'
      PUBLIC_API_BASE_URL: 'https://${publicApiPublicHostname}'
      PUBLIC_API_DOCS_URL: 'https://${publicApiPublicHostname}/docs'
      DEFAULT_CACHE_MAX_AGE_SECONDS: defaultCacheMaxAgeSeconds
      WEBSITES_DISABLE_CONTENT_COMPRESSION: true
    }
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
