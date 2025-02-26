// Added
import {
  FirewallRule
  AzureFileShareMount
} from '../types.bicep'

// param sites_dwtestfa_name string = 'dwtestfa'
// param serverfarms_s101d01_ees_papi_fa_processor_externalid string = '/subscriptions/48ea0797-73c6-4202-bf90-b01c817058e9/resourceGroups/s101d01-rg-ees/providers/Microsoft.Web/serverfarms/s101d01-ees-papi-fa-processor'

// Added
@description('An existing Managed Identity\'s Resource Id with which to associate this Function App')
param userAssignedManagedIdentityParams {
  id: string
  name: string
  principalId: string
}?

// Added
@description('Specifies the Function App name')
param functionAppName string

// Added
@description('Specifies additional Azure Storage Accounts to make available to this Function App')
param azureFileShares AzureFileShareMount[] = []

// Added
@description('Specifies whether or not the Function App will always be on and not idle after periods of no traffic - must be compatible with the chosen hosting plan')
param alwaysOn bool?

// Added
@description('Specifies whether this Function App is accessible from the public internet')
param publicNetworkAccessEnabled bool = false

// Added
@description('IP address ranges that are allowed to access the Function App endpoints. Dependent on "publicNetworkAccessEnabled" being true.')
param functionAppFirewallRules FirewallRule[] = []

// Added
@description('Specifies the optional subnet id for function app inbound traffic from the VNet')
param privateEndpoints {
  functionApp: string?
  storageAccounts: string
}

// Added
@description('Specifies the location for all resources.')
param location string

// Added
@description('A set of tags with which to tag the resource in Azure')
param tagValues object

// Added
@description('Specifies the App Service plan name')
param appServicePlanName string

// Added
@description('Specifies the SKU for the Function App hosting plan')
param sku object

// Added
@description('Function App Plan : operating system')
param operatingSystem 'Windows' | 'Linux' = 'Linux'

// Added
@description('Whether to create or update Azure Monitor alerts during this deploy')
param alerts {
  functionAppHealth: bool
  httpErrors: bool
  cpuPercentage: bool
  memoryPercentage: bool
  storageAccountAvailability: bool
  storageLatency: bool
  fileServiceAvailability: bool
  fileServiceLatency: bool
  fileServiceCapacity: bool
  alertsGroupName: string
}?

// Added
var firewallRules = [
  for (firewallRule, index) in functionAppFirewallRules: {
    name: firewallRule.name
    ipAddress: firewallRule.cidr
    action: 'Allow'
    tag: firewallRule.tag != null ? firewallRule.tag : 'Default'
    priority: firewallRule.priority != null ? firewallRule.priority : 100 + index
  }
]

// Added - rather than copying all this from durable, can it just be in 1 place?
module appServicePlanModule 'appServicePlan.bicep' = {
  name: appServicePlanName
  params: {
    planName: appServicePlanName
    location: location
    kind: 'functionapp'
    sku: sku
    operatingSystem: operatingSystem
    alerts: alerts != null
      ? {
          cpuPercentage: alerts!.cpuPercentage
          memoryPercentage: alerts!.memoryPercentage
          alertsGroupName: alerts!.alertsGroupName
        }
      : null
    tagValues: tagValues
  }
}

