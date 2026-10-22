import { FunctionAppServicePlanSku } from '../../common/components/app-service-plan/types.bicep'

@export()
type PublisherConfig = {

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
func mergePublisherConfig(overridden PublisherConfig) PublisherConfig =>
  union(defaultConfig, overridden)
