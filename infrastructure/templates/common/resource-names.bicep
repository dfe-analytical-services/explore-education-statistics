import { abbreviations } from 'abbreviations.bicep'
import { eventTopics } from 'eventTopics.bicep'

@export()
@description('Identifies the environment that resource names are being generated for.')
type ResourceNamingContext = {
  @description('Identifier for resources in this environment, used as a prefix for all resources e.g. s101d01.')
  environmentIdentifier: string

  @description('Name of this environment e.g. Development, Test.')
  environmentName: string
}

@export()
type ResourceNames = {
  acr: {
    serverName: string
  }
  admin: {
    appService: string
    appServicePlan: string
    appInsights: string
    signalRName: string
  }
  analytics: {
    storage: {
      storageAccountName: string
      fileShareName: string
    }
  }
  applicationGateway: {
    applicationGateway: string
    managedIdentity: string
    publicIpAddress: string
    frontendIpConfig: string
    globalWafPolicy: string
    sites: {
      publicSite: {
        backend: string
        route: string
        site: string
      }
      publicApi: {
        backend: string
        route: string
        site: string
        wafPolicy: string
        rewriteSet: string
      }
      publicApiDocs: {
        backend: string
        // Path rules on the Public API's route that serve the docs site.
        pathRules: {
          backend: string
          rootRedirect: string
        }
      }
    }
  }
  backups: {
    backupVault: string
    backupVaultBlobsPolicy: string
    backupVaultPsqlFlexibleServerPolicy: string
    recoveryServicesVault: string
  }
  containerAppEnvironment: string
  contentApi: {
    appService: string
    appServicePlan: string
    appInsights: string
  }
  dataApi: {
    appService: string
    appServicePlan: string
    appInsights: string
  }
  dataFactory: string
  frontDoor: {
    frontDoorName: string
    defaultEndpoint: {
      endpointName: string
    }
    wafPolicy: string
    wafSecurityPolicy: string
    publicSite: {
      customDomain: string
      originGroup: string
      origin: string
      route: string
    }
    contentApi: {
      customDomain: string
      originGroup: string
      origin: string
      route: string
      allFilesCacheRoute: string
    }
  }
  importer: {
    functionApp: string
    appServicePlan: string
    appInsights: string
    storageAccount: string
  }
  nlSearch: {
    functionApp: string
  }
  notifier: {
    functionApp: string
    appServicePlan: string
    appInsights: string
    storageAccount: string
  }
  publicApi: {
    apiApp: string
    apiAppIdentity: string
    appInsights: string
    docsApp: string
    processor: {
      functionApp: string
      appServicePlan: string
      identity: string
      storageAccountsPrefix: string
    }
    storage: {
      storageAccount: string
      fileShare: string
    }
  }
  publicSite: {
    appService: string
    appServicePlan: string
    appInsights: string
  }
  publisher: {
    functionApp: string
    appServicePlan: string
    appInsights: string
    storageAccount: string
  }
  screener: {
    functionApp: string
    appServicePlan: string
    appInsights: string
    functionStorageAccount: string
    logsStorageAccount: string
    logsFileShare: string
  }
  search: {
    service: string
  }
  storageAccounts: {
    core: string
    public: string
    logging: string
  }
  vnet: {
    vnet: string
    subnets: {
      // Core shared infrastructure subnets.
      applicationGateway: string
      eventGridCustomTopicPrivateEndpoints: string

      // Core application subnets.
      admin: string
      contentApi: string
      dataApi: string
      importer: string
      notifier: string
      publisher: string
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
    }
  }
  keyVault: {
    keyVault: string
    keys: {
      dataProtection: string
    }
    certificates: {
      publicSite: string
      contentApiFrontDoor: string
      publicApiAppGateway: string
    }
    secrets: {
      acr: {
        dockerPullUsername: string
        dockerPullPassword: string
      }
      admin: {
        adminSignalrConnectionString: string
        adminGovUkNotifyApiKey: string
        databaseUserPassword: string
        openIdConnectClientId: string
        openIdConnectAuthority: string
        openIdConnectValidAudience: string
        openIdConnectValidIssuers: string
        openIdConnectFullyQualifiedScopeName: string
        screenerStorageAccountConnectionString: string
      }
      contentApi: {
        databaseUserPassword: string
      }
      dataApi: {
        databaseUserPassword: string
      }
      importer: {
        databaseUserPassword: string
      }
      notifier: {
        databaseUserPassword: string
        govUkNotifyApiKey: string
        tokenSecretKey: string
      }
      publicApi: {
        apiAppRegistrationClientId: string
        dataProcessorAppRegistrationClientId: string
      }
      publicSite: {
        basicAuthUsername: string
        basicAuthPassword: string
      }
      publisher: {
        databaseUserPassword: string
        notifyApiKey: string
        publicDataDbConnectionString: string
      }
      screener: {
        appRegistrationClientId: string
      }
      bauEmail: string
      coreStorageAccountConnectionString: string
      importerStorageAccountConnectionString: string
      notifierStorageAccountConnectionString: string
      publicStorageAccountConnectionString: string
      publisherStorageAccountConnectionString: string
      publicApiContainerAppPrivateUrl: string
    }
  }
  alertsGroup: string
  alertsLogicApp: string
  databases: {
    coreSqlServer: string
    publicSqlServer: string
    contentDb: string
    statisticsDb: string
    postgreSqlFlexibleServer: string
  }
  eventGrid: {
    topics: {
      publicationChanged: string
      releaseChanged: string
      releaseVersionChanged: string
      themeChanged: string
    }
  }
  logAnalyticsWorkspace: string
}

