import { abbreviations } from 'abbreviations.bicep'
import { ContainerAppResourceConfig, IpRange, PrincipalNameAndId, StaticWebAppSku, ContainerAppWorkloadProfile } from 'types.bicep'

@description('Environment : Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string = ''

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Public API Storage : Size of the file share in GB.')
param publicApiDataFileShareQuota int = 1

@description('Provides access to resources for specific IP address ranges used for service maintenance.')
param maintenanceIpRanges IpRange[] = []

@description('Database : administrator login name.')
@minLength(0)
param postgreSqlAdminName string = ''

@description('Database : administrator password.')
@minLength(8)
@secure()
param postgreSqlAdminPassword string?

@description('Database : Azure Database for PostgreSQL sku name.')
@allowed([
  'Standard_B1ms'
  'Standard_D4ads_v5'
  'Standard_E4ads_v5'
])
param postgreSqlSkuName string = 'Standard_B1ms'

@description('Database : Azure Database for PostgreSQL Storage Size in GB.')
param postgreSqlStorageSizeGB int = 32

@description('Database : Azure Database for PostgreSQL Autogrow setting.')
param postgreSqlAutoGrowStatus string = 'Disabled'

@description('Database : Entra ID admin  principal names for this resource')
param postgreSqlEntraIdAdminPrincipals PrincipalNameAndId[] = []

@description('ACR : Specifies the resource group in which the shared Container Registry lives.')
param acrResourceGroupName string = ''

@description('Public API docs app : SKU to use.')
param docsAppSku StaticWebAppSku = 'Free'

@description('Tagging : Environment name e.g. Development. Used for tagging resources created by this infrastructure pipeline.')
param environmentName string

@description('Tagging : Used for tagging resources created by this infrastructure pipeline.')
param resourceTags {
  CostCentre: string
  Department: string
  Solution: string
  ServiceOwner: string
  CreatedBy: string
  DeploymentRepoUrl: string
}?

@description('Tagging : Date Provisioned. Used for tagging resources created by this infrastructure pipeline.')
param dateProvisioned string = utcNow('u')

@description('The tags of the Docker images to deploy.')
param dockerImagesTag string = ''

@description('Do the shared Private DNS Zones need creating or updating?')
param deploySharedPrivateDnsZones bool = false

// TODO EES-5128 - Note that this has been added temporarily to avoid 10+ minute deploys where it appears that PSQL
// will redeploy even if no changes exist in this deploy from the previous one.
@description('Does the PostgreSQL Flexible Server require any updates? False by default to avoid unnecessarily lengthy deploys.')
param deployPsqlFlexibleServer bool = false

@description('Does the Public API Container App need creating or updating? This is dependent on the PostgreSQL Flexible Server being set up and having users manually added.')
param deployContainerApp bool = true

@description('Does the Data Processor need creating or updating?')
param deployDataProcessor bool = true

param deployAlerts bool = false

@description('Public URLs of other components in the service.')
param publicUrls {
  contentApi: string
  publicSite: string
  publicApi: string
}

@description('Specifies whether or not the Data Processor Function App already exists.')
param dataProcessorFunctionAppExists bool = false

@description('Specifies the Application (Client) Id of a pre-existing App Registration used to represent the Data Processor Function App.')
param dataProcessorAppRegistrationClientId string = ''

@description('Specifies the Application (Client) Id of a pre-existing App Registration used to represent the API Container App.')
param apiAppRegistrationClientId string = ''

@description('Specifies the principal id of the Azure DevOps SPN.')
@secure()
param devopsServicePrincipalId string = ''

// TODO EES-5446 - reinstate pipelineRunnerCidr when the DevOps runners have a static IP range available. 
// @description('Specifies the IP address range of the pipeline runners.')
// param pipelineRunnerCidr string = ''

@description('Specifies whether or not test Themes can be deleted in the environment.')
param enableThemeDeletion bool = false

@description('Specifies the workload profiles for this Container App Environment - the default Consumption plan is always included')
param publicApiContainerAppWorkloadProfiles ContainerAppWorkloadProfile[] = []

@description('Resource configuration for the Public API Container App.')
param publicApiContainerAppConfig ContainerAppResourceConfig = {
  cpuCores: 4
  memoryGis: 8
  minReplicas: 0
  maxReplicas: 3
  workloadProfileName: 'Consumption'
}

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

//
// Define our resource prefix.
//

// The resource prefix for anything specific to the Public API.
var publicApiResourcePrefix = '${subscription}-ees-papi'

// The resource prefix for anything not specific solely to the Public API but set up within Bicep.
var commonResourcePrefix = '${subscription}-ees'

// The resource prefix for resources created in the ARM template.
var legacyResourcePrefix = subscription

var resourceNames = {
  existingResources: {
    adminApp: '${legacyResourcePrefix}-as-ees-admin'
    publisherFunction: '${legacyResourcePrefix}-fa-ees-publisher'
    keyVault: '${legacyResourcePrefix}-kv-ees-01'
    vNet: '${legacyResourcePrefix}-vnet-ees'
    alertsGroup: '${legacyResourcePrefix}-ag-ees-alertedusers'
    acr: 'eesacr'
    acrResourceGroup: acrResourceGroupName
    // The Test Resource Group has broken from the naming convention of other environments for Core Storage
    coreStorageAccount: subscription == 's101t01' ? '${legacyResourcePrefix}storageeescore' : '${legacyResourcePrefix}saeescore'
    subnets: {
      adminApp: '${legacyResourcePrefix}-snet-ees-admin'
      publisherFunction: '${legacyResourcePrefix}-snet-ees-publisher'
      appGateway: '${commonResourcePrefix}-snet-${abbreviations.networkApplicationGateways}-01'
      containerAppEnvironment: '${commonResourcePrefix}-snet-${abbreviations.appManagedEnvironments}-01'
      dataProcessor: '${publicApiResourcePrefix}-snet-${abbreviations.webSitesFunctions}-processor'
      dataProcessorPrivateEndpoints: '${publicApiResourcePrefix}-snet-${abbreviations.webSitesFunctions}-processor-pep'
      psqlFlexibleServer: '${commonResourcePrefix}-snet-${abbreviations.dBforPostgreSQLServers}'
    }
  }
  sharedResources: {
    appGateway: '${commonResourcePrefix}-${abbreviations.networkApplicationGateways}-01'
    appGatewayIdentity: '${commonResourcePrefix}-${abbreviations.managedIdentityUserAssignedIdentities}-${abbreviations.networkApplicationGateways}-01'
    containerAppEnvironment: '${commonResourcePrefix}-${abbreviations.appManagedEnvironments}-01'
    logAnalyticsWorkspace: '${commonResourcePrefix}-${abbreviations.operationalInsightsWorkspaces}'
    postgreSqlFlexibleServer: '${commonResourcePrefix}-${abbreviations.dBforPostgreSQLServers}'
  }
  publicApi: {
    apiApp: '${publicApiResourcePrefix}-${abbreviations.appContainerApps}-api'
    apiAppIdentity: '${publicApiResourcePrefix}-${abbreviations.managedIdentityUserAssignedIdentities}-${abbreviations.appContainerApps}-api'
    appInsights: '${publicApiResourcePrefix}-${abbreviations.insightsComponents}'
    dataProcessor: '${publicApiResourcePrefix}-${abbreviations.webSitesFunctions}-processor'
    dataProcessorIdentity: '${publicApiResourcePrefix}-${abbreviations.managedIdentityUserAssignedIdentities}-${abbreviations.webSitesFunctions}-processor'
    dataProcessorPlan: '${publicApiResourcePrefix}-${abbreviations.webServerFarms}-${abbreviations.webSitesFunctions}-processor'
    dataProcessorStorageAccountsPrefix: '${subscription}eessaprocessor'
    docsApp: '${publicApiResourcePrefix}-${abbreviations.staticWebApps}-docs'
    publicApiFileShare: '${publicApiResourcePrefix}-fs-data'
    publicApiStorageAccount: '${replace(publicApiResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}'
  }
}

var maintenanceFirewallRules = [for maintenanceIpRange in maintenanceIpRanges: {
  name: maintenanceIpRange.name
  cidr: maintenanceIpRange.cidr
  tag: 'Default'
  priority: 100
}]

module vNetModule 'application/shared/virtualNetwork.bicep' = {
  name: 'virtualNetworkApplicationModuleDeploy'
  params: {
    resourceNames: resourceNames
  }
}

module coreStorage 'application/shared/coreStorage.bicep' = {
  name: 'coreStorageApplicationModuleDeploy'
  params: {
    resourceNames: resourceNames
  }
}

module privateDnsZonesModule 'application/shared/privateDnsZones.bicep' = 
  if (deploySharedPrivateDnsZones) {
  name: 'privateDnsZonesApplicationModuleDeploy'
  params: {
    resourceNames: resourceNames
    tagValues: tagValues
  }
}

module publicApiStorageModule 'application/public-api/publicApiStorage.bicep' = {
  name: 'publicApiStorageAccountApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    publicApiDataFileShareQuota: publicApiDataFileShareQuota
    storageFirewallRules: maintenanceIpRanges
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

module appInsightsModule 'application/public-api/publicApiAppInsights.bicep' = {
  name: 'publicApiAppInsightsApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
  }
}

// Create a generic, shared Log Analytics Workspace for any relevant resources to use.
module logAnalyticsWorkspaceModule 'application/shared/logAnalyticsWorkspace.bicep' = {
  name: 'logAnalyticsWorkspaceApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    tagValues: tagValues
  }
}

