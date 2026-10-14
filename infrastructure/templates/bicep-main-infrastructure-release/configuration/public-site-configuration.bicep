import { AppServicePlanSku } from '../../common/components/app-service-plan/types.bicep'

@export()
type PublicSiteConfig = {

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
func mergePublicSiteConfig(overridden PublicSiteConfig) PublicSiteConfig =>
  union(defaultConfig, overridden)
