import { FunctionAppServicePlanSku } from '../../common/components/app-service-plan/types.bicep'

@export()
type PublisherConfig = {

  @description('Function App Service SKU')
  appServiceSku: FunctionAppServicePlanSku?

  @description('Whether the PrepareScheduledReleaseVersionsNow HTTP-triggered function is enabled. Should remain disabled in Production.')
  prepareScheduledReleaseVersionsNowEnabled: bool?

  @description('Whether the PublishScheduledReleaseVersionsNow HTTP-triggered function is enabled. Should remain disabled in Production.')
  publishScheduledReleaseVersionsNowEnabled: bool?

  @description('The time zone used for evaluating Cron expressions of the functions running with Cron triggers.')
  functionAppTimeZone: string?
}

var defaultConfig = {
  appServiceSku: {
    tier: 'Standard'
    name: 'S1'
  }
  prepareScheduledReleaseVersionsNowEnabled: false
  publishScheduledReleaseVersionsNowEnabled: false
  functionAppTimeZone: 'GMT Standard Time'
}

@export()
func mergePublisherConfig(overridden PublisherConfig) PublisherConfig =>
  union(defaultConfig, overridden)