module postgreSqlServerModule 'application/shared/postgreSqlFlexibleServer.bicep' = if (deployPsqlFlexibleServer) {
  name: 'postgreSqlFlexibleServerApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    adminName: postgreSqlAdminName
    adminPassword: postgreSqlAdminPassword!
    entraIdAdminPrincipals: postgreSqlEntraIdAdminPrincipals
    privateEndpointSubnetId: vNetModule.outputs.psqlFlexibleServerSubnetRef
    autoGrowStatus: postgreSqlAutoGrowStatus
    firewallRules: maintenanceIpRanges
    sku: postgreSqlSkuName
    storageSizeGB: postgreSqlStorageSizeGB
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
  dependsOn: [
    privateDnsZonesModule
  ]
}

module containerAppEnvironmentModule 'application/shared/containerAppEnvironment.bicep' = {
  name: 'containerAppEnvironmentApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    applicationInsightsKey: appInsightsModule.outputs.appInsightsKey
    workloadProfiles: publicApiContainerAppWorkloadProfiles
    tagValues: tagValues
  }
  dependsOn: [
    publicApiStorageModule
  ]
}

// Deploy main Public API Container App.
module apiAppIdentityModule 'application/public-api/publicApiAppIdentity.bicep' = if (deployContainerApp) {
  name: 'publicApiAppIdentityApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    tagValues: tagValues
  }
}

