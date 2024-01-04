@description('Specifies the Subscription to be used.')
param subscription string

@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Application : Insights name')
param applicationInsightsName string

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

@description('Specifies the name of the Key Vault.')
param keyVaultName string

@description('Database Connection String URI')
@secure()
param databaseConnectionStringURI string

@description('Storage Account Connection String')
param storageAccountConnectionString string

@description('Service Bus Connection String reference')
param serviceBusConnectionString string


//Passed in Tags
param departmentName string = 'Public API'
param environmentName string = 'Development'
param solutionName string = 'API'
param subscriptionName string = 'Unknown'
param costCentre string = 'Unknown'
param serviceOwnerName string = 'Unknown'
param dateProvisioned string = utcNow('u')
param createdBy string = 'Unknown'
param deploymentRepo string = 'N/A'
param deploymentScript string = 'N/A'


// Variables and created data
var buildNumber = uniqueString(resourceGroup().id)
var functionAppName = '${subscription}-fa-${buildNumber}'
var appServicePlanName = '${subscription}-apsp-${buildNumber}'


//---------------------------------------------------------------------------------------------------------------
// All resources via modules
//---------------------------------------------------------------------------------------------------------------

//Application Insights Deployment
module applicationInsightsModule '../components/appInsights.bicep' = {
  name: 'appInsightsDeploy-${buildNumber}'
  params: {
    location: location
    appInsightsName: applicationInsightsName
  }
}


//App Service Plan Deployment
module appServicePlan '../components/appServicePlan.bicep' = {
  name: 'servicePlanDeploy-${buildNumber}'
  params: {
    name: appServicePlanName
    location: location
    os: appServiceplanOS
  }
}


//Function App Deployment
module functionAppModule '../components/appFunction.bicep' = {
  name: 'funcionAppDeploy-${buildNumber}'
  params: {
    functionAppName: functionAppName
    location: location
    planId: appServicePlan.outputs.servicePlanId
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
    deploymentScript: deploymentScript
  }
  dependsOn: [
    applicationInsightsModule
    appServicePlan
  ]
}


//Key Vault Access Policy Deployment
module keyVaultAccessPolicy '../components/keyVaultAccessPolicy.bicep' = {
  name: 'keyVaultAccessPolicyDeploy-${buildNumber}'
  params: {
    keyVaultName: keyVaultName
    principalId: functionAppModule.outputs.principalId
    tenantId: functionAppModule.outputs.tenantId
  }
  dependsOn: [
    functionAppModule
  ]
}


//Function App Settings Deployment
module functionAppSettingsModule '../components/appFunctionSettings.bicep' = {
  name: 'functionConfDeploy-${buildNumber}'
  params: {
    applicationInsightsKey: applicationInsightsModule.outputs.applicationInsightsKey
    databaseConnectionString: databaseConnectionStringURI
    functionAppName: functionAppModule.outputs.functionAppName
    functionAppRuntime: functionAppRuntime
    storageAccountConnectionString: storageAccountConnectionString
    serviceBusConnectionString: serviceBusConnectionString
  }
  dependsOn: [
    functionAppModule
  ]
}

