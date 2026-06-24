@export()
type DayOfWeek = 'Monday' | 'Tuesday' | 'Wednesday' | 'Thursday' | 'Friday' | 'Saturday' | 'Sunday'

@export()
type WeekOfMonth = 'First' | 'Second' | 'Third' | 'Fourth' | 'Last'

@export()
type MonthOfYear =
  | 'January'
  | 'February'
  | 'March'
  | 'April'
  | 'May'
  | 'June'
  | 'July'
  | 'August'
  | 'September'
  | 'October'
  | 'November'
  | 'December'

@export()
type ResourceNames = {
  existingResources: {
    acr: string
    acrResourceGroup: string
    adminApp: string
    alertsGroup: string
    analyticsFileShare: string
    analyticsStorageAccount: string
    backupVault: {
      vault: string
      psqlFlexibleServerBackupPolicy: string
    }
    coreStorageAccount: string
    keyVault: string
    logAnalyticsWorkspace: string
    publisherFunction: string
    subnets: {
      dataProcessor: string
      dataProcessorPrivateEndpoints: string
      containerAppEnvironment: string
      psqlFlexibleServer: string
      appGateway: string
      adminApp: string
      publisherFunction: string
      storagePrivateEndpoints: string
    }
    vNet: string
  }
  sharedResources: {
    appGateway: string
    appGatewayIdentity: string
    containerAppEnvironment: string
    logAnalyticsWorkspace: string
    postgreSqlFlexibleServer: string
    recoveryVault: string
    recoveryVaultFileShareBackupPolicy: string
    searchService: string
  }
  publicApi: {
    apiApp: string
    apiAppIdentity: string
    appInsights: string
    dataProcessor: string
    dataProcessorIdentity: string
    dataProcessorPlan: string
    dataProcessorStorageAccountsPrefix: string
    docsApp: string
    publicApiStorageAccount: string
    publicApiFileShare: string
  }
}

@export()
type EntraIdAuthentication = {
  appRegistrationClientId: string
  allowedClientIds: string[]
  allowedPrincipalIds: string[]
  requireAuthentication: bool
}

@export()
type SearchServiceConfig = {
  endpoint: string
  indexName: string
}

@export()
type PrincipalNameAndId = {
  principalName: string
  objectId: string
}

@export()
type ContainerRegistryRole = 'AcrPull'

@export()
type StaticWebAppSku = 'Free' | 'Standard'

@export()
type ContainerAppResourceConfig = {
  workloadProfileName: string
  cpuCores: int
  memoryGis: int
  minReplicas: int
  maxReplicas: int
  scaleAtConcurrentHttpRequests: int?
}

@export()
type ContainerAppWorkloadProfile = {
  name: string
  workloadProfileType: 'D4' | 'D8' | 'D16' | 'D32' | 'E4' | 'E8' | 'E16' | 'E32'
  minimumCount: int
  maximumCount: int
}

@export()
type PostgreSqlFlexibleServerConfig = {

  @discriminator('pricingTier')
  @description('''
  Available compute options per pricing tier. Note that this is not an exhaustive list.
  A full list of options can be found at 
  https://azure.microsoft.com/en-us/pricing/details/postgresql/flexible-server.
  ''')
  sku: {
    pricingTier: 'Burstable'
    compute: 'Standard_B1ms' | 'Standard_B2s'
  } | {
    pricingTier: 'GeneralPurpose'
    compute: ''
  } | {
    pricingTier: 'MemoryOptimized'
    compute: ''
  }
  server: {
    @description('PostgreSQL version')
    postgreSqlVersion: '16'
  }
  backups: {
    @description('Backup retention duration in days')
    retentionDays: int
  
    @description('If the database server is restorable in a paired region from its backups.')
    geoRedundantBackup: bool
  }
  settings: {
    @description('Name of the database setting.')
    name: string
    
    @description('Value of the database setting.')
    value: string
  }[]
  storage: {
    @description('Storage Size in GB.')
    storageSizeGB: int

    @description('Whether the server storage will automatically grow when maximum capacity is reached or become read-only.')
    autoGrow: bool
  }
}
