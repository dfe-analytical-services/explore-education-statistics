import {
  ContainerAppResourceConfig
  PrincipalNameAndId
  StaticWebAppSku
  ContainerAppWorkloadProfile
  PostgreSqlFlexibleServerConfig
} from 'types.bicep'
import { IpRange } from '../common/types.bicep'
import { StorageAccountConfig } from '../common/components/storage/types.bicep'
import { getResourceNamesForEnvironment } from '../common/resource-names.bicep'

@description('Environment : Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string = ''

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Public API Storage configuration.')
param publicApiStorageConfig StorageAccountConfig = {
  kind: 'FileStorage'
  sku: 'Premium_ZRS'
  fileShare: {
    quotaGbs: 100
    accessTier: 'Premium'
  }
}

@description('Provides access to resources for specific IP address ranges used for service maintenance.')
param maintenanceIpRanges IpRange[] = []

@description('PostgreSQL Database : Server configuration.')
param postgreSqlServerConfig PostgreSqlFlexibleServerConfig = {
  sku: {
    pricingTier: 'Burstable'
    compute: 'Standard_B1ms'
  }
  settings: [
    {
      name: 'max_prepared_transactions'
      value: '100'
    }
  ]
  backups: {
    retentionDays: 7
    geoRedundantBackup: false
  }
  server: {
    postgreSqlVersion: '16'
  }
  storage: {
    storageSizeGB: 32
    autoGrow: true
  }
}

@description('Database : Entra ID admin  principal names for this resource')
param postgreSqlEntraIdAdminPrincipals PrincipalNameAndId[] = []

@description('Database : administrator login name.')
@minLength(0)
param postgreSqlAdminName string = ''

@description('Database : administrator password.')
@minLength(8)
@secure()
param postgreSqlAdminPassword string?

// Currently unused, as the Container App's ACR pull role assignment is commented out in publicApiAppIdentity.bicep,
// but kept as the pipeline still supplies it.
@description('ACR : Specifies the resource group in which the shared Container Registry lives.')
#disable-next-line no-unused-params
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

@description('Does the PostgreSQL Flexible Server need creating or updating?')
param deployPsqlFlexibleServer bool = false

@description('Does the Public API Container App need creating or updating? This is dependent on the PostgreSQL Flexible Server being set up and having users manually added.')
param deployContainerApp bool = true

@description('Does the Data Processor need creating or updating?')
param deployDataProcessor bool = true

@description('Does the Public API static docs site need creating or updating?')
param deployDocsSite bool = true

@description('Do Azure Monitor alerts need creating or updating?')
param deployAlerts bool = false

@description('Public URLs of other components in the service.')
param publicUrls {
  contentApi: string
  publicSite: string
  publicApi: string
}

@secure()
@description('The existing app settings for the Data Processor Function App production slot, fetched by the pipeline before deployment.')
param processorProdAppSettings object = {}

@secure()
@description('The existing app settings for the Data Processor Function App staging slot, fetched by the pipeline before deployment.')
param processorStagingAppSettings object = {}

@description('Specifies the principal id of the Azure DevOps SPN.')
@secure()
param devopsServicePrincipalId string = ''

// TODO EES-5446 - reinstate pipelineRunnerCidr when the DevOps runners have a static IP range available.
// @description('Specifies the IP address range of the pipeline runners.')
// param pipelineRunnerCidr string = ''

@description('Enable deletion of data relating to a theme that is being deleted.')
param enableThemeDeletion bool = false

@description('Specifies the workload profiles for this Container App Environment - the default Consumption plan is always included')
param publicApiContainerAppWorkloadProfiles ContainerAppWorkloadProfile[] = []

@description('Resource configuration for the Public API Container App.')
param publicApiContainerAppConfig ContainerAppResourceConfig = {
  cpuCores: 4
  memoryGis: 8
  minReplicas: 0
  maxReplicas: 3
  scaleAtConcurrentHttpRequests: 10
  workloadProfileName: 'Consumption'
}

@description('Name of the Azure AI Search service index used to search publications.')
param searchServiceIndexName string = ''

@description('Enable the Swagger UI for public API.')
param enableSwagger bool = false

@description('Whether to assign roles that enable us to register PostgreSQL Flexible Server with Backup Vault')
param deployPsqlBackupVaultRoleAssignment bool = false

@description('Whether to register PostgreSQL Flexible Server with Backup Vault')
param deployPsqlBackupVaultRegistration bool = false

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

var resourceNames = getResourceNamesForEnvironment({
  environmentIdentifier: subscription
  environmentName: environmentName
})

var maintenanceFirewallRules = [
  for maintenanceIpRange in maintenanceIpRanges: {
    name: maintenanceIpRange.name
    cidr: maintenanceIpRange.cidr
    tag: 'Default'
    priority: 100
  }
]

resource keyVault 'Microsoft.KeyVault/vaults@2026-02-01' existing = {
  name: resourceNames.keyVault.keyVault
}

module vNetModule 'application/shared/virtualNetwork.bicep' = {
  name: 'virtualNetworkApplicationModuleDeploy'
  params: {
    names: resourceNames.vnet
  }
}

module publicApiStorageModule 'application/public-api/publicApiStorage.bicep' = {
  name: 'publicApiStorageAccountApplicationModuleDeploy'
  params: {
    resourceNames: resourceNames
    config: publicApiStorageConfig
    storageFirewallRules: maintenanceIpRanges
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

module searchServiceModule 'application/shared/searchService.bicep' = {
  name: 'searchServiceApplicationModuleDeploy'
  params: {
    name: resourceNames.search.service
  }
}

module appInsightsModule 'application/public-api/publicApiAppInsights.bicep' = {
  name: 'publicApiAppInsightsApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    environmentName: environmentName
    publicApiUrl: publicUrls.publicApi
    deployAlerts: deployAlerts
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
    serverConfig: postgreSqlServerConfig
    firewallRules: maintenanceIpRanges
    deployAlerts: deployAlerts
    deployBackupVaultRoleAssignment: deployPsqlBackupVaultRoleAssignment
    deployBackupVaultRegistration: deployPsqlBackupVaultRegistration
    tagValues: tagValues
  }
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
module apiAppIdentityModule 'application/public-api/publicApiAppIdentity.bicep' = {
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
    apiAppRegistrationClientId: keyVault.getSecret('ees-public-api-app-registration-client-id')
    containerAppEnvironmentId: containerAppEnvironmentModule.outputs.containerAppEnvironmentId
    containerAppEnvironmentIpAddress: containerAppEnvironmentModule.outputs.containerAppEnvironmentIpAddress
    contentApiUrl: publicUrls.contentApi
    publicApiUrl: publicUrls.publicApi
    publicSiteUrl: publicUrls.publicSite
    dockerImagesTag: dockerImagesTag
    appInsightsConnectionString: appInsightsModule.outputs.appInsightsConnectionString
    deployAlerts: deployAlerts
    resourceAndScalingConfig: publicApiContainerAppConfig
    searchServiceConfig: {
      endpoint: searchServiceModule.outputs.searchServiceEndpoint
      indexName: searchServiceIndexName
    }
    enableSwagger: enableSwagger
    tagValues: tagValues
  }
  dependsOn: [
    postgreSqlServerModule
    apiAppIdentityModule
  ]
}

// Deploy Public API docs.
module docsModule 'application/public-api/publicApiDocs.bicep' = if (deployDocsSite) {
  name: 'publicApiDocsModuleDeploy'
  params: {
    appSku: docsAppSku
    names: resourceNames.publicApi
    tagValues: tagValues
  }
}

module dataProcessorModule 'application/public-api/publicApiDataProcessor.bicep' = if (deployDataProcessor) {
  name: 'publicApiDataProcessorApplicationModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    applicationInsightsKey: appInsightsModule.outputs.appInsightsKey
    dataProcessorAppRegistrationClientId: keyVault.getSecret('ees-public-api-data-processor-app-registration-client-id')
    devopsServicePrincipalId: devopsServicePrincipalId
    storageFirewallRules: maintenanceIpRanges
    functionAppFirewallRules: union(
      [
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
      ],
      maintenanceFirewallRules
    )
    processorProdAppSettings: processorProdAppSettings
    processorStagingAppSettings: processorStagingAppSettings
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
  dependsOn: [
    publicApiStorageModule
  ]
}

output dataProcessorContentDbConnectionStringSecretKey string = 'ees-publicapi-data-processor-connectionstring-contentdb'
output dataProcessorPsqlConnectionStringSecretKey string = 'ees-publicapi-data-processor-connectionstring-publicdatadb'

output dataProcessorFunctionAppManagedIdentityClientId string = deployDataProcessor
  ? dataProcessorModule!.outputs.managedIdentityClientId
  : ''

output dataProcessorFunctionAppUrl string = deployDataProcessor ? dataProcessorModule!.outputs.url : ''
output dataProcessorFunctionAppStagingUrl string = deployDataProcessor ? dataProcessorModule!.outputs.stagingUrl : ''

output dataProcessorPublicApiDataFileShareMountPath string = deployDataProcessor
  ? dataProcessorModule!.outputs.publicApiDataFileShareMountPath
  : ''

output dataProcessorDataFilesFileShareMountPath string = deployDataProcessor
  ? dataProcessorModule!.outputs.dataFilesFileShareMountPath
  : ''


output enableThemeDeletion bool = enableThemeDeletion