// Added
var keyVaultReferenceIdentity = userAssignedManagedIdentityParams != null ? userAssignedManagedIdentityParams!.id : null

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  // Changed from sites_dwtestfa_name_resource
  name: functionAppName // Changed from sites_dwtestfa_name
  location: 'West Europe'
  tags: {
    'hidden-link: /app-insights-resource-id': '/subscriptions/48ea0797-73c6-4202-bf90-b01c817058e9/resourceGroups/s101d01-rg-ees/providers/Microsoft.Insights/components/s101d01-ees-papi-ai'
    Environment: 'Dev'
    Product: 'Enterprise Data and Analytics Platform (EDAP)'
    'Service Offering': 'Enterprise Data and Analytics Platform (EDAP)'
  }
  kind: 'functionapp,linux,container'
  properties: {
    enabled: true
    // hostNameSslStates: [
    //   {
    //     name: '${sites_dwtestfa_name}.azurewebsites.net'
    //     sslState: 'Disabled'
    //     hostType: 'Standard'
    //   }
    //   {
    //     name: '${sites_dwtestfa_name}.scm.azurewebsites.net'
    //     sslState: 'Disabled'
    //     hostType: 'Repository'
    //   }
    // ]
    serverFarmId: appServicePlanModule.outputs.planId // Changed from serverfarms_s101d01_ees_papi_fa_processor_externalid
    reserved: true
    // isXenon: false
    // hyperV: false
    // dnsConfiguration: {}
    // vnetRouteAllEnabled: false
    // vnetImagePullEnabled: false
    // vnetContentShareEnabled: false
    siteConfig: {
      numberOfWorkers: 1
      linuxFxVersion: 'DOCKER|mcr.microsoft.com/azure-functions/base:TAG?' // Which TAG? Changed from DOCKER|mcr.microsoft.com/azure-functions/dotnet:4-appservice-quickstart
      acrUseManagedIdentityCreds: false
      alwaysOn: false
      http20Enabled: false
      functionAppScaleLimit: 0 // Set this?
      minimumElasticInstanceCount: 1
    }
    // scmSiteAlsoStopped: false
    clientAffinityEnabled: false // Durable is true + default is true?
    // clientCertEnabled: false
    // clientCertMode: 'Required'
    // hostNamesDisabled: false
    // ipMode: 'IPv4'
    // vnetBackupRestoreEnabled: false
    customDomainVerificationId: '1D0E852A969B34D7372E883265C507C224ECB6700979937294F66288E6165E1A' // Is this needed?
    containerSize: 1536 // Is this needed?
    dailyMemoryTimeQuota: 0
    httpsOnly: true
    // endToEndEncryptionEnabled: false
    redundancyMode: 'None'
    publicNetworkAccess: 'Disabled'
    storageAccountRequired: true // Changed from false
    keyVaultReferenceIdentity: 'SystemAssigned'
  }
}

// resource sites_dwtestfa_name_ftp 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-04-01' = {
//   parent: sites_dwtestfa_name_resource
//   name: 'ftp'
//   location: 'West Europe'
//   tags: {
//     'hidden-link: /app-insights-resource-id': '/subscriptions/48ea0797-73c6-4202-bf90-b01c817058e9/resourceGroups/s101d01-rg-ees/providers/Microsoft.Insights/components/s101d01-ees-papi-ai'
//     Environment: 'Dev'
//     Product: 'Enterprise Data and Analytics Platform (EDAP)'
//     'Service Offering': 'Enterprise Data and Analytics Platform (EDAP)'
//   }
//   properties: {
//     allow: false
//   }
// }

// resource sites_dwtestfa_name_scm 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-04-01' = {
//   parent: sites_dwtestfa_name_resource
//   name: 'scm'
//   location: 'West Europe'
//   tags: {
//     'hidden-link: /app-insights-resource-id': '/subscriptions/48ea0797-73c6-4202-bf90-b01c817058e9/resourceGroups/s101d01-rg-ees/providers/Microsoft.Insights/components/s101d01-ees-papi-ai'
//     Environment: 'Dev'
//     Product: 'Enterprise Data and Analytics Platform (EDAP)'
//     'Service Offering': 'Enterprise Data and Analytics Platform (EDAP)'
//   }
//   properties: {
//     allow: false
//   }
// }

