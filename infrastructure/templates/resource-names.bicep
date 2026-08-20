import { abbreviations } from 'common/abbreviations.bicep'

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
      screenerStorageAccountConnectionString: string
      adminSignalrConnectionString: string
      adminGovUkNotifyApiKey: string
      openIdConnectClientId: string
      openIdConnectAuthority: string
      openIdConnectValidAudience: string
      openIdConnectValidIssuers: string
      openIdConnectFullyQualifiedScopeName: string
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
      adminGovUkNotifyApiKey: 'ees_admin_govuknotify_api_key'
      adminSignalrConnectionString: 'ees-signalr-admin-connectionstring'
      openIdConnectClientId: 'ees_openidconnect_clientid'
      openIdConnectAuthority: 'ees_openidconnect_authority'
      openIdConnectValidAudience: 'ees_openidconnect_valid_audience'
      openIdConnectValidIssuers: 'ees_openidconnect_valid_issuers'
      openIdConnectFullyQualifiedScopeName: 'ees_openidconnect_fully_qualified_scope_name'
      publicApiContainerAppPrivateUrl: 'ees_publicapi_public_api_containerapp_private_url'
      screenerStorageAccountConnectionString: '${legacyResourcePrefix}-ees-sapisafn-connection-string'
      coreStorageAccountConnectionString: 'ees_storage_core'
      publicStorageAccountConnectionString: 'ees_storage_public'
      publisherStorageAccountConnectionString: 'ees_storage_publisher'
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

