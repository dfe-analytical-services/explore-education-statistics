import { AppServicePlanSku } from 'common/components/app-service-plan/types.bicep'

@export()
type AdminConfig = {

  @description('Admin App Service SKU')
  appServiceSku: AppServicePlanSku?

  @description('Whether or not to enable theme deletion in this environment (for test teardown).')
  enableThemeDeletion: bool?

  @description('Whether or not to enable published Education In Numbers pages deletion in this environment.')
  enableEinPublishedPageDeletion: bool?

  @description('Pre-release start time as number of minutes before a release is scheduled to be published.')
  preReleaseMinutesBeforeStart: int?
}

@export()
type AdminPipelineVariables = {

    @description('Client ID of the public API Container App app registration in Entra ID.')
    apiAppRegistrationClientId: string?

    @description('Client ID of the public API processor app registration in Entra ID.')
    publicDataProcessorAppRegistrationClientId: string?

    @description('Client ID of the Screener API Function App app registration in Entra ID.')
    screenerAppRegistrationClientId: string?
}

var defaultConfig = {
  appServiceSku: {
    tier: 'Premium'
    name: 'P1V2'
  }
  enableThemeDeletion: false
  enableEinPublishedPageDeletion: false
  preReleaseMinutesBeforeStart: 870
}

@export()
func mergeAdminConfig(overridden AdminConfig) AdminConfig =>
  mergeConfigInternal(json(string(defaultConfig)), json(string(overridden)))

func mergeConfigInternal(default object, overridden object) object => 
  union(default, overridden)
