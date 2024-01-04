@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('Function App name')
param functionAppName string

@description('App Service Plan Id')
param planId string

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
var kind = 'functionapp'


//Resources
resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppName
  location: location
  kind: kind
  properties: {
    serverFarmId: planId
  }
  identity: {
    type: 'SystemAssigned'
  }
  tags: {
    Department: departmentName
    Solution: solutionName
    ServiceType: 'App Functions'
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


//Output
output functionAppName string = functionApp.name
output principalId string = functionApp.identity.principalId
output tenantId string = functionApp.identity.tenantId
