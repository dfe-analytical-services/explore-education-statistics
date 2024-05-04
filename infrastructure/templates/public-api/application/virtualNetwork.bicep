@description('Specifies the full name of the existing VNet')
param vNetName string

@description('Specifies the Subscription name')
param subscription string

@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the name suffix of the Data Processor Function App')
param dataProcessorFunctionAppNameSuffix string

@description('Specifies the name suffix of the Container App Environment')
param containerAppEnvironmentNameSuffix string

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: vNetName
}

resource adminSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-09-01' existing = {
  name: '${subscription}-snet-ees-admin'
  parent: vNet
}

resource publisherSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-09-01' existing = {
  name: '${subscription}-snet-ees-publisher'
  parent: vNet
}

resource dataProcessorSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-09-01' existing = {
  name: '${resourcePrefix}-snet-fa-${dataProcessorFunctionAppNameSuffix}'
  parent: vNet
}

resource containerAppEnvironmentSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-09-01' existing = {
  name: '${subscription}-ees-snet-cae-${containerAppEnvironmentNameSuffix}'
  parent: vNet
}

@description('The fully qualified Azure resource ID of the Network.')
output vNetRef string = resourceId('Microsoft.Network/VirtualNetworks', vNetName)

@description('The fully qualified Azure resource ID of the Data Processor Function App Subnet.')
output dataProcessorSubnetRef string = dataProcessorSubnet.id

@description('The first usable IP address for the Data Processor Function App Subnet.')
output dataProcessorSubnetStartIpAddress string = parseCidr(dataProcessorSubnet.properties.addressPrefix).firstUsable

@description('The last usable IP address for the Data Processor Function App Subnet.')
output dataProcessorSubnetEndIpAddress string = parseCidr(dataProcessorSubnet.properties.addressPrefix).lastUsable

@description('The fully qualified Azure resource ID of the API Container App Subnet.')
output containerAppEnvironmentSubnetRef string = containerAppEnvironmentSubnet.id

@description('The first usable IP address for the API Container App Subnet.')
output containerAppEnvironmentSubnetStartIpAddress string = parseCidr(containerAppEnvironmentSubnet.properties.addressPrefix).firstUsable

@description('The last usable IP address for the API Container App Subnet.')
output containerAppEnvironmentSubnetEndIpAddress string = parseCidr(containerAppEnvironmentSubnet.properties.addressPrefix).lastUsable

@description('The first usable IP address for the Admin App Service Subnet.')
output adminAppServiceSubnetStartIpAddress string = parseCidr(adminSubnet.properties.addressPrefix).firstUsable

@description('The last usable IP address for the Admin App Service Subnet.')
output adminAppServiceSubnetEndIpAddress string = parseCidr(adminSubnet.properties.addressPrefix).lastUsable

@description('The first usable IP address for the Publisher Function App Subnet.')
output publisherFunctionAppSubnetStartIpAddress string = parseCidr(publisherSubnet.properties.addressPrefix).firstUsable

@description('The last usable IP address for the Publisher Function App Subnet.')
output publisherFunctionAppSubnetEndIpAddress string = parseCidr(publisherSubnet.properties.addressPrefix).lastUsable