// Deploy main Public API Container App.
module apiAppModule 'application/public-api/publicApiApp.bicep' = if (deployContainerApp) {
  name: 'publicApiAppApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    apiAppRegistrationClientId: apiAppRegistrationClientId
    containerAppEnvironmentId: containerAppEnvironmentModule.outputs.containerAppEnvironmentId
    containerAppEnvironmentIpAddress: containerAppEnvironmentModule.outputs.containerAppEnvironmentIpAddress
    contentApiUrl: publicUrls.contentApi
    publicApiUrl: publicUrls.publicApi
    publicSiteUrl: publicUrls.publicSite
    dockerImagesTag: dockerImagesTag
    appInsightsConnectionString: appInsightsModule.outputs.appInsightsConnectionString
    deployAlerts: deployAlerts
    resourceAndScalingConfig: publicApiContainerAppConfig
    tagValues: tagValues
  }
  dependsOn: [
    postgreSqlServerModule
    apiAppIdentityModule
  ]
}

// Deploy Public API docs.
module docsModule 'application/public-api/publicApiDocs.bicep' = {
  name: 'publicApiDocsModuleDeploy'
  params: {
    appSku: docsAppSku
    resourceNames: resourceNames
    tagValues: tagValues
  }
}

var docsRewriteSetName = '${publicApiResourcePrefix}-docs-rewrites'

