@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

@description('Specifies the login server from the registry.')
param acrLoginServer string

@description('Specifies the container image to deploy from the registry.')
param containerAppImageName string

@minLength(2)
@maxLength(32)
@description('Specifies the name of the container app.')
param containerAppName string

@description('Specifies the container port.')
param containerAppTargetPort int = 80

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
param appSettings {
  name: string
  @secure()
  value: string
}[]

@description('Specifies the subnet id')
param subnetId string

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

@description('The Application Insights key that is associated with this resource')
param applicationInsightsKey string

@description('An existing Managed Identity with which to associate this Container App')
param managedIdentity object?

var containerImageName = '${acrLoginServer}/${containerAppImageName}'
var containerEnvName = '${resourcePrefix}-cae-${containerAppName}'
var containerApplicationName = toLower('${resourcePrefix}-ca-${containerAppName}')
var containerLogName = '${resourcePrefix}-log-${containerAppName}'
var acrPullRole = resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

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

@description('This allows the managed identity of the container app to access the registry, note scope is applied to the wider ResourceGroup not the ACR')
resource managedIdentityRBAC 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (managedIdentity != null) {
  name: guid(resourceGroup().id, managedIdentity!.id, acrPullRole)
  properties: {
    roleDefinitionId: acrPullRole
    principalId: managedIdentity!.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerEnvName
  location: location
  properties: {
    vnetConfiguration: {
      infrastructureSubnetId: subnetId
    }
    daprAIInstrumentationKey: applicationInsightsKey
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



var containerAppIdentity = managedIdentity != null ? {
  type: 'UserAssigned'
  userAssignedIdentities: {
    '${managedIdentity!.id}': {}
  }
} : {
  type: 'SystemAssigned'
}

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: containerApplicationName
  location: location
  identity: containerAppIdentity
  properties: {
    managedEnvironmentId: containerAppEnvironment.id
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
          env: appSettings
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

output containerAppFQDN string = containerApp.properties.configuration.ingress.fqdn
output containerImage string = containerImageName
output managedIdentityName string = managedIdentity.name
