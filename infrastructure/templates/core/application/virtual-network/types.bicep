@export()
type SubnetReference = {
  name: string
  id: string
}

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
}
