//Environment Params -------------------------------------------------------------------
@description('Base domain name for Public API')
param domain string = 'publicapi'

@description('Data Hub Subscription Name e.g. s101d01. Used as a prefix for created resources')
param subscription string = 's101d01'

@description('Data Hub Environment Name e.g. api. Used as a prefix for created resources')
param environment string = 'api'

@description('Specifies the location in which the Azure Storage resources should be deployed.')
param location string = resourceGroup().location

//Tagging Params ------------------------------------------------------------------------
@description('Tag Value - Enter the Department name tag value e.g. Data Directorate')
param departmentName string = 'Public API'
@description('Tag Value - The name of the phase of the development lifecycle environment that the component will be used in e.g. Development / Test / Integration / Production')
param environmentName string = 'Development'
@description('Tag Value - Enter the solution name that the component is a part of e.g. EDAP, LDS, EES, API')
param solutionName string = 'API'
@description('Tag Value - Enter the full name of the Azure subscription where this resource is located e.g. s101-datahub-development / s101-datahub-test / s101-datahub-production')
param subscriptionName string = 'Unknown'
@description('Tag Value - Enter the cost centre identifying value provided by the Service Owner. Otherwise populate with Unknown.')
param costCentre string = 'Unknown'
@description('Tag Value - Enter the name of the Service or Application Owner in the SURNAME, Firstname format e.g. SINCLAIR, Paul / SHELBY, Laura')
param serviceOwnerName string = 'Unknown'
@description('Tag Value - Enter the date that the component was created using the YYYYMMDD format e.g. 20190417. Use of the utcNow function will automatically populate this entry at creation time. Note: This only works when forced as a default value.')
param dateProvisioned string = utcNow('u')
@description('Tag Value - Enter the name of the user who created these resources in the SURNAME, Firstname format e.g. FISHER, Paul')
param createdBy string = 'Unknown'
@description('Tag Value - Enter the name of the repo that the deployment script for the component name be found. If the component is deployed manually, the value should be N/A')
param deploymentRepo string = 'N/A'


//Networking Params --------------------------------------------------------------------------
@description('Networking : Deploy subnets for networking')
param deploySubnets bool = true

@description('Networking : Included whitelist')
param storageFirewallRules array = ['0.0.0.0/0']

//Storage Params ------------------------------------------------------------------------------
@description('Storage : Name of the root fileshare folder name')
@minLength(3)
@maxLength(63)
param fileShareName string = 'data'

@description('Storage : Type of the file share quota')
param fileShareQuota int = 1

//PostgreSQL Database Params -------------------------------------------------------------------
@description('Database : administrator login name')
@minLength(1)
param dbAdministratorLoginName string = 'PostgreSQLAdmin'

@description('Database : administrator password')
@minLength(8)
@secure()
param dbAdministratorLoginPassword string //P$&RW6*h2V1CKwTm

@description('Database : Azure Database for PostgreSQL sku name ')
@allowed([
  'Standard_B1ms'
  'Standard_D4ads_v5'
  'Standard_E4ads_v5'
])
param skuName string = 'Standard_B1ms'

@description('Database : Azure Database for PostgreSQL Storage Size in GB')
param storageSizeGB int = 32

@description('Database : Azure Database for PostgreSQL Autogrow setting')
param autoGrowStatus string = 'Disabled'

//Container Registry Params ----------------------------------------------------------------
@minLength(5)
@maxLength(50)
@description('Registry : Name of the azure container registry (must be globally unique)')
param containerRegistryName string = 'eesapiacr'

//Container App Params
@description('Container App : Specifies the container image to deploy from the registry <name>:<tag>.')
param acrHostedImageName string = 'hello-world'

@description('Specifies the container port.')
param targetPort int = 80

@description('Container App : Specifies the container image to seed to the ACR.')
param containerSeedImage string = 'mcr.microsoft.com/azuredocs/aci-helloworld'

@description('Select if you want to seed the ACR with a base image.')
param seedRegistry bool = true

//ServiceBus Queue Params -------------------------------------------------------------------
@description('Name of the Service Bus namespace')
param namespaceName string = 'etlnamespace'

