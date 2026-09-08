@description('Name of the App Service to connect to the VNet.')
param appServiceName string

@description('Name of the VNet.')
param vNetName string

@description('Name of the subnet.')
param subnetName string

var subnetRef = resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, subnetName)

resource virtualNetworkLink 'Microsoft.Web/sites/config@2025-03-01' = if (subnetRef != null) {
  name: '${appServiceName}/virtualNetwork'
  properties: {
    subnetResourceId: subnetRef
    swiftSupported: true
  }
}
