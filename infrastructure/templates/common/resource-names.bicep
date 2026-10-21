import { abbreviations } from 'abbreviations.bicep'
import { elzAbbreviations } from 'abbreviations-elz.bicep'
import { eventTopics } from 'eventTopics.bicep'

@export()
@description('Identifies the environment that resource names are being generated for.')
type ResourceNamingContext = {
  @description('Identifier for resources in this environment, used as a prefix for all resources e.g. s101d01.')
  environmentIdentifier: string

  @description('Name of this environment e.g. Development, Test.')
  environmentName: string

  @description('Whether to use the naming convention for the DfE Enterprise Landing Zone (ELZ) subscriptions rather than the legacy one. Defaults to false.')
  elz: bool?
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
    functionApp: string
    appServicePlan: string
    appInsights: string
    functionStorageAccount: string
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
    appServicePlan: string
    functionStorageAccount: string
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
    storageAccount: string
    appInsights: string
  }
  searchDocs: {
    functionApp: string
    appServicePlan: string
    functionStorageAccount: string
    eventSubscriptions: {
      publicationArchived: string
      publicationChanged: string
      publicationDeleted: string
      publicationLatestPublishedReleaseReordered: string
      publicationRestored: string
      releaseSlugChanged: string
      releaseVersionPublished: string
      themeChanged: string
    }
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
        dockerRegistryServerDomain: string
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
      analyticsStorageAccountConnectionString: string
      nlSearchStorageAccountConnectionString: string
      searchDocsStorageAccountConnectionString: string
      searchStorageAccountConnectionString: string
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
func getResourceNamesForEnvironment(context ResourceNamingContext) ResourceNames => (context.?elz ?? false)
  ? getElzResourceNames(context.environmentIdentifier)
  : getResourceNames(
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
    functionApp: '${newResourcePrefix}-${abbreviations.webSitesFunctions}-analytics'
    appServicePlan: '${newResourcePrefix}-${abbreviations.webServerFarms}-analytics'
    appInsights: '${newResourcePrefix}-${abbreviations.insightsComponents}-analytics'
    functionStorageAccount: '${replace(newResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}anlytfa'
    storage: {
      storageAccountName: '${replace(newResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}anlyt'
      fileShareName: '${newResourcePrefix}-${abbreviations.fileShare}-anlyt'
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
    appServicePlan: '${newResourcePrefix}-${abbreviations.webServerFarms}-nlsearch'
    functionStorageAccount: '${replace(newResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}nlsearchfn'
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
    service: '${newResourcePrefix}-${abbreviations.searchSearchServices}'
    storageAccount: '${replace(newResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}search'
    appInsights: '${newResourcePrefix}-${abbreviations.insightsComponents}-search'
  }
  searchDocs: {
    functionApp: '${newResourcePrefix}-${abbreviations.webSitesFunctions}-searchdocs'
    appServicePlan: '${newResourcePrefix}-${abbreviations.webServerFarms}-searchdocs'
    functionStorageAccount: '${replace(newResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}searchdocsfn'
    eventSubscriptions: {
      publicationArchived: '${newResourcePrefix}-${abbreviations.eventGridSubscriptions}-publication-archived'
      publicationChanged: '${newResourcePrefix}-${abbreviations.eventGridSubscriptions}-publication-changed'
      publicationDeleted: '${newResourcePrefix}-${abbreviations.eventGridSubscriptions}-publication-deleted'
      publicationLatestPublishedReleaseReordered: '${newResourcePrefix}-${abbreviations.eventGridSubscriptions}-publication-latest-published-release-reordered'
      publicationRestored: '${newResourcePrefix}-${abbreviations.eventGridSubscriptions}-publication-restored'
      releaseSlugChanged: '${newResourcePrefix}-${abbreviations.eventGridSubscriptions}-release-slug-changed'
      releaseVersionPublished: '${newResourcePrefix}-${abbreviations.eventGridSubscriptions}-release-version-published'
      themeChanged: '${newResourcePrefix}-${abbreviations.eventGridSubscriptions}-theme-changed'
    }
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
        dockerRegistryServerDomain: 'DOCKER-REGISTRY-SERVER-DOMAIN'
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
      }
      screener: {
        appRegistrationClientId: 'ees-screener-app-registration-client-id'
      }
      bauEmail: 'ees-bau-email'
      coreStorageAccountConnectionString: 'ees-storage-core'
      importerStorageAccountConnectionString: '${replace(newResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}importer-connection-string'
      notifierStorageAccountConnectionString: '${legacyResourcePrefix}${legacyStorageAccountAbbreviation}eesnotify-connection-string'
      publicStorageAccountConnectionString: 'ees-storage-public'
      publisherStorageAccountConnectionString: '${legacyResourcePrefix}${abbreviations.storageStorageAccounts}eespublisher-connection-string'
      analyticsStorageAccountConnectionString: '${replace(newResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}anlytfa-connection-string'
      nlSearchStorageAccountConnectionString: '${replace(newResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}nlsearchfn-connection-string'
      searchDocsStorageAccountConnectionString: '${replace(newResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}searchdocsfn-connection-string'
      searchStorageAccountConnectionString: '${replace(newResourcePrefix, '-', '')}${abbreviations.storageStorageAccounts}search-connection-string'
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

// Builds a resource name using the ELZ naming convention "{environment}{type}-uks-{workload}01"
// e.g. s292d01app-uks-dataapi01.
func elzName(environmentIdentifier string, abbreviation string, workload string) string =>
  '${environmentIdentifier}${abbreviation}-uks-${workload}01'

// Storage Account names may only contain lowercase letters and numbers, up to 24 characters, so these drop the
// hyphens and the trailing "01" from the ELZ naming convention e.g. s292d01stukscore.
func elzStorageAccountName(environmentIdentifier string, workload string) string =>
  '${environmentIdentifier}${elzAbbreviations.storageStorageAccounts}uks${workload}'

func getElzResourceNames(env string) ResourceNames => {
  acr: {
    serverName: '${env}${elzAbbreviations.containerRegistryRegistries}uksees01'
  }
  admin: {
    appService: elzName(env, elzAbbreviations.webSitesAppService, 'admin')
    appServicePlan: elzName(env, elzAbbreviations.webServerFarms, 'admin')
    appInsights: elzName(env, elzAbbreviations.insightsComponents, 'admin')
    signalRName: elzName(env, elzAbbreviations.signalRServiceSignalR, 'admin')
  }
  analytics: {
    functionApp: elzName(env, elzAbbreviations.webSitesFunctions, 'analytics')
    appServicePlan: elzName(env, elzAbbreviations.webServerFarms, 'analytics')
    appInsights: elzName(env, elzAbbreviations.insightsComponents, 'analytics')
    functionStorageAccount: elzStorageAccountName(env, 'analyticsfn')
    storage: {
      storageAccountName: elzStorageAccountName(env, 'analytics')
      fileShareName: elzName(env, elzAbbreviations.fileShare, 'analytics')
    }
  }
  applicationGateway: {
    applicationGateway: elzName(env, elzAbbreviations.networkApplicationGateways, 'ees')
    managedIdentity: elzName(env, elzAbbreviations.managedIdentityUserAssignedIdentities, 'agw')
    publicIpAddress: elzName(env, elzAbbreviations.networkPublicIPAddresses, 'agw')
    // The gateway's child resources only need to be unique within the gateway itself.
    frontendIpConfig: 'public-frontend-ip-config'
    globalWafPolicy: elzName(env, elzAbbreviations.networkFirewallPoliciesWebApplication, 'agwglobal')
    sites: {
      publicSite: {
        backend: 'publicsite-backend'
        route: 'publicsite-routing-rule'
        site: 'publicsite-site'
      }
      publicApi: {
        backend: 'papi-backend'
        route: 'papi-routing-rule'
        site: 'papi-site'
        wafPolicy: elzName(env, elzAbbreviations.networkFirewallPoliciesWebApplication, 'agwpapi')
        rewriteSet: 'papidocs-rewrites'
      }
      publicApiDocs: {
        backend: 'papidocs-backend'
        // Path rules on the Public API's route. "papidocs-backend" is already the backend pool's name.
        pathRules: {
          backend: 'papidocs-path-rule'
          rootRedirect: 'papidocs-root-redirect-rule'
        }
      }
    }
  }
  backups: {
    backupVault: elzName(env, elzAbbreviations.backupVaults, 'ees')
    backupVaultBlobsPolicy: elzName(env, elzAbbreviations.backupVaultPolicies, 'blob')
    backupVaultPsqlFlexibleServerPolicy: elzName(env, elzAbbreviations.backupVaultPolicies, 'psql')
    recoveryServicesVault: elzName(env, elzAbbreviations.recoveryServicesVaults, 'ees')
  }
  containerAppEnvironment: elzName(env, elzAbbreviations.appManagedEnvironments, 'ees')
  contentApi: {
    appService: elzName(env, elzAbbreviations.webSitesAppService, 'contentapi')
    appServicePlan: elzName(env, elzAbbreviations.webServerFarms, 'contentapi')
    appInsights: elzName(env, elzAbbreviations.insightsComponents, 'contentapi')
  }
  dataApi: {
    appService: elzName(env, elzAbbreviations.webSitesAppService, 'dataapi')
    appServicePlan: elzName(env, elzAbbreviations.webServerFarms, 'dataapi')
    appInsights: elzName(env, elzAbbreviations.insightsComponents, 'dataapi')
  }
  dataFactory: elzName(env, elzAbbreviations.dataFactoryFactories, 'release')
  frontDoor: {
    frontDoorName: elzName(env, elzAbbreviations.frontDoorProfiles, 'ees')
    defaultEndpoint: {
      endpointName: elzName(env, elzAbbreviations.frontDoorEndpoints, 'ees')
    }
    // Front Door WAF policy names may only contain letters and numbers.
    wafPolicy: '${env}${elzAbbreviations.frontDoorWafPolicies}uksees01'
    wafSecurityPolicy: elzName(env, elzAbbreviations.frontDoorWafSecurityPolicies, 'ees')
    publicSite: {
      customDomain: elzName(env, elzAbbreviations.frontDoorDomains, 'publicsite')
      originGroup: elzName(env, elzAbbreviations.frontDoorOriginGroups, 'publicsite')
      origin: elzName(env, elzAbbreviations.frontDoorOrigins, 'publicsite')
      route: elzName(env, elzAbbreviations.frontDoorRoutes, 'publicsite')
    }
    contentApi: {
      customDomain: elzName(env, elzAbbreviations.frontDoorDomains, 'contentapi')
      originGroup: elzName(env, elzAbbreviations.frontDoorOriginGroups, 'contentapi')
      origin: elzName(env, elzAbbreviations.frontDoorOrigins, 'contentapi')
      route: elzName(env, elzAbbreviations.frontDoorRoutes, 'contentapi')
      allFilesCacheRoute: elzName(env, elzAbbreviations.frontDoorRoutes, 'contentapiallfilescache')
    }
  }
  importer: {
    functionApp: elzName(env, elzAbbreviations.webSitesFunctions, 'importer')
    appServicePlan: elzName(env, elzAbbreviations.webServerFarms, 'importer')
    appInsights: elzName(env, elzAbbreviations.insightsComponents, 'importer')
    storageAccount: elzStorageAccountName(env, 'importer')
  }
  nlSearch: {
    functionApp: elzName(env, elzAbbreviations.webSitesFunctions, 'nlsearch')
    appServicePlan: elzName(env, elzAbbreviations.webServerFarms, 'nlsearch')
    functionStorageAccount: elzStorageAccountName(env, 'nlsearchfn')
  }
  notifier: {
    functionApp: elzName(env, elzAbbreviations.webSitesFunctions, 'notifier')
    appServicePlan: elzName(env, elzAbbreviations.webServerFarms, 'notifier')
    appInsights: elzName(env, elzAbbreviations.insightsComponents, 'notifier')
    storageAccount: elzStorageAccountName(env, 'notifier')
  }
  publicApi: {
    apiApp: elzName(env, elzAbbreviations.appContainerApps, 'papiapi')
    apiAppIdentity: elzName(env, elzAbbreviations.managedIdentityUserAssignedIdentities, 'papiapi')
    appInsights: elzName(env, elzAbbreviations.insightsComponents, 'papi')
    docsApp: elzName(env, elzAbbreviations.staticWebApps, 'papidocs')
    processor: {
      functionApp: elzName(env, elzAbbreviations.webSitesFunctions, 'papiprocessor')
      appServicePlan: elzName(env, elzAbbreviations.webServerFarms, 'papiprocessor')
      identity: elzName(env, elzAbbreviations.managedIdentityUserAssignedIdentities, 'papiprocessor')
      // Suffixed with "mg", "s1" and "s2". "papiprocessor" would take the longest of these over the
      // 24 character limit for Storage Account names.
      storageAccountsPrefix: elzStorageAccountName(env, 'papiproc')
    }
    storage: {
      storageAccount: elzStorageAccountName(env, 'papi')
      fileShare: elzName(env, elzAbbreviations.fileShare, 'papidata')
    }
  }
  publicSite: {
    appService: elzName(env, elzAbbreviations.webSitesAppService, 'publicsite')
    appServicePlan: elzName(env, elzAbbreviations.webServerFarms, 'publicsite')
    appInsights: elzName(env, elzAbbreviations.insightsComponents, 'publicsite')
  }
  publisher: {
    functionApp: elzName(env, elzAbbreviations.webSitesFunctions, 'publisher')
    appServicePlan: elzName(env, elzAbbreviations.webServerFarms, 'publisher')
    appInsights: elzName(env, elzAbbreviations.insightsComponents, 'publisher')
    storageAccount: elzStorageAccountName(env, 'publisher')
  }
  screener: {
    functionApp: elzName(env, elzAbbreviations.webSitesFunctions, 'screener')
    appServicePlan: elzName(env, elzAbbreviations.webServerFarms, 'screener')
    appInsights: elzName(env, elzAbbreviations.insightsComponents, 'screener')
    functionStorageAccount: elzStorageAccountName(env, 'screenerfn')
    logsStorageAccount: elzStorageAccountName(env, 'screenerlogs')
    logsFileShare: elzName(env, elzAbbreviations.fileShare, 'screenerlogs')
  }
  search: {
    service: elzName(env, elzAbbreviations.searchSearchServices, 'ees')
    storageAccount: elzStorageAccountName(env, 'search')
    appInsights: elzName(env, elzAbbreviations.insightsComponents, 'search')
    searchDocsEventSubscriptions: {
      publicationArchived: elzName(env, elzAbbreviations.eventGridSubscriptions, 'publicationarchived')
      publicationChanged: elzName(env, elzAbbreviations.eventGridSubscriptions, 'publicationchanged')
      publicationDeleted: elzName(env, elzAbbreviations.eventGridSubscriptions, 'publicationdeleted')
      publicationLatestPublishedReleaseReordered: elzName(env, elzAbbreviations.eventGridSubscriptions, 'publicationlatestpublishedreleasereordered')
      publicationRestored: elzName(env, elzAbbreviations.eventGridSubscriptions, 'publicationrestored')
      releaseSlugChanged: elzName(env, elzAbbreviations.eventGridSubscriptions, 'releaseslugchanged')
      releaseVersionPublished: elzName(env, elzAbbreviations.eventGridSubscriptions, 'releaseversionpublished')
      themeChanged: elzName(env, elzAbbreviations.eventGridSubscriptions, 'themechanged')
    }
  }
  searchDocs: {
    functionApp: elzName(env, elzAbbreviations.webSitesFunctions, 'searchdocs')
    appServicePlan: elzName(env, elzAbbreviations.webServerFarms, 'searchdocs')
    functionStorageAccount: elzStorageAccountName(env, 'searchdocsfn')
  }
  storageAccounts: {
    core: elzStorageAccountName(env, 'core')
    public: elzStorageAccountName(env, 'public')
    logging: elzStorageAccountName(env, 'logging')
  }
  // TODO EES-7502 - these keep the existing one-to-one set of subnets for now. See "Subnet migration" in
  // azure-resource-update-plan-for-elz.md for the proposed consolidation into shared private endpoint subnets.
  vnet: {
    vnet: elzName(env, elzAbbreviations.networkVirtualNetworks, 'ees')
    subnets: {
      // Core shared infrastructure subnets.
      applicationGateway: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'agw')
      eventGridCustomTopicPrivateEndpoints: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'evgtpep')

      // Core application subnets.
      admin: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'admin')
      contentApi: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'contentapi')
      dataApi: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'dataapi')
      importer: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'importer')
      notifier: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'notifier')
      publisher: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'publisher')
      sqlServerPrivateEndpoints: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'sqlpep')

      // Screener subnets.
      screenerFunctionApp: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'screener')
      screenerStoragePrivateEndpoints: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'screenerpep')

      // Public API subnets.
      publicApiDataProcessor: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'papiprocessor')
      publicApiDataProcessorPrivateEndpoints: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'papiprocessorpep')
      publicApiStoragePrivateEndpoints: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'papistpep')
      containerAppEnvironment: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'cae')
      psqlFlexibleServer: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'psql')

      // NL Search subnets.
      nlSearchFunctionApp: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'nlsearch')
      nlSearchFunctionAppPrivateEndpoints: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'nlsearchpep')

      // Search subnets.
      searchDocsFunctionApp: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'searchdocs')
      searchDocsFunctionAppPrivateEndpoints: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'searchdocspep')
      searchStoragePrivateEndpoints: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'searchstpep')

      // Analytics subnets.
      analyticsFunctionApp: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'analytics')
      analyticsStoragePrivateEndpoints: elzName(env, elzAbbreviations.networkVirtualNetworksSubnets, 'analyticsstpep')
    }
  }
  keyVault: {
    keyVault: elzName(env, elzAbbreviations.keyVaultVaults, 'ees')
    keys: {
      dataProtection: 'ees-dataprotection-key'
    }
    // Certificates and secrets live in a Key Vault dedicated to EES in a single environment, so their names
    // need neither an "ees-" prefix nor an environment prefix. They are named "{workload}-{purpose}", using the
    // same workload names as the resources above e.g. "publicsite-basic-auth-username".
    certificates: {
      publicSite: 'publicsite-certificate'
      contentApiFrontDoor: 'contentapi-afd-certificate'
      publicApiAppGateway: 'papi-agw-certificate'
    }
    secrets: {
      acr: {
        dockerPullUsername: 'DOCKER-REGISTRY-SERVER-USERNAME'
        dockerPullPassword: 'DOCKER-REGISTRY-SERVER-PASSWORD'
        dockerRegistryServerDomain: 'DOCKER-REGISTRY-SERVER-DOMAIN'
      }
      admin: {
        adminGovUkNotifyApiKey: 'admin-govuknotify-api-key'
        adminSignalrConnectionString: 'admin-signalr-connection-string'
        databaseUserPassword: 'admin-sql-password'
        openIdConnectClientId: 'admin-openidconnect-client-id'
        openIdConnectAuthority: 'admin-openidconnect-authority'
        openIdConnectValidAudience: 'admin-openidconnect-valid-audience'
        openIdConnectValidIssuers: 'admin-openidconnect-valid-issuers'
        openIdConnectFullyQualifiedScopeName: 'admin-openidconnect-fully-qualified-scope-name'
        screenerStorageAccountConnectionString: 'screener-storage-connection-string'
      }
      contentApi: {
        databaseUserPassword: 'contentapi-sql-password'
      }
      dataApi: {
        databaseUserPassword: 'dataapi-sql-password'
      }
      importer: {
        databaseUserPassword: 'importer-sql-password'
      }
      notifier: {
        databaseUserPassword: 'notifier-sql-password'
        govUkNotifyApiKey: 'notifier-govuknotify-api-key'
        tokenSecretKey: 'notifier-token-secret-key'
      }
      publicApi: {
        apiAppRegistrationClientId: 'papi-app-registration-client-id'
        dataProcessorAppRegistrationClientId: 'papiprocessor-app-registration-client-id'
      }
      publicSite: {
        basicAuthUsername: 'publicsite-basic-auth-username'
        basicAuthPassword: 'publicsite-basic-auth-password'
      }
      publisher: {
        databaseUserPassword: 'publisher-sql-password'
        notifyApiKey: 'publisher-govuknotify-api-key'
      }
      screener: {
        appRegistrationClientId: 'screener-app-registration-client-id'
      }
      bauEmail: 'bau-email'
      coreStorageAccountConnectionString: 'core-storage-connection-string'
      importerStorageAccountConnectionString: 'importer-storage-connection-string'
      notifierStorageAccountConnectionString: 'notifier-storage-connection-string'
      publicStorageAccountConnectionString: 'public-storage-connection-string'
      publisherStorageAccountConnectionString: 'publisher-storage-connection-string'
      analyticsStorageAccountConnectionString: 'analytics-storage-connection-string'
      nlSearchStorageAccountConnectionString: 'nlsearch-storage-connection-string'
      searchDocsStorageAccountConnectionString: 'searchdocs-storage-connection-string'
      searchStorageAccountConnectionString: 'search-storage-connection-string'
    }
  }
  alertsGroup: elzName(env, elzAbbreviations.insightsActionGroups, 'alertedusers')
  alertsLogicApp: elzName(env, elzAbbreviations.logicWorkflows, 'slackwebhook')
  databases: {
    coreSqlServer: elzName(env, elzAbbreviations.sqlServers, 'core')
    publicSqlServer: elzName(env, elzAbbreviations.sqlServers, 'public')
    contentDb: 'content'
    statisticsDb: 'statistics'
    postgreSqlFlexibleServer: elzName(env, elzAbbreviations.dBforPostgreSQLServers, 'ees')
  }
  eventGrid: {
    topics: {
      publicationChanged: elzName(env, elzAbbreviations.eventGridTopics, replace(eventTopics.publicationChanged, '-', ''))
      releaseChanged: elzName(env, elzAbbreviations.eventGridTopics, replace(eventTopics.releaseChanged, '-', ''))
      releaseVersionChanged: elzName(env, elzAbbreviations.eventGridTopics, replace(eventTopics.releaseVersionChanged, '-', ''))
      themeChanged: elzName(env, elzAbbreviations.eventGridTopics, replace(eventTopics.themeChanged, '-', ''))
    }
  }
  logAnalyticsWorkspace: elzName(env, elzAbbreviations.operationalInsightsWorkspaces, 'ees')
}

