import { AppServicePlanSku } from '../../common/components/app-service-plan/types.bicep'

@export()
type ImporterConfig = {

  @description('App Service SKU')
  appServiceSku: AppServicePlanSku?
}

var defaultConfig = {
  appServiceSku: {
    tier: 'PremiumV2'
    name: 'P2V2'
  }
}

@export()
func mergeImporterConfig(overridden ImporterConfig) ImporterConfig =>
  union(defaultConfig, overridden)
