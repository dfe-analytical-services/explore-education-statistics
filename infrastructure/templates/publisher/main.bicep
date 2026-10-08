import { ResourceNames } from '../bicep-main-infrastructure-release/resource-names.bicep'
import { FunctionAppServicePlanSku } from '../common/components/app-service-plan/types.bicep'
import { IpRange } from '../common/types.bicep'

@description('Names of resources in this deploy.')
param resourceNames ResourceNames

@description('Minimum TLS version supported.')
param minTlsVersion string

@description('App Service Plan SKU.')
param appServiceSku FunctionAppServicePlanSku

@description('The id of the Log Analytics workspace which logs and metrics will be sent to.')
param logAnalyticsWorkspaceId string

@secure()
@description('''The database user's password.''')
param databaseUserPassword string

@description('Provides access to resources for specific IP address ranges used for service maintenance.')
param maintenanceIpRanges IpRange[]

@description('Number of days to retain blobs after delete.')
param blobDeleteRetentionDays int

@description('Whether or not to deploy Azure Metric alerts.')
param deployAlerts bool

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

@secure()
@description('''
The existing appsettings for this Function App, fetched by the pipeline before deployment. Used to
prevent infrastructure deploys from overriding application-specific appsettings that the code
deployment pipeline has since applied - see publisher-appsettings.bicep.
''')
param existingAppSettings object = {}

var coreSqlServerFqdn = reference('Microsoft.Sql/servers/${resourceNames.databases.coreSqlServer}', '2025-02-01-preview').fullyQualifiedDomainName

var publicApiFileshareMountPath = '\\mounts\\public-api-data'

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: resourceNames.vnet.vnet
}

resource outboundVnetSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: resourceNames.vnet.subnets.publisher
  parent: vNet
}
    
resource publicApiStorageAccount 'Microsoft.Storage/storageAccounts@2026-04-01' existing = {
  name: resourceNames.publicApi.storage.storageAccount
}

module appInsightsModule '../common/components/monitoring/appInsights.bicep' = {
  name: 'publisherAppInsightsModuleDeploy'
  params: {
    appInsightsName: resourceNames.publisher.appInsights
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

module functionAppModule '../common/components/function-app/function-app.bicep' = {
  name: 'publisherFunctionAppModuleDeploy'
  params: {
    functionAppName: resourceNames.publisher.functionApp
    appServicePlanName: resourceNames.publisher.appServicePlan
    storageAccountName: resourceNames.publisher.storageAccount
    storageAccountSku: 'Standard_GZRS'
    keyVaultName: resourceNames.keyVault.keyVault
    keyVaultRoles: {
      secretsUser: true
      legacyKeyVaultRoleAssignmentName: true
    }
    sku: appServiceSku
    functionAppRuntime: 'dotnet-isolated'
    operatingSystem: 'Windows'
    netFrameworkVersion: 'v10.0'
    elasticCapacity: null
    alwaysOn: true
    publicNetworkAccessEnabled: true
    storageAccountPublicNetworkAccessEnabled: true
    storageAccountAllowedSubnetIds: [
      resourceId('Microsoft.Network/virtualNetworks/subnets', vNet.name, resourceNames.vnet.subnets.admin)
      resourceId('Microsoft.Network/virtualNetworks/subnets', vNet.name, outboundVnetSubnet.name)
    ]
    storageFirewallRules: maintenanceIpRanges
    deployQueueRoleAssignment: true
    functionAppFirewallRules: []
    healthCheckPath: '/api/health'
    applicationInsightsConnectionString: appInsightsModule.outputs.applicationInsightsConnectionString
    outboundSubnetId: outboundVnetSubnet.id
    minTlsVersion: minTlsVersion
    connectionStrings: [
      {
        name: 'ContentDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${coreSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.contentDb};User Id=publisher@${coreSqlServerFqdn};Password=${databaseUserPassword};'
      }
      {
        name: 'StatisticsDb'
        type: 'SQLAzure'
        connectionString: 'Data Source=tcp:${coreSqlServerFqdn},1433;Initial Catalog=${resourceNames.databases.statisticsDb};User Id=publisher@${coreSqlServerFqdn};Password=${databaseUserPassword};'
      }
      {
        name: 'PublicDataDb'
        type: 'PostgreSQL'
        connectionString: '@Microsoft.KeyVault(VaultName=${resourceNames.keyVault.keyVault};SecretName=${resourceNames.keyVault.secrets.publisher.publicDataDbConnectionString})'
      }
    ]
    alerts: deployAlerts ? {
      cpuPercentage: true
      functionAppHealth: true
      httpErrors: true
      memoryPercentage: true
      storageAccountAvailability: false
      storageLatency: false
      fileServiceAvailability: false
      fileServiceLatency: false
      fileServiceCapacity: false
      alertsGroupName: resourceNames.alertsGroup
    } : null
    diagnosticSettingsLogAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    azureFileShares: [
      {
        storageName: publicApiStorageAccount.name
        storageAccountKey: publicApiStorageAccount.listKeys().keys[0].value
        storageAccountName: publicApiStorageAccount.name
        fileShareName: resourceNames.publicApi.storage.fileShare
        mountPath: publicApiFileshareMountPath
      }
    ]
    // Application-specific appsettings are no longer seeded from here - the app-release
    // pipeline applies them ahead of each code deploy via publisher-appsettings.bicep. See
    // existingAppSettings above for how they're preserved across infrastructure deploys.
    appSettings: []
    existingAppSettings: existingAppSettings
    tagValues: tagValues
  }
}

module storageAccountBlobServiceModule '../common/components/blobService.bicep' = {
  name: 'publisherStorageAccountBlobServiceModuleDeploy'
  params: {
    storageAccountName: resourceNames.publisher.storageAccount
    deleteRetentionPolicy: blobDeleteRetentionDays
  }
  dependsOn: [
    functionAppModule
  ]
}

// Publication metadata changes can make a previously cached ZIP stale without publishing a new release.
// Deliver the existing PublicationChanged event to Publisher so it can purge affected AFD paths.
module publicationZipPurgeQueue '../common/components/queueService.bicep' = {
  name: 'publicationZipPurgeQueueModuleDeploy'
  params: {
    storageAccountName: resourceNames.publisher.storageAccount
    queueNames: ['publication-zip-purge']
  }
  dependsOn: [functionAppModule]
}

module publicationZipPurgeSubscription '../common/components/event-grid/eventGridCustomTopicQueueSubscription.bicep' = {
  name: 'publicationZipPurgeSubscriptionModuleDeploy'
  params: {
    name: '${resourceNames.publisher.functionApp}-publication-zip-purge'
    topicName: resourceNames.eventGrid.topics.publicationChanged
    includedEventTypes: ['publication-changed']
    storageAccountName: resourceNames.publisher.storageAccount
    queueName: 'publication-zip-purge'
  }
  dependsOn: [publicationZipPurgeQueue]
}
