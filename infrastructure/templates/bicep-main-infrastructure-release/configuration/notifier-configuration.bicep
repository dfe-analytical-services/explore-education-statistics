import { FunctionAppServicePlanSku } from '../../common/components/app-service-plan/types.bicep'

@export()
type NotifierConfig = {

  @description('Function App Service SKU')
  appServiceSku: FunctionAppServicePlanSku?
}

var defaultConfig = {
  appServiceSku: {
    tier: 'Standard'
    name: 'S1'
  }
}

@export()
func mergeNotifierConfig(overridden NotifierConfig) NotifierConfig =>
  union(defaultConfig, overridden)
