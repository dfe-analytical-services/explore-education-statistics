import { AppServicePlanSku } from '../../common/components/app-service-plan/types.bicep'

@export()
type ContentApiConfig = {

  @description('App Service SKU')
  appServiceSku: AppServicePlanSku?
}

var defaultConfig = {
  appServiceSku: {
    tier: 'PremiumV2'
    name: 'P1V2'
  }
}

@export()
func mergeContentApiConfig(overridden ContentApiConfig) ContentApiConfig =>
  union(defaultConfig, overridden)
