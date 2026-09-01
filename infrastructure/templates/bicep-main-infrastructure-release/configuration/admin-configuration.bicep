import { AppServicePlanSku } from '../../common/components/app-service-plan/types.bicep'
import { SignalRSku } from '../../common/components/signalr/types.bicep'

@export()
type AdminConfig = {

  @description('App Service SKU')
  appServiceSku: AppServicePlanSku?

  @description('SignalR Service SKU')
  signalRSku: SignalRSku?

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
  signalRSku: {
    name: 'Standard_S1'
    capacity: 1
  }
  enableThemeDeletion: false
  enableEinPublishedPageDeletion: false
  preReleaseMinutesBeforeStart: 870
}

@export()
func mergeAdminConfig(overridden AdminConfig) AdminConfig =>
  union(defaultConfig, overridden)
