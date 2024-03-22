@description('Environment : Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Storage : Size of the file share in GB.')
param fileShareQuota int = 1

@description('Database : administrator login name.')
@minLength(0)
param postgreSqlAdminName string?

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

@description('Database : Firewall rules.')
param postgreSqlFirewallRules {
  name: string
  startIpAddress: string
  endIpAddress: string
}[] = []

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
param dockerImagesTag string?

@description('Have database users been added to PSQL yet for Container App and Function App?')
param psqlDbUsersAdded bool = true

@description('Public URLs of other components in the service.')
param publicUrls {
  contentApi: string
}?

@description('URL of Data Processor artifact ZIP to deploy.')
param dataProcessorZipFileUrl string?

var resourcePrefix = '${subscription}-ees-publicapi'
var storageAccountName = 's101d01saeescoredw'
var apiContainerAppName = 'api'
var apiContainerAppManagedIdentityName = '${resourcePrefix}-id-${apiContainerAppName}'
var dataProcessorFunctionAppName = 'dataset-processor6'

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

// Reference the existing Azure Container Registry resource as currently managed by the EES ARM template.
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: 'eesacrdw'
}

// Reference the existing core Storage Account as currently managed by the EES ARM template.
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
  scope: resourceGroup(resourceGroup().name)
}
var storageAccountKey = storageAccount.listKeys().keys[0].value
var endpointSuffix = environment().suffixes.storage
var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${endpointSuffix};AccountKey=${storageAccountKey}'

// Reference the existing VNet as currently managed by the EES ARM template, and register new subnets for Bicep-controlled resources.
module vNetModule 'application/virtualNetwork.bicep' = {
  name: 'networkDeploy'
  params: {
    subscription: subscription
    resourcePrefix: resourcePrefix
    apiContainerAppName: apiContainerAppName
    dataProcessorFunctionAppName: dataProcessorFunctionAppName
    postgreSqlServerName: 'psql-flexibleserver'
  }
}

// Deploy a single shared Application Insights for all relevant Public API resources to use.
module applicationInsightsModule 'components/appInsights.bicep' = {
  name: 'appInsightsDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    appInsightsName: ''
  }
}

// Deploy File Share.
module fileShareModule 'components/fileShares.bicep' = {
  name: 'fileShareDeploy'
  params: {
    resourcePrefix: resourcePrefix
    fileShareName: 'data'
    fileShareQuota: fileShareQuota
    storageAccountName: storageAccountName
  }
}

// In order to link PostgreSQL Flexible Server to a VNet, it must have a Private DNS zone available with a name ending
// with "postgres.database.azure.com".
resource postgreSqlPrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: 'private.postgres.database.azure.com'
  location: 'global'
  resource vNetLink 'virtualNetworkLinks' = {
    name: '${resourcePrefix}-psql-flexibleserver-vnet-link'
    location: 'global'
    properties: {
      registrationEnabled: false
      virtualNetwork: {
        id: vNetModule.outputs.vNetRef
      }
    }
  }
}

// Deploy PostgreSQL Database.
module postgreSqlServerModule 'components/postgresqlDatabase.bicep' = {
  name: 'postgreSQLDatabaseDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    createMode: psqlDbUsersAdded ? 'Update' : 'Default'
    adminName: postgreSqlAdminName!
    adminPassword: postgreSqlAdminPassword!
    dbSkuName: postgreSqlSkuName
    dbStorageSizeGB: postgreSqlStorageSizeGB
    dbAutoGrowStatus: postgreSqlAutoGrowStatus
    postgreSqlVersion: '16'
    tagValues: tagValues
    privateDnsZoneId: postgreSqlPrivateDnsZone.id
    firewallRules: postgreSqlFirewallRules
    subnetId: vNetModule.outputs.postgreSqlSubnetRef
    databaseNames: ['public_data']
  }
  dependsOn: [
    postgreSqlPrivateDnsZone
  ]
}

resource apiContainerAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: apiContainerAppManagedIdentityName
  location: location
}

var acrPullRole = resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

@description('This allows the managed identity of the container app to access the registry, note scope is applied to the wider ResourceGroup not the ACR')
resource apiContainerAppManagedIdentityRBAC 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, apiContainerAppManagedIdentity.id, acrPullRole)
  properties: {
    roleDefinitionId: acrPullRole
    principalId: apiContainerAppManagedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: '${resourcePrefix}-log-${apiContainerAppName}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
  tags: tagValues
}

resource apiContainerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: '${resourcePrefix}-cae-${apiContainerAppName}'
  location: location
  properties: {
    vnetConfiguration: {
      infrastructureSubnetId: vNetModule.outputs.apiContainerAppSubnetRef
    }
    daprAIInstrumentationKey: applicationInsightsModule.outputs.applicationInsightsKey
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
  tags: tagValues
}

// Deploy main Public API Container App.
module apiContainerAppModule 'components/containerApp.bicep' = if (psqlDbUsersAdded) {
  name: 'apiContainerAppDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    containerAppName: apiContainerAppName
    acrLoginServer: containerRegistry.properties.loginServer
    containerAppImageName: 'ees-public-api/api:${dockerImagesTag}'
    managedIdentityId: apiContainerAppManagedIdentity.id
    managedEnvironmentId: apiContainerAppEnvironment.id
    appSettings: [
      {
        name: 'ConnectionStrings__PublicDataDb'
        value: replace(replace(postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate, '[database_name]', 'public_data'), '[managed_identity_name]', apiContainerAppManagedIdentityName)
      }
      {
        name: 'AZURE_CLIENT_ID'
        value: apiContainerAppManagedIdentity.properties.clientId
      }
      {
        name: 'ContentApi__Url'
        value: publicUrls!.contentApi
      }
    ]
    tagValues: tagValues
  }
}

// Deploy data-processing Function.
module dataProcessorFunctionAppModule 'application/dataProcessorFunctionApp.bicep' = if (psqlDbUsersAdded) {
  name: 'dataProcessorFunctionAppDeploy'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    functionAppName: dataProcessorFunctionAppName
    storageAccountConnectionString: storageAccountConnectionString
    dbConnectionString: postgreSqlServerModule.outputs.managedIdentityConnectionStringTemplate
    tagValues: tagValues
    applicationInsightsKey: applicationInsightsModule.outputs.applicationInsightsKey
    subnetId: vNetModule.outputs.dataProcessorSubnetRef
  }
}

//resource dataProcessorFunctionAppZipDeploy 'Microsoft.Web/sites/extensions@2021-02-01' = if (psqlDbUsersAdded && dataProcessorZipFileUrl != null) {
//  name: '${resourcePrefix}-fa-${dataProcessorFunctionAppName}/ZipDeploy'
//  properties: {
//      packageUri: dataProcessorZipFileUrl
//  }
//  dependsOn: [
//    dataProcessorFunctionAppModule
//  ]
//}
