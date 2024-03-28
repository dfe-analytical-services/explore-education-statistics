@description('Specifies the subscription name of the Container App Environment - used for creating name prefix')
param subscription string

@description('Specifies the location of the Container App Environment - defaults to that of the Resource Group')
param location string

@description('Specifies the dedicated subnet created for the Container App Environment within the main VNet')
param subnetId string

@description('Specifies the name of the Log Analytics workspace responsible for capturing logging from this resource')
param logAnalyticsWorkspaceName string

@description('Specifies the Application Insights key that is associated with this resource')
param applicationInsightsKey string

@description('Specifies the workload profiles for this Container App Environment - defaults to Consumption')
param workloadProfiles {
 name: string
 workloadProfileType: string
}[] = [{
 name: 'Consumption'
 workloadProfileType: 'Consumption'
}]

@description('Specifies a set of tags with which to tag the resource in Azure')
param tagValues object

@description('Specifies the name of the new Resource Group created to host the resources of the Container App Environment')
param infrastructureResourceGroupName string

var containerAppEnvironmentName = '${subscription}-ees-cae'

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' existing = {
  name: logAnalyticsWorkspaceName
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppEnvironmentName
  location: location
  properties: {
    vnetConfiguration: {
      infrastructureSubnetId: subnetId
    }
    daprAIInstrumentationKey: applicationInsightsKey
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
    infrastructureResourceGroup: infrastructureResourceGroupName
    workloadProfiles: workloadProfiles
  }
  tags: tagValues
}

output containerAppEnvironmentName string = containerAppEnvironmentName
output containerAppEnvironmentId string = containerAppEnvironment.id
