import { abbreviations } from '../../../common/abbreviations.bicep'
import { VNetAddressRanges, VNetSubnets } from 'types.bicep'

@description('Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string

@description('Whether or not to deploy the subnets.')
param deploySubnets bool

@description('CIDR address ranges for the VNet and each of its subnets.')
param addressRanges VNetAddressRanges

@description('A set of tags with which to tag the resource in Azure.')
param tagValues object

var vNetName = '${subscription}-${abbreviations.networkVirtualNetworks}-ees'

var environment = 'ees'
var subnetAbbreviation = abbreviations.networkVirtualNetworksSubnets

// TODO EES-7502 - we have a number of different naming conventions for subnets.
// Handle as part of the overall naming convention alignment.
var generalSubnetNamePrefix = '${subscription}-${environment}-${subnetAbbreviation}-'
var legacySubnetNamePrefix = '${subscription}-${subnetAbbreviation}-${environment}-'
var publicApiSubnetNamePrefix = '${subscription}-${environment}-papi-${subnetAbbreviation}-'
var screenerSubnetNamePrefix = '${subscription}-${environment}-sapi-${subnetAbbreviation}-'

// Core shared infrastructure subnets.
var applicationGatewaySubnetName = '${generalSubnetNamePrefix}agw-01'
var eventGridCustomTopicPrivateEndpointsSubnetName = '${generalSubnetNamePrefix}evgt-pep'

// Core application subnets. 
var adminSubnetName = '${legacySubnetNamePrefix}admin'
var importerSubnetName = '${legacySubnetNamePrefix}importer'
var publisherSubnetName = '${legacySubnetNamePrefix}publisher'
var notifySubnetName = '${legacySubnetNamePrefix}notify'
var contentSubnetName = '${legacySubnetNamePrefix}content'
var dataSubnetName = '${legacySubnetNamePrefix}data'
var sqlServerPrivateEndpointsSubnetName = '${generalSubnetNamePrefix}sqlsvr-pep'

// Screener subnets. 
var screenerFunctionAppSubnetName = '${screenerSubnetNamePrefix}fa-screener'
var screenerStoragePrivateEndpointsSubnetName = '${screenerSubnetNamePrefix}sa-screener-pep'

// Public API subnets. 
var publicApiDataProcessorSubnetName = '${publicApiSubnetNamePrefix}fa-processor'
var publicApiDataProcessorPrivateEndpointsSubnetName = '${publicApiSubnetNamePrefix}fa-processor-pep'
var publicApiStoragePrivateEndpointsSubnetName = '${publicApiSubnetNamePrefix}sa-pep'
var containerAppEnvironmentSubnetName = '${generalSubnetNamePrefix}cae-01'
var psqlFlexibleServerSubnetName = '${generalSubnetNamePrefix}psql-flexibleserver'

// NL Search subnets. 
var nlSearchFunctionAppSubnetName = '${generalSubnetNamePrefix}fa-nlsearch'
var nlSearchFunctionAppPrivateEndpointsSubnetName = '${generalSubnetNamePrefix}fa-nlsearch-pep'

// Search subnets. 
var searchDocsFunctionAppSubnetName = '${generalSubnetNamePrefix}fa-searchdocs'
var searchDocsFunctionAppPrivateEndpointsSubnetName = '${generalSubnetNamePrefix}fa-searchdocs-pep'
var searchStoragePrivateEndpointsSubnetName = '${generalSubnetNamePrefix}sa-search-pep'

// Analytics subnets. 
var analyticsFunctionAppSubnetName = '${generalSubnetNamePrefix}fa-analytics'
var analyticsStoragePrivateEndpointsSubnetName = '${generalSubnetNamePrefix}sa-anlyt-pep'
    

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
