@description('Name of the App Service that owns the swap slot.')
param appServiceName string

@description('Name of the swap slot to connect to the VNet.')
param slotName string

@description('Name of the VNet.')
param vNetName string

@description('Name of the subnet.')
param subnetName string

var subnetRef = resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, subnetName)

resource virtualNetworkLink 'Microsoft.Web/sites/slots/config@2025-03-01' = if (subnetRef != null) {
  name: '${appServiceName}/${slotName}/virtualNetwork'
  properties: {
    subnetResourceId: subnetRef
    swiftSupported: true
  }
}
