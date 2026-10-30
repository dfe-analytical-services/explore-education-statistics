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

var defaultConfig = {
  appServiceSku: {
    tier: 'PremiumV2'
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
