@description('Specifies the full name of the existing VNet')
param vNetName string

@description('Specifies the Public API resource prefix')
param publicApiResourcePrefix string

@description('Specifies common resource prefix')
param commonResourcePrefix string

@description('Specifies legacy resource prefix')
param legacyResourcePrefix string

var adminAppServiceName = '${legacyResourcePrefix}-as-ees-admin'
var publisherFunctionAppName = '${legacyResourcePrefix}-fa-ees-publisher'
var dataProcessorName = '${publicApiResourcePrefix}-fa-processor'
var psqlFlexibleServerName = '${commonResourcePrefix}-psql-flexibleserver'
var containerAppEnvironmentName = '${commonResourcePrefix}-cae-01'
var appGatewayName = '${commonResourcePrefix}-agw-01'

var dataProcessorSubnetName = replace(dataProcessorName, publicApiResourcePrefix, '${publicApiResourcePrefix}-snet-')
var dataProcessorPrivateEndpointsSubnetName = '${dataProcessorSubnetName}-pep'
var containerAppEnvironmentSubnetName = replace(containerAppEnvironmentName, commonResourcePrefix, '${commonResourcePrefix}-snet-')
var psqlFlexibleServerSubnetName = replace(psqlFlexibleServerName, commonResourcePrefix, '${commonResourcePrefix}-snet-')
var appGatewaySubnetName = replace(appGatewayName, commonResourcePrefix, '${commonResourcePrefix}-snet-')
var adminAppServiceSubnetName = replace(adminAppServiceName, legacyResourcePrefix, '${legacyResourcePrefix}-snet-')
var publisherFunctionAppSubnetName = replace(publisherFunctionAppName, legacyResourcePrefix, '${legacyResourcePrefix}-snet-')

resource vNet 'Microsoft.Network/virtualNetworks@2023-11-01' existing = {
  name: vNetName
}

resource adminSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: adminAppServiceSubnetName
  parent: vNet
}

resource publisherSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: publisherFunctionAppSubnetName
  parent: vNet
}

resource dataProcessorSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: dataProcessorSubnetName
  parent: vNet
}

resource dataProcessorPrivateEndpointSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: dataProcessorPrivateEndpointsSubnetName
  parent: vNet
}

resource containerAppEnvironmentSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: containerAppEnvironmentSubnetName
  parent: vNet
}

resource psqlFlexibleServerSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: psqlFlexibleServerSubnetName
  parent: vNet
}

resource appGatewaySubnet 'Microsoft.Network/virtualNetworks/subnets@2023-11-01' existing = {
  name: appGatewaySubnetName
  parent: vNet
}

@description('The fully qualified Azure resource ID of the Network.')
output vnetId string = resourceId('Microsoft.Network/VirtualNetworks', vNetName)

@description('The fully qualified Azure resource ID of the Data Processor Function App Subnet.')
output dataProcessorSubnetRef string = dataProcessorSubnet.id

@description('The first usable IP address for the Data Processor Function App Subnet.')
output dataProcessorSubnetStartIpAddress string = parseCidr(dataProcessorSubnet.properties.addressPrefix).firstUsable

@description('The last usable IP address for the Data Processor Function App Subnet.')
output dataProcessorSubnetEndIpAddress string = parseCidr(dataProcessorSubnet.properties.addressPrefix).lastUsable

@description('The fully qualified Azure resource ID of the Data Processor Function App Private Endpoint Subnet.')
output dataProcessorPrivateEndpointSubnetRef string = dataProcessorPrivateEndpointSubnet.id

@description('The first usable IP address for the Data Processor Function App Private Endpoint Subnet.')
output dataProcessorPrivateEndpointSubnetStartIpAddress string = parseCidr(dataProcessorPrivateEndpointSubnet.properties.addressPrefix).firstUsable

@description('The last usable IP address for the Data Processor Function App Private Endpoint Subnet.')
output dataProcessorPrivateEndpointSubnetEndIpAddress string = parseCidr(dataProcessorPrivateEndpointSubnet.properties.addressPrefix).lastUsable

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

@description('The fully qualified Azure resource ID of the PSQL Flexible Server Subnet.')
output psqlFlexibleServerSubnetRef string = psqlFlexibleServerSubnet.id

@description('The first usable IP address for the PSQL Flexible Server Subnet.')
output psqlFlexibleServerSubnetStartIpAddress string = parseCidr(psqlFlexibleServerSubnet.properties.addressPrefix).firstUsable

@description('The last usable IP address for the PSQL Flexible Server Subnet.')
output psqlFlexibleServerSubnetEndIpAddress string = parseCidr(psqlFlexibleServerSubnet.properties.addressPrefix).lastUsable

@description('The fully qualified Azure resource ID of the App Gateway Subnet.')
output appGatewaySubnetRef string = appGatewaySubnet.id
