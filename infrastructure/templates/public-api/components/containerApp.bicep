@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Specifies the login server from the registry.')
param acrLoginServer string

@description('Specifies the container image to deploy from the registry.')
param containerAppImageName string

@minLength(2)
@maxLength(32)
@description('Specifies the name of the container app.')
param containerAppName string

@description('Specifies the name of the container app environment.')
param containerAppEnvName string

@description('Specifies the name of the log analytics workspace.')
param containerAppLogAnalyticsName string

@description('Specifies the container port.')
param containerAppTargetPort int = 80

@description('Select if you want to use a public dummy image to start the container app.')
param useDummyImage bool

@description('Number of CPU cores the container can use. Can be with a maximum of two decimals.')
@allowed([
  '1'
  '2'
  '3'
  '4'
])
param cpuCore string = '4'

@description('Amount of memory (in gibibytes, GiB) allocated to the container up to 4GiB. Can be with a maximum of two decimals. Ratio with CPU cores must be equal to 2.')
@allowed([
  '1'
  '2'
  '3'
  '4'
  '5'
  '6'
  '7'
  '8'
])
param memorySize string = '8'

@description('Minimum number of replicas that will be deployed')
@minValue(0)
@maxValue(25)
param minReplica int = 1

@description('Maximum number of replicas that will be deployed')
@minValue(0)
@maxValue(25)
param maxReplica int = 3

@description('Specifies the database connection string')
@secure()
param dbConnectionString string

@description('Specifies the service bus connection string.')
@secure()
param serviceBusConnectionString string

//Passed in Tags
param tagValues object


//Variables 
//var containerImageName = '${acrLoginServer}/${acrHostedImageName}'
var containerImageName = useDummyImage == true ? 'mcr.microsoft.com/azuredocs/aci-helloworld' : '${acrLoginServer}/${containerAppImageName}'
var containerEnvName = '${resourcePrefix}-cae-${containerAppEnvName}'
var containerApplicationName = toLower('${resourcePrefix}-ca-${containerAppName}')
var userIdentityName = '${resourcePrefix}-id-${containerAppName}'
var containerLogName = '${resourcePrefix}-log-${containerAppLogAnalyticsName}'
var applicationInsightsName ='${resourcePrefix}-ai-${containerAppName}'
var acrPullRole = resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')



//Resources 

//Log Analytics
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: containerLogName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
  tags: tagValues
}

//Application Insights Deployment
module applicationInsightsModule '../components/appInsights.bicep' = {
  name: 'appInsightsDeploy-${containerAppName}'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    appInsightsName: applicationInsightsName
  }
}

//Managed Identity
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userIdentityName
  location: location
}

@description('This allows the managed identity of the container app to access the registry, note scope is applied to the wider ResourceGroup not the ACR')
resource managedIdentityRBAC 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!useDummyImage) {
  name: guid(resourceGroup().id, managedIdentity.id, acrPullRole)
  properties: {
    roleDefinitionId: acrPullRole
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

//Container environment
resource containerAppEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerEnvName
  location: location
  properties: {
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

//Container Application
resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: containerApplicationName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      maxInactiveRevisions: 1
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: containerAppTargetPort
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }

    }
    template: {
      containers: [
        {
          name: containerAppName 
          image: containerImageName
          env: [
            {
              name: 'dbConnectionString'
              value: dbConnectionString
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
    workloadProfileName: 'Consumption'
  }
  tags: tagValues
}


// Outputs for exported use
output containerAppFQDN string = containerApp.properties.configuration.ingress.fqdn
output containerImage string = containerImageName
output managedIdentityName string = managedIdentity.name
