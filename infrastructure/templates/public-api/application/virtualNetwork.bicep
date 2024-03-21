@description('Subscription Name e.g. s101d01. Used as a prefix for created resources')
param subscription string

@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the name suffix of the API Container App')
param apiContainerAppName string

@description('Specifies the name suffix of the Data Processor Function App')
param dataProcessorFunctionAppName string

@description('Specifies the name suffix of the PostgreSQL Flexible Server')
param postgreSqlServerName string

var vNetName = 's101d01-vnet-eesdw'
var dataProcessorSubnetName = '${resourcePrefix}-snet-fa-${dataProcessorFunctionAppName}'
var postgreSqlSubnetName = '${resourcePrefix}-snet-${postgreSqlServerName}'
var apiContainerAppSubnetName = '${resourcePrefix}-snet-ca-${apiContainerAppName}'

// Note that the current vNet has subnets with reserved address ranges up to 10.0.5.0/24 currently.
var dataProcessorSubnetPrefix = '10.0.10.0/24'
var postgreSqlSubnetPrefix = '10.0.7.0/24'
var apiContainerAppSubnetPrefix = '10.0.8.0/24'

// Reference the existing VNet.
resource vNet 'Microsoft.Network/virtualNetworks@2023-09-01' existing = {
  name: vNetName
}

var dataProcessorSubnet = {
  name: dataProcessorSubnetName
  properties: {
    addressPrefix: dataProcessorSubnetPrefix
    delegations: [
      {
        name: '${resourcePrefix}-snet-delegation-fa-${dataProcessorFunctionAppName}'
        properties: {
          serviceName: 'Microsoft.Web/serverFarms'
        }
      }
    ]
  }
}

var postgreSqlSubnet = {
  name: postgreSqlSubnetName
  properties: {
    addressPrefix: postgreSqlSubnetPrefix
    delegations: [
    {
      name: '${resourcePrefix}-snet-delegation-${postgreSqlServerName}'
      properties: {
        serviceName: 'Microsoft.DBforPostgreSQL/flexibleServers'
      }
    }]
  }
}

var apiContainerAppSubnet = {
  name: apiContainerAppSubnetName
  properties: {
    addressPrefix: apiContainerAppSubnetPrefix
    delegations: [
      {
        name: '${resourcePrefix}-snet-delegation-cae-${apiContainerAppName}'
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
  apiContainerAppSubnet
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

@description('The fully qualified Azure resource ID of the PostgreSQL Flexible Server Subnet.')
output postgreSqlSubnetRef string = resourceId('Microsoft.Network/VirtualNetworks/subnets', vNetName, postgreSqlSubnetName)

@description('The fully qualified Azure resource ID of the API Container App Subnet.')
output apiContainerAppSubnetRef string = resourceId('Microsoft.Network/VirtualNetworks/subnets', vNetName, apiContainerAppSubnetName)
