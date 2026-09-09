import { FunctionAppServicePlanSku } from '../../common/components/app-service-plan/types.bicep'

@export()
type ImporterConfig = {

  @description('Function App Service SKU')
  appServiceSku: FunctionAppServicePlanSku?
}

@export()
type ImporterPipelineVariables = {

  @description('Does the Importer Function App have a dedicated storage account yet?')
  storageAccountExists: bool?
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