@description('Name of the Queue')
param queueName string = 'etlfunctionqueue'

//ETL Function Paramenters ------------------------------------------------------------------
@description('Application : Insights name')
param applicationInsightsName string = 'etlFunctionInsights'

@description('Function App Plan operating system')
@allowed([
  'Windows'
  'Linux'
])
param appServiceplanOS string = 'Linux'

@description('Function App : Runtime Language')
@allowed([
  'dotnet'
  'node'
  'python'
  'java'
])
param functionAppRuntime string = 'python'


//---------------------------------------------------------------------------------------------------------------
// All resources via modules
//---------------------------------------------------------------------------------------------------------------

//Deploy Networking
module vnetModule 'components/network.bicep' = {
  name: 'virtualNetworkDeploy'
  params: {
    subscription: subscription
    location: location
    environment: environment
    deploySubnets: deploySubnets
    //tags
    departmentName: departmentName
    environmentName: environmentName
    solutionName: solutionName
    subscriptionName: subscriptionName
    costCentre: costCentre
    serviceOwnerName: serviceOwnerName
    dateProvisioned: dateProvisioned
    createdBy: createdBy
    deploymentRepo: deploymentRepo
    deploymentScript: 'network.bicep'
  }
}

//Deploy Storage Account
module storageAccountModule 'components/storageAccount.bicep' = {
  name: 'storageAccountDeploy'
  params: {
    subscription: subscription
    location: location
    adminSubnetRef: vnetModule.outputs.adminSubnetRef
    importerSubnetRef: vnetModule.outputs.importerSubnetRef
    publisherSubnetRef: vnetModule.outputs.publisherSubnetRef
    storageFirewallRules: storageFirewallRules
    //tags
    departmentName: departmentName
    environmentName: environmentName
    solutionName: solutionName
    subscriptionName: subscriptionName
    costCentre: costCentre
    serviceOwnerName: serviceOwnerName
    dateProvisioned: dateProvisioned
    createdBy: createdBy
    deploymentRepo: deploymentRepo
    deploymentScript: 'storageaccount.bicep'
  }
  dependsOn: [
    vnetModule
  ]
}

//Deploy File Share
module fileShareModule 'components/fileShares.bicep' = {
  name: 'fileShareDeploy'
  params: {
    fileShareName: fileShareName
    fileShareQuota: fileShareQuota
    storageAccountName: storageAccountModule.outputs.storageAccountName
    //tags
  }
  dependsOn: [
    storageAccountModule
  ]
}

//Deploy Function blob store
module blobStoreModule 'components/fileShares.bicep' = {
  name: 'blobStoreDeploy'
  params: {
    storageAccountName: storageAccountModule.outputs.storageAccountName
    //tags
  }
  dependsOn: [
    storageAccountModule
  ]
}


//Deploy Key Vault
module keyVaultModule 'components/keyvault.bicep' = {
  name: 'keyVaultDeploy'
  params: {
    subscription: subscription
    location: location
    environment: environment
    tenantId: az.subscription().tenantId
    //tags
    departmentName: departmentName
    environmentName: environmentName
    solutionName: solutionName
    subscriptionName: subscriptionName
    costCentre: costCentre
    serviceOwnerName: serviceOwnerName
    dateProvisioned: dateProvisioned
    createdBy: createdBy
    deploymentRepo: deploymentRepo
    deploymentScript: 'keyvault.bicep'
  }
}

//Deploy PostgreSQL Database
module databaseModule 'components/postgresqlDatabase.bicep' = {
  name: 'postgreSQLDatabaseDeploy'
  params: {
    subscription: subscription
    location: location
    administratorLoginName: dbAdministratorLoginName
    administratorLoginPassword: dbAdministratorLoginPassword
    skuName: skuName
    storageSizeGB: storageSizeGB
    autoGrowStatus: autoGrowStatus
    KeyVaultName: keyVaultModule.outputs.keyVaultName
    //tags
    departmentName: departmentName
    environmentName: environmentName
    solutionName: solutionName
    subscriptionName: subscriptionName
    costCentre: costCentre
    serviceOwnerName: serviceOwnerName
    dateProvisioned: dateProvisioned
    createdBy: createdBy
    deploymentRepo: deploymentRepo
    deploymentScript: 'database.bicep'
  }
  dependsOn: [
    vnetModule
    keyVaultModule
  ]
}

