import { AppServicePlanSku } from '../../common/components/app-service-plan/types.bicep'

@export()
type PublicSiteConfig = {

  @description('App Service SKU')
  appServiceSku: AppServicePlanSku?

  @description('Google Analytics tracking ID for the public app. Leave as empty string to disable Google Analytics.')
  googleAnalyticsTrackingId: string?

  @description('The default duration in seconds for Azure Front Door to cache content.')
  defaultCacheMaxAgeSeconds: int?
}

var defaultConfig = {
  appServiceSku: {
    tier: 'PremiumV2'
    name: 'P2V2'
  }
  defaultCacheMaxAgeSeconds: 30
}

@export()
func mergePublicSiteConfig(overridden PublicSiteConfig) PublicSiteConfig =>
  union(defaultConfig, overridden)
