@export()
type resourceNamesType = {
  existingResources: {
    adminApp: string
    publisherFunction: string
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
      adminApp: string
      publisherFunction: string
    }
  }
  sharedResources: {
    appGateway: string
    appGatewayIdentity: string
    containerAppEnvironment: string
    logAnalyticsWorkspace: string
    postgreSqlFlexibleServer: string
  }
  publicApi: {
    apiApp: string
    apiAppIdentity: string
    appInsights: string
    dataProcessor: string
    dataProcessorIdentity: string
    dataProcessorPlan: string
    dataProcessorStorageAccountsPrefix: string
    publicApiStorageAccount: string
    publicApiFileshare: string
  }
}

@export()
type firewallRuleType = {
  name: string
  cidr: string
}

@export()
type azureFileshareMountType = {
  storageName: string
  storageAccountKey: string
  storageAccountName: string
  fileShareName: string
  mountPath: string
}

@export()
type entraIdAuthenticationType = {
  appRegistrationClientId: string
  allowedClientIds: string[]
  allowedPrincipalIds: string[]
  requireAuthentication: bool
}

@export()
type appGatewaySiteConfigType = {
  resourceName: string
  backendFqdn: string
  publicFqdn: string
  certificateName: string
  healthProbeRelativeUrl: string
}

@export()
type privateDnsZoneType = 'sites' | 'postgres'

@export()
type containerRegistryRoleType = 'AcrPull'

@export()
type keyVaultRoleType = 'Secrets User' | 'Certificate User'
