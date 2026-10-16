import { SubnetReference } from '../../../common/types.bicep'

@export()
type VNetSubnets = {
  admin: SubnetReference
  importer: SubnetReference
  publisher: SubnetReference
  notify: SubnetReference
  content: SubnetReference
  data: SubnetReference
  publicApiDataProcessor: SubnetReference
  publicApiDataProcessorPrivateEndpoints: SubnetReference
  containerAppEnvironment: SubnetReference
  applicationGateway: SubnetReference
  publicApiStoragePrivateEndpoints: SubnetReference
  psqlFlexibleServer: SubnetReference
  searchStoragePrivateEndpoints: SubnetReference
  searchDocsFunctionApp: SubnetReference
  searchDocsFunctionAppPrivateEndpoints: SubnetReference
  analyticsStoragePrivateEndpoints: SubnetReference
  analyticsFunctionApp: SubnetReference
  screenerStoragePrivateEndpoints: SubnetReference
  screenerFunctionApp: SubnetReference
  eventGridCustomTopicPrivateEndpoints: SubnetReference
  nlSearchFunctionAppPrivateEndpoints: SubnetReference
  nlSearchFunctionApp: SubnetReference
  sqlServerPrivateEndpoints: SubnetReference
  dataFilesStoragePrivateEndpoints: SubnetReference
}

@export()
@description('CIDR address ranges for the VNet and each of its subnets.')
type VNetAddressRanges = {
  @description('Address space of the VNet e.g. 10.0.0.0/16.')
  vNet: string

  @description('Address range of each subnet. Each must fall within the VNet address space and not overlap any other subnet.')
  subnets: {
    // Core shared infrastructure subnets.
    applicationGateway: string
    eventGridCustomTopicPrivateEndpoints: string

    // Core application subnets.
    admin: string
    importer: string
    publisher: string
    notify: string
    content: string
    data: string
    sqlServerPrivateEndpoints: string

    // Screener subnets.
    screenerFunctionApp: string
    screenerStoragePrivateEndpoints: string

    // Public API subnets.
    publicApiDataProcessor: string
    publicApiDataProcessorPrivateEndpoints: string
    publicApiStoragePrivateEndpoints: string
    containerAppEnvironment: string
    psqlFlexibleServer: string

    // NL Search subnets.
    nlSearchFunctionApp: string
    nlSearchFunctionAppPrivateEndpoints: string

    // Search subnets.
    searchDocsFunctionApp: string
    searchDocsFunctionAppPrivateEndpoints: string
    searchStoragePrivateEndpoints: string

    // Analytics subnets.
    analyticsFunctionApp: string
    analyticsStoragePrivateEndpoints: string

    // Data files subnets.
    dataFilesStoragePrivateEndpoints: string
  }
}
