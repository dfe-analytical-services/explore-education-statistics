@description('Specifies the full name of the existing VNet')
param vNetName string

@description('Specifies the Subscription name')
param subscription string

@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the name suffix of the Data Processor Function App')
param dataProcessorFunctionAppName string

@description('Specifies the name suffix of the PostgreSQL Flexible Server')
param postgreSqlServerName string

var dataProcessorSubnetName = '${resourcePrefix}-snet-fa-${dataProcessorFunctionAppName}'
var postgreSqlSubnetName = '${subscription}-ees-snet-${postgreSqlServerName}'
var containerAppEnvironmentSubnetName = '${subscription}-ees-snet-cae-01'

// Note that the current vNet has subnets with reserved address ranges up to 10.0.5.0/24 currently.
var dataProcessorSubnetRange = '10.0.6.0/24'
var postgreSqlSubnetRange = '10.0.7.0/24'
var containerAppEnvironmentSubnetRange = '10.0.8.0/24'

// Reference the existing VNet.
resource vNet 'Microsoft.Network/virtualNetworks@2023-09-01' existing = {
  name: vNetName
}

var dataProcessorSubnet = {
  name: dataProcessorSubnetName
  properties: {
    addressPrefix: dataProcessorSubnetRange
    delegations: [
      {
        name: '${resourcePrefix}-snet-delegation-fa-${dataProcessorFunctionAppName}'
        properties: {
          serviceName: 'Microsoft.Web/serverFarms'
        }
      }
    ]
    serviceEndpoints: [
      {
         service: 'Microsoft.Storage'
      }
    ]
  }
}

var postgreSqlSubnet = {
  name: postgreSqlSubnetName
  properties: {
    addressPrefix: postgreSqlSubnetRange
    delegations: [
    {
      name: '${resourcePrefix}-snet-delegation-${postgreSqlServerName}'
      properties: {
        serviceName: 'Microsoft.DBforPostgreSQL/flexibleServers'
      }
    }]
  }
}

var containerAppEnvironmentSubnet = {
  name: containerAppEnvironmentSubnetName
  properties: {
    addressPrefix: containerAppEnvironmentSubnetRange
    delegations: [
      {
        name: '${resourcePrefix}-snet-delegation-cae'
        properties: {
          serviceName: 'Microsoft.App/environments'
        }
      }
    ]
  }
}

var subnets = [
  dataProcessorSubnet
  postgreSqlSubnet
  containerAppEnvironmentSubnet
]

// Create the subnets sequentially rather than in parallel to avoid "AnotherOperationInProgress" errors when multiple
// subnets attempt to update the parent VNet at the same time.
@batchSize(1)
resource subnetResources 'Microsoft.Network/virtualNetworks/subnets@2023-09-01' = [for subnet in subnets: {
  parent: vNet
  name: subnet.name
  properties: subnet.properties
}]

@description('The fully qualified Azure resource ID of the Network.')
output vNetRef string = resourceId('Microsoft.Network/VirtualNetworks', vNetName)

@description('The fully qualified Azure resource ID of the Data Processor Function App Subnet.')
output dataProcessorSubnetRef string = resourceId('Microsoft.Network/VirtualNetworks/subnets', vNetName, dataProcessorSubnetName)

@description('The first usable IP address for the Data Processor Function App Subnet.')
output dataProcessorSubnetStartIpAddress string = parseCidr(dataProcessorSubnetRange).firstUsable

@description('The last usable IP address for the Data Processor Function App Subnet.')
output dataProcessorSubnetEndIpAddress string = parseCidr(dataProcessorSubnetRange).lastUsable

@description('The fully qualified Azure resource ID of the PostgreSQL Flexible Server Subnet.')
output postgreSqlSubnetRef string = resourceId('Microsoft.Network/VirtualNetworks/subnets', vNetName, postgreSqlSubnetName)

@description('The fully qualified Azure resource ID of the API Container App Subnet.')
output containerAppEnvironmentSubnetRef string = resourceId('Microsoft.Network/VirtualNetworks/subnets', vNetName, containerAppEnvironmentSubnetName)

@description('The first usable IP address for the API Container App Subnet.')
output containerAppEnvironmentSubnetStartIpAddress string = parseCidr(containerAppEnvironmentSubnetRange).firstUsable

@description('The last usable IP address for the API Container App Subnet.')
output containerAppEnvironmentSubnetEndIpAddress string = parseCidr(containerAppEnvironmentSubnetRange).lastUsable