// Create an Application Gateway to serve public traffic for the Public API Container App.
module appGatewayModule 'application/shared/appGateway.bicep' = if (deployContainerApp) {
  name: 'appGatewayModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    sites: [
      {
        name: publicApiResourcePrefix
        certificateName: '${publicApiResourcePrefix}-certificate'
        fqdn: replace(publicUrls.publicApi, 'https://', '')
      }
    ]
    backends: [
      {
        name: resourceNames.publicApi.apiApp
        fqdn: apiAppModule.outputs.containerAppFqdn
        healthProbePath: apiAppModule.outputs.healthProbePath
      }
      {
        name: resourceNames.publicApi.docsApp
        fqdn: docsModule.outputs.appFqdn
        healthProbePath: docsModule.outputs.healthProbePath
      }
    ]
    routes: [
      {
        name: publicApiResourcePrefix
        siteName: publicApiResourcePrefix
        defaultBackendName: resourceNames.publicApi.apiApp
        pathRules: [
          {
            name: 'docs-backend'
            paths: ['/docs/*']
            type: 'backend'
            backendName: resourceNames.publicApi.docsApp
            rewriteSetName: docsRewriteSetName
          }
          {
            // Redirect non-rooted URL (has no trailing slash) to the
            // rooted URL so that relative links in the docs site
            // can resolve correctly.
            name: 'docs-root-redirect'
            paths: ['/docs']
            type: 'redirect'
            redirectUrl: '${publicUrls.publicApi}/docs/'
            redirectType: 'Permanent'
            includePath: false
          }
        ]
      }
    ]
    rewrites: [
      {
        name: docsRewriteSetName
        rules: [
          {
            name: 'trim-docs-path-prefix'
            conditions: [
              {
                variable: 'var_uri_path'
                pattern: '^/docs/(.*)'
                ignoreCase: true
              }
            ]
            actionSet: {
              urlConfiguration: {
                modifiedPath: '/{var_uri_path_1}'
              }
            }
          }
        ]
      }
    ]
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

module dataProcessorModule 'application/public-api/publicApiDataProcessor.bicep' = if (deployDataProcessor) {
  name: 'publicApiDataProcessorApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    applicationInsightsKey: appInsightsModule.outputs.appInsightsKey
    dataProcessorAppRegistrationClientId: dataProcessorAppRegistrationClientId
    devopsServicePrincipalId: devopsServicePrincipalId
    storageFirewallRules: maintenanceIpRanges
    functionAppFirewallRules: union([
      {
        name: 'Admin App Service subnet range'
        cidr: vNetModule.outputs.adminAppServiceSubnetCidr
        tag: 'Default'
        priority: 100
      }
      // TODO EES-5446 - remove service tag whitelisting when runner scale set IP range reinstated 
      {
        cidr: 'AzureCloud'
        tag: 'ServiceTag'
        priority: 101
        name: 'AzureCloud'
      }
      // TODO EES-5446 - reinstate when static IP range available for runner scale sets
      // {
      //   name: 'Pipeline runner IP address range'
      //   cidr: pipelineRunnerCidr
      // }
    ], maintenanceFirewallRules)
    dataProcessorFunctionAppExists: dataProcessorFunctionAppExists
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
  dependsOn: [
    privateDnsZonesModule
    publicApiStorageModule
  ]
}

output dataProcessorContentDbConnectionStringSecretKey string = 'ees-publicapi-data-processor-connectionstring-contentdb'
output dataProcessorPsqlConnectionStringSecretKey string = 'ees-publicapi-data-processor-connectionstring-publicdatadb'

output dataProcessorFunctionAppManagedIdentityClientId string = deployDataProcessor 
  ? dataProcessorModule.outputs.managedIdentityClientId
  : ''

output dataProcessorFunctionAppUrl string = deployDataProcessor
  ? dataProcessorModule.outputs.url
  : ''
output dataProcessorFunctionAppStagingUrl string = deployDataProcessor
  ? dataProcessorModule.outputs.stagingUrl
  : ''

output dataProcessorPublicApiDataFileShareMountPath string = deployDataProcessor
  ? dataProcessorModule.outputs.publicApiDataFileShareMountPath
  : ''

output coreStorageConnectionStringSecretKey string = coreStorage.outputs.coreStorageConnectionStringSecretKey
output keyVaultName string = resourceNames.existingResources.keyVault

output enableThemeDeletion bool = enableThemeDeletion
