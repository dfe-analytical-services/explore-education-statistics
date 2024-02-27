@description('Subscription Name e.g. s101d01. Used as a prefix for created resources')
param subscription string

@description('Specifies the Resource Prefix')
param resourcePrefix string

// Variables and created data
var vNetName = '${subscription}-vnet-eesdw'
var dataProcessorSubnetName = '${resourcePrefix}-snet-fa-data-processor'
var postgreSqlSubnetName = '${resourcePrefix}-snet-psql'
var apiContainerAppSubnetName = '${resourcePrefix}-snet-ca-api'

// Note that the current vNet has subnets with reserved address ranges up to 10.0.5.0/24 currently.
var dataProcessorSubnetPrefix = '10.0.6.0/24'
var postgreSqlSubnetPrefix = '10.0.7.0/24'
var apiContainerAppSubnetPrefix = '10.0.8.0/24'


//Resources 

// Reference the existing VNet.
resource vNet 'Microsoft.Network/virtualNetworks@2023-04-01' existing = {
  name: vNetName
}

// Define a subnet for the Data Processor Function App.
var dataProcessorSubnet = {
  name: dataProcessorSubnetName
  properties: {
    addressPrefix: dataProcessorSubnetPrefix
    delegations: [
      {
        name: '${resourcePrefix}-snet-delegation-fa-data-processor'
        properties: {
          serviceName: 'Microsoft.Web/serverFarms'
        }
      }
    ]
  }
}

// Define a subnet for the PostgreSQL flexible database server.
var postgreSqlSubnet = {
  name: postgreSqlSubnetName
  properties: {
    addressPrefix: postgreSqlSubnetPrefix
    delegations: [
    {
      name: '${resourcePrefix}-snet-delegation-psql'
      properties: {
        serviceName: 'Microsoft.DBforPostgreSQL/flexibleServers'
      }
    }]
  }
}

// Define a subnet for the API Container App.
var apiContainerAppSubnet = {
  name: apiContainerAppSubnetName
  properties: {
    addressPrefix: apiContainerAppSubnetPrefix
    delegations: [
      {
        name: '${resourcePrefix}-snet-delegation-cae-api'
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
resource subnetResources 'Microsoft.Network/virtualNetworks/subnets@2020-11-01' = [for subnet in subnets: {
  parent: vNet
  name: subnet.name
  properties: subnet.properties
}]


// Outputs for exported use
@description('The fully qualified Azure resource ID of the Network.')
output vNetRef string = resourceId('Microsoft.Network/VirtualNetworks', vNetName)

@description('The fully qualified Azure resource ID of the Data Processor Function App Subnet.')
output dataProcessorSubnetRef string = resourceId('Microsoft.Network/VirtualNetworks/subnets', vNetName, dataProcessorSubnetName)

@description('The fully qualified Azure resource ID of the PostgreSQL Flexible Server Subnet.')
output postgreSqlSubnetRef string = resourceId('Microsoft.Network/VirtualNetworks/subnets', vNetName, postgreSqlSubnetName)

@description('The fully qualified Azure resource ID of the API Container App Subnet.')
output apiContainerAppSubnetRef string = resourceId('Microsoft.Network/VirtualNetworks/subnets', vNetName, apiContainerAppSubnetName)
