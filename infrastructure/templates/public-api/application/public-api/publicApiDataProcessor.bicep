@description('Specifies the location for all resources.')
param location string

@description('Specifies the Data Processor Function App name.')
param dataProcessorAppName string

@description('Specifies the App Service plan name')
param appServicePlanName string

@description('Specifies the Managed Identity name.')
param dataProcessorIdentityName string

@description('Specifies the Data Processor Function App name.')
param dataProcessorStorageAccountsPrefix string

@description('Specifies the name of an alerts group for reporting metric alerts')
param alertsGroupName string

@description('Alert metric name prefix')
param metricsNamePrefix string

@description('Specifies the Admin App Service name.')
param adminAppName string

@description('Specifies the name of the Key Vault.')
param keyVaultName string

@description('The Application Insights key that is associated with this resource')
param applicationInsightsKey string

@description('Specifies the subnet id for the function app outbound traffic across the VNet')
param subnetId string

@description('Specifies the optional subnet id for function app inbound traffic from the VNet')
param privateEndpointSubnetId string

@description('Specifies whether or not the Data Processor Function App already exists.')
param dataProcessorFunctionAppExists bool = false

@description('Specifies the Application (Client) Id of a pre-existing App Registration used to represent the Data Processor Function App.')
param dataProcessorAppRegistrationClientId string

@description('Specifies the name of the fileshare used to store Public API Data.')
param publicApiDataFileShareName string

@description('Specifies the name of the Public API storage account.')
param publicApiStorageAccountName string

@description('Public API Storage : Firewall rules.')
param storageFirewallRules {
  name: string
  cidr: string
}[] = []

@description('Specifies a set of tags with which to tag the resource in Azure.')
param tagValues object

resource adminAppService 'Microsoft.Web/sites@2023-12-01' existing = {
  name: adminAppName
}

resource adminAppServiceIdentity 'Microsoft.ManagedIdentity/identities@2023-01-31' existing = {
  scope: adminAppService
  name: 'default'
}

var adminAppClientId = adminAppServiceIdentity.properties.clientId
var adminAppPrincipalId = adminAppServiceIdentity.properties.principalId

resource publicApiStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: publicApiStorageAccountName
}

resource dataProcessorFunctionAppManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: dataProcessorIdentityName
  location: location
}

// Deploy Data Processor Function.
module dataProcessorFunctionAppModule '../../components/functionApp.bicep' = {
  name: 'dataProcessorFunctionAppDeploy'
  params: {
    functionAppName: dataProcessorAppName
    appServicePlanName: appServicePlanName
    storageAccountsNamePrefix: dataProcessorStorageAccountsPrefix
    alertsGroupName: alertsGroupName
    location: location
    applicationInsightsKey: applicationInsightsKey
    subnetId: subnetId
    privateEndpointSubnetId: privateEndpointSubnetId
    publicNetworkAccessEnabled: false
    entraIdAuthentication: {
      appRegistrationClientId: dataProcessorAppRegistrationClientId
      allowedClientIds: [
        adminAppClientId
      ]
      allowedPrincipalIds: [
        adminAppPrincipalId
      ]
      requireAuthentication: true
    }
    userAssignedManagedIdentityParams: {
      id: dataProcessorFunctionAppManagedIdentity.id
      name: dataProcessorFunctionAppManagedIdentity.name
      principalId: dataProcessorFunctionAppManagedIdentity.properties.principalId
    }
    functionAppExists: dataProcessorFunctionAppExists
    keyVaultName: keyVaultName
    functionAppRuntime: 'dotnet-isolated'
    sku: {
      name: 'EP1'
      tier: 'ElasticPremium'
      family: 'EP'
    }
    preWarmedInstanceCount: 1
    healthCheck: {
      path: '/api/HealthCheck'
      unhealthyMetricName: '${metricsNamePrefix}Unhealthy'
    }
    appSettings: {
      AppSettings__MetaInsertBatchSize: 1000
    }
    azureFileShares: [{
      storageName: publicApiDataFileShareName
      storageAccountKey: publicApiStorageAccount.listKeys().keys[0].value
      storageAccountName: publicApiStorageAccountName
      fileShareName: publicApiDataFileShareName
      mountPath: '/data/public-api-data'
    }]
    storageFirewallRules: storageFirewallRules
    tagValues: tagValues
  }
}

output managedIdentityClientId string = dataProcessorFunctionAppManagedIdentity.properties.clientId
