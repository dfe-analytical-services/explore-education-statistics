import { AppServicePlanSku } from '../../common/components/app-service-plan/types.bicep'

@export()
type DataApiConfig = {

  @description('App Service SKU')
  appServiceSku: AppServicePlanSku?
}

var defaultConfig = {
  appServiceSku: {
    tier: 'Premium'
    name: 'P1V2'
  }
}

@export()
func mergeDataApiConfig(overridden DataApiConfig) DataApiConfig =>
  union(defaultConfig, overridden)
