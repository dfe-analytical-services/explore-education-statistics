import { FunctionAppServicePlanSku } from '../../common/components/app-service-plan/types.bicep'

@export()
type ImporterConfig = {

  @description('Function App Service SKU')
  appServiceSku: FunctionAppServicePlanSku?
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
