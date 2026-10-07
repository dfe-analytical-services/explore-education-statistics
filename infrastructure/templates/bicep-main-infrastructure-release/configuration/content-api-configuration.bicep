import { AppServicePlanSku } from '../../common/components/app-service-plan/types.bicep'

@export()
type ContentApiConfig = {

  @description('App Service SKU')
  appServiceSku: AppServicePlanSku?

  @description('''
  Whether to restrict the Content API App Service origin to requests routed through this environment's Azure
  Front Door profile. Enable only after this environment's Content API custom domain has been cut over to
  Front Door.
  ''')
  restrictOriginToFrontDoor: bool?
}

var defaultConfig = {
  appServiceSku: {
    tier: 'PremiumV2'
    name: 'P1V2'
  }
  restrictOriginToFrontDoor: false
}

@export()
func mergeContentApiConfig(overridden ContentApiConfig) ContentApiConfig =>
  union(defaultConfig, overridden)
