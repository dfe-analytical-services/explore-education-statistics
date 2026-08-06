import { abbreviations } from '../../../common/abbreviations.bicep'

@description('Subscription name e.g. s101d01. Used as a prefix for created resources.')
param subscription string

@description('Whether or not to deploy the subnets.')
param deploySubnets bool

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
    addressPrefix: '10.0.0.0/24'
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
    addressPrefix: '10.0.1.0/24'
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
    addressPrefix: '10.0.2.0/24'
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
    addressPrefix: '10.0.3.0/24'
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
    addressPrefix: '10.0.4.0/24'
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
    addressPrefix: '10.0.5.0/24'
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
    addressPrefix: '10.0.6.0/24'
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
    addressPrefix: '10.0.7.0/24'
  }
}
{
  name: containerAppEnvironmentSubnetName
  properties: {
    addressPrefix: '10.0.8.0/23'
    serviceEndpoints: ['Microsoft.Storage']
    delegations: ['environment']
  }
}
{
  name: applicationGatewaySubnetName
  properties: {
    addressPrefix: '10.0.10.0/24'
  }
}
{
  name: publicApiStoragePrivateEndpointsSubnetName
  properties: {
    addressPrefix: '10.0.11.0/24'
  }
}
{
  name: psqlFlexibleServerSubnetName
  properties: {
    addressPrefix: '10.0.12.0/24'
  }
}
{
  name: searchStoragePrivateEndpointsSubnetName
  properties: {
    addressPrefix: '10.0.13.0/24'
  }
}
{
  name: searchDocsFunctionAppSubnetName
  properties: {
    addressPrefix: '10.0.14.0/24'
    serviceEndpoints: ['Microsoft.Storage']
    delegations: ['webapp']
  }
}
{
  name: searchDocsFunctionAppPrivateEndpointsSubnetName
  properties: {
    addressPrefix: '10.0.15.0/24'
  }
}
{
  name: analyticsStoragePrivateEndpointsSubnetName
  properties: {
    addressPrefix: '10.0.16.0/24'
  }
}
{
  name: analyticsFunctionAppSubnetName
  properties: {
    addressPrefix: '10.0.17.0/24'
    serviceEndpoints: ['Microsoft.Storage']
    delegations: ['webapp']
  }
}
{
  name: screenerStoragePrivateEndpointsSubnetName
  properties: {
    addressPrefix: '10.0.18.0/24'
  }
}
{
  name: screenerFunctionAppSubnetName
  properties: {
    addressPrefix: '10.0.19.0/24'
    serviceEndpoints: ['Microsoft.Storage']
    delegations: ['webapp']
  }
}
{
  name: eventGridCustomTopicPrivateEndpointsSubnetName
  properties: {
    addressPrefix: '10.0.20.0/24'
  }
}
{
  name: nlSearchFunctionAppPrivateEndpointsSubnetName
  properties: {
    addressPrefix: '10.0.21.0/24'
  }
}
{
  name: nlSearchFunctionAppSubnetName
  properties: {
    addressPrefix: '10.0.22.0/24'
    serviceEndpoints: ['Microsoft.Storage']
    delegations: ['webapp']
  }
}
{
  name: sqlServerPrivateEndpointsSubnetName
  properties: {
    addressPrefix: '10.0.23.0/24'
  }
}] : null

module vNetModule '../../../common/components/virtual-network/virtual-network.bicep' = {
  name: '${vNetName}ModuleDeploy'
  params: {
    vNetName: vNetName
    addressSpacePrefix: '10.0.0.0/16'
    subnets: subnets
    tagValues: tagValues
  }
}

output vNetName string = vNetName 
output eventGridCustomTopicPrivateEndpointsSubnetName string = eventGridCustomTopicPrivateEndpointsSubnetName
output applicationGatewaySubnetName string = applicationGatewaySubnetName