// TODO EES-7502 - use standardised naming convention for Core and Notifier storage, which use
// "storage" rather than "sa" in Test and Pre-Production.
@export()
func getResourceNamesForEnvironment(context ResourceNamingContext) ResourceNames => getResourceNames(
  context.environmentIdentifier,
  '${context.environmentIdentifier}-ees-papi',
  '${context.environmentIdentifier}-ees-sapi',
  '${context.environmentIdentifier}-ees',
  (context.environmentName == 'Test' || context.environmentName == 'Pre-Production') ? 'storage' : abbreviations.storageStorageAccounts
)

func getResourceNames(
  legacyResourcePrefix string,
  publicApiResourcePrefix string,
  screenerResourcePrefix string,
  newResourcePrefix string,
  legacyStorageAccountAbbreviation string) ResourceNames => {

    acr: {
      serverName: 'eesacr'
    }
    admin: {
    appService: '${legacyResourcePrefix}-${abbreviations.webSitesAppService}-ees-admin'
    appServicePlan: '${legacyResourcePrefix}-${abbreviations.webServerFarms}-ees-admin'
    appInsights: '${legacyResourcePrefix}-${abbreviations.insightsComponents}-ees-admin'
    signalRName: '${legacyResourcePrefix}-${abbreviations.signalRServiceSignalR}-ees-admin'
  }
  analytics: {
    storage: {
      storageAccountName: '${replace(newResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}anlyt'
      fileShareName: '${newResourcePrefix}-share-anlyt'
    }
  }
  applicationGateway: {
    applicationGateway: '${newResourcePrefix}-${abbreviations.networkApplicationGateways}-01'
    managedIdentity: '${newResourcePrefix}-${abbreviations.managedIdentityUserAssignedIdentities}-${abbreviations.networkApplicationGateways}-01'
    publicIpAddress: '${publicApiResourcePrefix}-pip'
    frontendIpConfig: '${publicApiResourcePrefix}-public-frontend-ip-config'
    globalWafPolicy: '${newResourcePrefix}-${abbreviations.networkApplicationGateways}-01-global-afwp'
    sites: {
      publicSite: {
        backend: '${legacyResourcePrefix}-${abbreviations.webSitesAppService}-ees-public-site-backend'
        route: '${legacyResourcePrefix}-${abbreviations.webSitesAppService}-ees-public-site-route'
        site: '${legacyResourcePrefix}-${abbreviations.webSitesAppService}-ees-public-site-site'
      }
      publicApi: {
        backend: '${publicApiResourcePrefix}-${abbreviations.appContainerApps}-api'
        route: publicApiResourcePrefix
        site: publicApiResourcePrefix
        wafPolicy: '${newResourcePrefix}-${abbreviations.networkApplicationGateways}-01-public-api-afwp'
        rewriteSet: '${publicApiResourcePrefix}-docs-rewrites'
      }
      publicApiDocs: {
        backend: '${publicApiResourcePrefix}-${abbreviations.staticWebApps}-docs'
        pathRules: {
          backend: 'docs-backend'
          rootRedirect: 'docs-root-redirect'
        }
      }
    }
  }
  backups: {
    backupVault: '${newResourcePrefix}-${abbreviations.backupVaults}'
    backupVaultBlobsPolicy: '${newResourcePrefix}-blob-${abbreviations.backupVaultPolicies}'
    backupVaultPsqlFlexibleServerPolicy: '${newResourcePrefix}-psql-flexible-server-${abbreviations.backupVaultPolicies}'
    recoveryServicesVault: '${newResourcePrefix}-${abbreviations.recoveryServicesVaults}'
  }
  containerAppEnvironment: '${newResourcePrefix}-${abbreviations.appManagedEnvironments}-01'
  contentApi: {
    appService: '${legacyResourcePrefix}-${abbreviations.webSitesAppService}-ees-content'
    appServicePlan: '${legacyResourcePrefix}-${abbreviations.webServerFarms}-ees-content'
    appInsights: '${legacyResourcePrefix}-${abbreviations.insightsComponents}-ees-content'
  }
  dataApi: {
    appService: '${legacyResourcePrefix}-${abbreviations.webSitesAppService}-ees-data'
    appServicePlan: '${legacyResourcePrefix}-${abbreviations.webServerFarms}-ees-data'
    appInsights: '${legacyResourcePrefix}-${abbreviations.insightsComponents}-ees-data'
  }
  dataFactory: '${legacyResourcePrefix}-${abbreviations.dataFactoryFactories}-ees-release'
  frontDoor: {
    frontDoorName: '${newResourcePrefix}-${abbreviations.frontDoorProfiles}'
    defaultEndpoint: {
      endpointName: '${newResourcePrefix}-${abbreviations.frontDoorEndpoints}'
    }
    wafPolicy: '${replace(newResourcePrefix, '-', '')}${abbreviations.frontDoorProfiles}${abbreviations.frontDoorWafPolicies}'
    wafSecurityPolicy: '${replace(newResourcePrefix, '-', '')}${abbreviations.frontDoorProfiles}${abbreviations.frontDoorWafSecurityPolicies}'
    // TODO EES-7502 - the public site's origin group, origin and route were the first Front Door
    // resources added and have generic names, whereas later ones (e.g. the Content API's) name their
    // purpose. Name these after the public site under the ELZ naming convention.
    publicSite: {
      customDomain: '${newResourcePrefix}-public-site-${abbreviations.frontDoorDomains}'
      originGroup: '${newResourcePrefix}-${abbreviations.frontDoorOriginGroups}'
      origin: '${newResourcePrefix}-${abbreviations.frontDoorOrigins}'
      route: '${newResourcePrefix}-${abbreviations.frontDoorRoutes}'
    }
    contentApi: {
      customDomain: '${newResourcePrefix}-content-${abbreviations.frontDoorDomains}'
      originGroup: '${newResourcePrefix}-content-${abbreviations.frontDoorOriginGroups}'
      origin: '${newResourcePrefix}-content-${abbreviations.frontDoorOrigins}'
      route: '${newResourcePrefix}-content-${abbreviations.frontDoorRoutes}'
      allFilesCacheRoute: '${newResourcePrefix}-content-all-files-cache-${abbreviations.frontDoorRoutes}'
    }
  }
  importer: {
    functionApp: '${legacyResourcePrefix}-${abbreviations.webSitesFunctions}-ees-importer'
    appServicePlan: '${legacyResourcePrefix}-${abbreviations.webServerFarms}-ees-importer'
    appInsights: '${legacyResourcePrefix}-${abbreviations.insightsComponents}-ees-importer'
    storageAccount: '${replace(newResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}importer'
  }
  nlSearch: {
    functionApp: '${newResourcePrefix}-${abbreviations.webSitesFunctions}-nlsearch'
  }
  notifier: {
    functionApp: '${legacyResourcePrefix}-${abbreviations.webSitesFunctions}-ees-notify'
    appServicePlan: '${legacyResourcePrefix}-${abbreviations.webServerFarms}-ees-notify'
    appInsights: '${legacyResourcePrefix}-${abbreviations.insightsComponents}-ees-notify'
    storageAccount: '${legacyResourcePrefix}${legacyStorageAccountAbbreviation}eesnotify'
  }
  publicApi: {
    apiApp: '${publicApiResourcePrefix}-${abbreviations.appContainerApps}-api'
    apiAppIdentity: '${publicApiResourcePrefix}-${abbreviations.managedIdentityUserAssignedIdentities}-${abbreviations.appContainerApps}-api'
    appInsights: '${publicApiResourcePrefix}-${abbreviations.insightsComponents}'
    docsApp: '${publicApiResourcePrefix}-${abbreviations.staticWebApps}-docs'
    processor: {
      functionApp: '${publicApiResourcePrefix}-${abbreviations.webSitesFunctions}-processor'
      // TODO EES-7502 - the Data Processor's App Service Plan was given the same name as the Function App
      // itself, rather than a name using the App Service Plan abbreviation.
      appServicePlan: '${publicApiResourcePrefix}-${abbreviations.webSitesFunctions}-processor'
      identity: '${publicApiResourcePrefix}-${abbreviations.managedIdentityUserAssignedIdentities}-${abbreviations.webSitesFunctions}-processor'
      storageAccountsPrefix: '${replace(newResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}processor'
    }
    storage: {
      storageAccount: '${replace(publicApiResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}'
      fileShare: '${publicApiResourcePrefix}-share-data'
    }
  }
  publicSite: {
    appService: '${legacyResourcePrefix}-${abbreviations.webSitesAppService}-ees-public-site'
    appServicePlan: '${legacyResourcePrefix}-${abbreviations.webServerFarms}-ees-public-site'
    appInsights: '${legacyResourcePrefix}-${abbreviations.insightsComponents}-ees-public-site'
  }
  publisher: {
    functionApp: '${legacyResourcePrefix}-${abbreviations.webSitesFunctions}-ees-publisher'
    appServicePlan: '${legacyResourcePrefix}-${abbreviations.webServerFarms}-ees-publisher'
    appInsights: '${legacyResourcePrefix}-${abbreviations.insightsComponents}-ees-publisher'
    storageAccount: '${legacyResourcePrefix}${abbreviations.storageStorageAccounts}eespublisher'
  }
  screener: {
    functionApp: '${screenerResourcePrefix}-${abbreviations.webSitesFunctions}-screener'
    // TODO EES-7502 - the Screener's App Service Plan was given the same name as the Function App itself,
    // rather than a name using the App Service Plan abbreviation.
    appServicePlan: '${screenerResourcePrefix}-${abbreviations.webSitesFunctions}-screener'
    appInsights: '${screenerResourcePrefix}-${abbreviations.insightsComponents}-screener'
    functionStorageAccount: '${replace(screenerResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}fn'
    logsStorageAccount: '${replace(screenerResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}logs'
    logsFileShare: '${screenerResourcePrefix}-${abbreviations.webSitesFunctions}-screener-logs-${abbreviations.fileShare}'
  }
  search: {
    service: '${newResourcePrefix}-srch'
  }
  storageAccounts: {
    core: '${legacyResourcePrefix}${legacyStorageAccountAbbreviation}eescore'
    public: '${legacyResourcePrefix}${abbreviations.storageStorageAccounts}eespublic'
    logging: '${legacyResourcePrefix}${abbreviations.storageStorageAccounts}eeslogging'
  }
  // TODO EES-7502 - we have a number of different naming conventions for subnets.
  vnet: {
    vnet: '${legacyResourcePrefix}-vnet-ees'
    subnets: {
      // Core shared infrastructure subnets.
      applicationGateway: '${newResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-agw-01'
      eventGridCustomTopicPrivateEndpoints: '${newResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-evgt-pep'

      // Core application subnets.
      admin: '${legacyResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-ees-admin'
      contentApi: '${legacyResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-ees-content'
      dataApi: '${legacyResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-ees-data'
      importer: '${legacyResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-ees-importer'
      notifier: '${legacyResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-ees-notify'
      publisher: '${legacyResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-ees-publisher'
      sqlServerPrivateEndpoints: '${newResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-sqlsvr-pep'

      // Screener subnets.
      screenerFunctionApp: '${screenerResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-fa-screener'
      screenerStoragePrivateEndpoints: '${screenerResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-sa-screener-pep'

      // Public API subnets.
      publicApiDataProcessor: '${publicApiResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-fa-processor'
      publicApiDataProcessorPrivateEndpoints: '${publicApiResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-fa-processor-pep'
      publicApiStoragePrivateEndpoints: '${publicApiResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-sa-pep'
      containerAppEnvironment: '${newResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-cae-01'
      psqlFlexibleServer: '${newResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-psql-flexibleserver'

      // NL Search subnets.
      nlSearchFunctionApp: '${newResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-fa-nlsearch'
      nlSearchFunctionAppPrivateEndpoints: '${newResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-fa-nlsearch-pep'

      // Search subnets.
      searchDocsFunctionApp: '${newResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-fa-searchdocs'
      searchDocsFunctionAppPrivateEndpoints: '${newResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-fa-searchdocs-pep'
      searchStoragePrivateEndpoints: '${newResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-sa-search-pep'

      // Analytics subnets.
      analyticsFunctionApp: '${newResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-fa-analytics'
      analyticsStoragePrivateEndpoints: '${newResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-sa-anlyt-pep'
    }
  }
  keyVault: {
    keyVault: '${legacyResourcePrefix}-kv-ees-01'
    keys: {
      dataProtection: 'ees-dataprotection-key'
    }
    certificates: {
      publicSite: '${legacyResourcePrefix}-${abbreviations.webSitesAppService}-ees-public-site-certificate'
      contentApiFrontDoor: '${legacyResourcePrefix}-${abbreviations.webSitesAppService}-ees-content-afd-certificate'
      publicApiAppGateway: '${publicApiResourcePrefix}-certificate'
    }
    secrets: {
      acr: {
        dockerPullUsername: 'DOCKER-REGISTRY-SERVER-USERNAME'
        dockerPullPassword: 'DOCKER-REGISTRY-SERVER-PASSWORD'
      }
      admin: {
        adminGovUkNotifyApiKey: 'ees-admin-govuknotify-api-key'
        adminSignalrConnectionString: 'ees-signalr-admin-connectionstring'
        databaseUserPassword: 'ees-sql-password-admin'
        openIdConnectClientId: 'ees-openidconnect-clientid'
        openIdConnectAuthority: 'ees-openidconnect-authority'
        openIdConnectValidAudience: 'ees-openidconnect-valid-audience'
        openIdConnectValidIssuers: 'ees-openidconnect-valid-issuers'
        openIdConnectFullyQualifiedScopeName: 'ees-openidconnect-fully-qualified-scope-name'
        screenerStorageAccountConnectionString: '${legacyResourcePrefix}eessapisafn-connection-string'
      }
      contentApi: {
        databaseUserPassword: 'ees-sql-public-password-content'
      }
      dataApi: {
        databaseUserPassword: 'ees-sql-public-password-data'
      }
      importer: {
        databaseUserPassword: 'ees-sql-password-importer'
      }
      notifier: {
        databaseUserPassword: 'ees-sql-password-notifier'
        govUkNotifyApiKey: 'ees-notifier-govuknotify-api-key'
        tokenSecretKey: 'ees-notifier-token-secret-key'
      }
      publicApi: {
        apiAppRegistrationClientId: 'ees-public-api-app-registration-client-id'
        dataProcessorAppRegistrationClientId: 'ees-public-api-data-processor-app-registration-client-id'
      }
      publicSite: {
        basicAuthUsername: 'ees-public-site-basic-auth-username'
        basicAuthPassword: 'ees-public-site-basic-auth-password'
      }
      publisher: {
        databaseUserPassword: 'ees-sql-password-publisher'
        notifyApiKey: 'ees-publisher-govuknotify-api-key'
        publicDataDbConnectionString: 'ees-publisher-connectionstring-publicdatadb'
      }
      screener: {
        appRegistrationClientId: 'ees-screener-app-registration-client-id'
      }
      publicApiContainerAppPrivateUrl: 'ees-publicapi-public-api-containerapp-private-url'
      bauEmail: 'ees-bau-email'
      coreStorageAccountConnectionString: 'ees-storage-core'
      importerStorageAccountConnectionString: '${replace(newResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}importer-connection-string'
      notifierStorageAccountConnectionString: '${legacyResourcePrefix}${legacyStorageAccountAbbreviation}eesnotify-connection-string'
      publicStorageAccountConnectionString: 'ees-storage-public'
      publisherStorageAccountConnectionString: '${legacyResourcePrefix}${abbreviations.storageStorageAccounts}eespublisher-connection-string'
    }
  } 
  alertsGroup: '${legacyResourcePrefix}-ag-ees-alertedusers'
  alertsLogicApp: '${legacyResourcePrefix}-${abbreviations.logicWorkflows}-ees-slackwebhook'
    databases: {
    coreSqlServer: '${legacyResourcePrefix}-sqlsvr-ees-01'
    publicSqlServer: '${legacyResourcePrefix}-sqlsvr-ees-02'
    contentDb: 'content'
    statisticsDb: 'statistics'
    postgreSqlFlexibleServer: '${newResourcePrefix}-${abbreviations.dBforPostgreSQLServers}'
  }
  eventGrid: {
    topics: {
      publicationChanged: '${newResourcePrefix}-${abbreviations.eventGridTopics}-${eventTopics.publicationChanged}'
      releaseChanged: '${newResourcePrefix}-${abbreviations.eventGridTopics}-${eventTopics.releaseChanged}'
      releaseVersionChanged: '${newResourcePrefix}-${abbreviations.eventGridTopics}-${eventTopics.releaseVersionChanged}'
      themeChanged: '${newResourcePrefix}-${abbreviations.eventGridTopics}-${eventTopics.themeChanged}'
    }
  }
  logAnalyticsWorkspace: '${newResourcePrefix}-${abbreviations.operationalInsightsWorkspaces}'
}

