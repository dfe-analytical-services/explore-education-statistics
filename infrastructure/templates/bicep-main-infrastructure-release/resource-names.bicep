import { abbreviations } from '../common/abbreviations.bicep'

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
  frontDoor: {
    frontDoorName: string
    defaultEndpoint: {
      endpointName: string
    }
  }
  nlSearch: {
    functionApp: string
  }
  notifier: {
    functionApp: string
  }
  publicApi: {
    processor: {
      functionApp: string
    }
  }
  publicSite: {
    appService: string
    appServicePlan: string
    appInsights: string
  }
  screener: {
    functionApp: string
  }
  search: {
    service: string
  }
  vnet: {
    vnet: string
    subnets: {
      admin: string
      contentApi: string
      dataApi: string
      publicSite: string
    }
  }
  keyVault: {
    keyVault: string
    secrets: {
      acr: {
        dockerPullUsername: string
        dockerPullPassword: string
      }
      admin: {
        adminSignalrConnectionString: string
        adminGovUkNotifyApiKey: string
        openIdConnectClientId: string
        openIdConnectAuthority: string
        openIdConnectValidAudience: string
        openIdConnectValidIssuers: string
        openIdConnectFullyQualifiedScopeName: string
        screenerStorageAccountConnectionString: string
      }
      coreStorageAccountConnectionString: string
      publicStorageAccountConnectionString: string
      publisherStorageAccountConnectionString: string
      publicApiContainerAppPrivateUrl: string
    }
  }
  alertsGroup: string
  databases: {
    coreSqlServer: string
    publicSqlServer: string
    contentDb: string
    statisticsDb: string
  }
  eventGrid: {
    topics: {
      releaseChanged: string
      publicationChanged: string
      themeChanged: string
    }
  }
  logAnalyticsWorkspace: string
}

@export()
func getResourceNames(
  legacyResourcePrefix string,
  publicApiResourcePrefix string,
  screenerResourcePrefix string,
  newResourcePrefix string) ResourceNames => {

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
  nlSearch: {
    functionApp: '${newResourcePrefix}-${abbreviations.webSitesFunctions}-nlsearch'
  }
  notifier: {
    functionApp: '${legacyResourcePrefix}-${abbreviations.webSitesFunctions}-ees-notify'
  }
  frontDoor: {
    frontDoorName: '${newResourcePrefix}-${abbreviations.frontDoorProfiles}'
    defaultEndpoint: {
      endpointName: '${newResourcePrefix}-${abbreviations.frontDoorEndpoints}'
    }
  }
  publicApi: {
    processor: {
      functionApp: '${publicApiResourcePrefix}-${abbreviations.webSitesFunctions}-processor'
    }
  }
  publicSite: {
    appService: '${legacyResourcePrefix}-${abbreviations.webSitesAppService}-ees-public-site'
    appServicePlan: '${legacyResourcePrefix}-${abbreviations.webServerFarms}-ees-public-site'
    appInsights: '${legacyResourcePrefix}-${abbreviations.insightsComponents}-ees-public-site'
  }
  screener: {
    functionApp: '${screenerResourcePrefix}-${abbreviations.webSitesFunctions}-screener'
  }
  search: {
    service: '${newResourcePrefix}-srch'
  }
  vnet: {
    vnet: '${legacyResourcePrefix}-vnet-ees'
    subnets: {
      admin: '${legacyResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-ees-admin'
      contentApi: '${legacyResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-ees-content'
      dataApi: '${legacyResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-ees-data'
      publicSite: '${legacyResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-ees-public-site'
    }
  }
  keyVault: {
    keyVault: '${legacyResourcePrefix}-kv-ees-01'
    secrets: {
      acr: {
        dockerPullUsername: 'DOCKER-REGISTRY-SERVER-USERNAME'
        dockerPullPassword: 'DOCKER-REGISTRY-SERVER-PASSWORD'
      }
      admin: {
        adminGovUkNotifyApiKey: 'ees-admin-govuknotify-api-key'
        adminSignalrConnectionString: 'ees-signalr-admin-connectionstring'
        openIdConnectClientId: 'ees-openidconnect-clientid'
        openIdConnectAuthority: 'ees-openidconnect-authority'
        openIdConnectValidAudience: 'ees-openidconnect-valid-audience'
        openIdConnectValidIssuers: 'ees-openidconnect-valid-issuers'
        openIdConnectFullyQualifiedScopeName: 'ees-openidconnect-fully-qualified-scope-name'
        screenerStorageAccountConnectionString: '${legacyResourcePrefix}eessapisafn-connection-string'
      }
      publicApiContainerAppPrivateUrl: 'ees-publicapi-public-api-containerapp-private-url'
      coreStorageAccountConnectionString: 'ees-storage-core'
      publicStorageAccountConnectionString: 'ees-storage-public'
      publisherStorageAccountConnectionString: 'ees-storage-publisher'
    }
  } 
  alertsGroup: '${legacyResourcePrefix}-ag-ees-alertedusers'
    databases: {
    coreSqlServer: '${legacyResourcePrefix}-sqlsvr-ees-01'
    publicSqlServer: '${legacyResourcePrefix}-sqlsvr-ees-02'
    contentDb: 'content'
    statisticsDb: 'statistics'
  }
  eventGrid: {
    topics: {
      releaseChanged: '${newResourcePrefix}-evgt-release-changed'
      publicationChanged: '${newResourcePrefix}-evgt-publication-changed'
      themeChanged: '${newResourcePrefix}-evgt-theme-changed'
    }
  }
  logAnalyticsWorkspace: '${newResourcePrefix}-${abbreviations.operationalInsightsWorkspaces}'
}

