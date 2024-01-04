@description('Specifies the Subscription to be used.')
param subscription string

@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Specifies the login server from the registry.')
param acrLoginServer string

@description('Specifies the container image to deploy from the registry.')
param acrHostedImageName string = 'containerapps-helloworld'

@minLength(2)
@maxLength(32)
@description('Specifies the name of the container app.')
param containerAppName string = uniqueString(resourceGroup().id)

@description('Specifies the name of the container app environment.')
param containerAppEnvName string = uniqueString(resourceGroup().id)

@description('Specifies the name of the log analytics workspace.')
param containerAppLogAnalyticsName string = uniqueString(resourceGroup().id)

@description('Specifies the container port.')
param targetPort int = 80

@description('Number of CPU cores the container can use. Can be with a maximum of two decimals.')
@allowed([
  '0.25'
  '0.5'
  '0.75'
  '1'
  '1.25'
  '1.5'
  '1.75'
  '2'
])
param cpuCore string = '0.5'

@description('Amount of memory (in gibibytes, GiB) allocated to the container up to 4GiB. Can be with a maximum of two decimals. Ratio with CPU cores must be equal to 2.')
@allowed([
  '0.5'
  '1'
  '1.5'
  '2'
  '3'
  '3.5'
  '4'
])
param memorySize string = '1'

@description('Minimum number of replicas that will be deployed')
@minValue(0)
@maxValue(25)
param minReplica int = 1

@description('Maximum number of replicas that will be deployed')
@minValue(0)
@maxValue(25)
param maxReplica int = 3

@minLength(5)
@maxLength(50)
@description('Name of the azure container registry (must be globally unique)')
param containerRegistryName string = 'eesapiacr'

@description('Specifies the base docker container image to deploy.')
param containerSeedImage string = 'mcr.microsoft.com/azuredocs/aci-helloworld'

@description('Select if you want to seed the ACR with a base image.')
param seedRegistry bool = true

@description('Database Connection String')
param databaseConnectionString string

@description('Key Vault URI Connection String reference')
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


//Variables 
var ContainerEnvName = '${subscription}-env-${containerAppEnvName}'
var containerImageName = '${acrLoginServer}/${acrHostedImageName}'
var ContainerAppName = '${subscription}-app-${containerAppName}'
var UserIdentityName = '${subscription}-id-${containerAppName}'
var ContainerLogName = '${subscription}-log-${containerAppLogAnalyticsName}'
var ContainerImportName = '${subscription}importContainerImage'

//var revisionSuffix = uniqueString(containerImage, containerAppName)
//var sanitizedRevisionSuffix = substring(revisionSuffix, 0, 10)

var acrPullRole = resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')


//Resources 

//Log Analytics
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: ContainerLogName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
  tags: {
    Department: departmentName
    Solution: solutionName
    ServiceType: 'Operation Insights'
    Environment: environmentName
    Subscription: subscriptionName
    CostCentre: costCentre
    ServiceOwner: serviceOwnerName
    DateProvisioned: dateProvisioned
    CreatedBy: createdBy
    DeploymentRepo: deploymentRepo
    DeploymentScript: deploymentScript
  }
}

//Registry Seeder
@description('This module seeds the ACR with the public version of the app')
module acrImportImage 'br/public:deployment-scripts/import-acr:3.0.1' = if (seedRegistry) {
  name: ContainerImportName
  params: {
    useExistingManagedIdentity: true
    managedIdentityName: uai.name
    existingManagedIdentityResourceGroupName: resourceGroup().name
    existingManagedIdentitySubId: az.subscription().subscriptionId
    acrName: containerRegistryName
    location: location
    images: array(containerSeedImage)
  }
}


//Managed Identity
resource uai 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' = {
  name: UserIdentityName
  location: location
}

@description('This allows the managed identity of the container app to access the registry, note scope is applied to the wider ResourceGroup not the ACR')
resource uaiRbac 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, uai.id, acrPullRole)
  properties: {
    roleDefinitionId: acrPullRole
    principalId: uai.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

//Container environment
resource containerAppEnv 'Microsoft.App/managedEnvironments@2022-06-01-preview' = {
  name: ContainerEnvName
  location: location
  sku: {
    name: 'Consumption'
  }
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
  tags: {
    Department: departmentName
    Solution: solutionName
    ServiceType: 'Managed Environment'
    Environment: environmentName
    Subscription: subscriptionName
    CostCentre: costCentre
    ServiceOwner: serviceOwnerName
    DateProvisioned: dateProvisioned
    CreatedBy: createdBy
    DeploymentRepo: deploymentRepo
    DeploymentScript: deploymentScript
  }
}

//Container Application
resource containerApp 'Microsoft.App/containerApps@2022-06-01-preview' = {
  name: ContainerAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${uai.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      maxInactiveRevisions: 1
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: targetPort
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: [
        {
          identity: uai.id
          server: acrLoginServer
        }
      ]
    }
    template: {
//      revisionSuffix: sanitizedRevisionSuffix
      containers: [
        {
          name: containerAppName 
          image: containerImageName //'${azureContainerRegistry}/testimage:latest'
          env: [
            {
              name: 'adoDBConnectionString'
              value: databaseConnectionString
            }
            {
              name: 'serviceBusConnectionString'
              value: serviceBusConnectionString
            }
          ]
          resources: {
            cpu: json(cpuCore)
            memory: '${memorySize}Gi'
          }
        }
      ]
      scale: {
        minReplicas: minReplica
        maxReplicas: maxReplica
        rules: [
          {
            name: 'http-requests'
            http: {
              metadata: {
                concurrentRequests: '10'
              }
            }
          }
        ]
      }
    }
  }
  tags: {
    Department: departmentName
    Solution: solutionName
    ServiceType: 'Container App'
    Environment: environmentName
    Subscription: subscriptionName
    CostCentre: costCentre
    ServiceOwner: serviceOwnerName
    DateProvisioned: dateProvisioned
    CreatedBy: createdBy
    DeploymentRepo: deploymentRepo
    DeploymentScript: deploymentScript
  }
}



// Outputs for exported use
output containerAppFQDN string = containerApp.properties.configuration.ingress.fqdn
output containerImage string = containerImageName
