import { FunctionAppServicePlanSku } from '../../common/components/app-service-plan/types.bicep'

@export()
type NotifierConfig = {

  @description('Function App Service SKU')
  appServiceSku: FunctionAppServicePlanSku?

  @description('Replaces Notify exceptions with logged messages only when team-only API keys are used and a recipient email address is not valid for that key.')
  suppressExceptionsForTeamOnlyApiKeyErrors: bool?
}

var defaultConfig = {
  appServiceSku: {
    tier: 'Standard'
    name: 'S1'
  }
  suppressExceptionsForTeamOnlyApiKeyErrors: false
}

@export()
func mergeNotifierConfig(overridden NotifierConfig) NotifierConfig =>
  union(defaultConfig, overridden)