// What's the purpose of this other "sites" object?
resource sites_dwtestfa_name_web 'Microsoft.Web/sites/config@2024-04-01' = {
  parent: functionApp // Changed from sites_dwtestfa_name_resource
  name: 'web'
  location: 'West Europe'
  tags: {
    'hidden-link: /app-insights-resource-id': '/subscriptions/48ea0797-73c6-4202-bf90-b01c817058e9/resourceGroups/s101d01-rg-ees/providers/Microsoft.Insights/components/s101d01-ees-papi-ai'
    Environment: 'Dev'
    Product: 'Enterprise Data and Analytics Platform (EDAP)'
    'Service Offering': 'Enterprise Data and Analytics Platform (EDAP)'
  }
  properties: {
    numberOfWorkers: 1
    // defaultDocuments: [
    //   'Default.htm'
    //   'Default.html'
    //   'Default.asp'
    //   'index.htm'
    //   'index.html'
    //   'iisstart.htm'
    //   'default.aspx'
    //   'index.php'
    // ]
    // netFrameworkVersion: 'v4.0'
    linuxFxVersion: 'DOCKER|mcr.microsoft.com/azure-functions/base:TAG?' // Which TAG? Changed from DOCKER|mcr.microsoft.com/azure-functions/dotnet:4-appservice-quickstart
    // requestTracingEnabled: false
    // remoteDebuggingEnabled: false
    // httpLoggingEnabled: false
    // acrUseManagedIdentityCreds: false
    logsDirectorySizeLimit: 35
    // detailedErrorLoggingEnabled: false
    publishingUsername: 'REDACTED'
    scmType: 'None'
    // use32BitWorkerProcess: false
    // webSocketsEnabled: false
    alwaysOn: alwaysOn ?? null // Changed from false
    managedPipelineMode: 'Integrated'
    // virtualApplications: [
    //   {
    //     virtualPath: '/'
    //     physicalPath: 'site\\wwwroot'
    //     preloadEnabled: false
    //   }
    // ]
    loadBalancing: 'LeastRequests'
    // experiments: {
    //   rampUpRules: []
    // }
    // autoHealEnabled: false
    // vnetRouteAllEnabled: false
    vnetPrivatePortsCount: 0
    publicNetworkAccess: 'Disabled'
    cors: {
      allowedOrigins: [
        'https://portal.azure.com'
      ]
      supportCredentials: false
    }
    // localMySqlEnabled: false
    // ipSecurityRestrictions: [
    //   {
    //     ipAddress: 'Any'
    //     action: 'Allow'
    //     priority: 2147483647
    //     name: 'Allow all'
    //     description: 'Allow all access'
    //   }
    // ]
    ipSecurityRestrictions: publicNetworkAccessEnabled && length(firewallRules) > 0 ? firewallRules : null
    scmIpSecurityRestrictions: [
      {
        ipAddress: 'Any'
        action: 'Allow'
        priority: 2147483647
        name: 'Allow all'
        description: 'Allow all access'
      }
    ]
    scmIpSecurityRestrictionsUseMain: false
    http20Enabled: true // Changed from false
    minTlsVersion: '1.3' // Changed from 1.2
    scmMinTlsVersion: '1.2' // Needed?
    ftpsState: 'FtpsOnly' // What's FTP needed for?
    // preWarmedInstanceCount: 1 // Don't think this is needed for non-durable?
    functionAppScaleLimit: 0 // Set this?
    functionsRuntimeScaleMonitoringEnabled: false // Should we enable this?
    minimumElasticInstanceCount: 1
    azureStorageAccounts: azureStorageAccountsConfig // Changed from {}
  }
}

resource azureStorageAccountsConfig 'Microsoft.Web/sites/config@2023-12-01' = {
  name: 'azurestorageaccounts'
  parent: functionApp
  properties: reduce(
    azureFileShares,
    {},
    (cur, next) =>
      union(cur, {
        '${next.storageName}': {
          type: 'AzureFiles'
          shareName: next.fileShareName
          mountPath: next.mountPath
          accountName: next.storageAccountName
          accessKey: next.storageAccountKey
        }
      })
  )
}

// Needed?
resource sites_dwtestfa_name_sites_dwtestfa_name_azurewebsites_net 'Microsoft.Web/sites/hostNameBindings@2024-04-01' = {
  parent: functionApp // Changed from sites_dwtestfa_name_resource
  name: '${functionAppName}.azurewebsites.net' // Changed from sites_dwtestfa_name
  location: location // Changed from 'West Europe'
  properties: {
    siteName: 'dwtestfa' // What should this value be?
    hostNameType: 'Verified'
  }
}

// resource sites_dwtestfa_name_dwtemppep_3d790119_6b62_45b8_9ed4_1b90ea75dd9b 'Microsoft.Web/sites/privateEndpointConnections@2024-04-01' = {
//   parent: sites_dwtestfa_name_resource
//   name: 'dwtemppep-3d790119-6b62-45b8-9ed4-1b90ea75dd9b'
//   location: 'West Europe'
//   properties: {
//     privateEndpoint: {}
//     privateLinkServiceConnectionState: {
//       status: 'Approved'
//       actionsRequired: 'None'
//     }
//     ipAddresses: [
//       '10.0.15.4'
//     ]
//   }
// }

module privateEndpointModule 'privateEndpoint.bicep' = if (privateEndpoints.?functionApp != null) {
  name: '${functionAppName}PrivateEndpointDeploy'
  params: {
    serviceId: functionApp.id
    serviceName: functionApp.name
    serviceType: 'sites'
    subnetId: privateEndpoints.?functionApp ?? ''
    location: location
    tagValues: tagValues
  }
}

// Container stuff
@description('Specifies the login server from the registry.')
@secure()
param acrLoginServer string

@description('Specifies the container image to deploy from the registry.')
param containerAppImageName string

@description('Specifies the container port.')
param containerAppTargetPort int = 8080

@description('The CORS policy to use for the Container App.')
param corsPolicy {
  allowedHeaders: string[]?
  allowedMethods: string[]?
  allowedOrigins: string[]?
}
