import { abbreviations } from '../common/abbreviations.bicep'

@export()
type ResourceNames = {
  admin: {
    appService: string
    appServicePlan: string
    appInsights: string
  }
  analytics: {
    storage: {
      storageAccountName: string
      fileShareName: string
    }
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
  publicApi: {
    processor: {
      functionApp: string
    }
  }
  publicSite: {
    appService: {
      appServiceName: string
    }
  }
  screener: {
    functionApp: string
  }
  vnet: {
    vnet: string
    subnets: {
      admin: string
      dataApi: string
    }
  }
  keyVault: {
    keyVault: string
    secrets: {
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

  admin: {
    appService: '${legacyResourcePrefix}-${abbreviations.webSitesAppService}-ees-admin'
    appServicePlan: '${legacyResourcePrefix}-${abbreviations.webServerFarms}-ees-admin'
    appInsights: '${legacyResourcePrefix}-${abbreviations.insightsComponents}-ees-admin'
  }
  analytics: {
    storage: {
      storageAccountName: '${newResourcePrefix}eespapisa'
      fileShareName: '${newResourcePrefix}-share-anlyt'
    }
  }
  dataApi: {
    appService: '${legacyResourcePrefix}-${abbreviations.webSitesAppService}-ees-data'
    appServicePlan: '${legacyResourcePrefix}-${abbreviations.webServerFarms}-ees-data'
    appInsights: '${legacyResourcePrefix}-${abbreviations.insightsComponents}-ees-data'
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
    appService: {
      appServiceName: '${legacyResourcePrefix}-${abbreviations.webSitesAppService}-ees-public-site'
    }
  }
  screener: {
    functionApp: '${screenerResourcePrefix}-${abbreviations.webSitesFunctions}-screener'
  }
  vnet: {
    vnet: '${legacyResourcePrefix}-vnet-ees'
    subnets: {
      admin: '${legacyResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-ees-admin'
      dataApi: '${legacyResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-ees-data'
    }
  }
  keyVault: {
    keyVault: '${legacyResourcePrefix}-kv-ees-01'
    secrets: {
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

