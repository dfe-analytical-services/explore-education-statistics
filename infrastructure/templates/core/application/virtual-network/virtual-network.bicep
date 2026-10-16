import { ResourceNames } from '../../../common/resource-names.bicep'
import { VNetAddressRanges, VNetSubnets } from 'types.bicep'

@description('Names of the VNet and its subnets.')
param names ResourceNames.vnet

@description('Whether or not to deploy the subnets.')
param deploySubnets bool

@description('CIDR address ranges for the VNet and each of its subnets.')
param addressRanges VNetAddressRanges

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

var vNetName = names.vnet

// Core shared infrastructure subnets.
var applicationGatewaySubnetName = names.subnets.applicationGateway
var eventGridCustomTopicPrivateEndpointsSubnetName = names.subnets.eventGridCustomTopicPrivateEndpoints

// Core application subnets.
var adminSubnetName = names.subnets.admin
var importerSubnetName = names.subnets.importer
var publisherSubnetName = names.subnets.publisher
var notifySubnetName = names.subnets.notifier
var contentSubnetName = names.subnets.contentApi
var dataSubnetName = names.subnets.dataApi
var sqlServerPrivateEndpointsSubnetName = names.subnets.sqlServerPrivateEndpoints

// Screener subnets.
var screenerFunctionAppSubnetName = names.subnets.screenerFunctionApp
var screenerStoragePrivateEndpointsSubnetName = names.subnets.screenerStoragePrivateEndpoints

// Public API subnets.
var publicApiDataProcessorSubnetName = names.subnets.publicApiDataProcessor
var publicApiDataProcessorPrivateEndpointsSubnetName = names.subnets.publicApiDataProcessorPrivateEndpoints
var publicApiStoragePrivateEndpointsSubnetName = names.subnets.publicApiStoragePrivateEndpoints
var containerAppEnvironmentSubnetName = names.subnets.containerAppEnvironment
var psqlFlexibleServerSubnetName = names.subnets.psqlFlexibleServer

// NL Search subnets.
var nlSearchFunctionAppSubnetName = names.subnets.nlSearchFunctionApp
var nlSearchFunctionAppPrivateEndpointsSubnetName = names.subnets.nlSearchFunctionAppPrivateEndpoints

// Search subnets.
var searchDocsFunctionAppSubnetName = names.subnets.searchDocsFunctionApp
var searchDocsFunctionAppPrivateEndpointsSubnetName = names.subnets.searchDocsFunctionAppPrivateEndpoints
var searchStoragePrivateEndpointsSubnetName = names.subnets.searchStoragePrivateEndpoints

// Analytics subnets.
var analyticsFunctionAppSubnetName = names.subnets.analyticsFunctionApp
var analyticsStoragePrivateEndpointsSubnetName = names.subnets.analyticsStoragePrivateEndpoints

var subnets = deploySubnets ? [{
  name: adminSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.admin
    serviceEndpoints: [
      'Microsoft.Sql'
      'Microsoft.Storage'
    ]
    delegations: ['webapp']
  }
}
{
  name: importerSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.importer
    serviceEndpoints: [
      'Microsoft.Sql'
      'Microsoft.Storage'
    ]
    delegations: ['webapp']
  }
}
{
  name: publisherSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.publisher
    serviceEndpoints: [
      'Microsoft.Sql'
      'Microsoft.Storage'
    ]
    delegations: ['webapp']
  }
}
{
  name: notifySubnetName
  properties: {
    addressPrefix: addressRanges.subnets.notify
    serviceEndpoints: [
      'Microsoft.Sql'
      'Microsoft.Storage'
    ]
    delegations: ['webapp']
  }
}
{
  name: contentSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.content
    serviceEndpoints: [
      'Microsoft.Sql'
      'Microsoft.Storage'
    ]
    delegations: ['webapp']
  }
}
{
  name: dataSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.data
    serviceEndpoints: [
      'Microsoft.Sql'
      'Microsoft.Storage'
    ]
    delegations: ['webapp']
  }
}
{
  name: publicApiDataProcessorSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.publicApiDataProcessor
    serviceEndpoints: [
      'Microsoft.Sql'
      'Microsoft.Storage'
    ]
    delegations: ['webapp']
  }
}
{
  name: publicApiDataProcessorPrivateEndpointsSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.publicApiDataProcessorPrivateEndpoints
  }
}
{
  name: containerAppEnvironmentSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.containerAppEnvironment
    serviceEndpoints: ['Microsoft.Storage']
    delegations: ['environment']
  }
}
{
  name: applicationGatewaySubnetName
  properties: {
    addressPrefix: addressRanges.subnets.applicationGateway
  }
}
{
  name: publicApiStoragePrivateEndpointsSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.publicApiStoragePrivateEndpoints
  }
}
{
  name: psqlFlexibleServerSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.psqlFlexibleServer
  }
}
{
  name: searchStoragePrivateEndpointsSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.searchStoragePrivateEndpoints
  }
}
{
  name: searchDocsFunctionAppSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.searchDocsFunctionApp
    serviceEndpoints: ['Microsoft.Storage']
    delegations: ['webapp']
  }
}
{
  name: searchDocsFunctionAppPrivateEndpointsSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.searchDocsFunctionAppPrivateEndpoints
  }
}
{
  name: analyticsStoragePrivateEndpointsSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.analyticsStoragePrivateEndpoints
  }
}
{
  name: analyticsFunctionAppSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.analyticsFunctionApp
    serviceEndpoints: ['Microsoft.Storage']
    delegations: ['webapp']
  }
}
{
  name: screenerStoragePrivateEndpointsSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.screenerStoragePrivateEndpoints
  }
}
{
  name: screenerFunctionAppSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.screenerFunctionApp
    serviceEndpoints: ['Microsoft.Storage']
    delegations: ['webapp']
  }
}
{
  name: eventGridCustomTopicPrivateEndpointsSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.eventGridCustomTopicPrivateEndpoints
  }
}
{
  name: nlSearchFunctionAppPrivateEndpointsSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.nlSearchFunctionAppPrivateEndpoints
  }
}
{
  name: nlSearchFunctionAppSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.nlSearchFunctionApp
    serviceEndpoints: ['Microsoft.Storage']
    delegations: ['webapp']
  }
}
{
  name: sqlServerPrivateEndpointsSubnetName
  properties: {
    addressPrefix: addressRanges.subnets.sqlServerPrivateEndpoints
  }
}] : null