//Deploy Container Registry 
module containerRegistryModule 'components/containerRegistry.bicep' = {
  name: 'acrDeploy'
  params: {
    subscription: subscription
    location: location
    containerRegistryName: containerRegistryName
    //tags
    departmentName: departmentName
    environmentName: environmentName
    solutionName: solutionName
    subscriptionName: subscriptionName
    costCentre: costCentre
    serviceOwnerName: serviceOwnerName
    dateProvisioned: dateProvisioned
    createdBy: createdBy
    deploymentRepo: deploymentRepo
    deploymentScript: 'containerRegistry.bicep'
  }
}

//Deploy Container Application
module containerAppModule 'components/containerApp.bicep' = {
  name: 'appContainerDeploy'
  params: {
    subscription: subscription
    location: location
    acrLoginServer: containerRegistryModule.outputs.crLoginServer
    acrHostedImageName: acrHostedImageName //image name plus tag i.e. 'azuredocs/aci-helloworld:Latest'
    targetPort: targetPort
    containerRegistryName: containerRegistryModule.outputs.crName
    containerSeedImage: containerSeedImage // seeder image name 'mcr.microsoft.com/azuredocs/aci-helloworld'
    seedRegistry: seedRegistry
    databaseConnectionString: databaseModule.outputs.adoNetDbConnectionString
    serviceBusConnectionString: serviceBusFunctionQueueModule.outputs.ServiceBusConnectionString
    //tags
    departmentName: departmentName
    environmentName: environmentName
    solutionName: solutionName
    subscriptionName: subscriptionName
    costCentre: costCentre
    serviceOwnerName: serviceOwnerName
    dateProvisioned: dateProvisioned
    createdBy: createdBy
    deploymentRepo: deploymentRepo
    deploymentScript: 'containerApp.bicep'
  }
  dependsOn: [
    containerRegistryModule
    keyVaultModule
  ]
}


//Deploy Service Bus
module serviceBusFunctionQueueModule 'components/serviceBusQueue.bicep' = {
  name: 'serviceBusQueueDeploy'
  params: {
    subscription: subscription
    location: location
    namespaceName: namespaceName
    queueName:queueName
    //tags
    departmentName: departmentName
    environmentName: environmentName
    solutionName: solutionName
    subscriptionName: subscriptionName
    costCentre: costCentre
    serviceOwnerName: serviceOwnerName
    dateProvisioned: dateProvisioned
    createdBy: createdBy
    deploymentRepo: deploymentRepo
    deploymentScript: 'functionQueue.bicep'
  }
}


//Deploy ETL Function
module etlFunctionAppModule 'application/etlFunctionApp.bicep' = {
  name: 'etlFunctionAppDeploy'
  params: {
    subscription: subscription
    location: location
    applicationInsightsName: applicationInsightsName
    appServiceplanOS: appServiceplanOS
    functionAppRuntime: functionAppRuntime
    keyVaultName: keyVaultModule.outputs.keyVaultName
    databaseConnectionStringURI: databaseModule.outputs.pythonConnectionStringSecretUri
    storageAccountConnectionString: storageAccountModule.outputs.storageAccountConnectionString
    serviceBusConnectionString: serviceBusFunctionQueueModule.outputs.ServiceBusConnectionString
    //tags
    departmentName: departmentName
    environmentName: environmentName
    solutionName: solutionName
    subscriptionName: subscriptionName
    costCentre: costCentre
    serviceOwnerName: serviceOwnerName
    dateProvisioned: dateProvisioned
    createdBy: createdBy
    deploymentRepo: deploymentRepo
    deploymentScript: 'etlFunctionApp.bicep'
  }
  dependsOn: [
    serviceBusFunctionQueueModule
    keyVaultModule
  ]
}




//outputs
output crLoginServer string = containerRegistryModule.outputs.crLoginServer
output crName string = containerRegistryModule.name
output metadataDatabaseRef string = databaseModule.outputs.metadataDatabaseRef




