import { HttpStatusAlertConfig } from '../common/components/alerts/httpStatusAlertConfig.bicep'
import { ResourceNames } from '../common/resource-names.bicep'
import { publicDataDbConnectionString } from '../common/functions.bicep'
import { AppServicePlanSku } from '../common/components/app-service-plan/types.bicep'
import { SignalRSku } from '../common/components/signalr/types.bicep'
import { builtInRoleDefinitionIds } from '../common/builtInRoles.bicep'

@description('Names of resources in this deploy.')
param resourceNames ResourceNames

@description('Minimum TLS version supported.')
param minTlsVersion string

@secure()
@description('''The database user's password.''')
param databaseUserPassword string

@description('App Service Plan SKU.')
param appServiceSku AppServicePlanSku

@description('The operating system to host the App Service on.')
param operatingSystem 'Windows' | 'Linux'

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@description('The origins supported for CORS calls to the Admin SignalR service.')
@minLength(1)
param signalRAllowedOrigins string[]

@description('SKU of the Admin SignalR service.')
param signalRSku SignalRSku

@description('Whether to display detailed error messages in this environment or not.')
param detailedErrors bool

@description('Whether or not to enable autoscaling of App Services in this environment.')
param autoscaleAppServices bool

@description('Whether to spread the App Service plan\'s instances across availability zones.')
param zoneRedundant bool

@description('Whether the App Service and its deploy slot use their user-assigned managed identity rather than system-assigned ones.')
param useUserAssignedIdentity bool = false

@description('Whether or not to deploy Azure Metric alerts.')
param deployAlerts bool

@secure()
@description('The existing appsettings for the production slot, fetched by the pipeline before deployment. Used to prevent infrastructure deploys from overriding application-specific appsettings back to their original values.')
param existingProdAppSettings object = {}

@secure()
@description('The existing appsettings for the staging slot, fetched by the pipeline before deployment. Used to prevent infrastructure deploys from overriding application-specific appsettings back to their original values.')
param existingStagingSlotAppSettings object = {}

@description('Optional override of the existing Http401 rule. Omit to preserve current behaviour.')
param http401Alert HttpStatusAlertConfig?

@description('Optional override of the existing Http5xx rule. Omit to preserve current behaviour.')
param http5xxAlert HttpStatusAlertConfig?

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

var coreSqlServerFqdn = reference('Microsoft.Sql/servers/${resourceNames.databases.coreSqlServer}', '2025-02-01-preview').fullyQualifiedDomainName

module appServicePlanModule '../common/components/app-service-plan/app-service-plan.bicep' = {
  name: 'adminAppServicePlanModule'
  params: {
    planName: resourceNames.admin.appServicePlan
    sku: appServiceSku
    zoneRedundant: zoneRedundant
    kind: operatingSystem == 'Linux' ? 'app,linux' : 'app'
    operatingSystem: operatingSystem
    alerts: deployAlerts ? {
      alertsGroupName: resourceNames.alertsGroup
      cpuPercentage: true
      memoryPercentage: true
    } : null
    tagValues: tagValues
  }
}

module appInsightsModule '../common/components/monitoring/appInsights.bicep' = {
  name: 'adminAppInsightsModuleDeploy'
  params: {
    appInsightsName: resourceNames.admin.appInsights
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
  name: 'adminAppServiceModuleDeploy'
  params: {
    userAssignedIdentityName: useUserAssignedIdentity ? resourceNames.admin.identity : ''
    appServiceName: resourceNames.admin.appService
    kind: operatingSystem == 'Linux' ? 'app,linux' : 'app'
    operatingSystem: operatingSystem
    minTlsVersion: minTlsVersion
    appServicePlanId: appServicePlanModule.outputs.planId
    keyVaultRoles: {
      keyVaultName: resourceNames.keyVault.keyVault
      secretsUser: true
      secretsOfficer: true
      cryptoUser: true
      legacyKeyVaultRoleAssignmentName: true
    }
    connectionStrings: [
      {
        name: 'StatisticsDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${coreSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.statisticsDb};User Id=adminapp@${coreSqlServerFqdn};Password=${databaseUserPassword};'
      }
      {
        name: 'ContentDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${coreSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.contentDb};User Id=adminapp@${coreSqlServerFqdn};Password=${databaseUserPassword};'
      }
      {
        name: 'PublicDataDb'
        type: 'Custom'
        connectionString: publicDataDbConnectionString(resourceNames.databases.postgreSqlFlexibleServer, resourceNames.admin.appService)
      }
    ]
    vnetLink: {
      vnetName: resourceNames.vnet.vnet
      subnetName: resourceNames.vnet.subnets.admin
    }
    appInsightsName: appInsightsModule.outputs.applicationInsightsName
    detailedErrors: detailedErrors
    autoscaleEnabled: autoscaleAppServices
    alerts: deployAlerts ? {
      appServiceHealth: true
      httpErrors: true
      http401: http401Alert
      http5xx: http5xxAlert
      alertsGroupName: resourceNames.alertsGroup
    } : null
    // Application-specific appsettings are controlled in the application release pipeline
    // rather than in the infrastructure rollout so that we can support slot swapping.
    applicationAppSettings: {}
    existingProdAppSettings: existingProdAppSettings
    existingStagingSlotAppSettings: existingStagingSlotAppSettings
    tagValues: tagValues
  }
}

// When Admin uses its user-assigned identity, grant it here the role that the core pipeline grants its system-assigned
// identity. The core pipeline can then deploy before Admin's identity exists.
// TODO EES-7502 - make this grant unconditional once every environment has moved to the ELZ.
resource adminIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = if (useUserAssignedIdentity) {
  name: resourceNames.admin.identity
}

// Allow Admin to send events to the Event Grid topics.
module eventGridTopicRoleAssignmentModule '../common/components/event-grid/eventGridTopicRoleAssignment.bicep' = [
  for (topicName, index) in map(items(resourceNames.eventGrid.topics), item => item.value): if (useUserAssignedIdentity) {
    name: 'adminTopicRoleAssignmentModuleDeploy-${index}'
    params: {
      principalId: adminIdentity!.properties.principalId
      roleDefinitionId: builtInRoleDefinitionIds.EventGridDataSender
      topicName: topicName
    }
  }
]

module adminSignalRService '../common/components/signalr/signalr.bicep' = {
  name: 'adminSignalRServiceDeploy'
  params: {
    signalRName: resourceNames.admin.signalRName
    sku: signalRSku
    allowedOrigins: signalRAllowedOrigins
  }
}