module vNetModule '../../../common/components/virtual-network/virtual-network.bicep' = {
  name: '${vNetName}ModuleDeploy'
  params: {
    vNetName: vNetName
    addressSpacePrefix: addressRanges.vNet
    subnets: subnets
    tagValues: tagValues
  }
}

output vNetName string = vNetName
output eventGridCustomTopicPrivateEndpointsSubnetName string = eventGridCustomTopicPrivateEndpointsSubnetName
output applicationGatewaySubnetName string = applicationGatewaySubnetName

output subnets VNetSubnets = {
  admin: {
    name: adminSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, adminSubnetName)
  }
  importer: {
    name: importerSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, importerSubnetName)
  }
  publisher: {
    name: publisherSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, publisherSubnetName)
  }
  notify: {
    name: notifySubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, notifySubnetName)
  }
  content: {
    name: contentSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, contentSubnetName)
  }
  data: {
    name: dataSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, dataSubnetName)
  }
  publicApiDataProcessor: {
    name: publicApiDataProcessorSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, publicApiDataProcessorSubnetName)
  }
  publicApiDataProcessorPrivateEndpoints: {
    name: publicApiDataProcessorPrivateEndpointsSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, publicApiDataProcessorPrivateEndpointsSubnetName)
  }
  containerAppEnvironment: {
    name: containerAppEnvironmentSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, containerAppEnvironmentSubnetName)
  }
  applicationGateway: {
    name: applicationGatewaySubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, applicationGatewaySubnetName)
  }
  publicApiStoragePrivateEndpoints: {
    name: publicApiStoragePrivateEndpointsSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, publicApiStoragePrivateEndpointsSubnetName)
  }
  psqlFlexibleServer: {
    name: psqlFlexibleServerSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, psqlFlexibleServerSubnetName)
  }
  searchStoragePrivateEndpoints: {
    name: searchStoragePrivateEndpointsSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, searchStoragePrivateEndpointsSubnetName)
  }
  searchDocsFunctionApp: {
    name: searchDocsFunctionAppSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, searchDocsFunctionAppSubnetName)
  }
  searchDocsFunctionAppPrivateEndpoints: {
    name: searchDocsFunctionAppPrivateEndpointsSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, searchDocsFunctionAppPrivateEndpointsSubnetName)
  }
  analyticsStoragePrivateEndpoints: {
    name: analyticsStoragePrivateEndpointsSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, analyticsStoragePrivateEndpointsSubnetName)
  }
  analyticsFunctionApp: {
    name: analyticsFunctionAppSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, analyticsFunctionAppSubnetName)
  }
  screenerStoragePrivateEndpoints: {
    name: screenerStoragePrivateEndpointsSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, screenerStoragePrivateEndpointsSubnetName)
  }
  screenerFunctionApp: {
    name: screenerFunctionAppSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, screenerFunctionAppSubnetName)
  }
  eventGridCustomTopicPrivateEndpoints: {
    name: eventGridCustomTopicPrivateEndpointsSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, eventGridCustomTopicPrivateEndpointsSubnetName)
  }
  nlSearchFunctionAppPrivateEndpoints: {
    name: nlSearchFunctionAppPrivateEndpointsSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, nlSearchFunctionAppPrivateEndpointsSubnetName)
  }
  nlSearchFunctionApp: {
    name: nlSearchFunctionAppSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, nlSearchFunctionAppSubnetName)
  }
  sqlServerPrivateEndpoints: {
    name: sqlServerPrivateEndpointsSubnetName
    id: resourceId('Microsoft.Network/virtualNetworks/subnets', vNetName, sqlServerPrivateEndpointsSubnetName)
  }
}
