import { abbreviations } from '../common/abbreviations.bicep'

@export()
type ResourceNames = {
  admin: {
    appService: string
    appServicePlan: string
    appInsights: string
  }
  publicApi: {
    processor: {
      functionApp: string
    }
  }
  screener: {
    functionApp: string
  }
  vnet: {
    vnet: string
    subnets: {
      admin: string
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
  publicApi: {
    processor: {
      functionApp: '${publicApiResourcePrefix}-${abbreviations.webSitesFunctions}-processor'
    }
  }
  screener: {
    functionApp: '${screenerResourcePrefix}-${abbreviations.webSitesFunctions}-screener'
  }
  vnet: {
    vnet: '${legacyResourcePrefix}-vnet-ees'
    subnets: {
      admin: '${legacyResourcePrefix}-${abbreviations.networkVirtualNetworksSubnets}-ees-admin'
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

