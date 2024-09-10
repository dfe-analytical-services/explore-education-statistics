@export()
type resourceNamesType = {
  existingResources: {
    adminApp: string
    publisherApp: string
    keyVault: string
    vNet: string
    alertsGroup: string
    acr: string
    coreStorageAccount: string
    subnets: {
      dataProcessor: string
      dataProcessorPrivateEndpoints: string
      containerAppEnvironment: string
      psqlFlexibleServer: string
      appGateway: string
      adminAppService: string
      publisherFunctionApp: string
    }
  }
  sharedResources: {
    appGateway: string
    containerAppEnvironment: string
    logAnalyticsWorkspace: string
    postgreSqlFlexibleServer: string
    privateDnsZones: {
      sites: string
      postgres: string
    }
  }
  publicAPi: {
    apiApp: string
    appInsights: string
    dataProcessor: string
    publicApiStorage: string
  }
}
