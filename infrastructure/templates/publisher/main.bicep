import { HttpStatusAlertConfig } from '../common/components/alerts/httpStatusAlertConfig.bicep'
import { StorageAccountSku } from '../common/components/storage/types.bicep'
import { ResourceNames } from '../common/resource-names.bicep'
import { publicDataDbConnectionString } from '../common/functions.bicep'
import { FunctionAppServicePlanSku } from '../common/components/app-service-plan/types.bicep'
import { IpRange } from '../common/types.bicep'
import { builtInRoleDefinitionIds } from '../common/builtInRoles.bicep'

@description('Names of resources in this deploy.')
param resourceNames ResourceNames

@description('Minimum TLS version supported.')
param minTlsVersion string

@description('App Service Plan SKU.')
param appServiceSku FunctionAppServicePlanSku

@description('The operating system to host the Function App on.')
param operatingSystem 'Windows' | 'Linux'

@description('SKU of the Function App\'s storage account, which sets its redundancy.')
param storageAccountSku StorageAccountSku

@description('Whether the Function App uses its user-assigned managed identity rather than a system-assigned one.')
param useUserAssignedIdentity bool = false

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

@description('Optional override of the existing Http401 rule. Omit to preserve current behaviour.')
param http401Alert HttpStatusAlertConfig?

@description('Optional override of the existing Http5xx rule. Omit to preserve current behaviour.')
param http5xxAlert HttpStatusAlertConfig?

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
    userAssignedIdentityName: useUserAssignedIdentity ? resourceNames.publisher.identity : ''
    // TODO EES-7502 - stop keeping the system-assigned identity, and remove PsqlUseSystemAssignedIdentity below, once
    // the Publisher logs in to PostgreSQL with its user-assigned identity. This needs a PostgreSQL role named after
    // resourceNames.publisher.identity, and the connection string's User Id changing to match.
    //
    // This would best be done when data is fully migrated into ELZ and the service switched over.
    keepSystemAssignedIdentity: true
    functionAppName: resourceNames.publisher.functionApp
    appServicePlanName: resourceNames.publisher.appServicePlan
    storageAccountName: resourceNames.publisher.storageAccount
    storageAccountConnectionStringSecretName: resourceNames.keyVault.secrets.publisherStorageAccountConnectionString
    storageAccountSku: storageAccountSku
    keyVaultName: resourceNames.keyVault.keyVault
    keyVaultRoles: {
      secretsUser: true
      legacyKeyVaultRoleAssignmentName: true
    }
    sku: appServiceSku
    functionAppRuntime: 'dotnet-isolated'
    operatingSystem: operatingSystem
    netFrameworkVersion: operatingSystem == 'Windows' ? 'v10.0' : null
    linuxFxVersion: 'DOTNET-ISOLATED|10.0'
    elasticCapacity: null
    alwaysOn: true
    publicNetworkAccessEnabled: true
    storageAccountPublicNetworkAccessEnabled: true
    storageAccountAllowedSubnetIds: [
      resourceId('Microsoft.Network/virtualNetworks/subnets', vNet.name, resourceNames.vnet.subnets.admin)
      resourceId('Microsoft.Network/virtualNetworks/subnets', vNet.name, outboundVnetSubnet.name)
    ]
    storageFirewallRules: maintenanceIpRanges
    ipSecurityRestrictionsDefaultAction: 'Allow'
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
        // Logs in as the identity the Publisher uses, whose name is also its PostgreSQL role name.
        connectionString: publicDataDbConnectionString(
          resourceNames.databases.postgreSqlFlexibleServer,
          useUserAssignedIdentity ? resourceNames.publisher.identity : resourceNames.publisher.functionApp
        )
      }
    ]
    alerts: deployAlerts ? {
      cpuPercentage: true
      functionAppHealth: true
      httpErrors: true
      http401: http401Alert
      http5xx: http5xxAlert
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
    // Log in to PostgreSQL with the system-assigned identity, whose PostgreSQL role is named after the Function App.
    appSettings: useUserAssignedIdentity
      ? [
          {
            name: 'PsqlUseSystemAssignedIdentity'
            value: 'true'
          }
        ]
      : []
    existingAppSettings: existingAppSettings
    tagValues: tagValues
  }
}

// When the Publisher uses its user-assigned identity, grant it here the roles that the core pipeline grants its
// system-assigned identity. The core pipeline can then deploy before the Publisher's identity exists.
// TODO EES-7502 - make these grants unconditional once every environment has moved to the ELZ.
resource publisherIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = if (useUserAssignedIdentity) {
  name: resourceNames.publisher.identity
}

// Allow the Publisher to send events to the Event Grid topics.
module eventGridTopicRoleAssignmentModule '../common/components/event-grid/eventGridTopicRoleAssignment.bicep' = [
  for (topicName, index) in map(items(resourceNames.eventGrid.topics), item => item.value): if (useUserAssignedIdentity) {
    name: 'publisherTopicRoleAssignmentModuleDeploy-${index}'
    params: {
      principalId: publisherIdentity!.properties.principalId
      roleDefinitionId: builtInRoleDefinitionIds.EventGridDataSender
      topicName: topicName
    }
  }
]

// Allow the Publisher to purge cached files from the Front Door endpoint.
module frontDoorRoleAssignmentModule '../common/components/front-door/afd-endpoint-role-assignment.bicep' = if (useUserAssignedIdentity) {
  name: 'publisherFrontDoorRoleAssignmentModuleDeploy'
  params: {
    frontDoorProfileName: resourceNames.frontDoor.frontDoorName
    frontDoorEndpointName: resourceNames.frontDoor.defaultEndpoint.endpointName
    principalResourceId: publisherIdentity.id
    principalId: publisherIdentity!.properties.principalId
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
