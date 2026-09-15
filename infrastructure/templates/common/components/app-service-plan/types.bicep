type FunctionAppPlanSkuElastic = {
  name: 'EP1' | 'EP2' | 'EP3'
  tier: 'ElasticPremium'
  family: 'EP'
}

type FunctionAppPlanSkuPremiumV2 = {
  tier: 'PremiumV2'
  name: 'P1V2' | 'P2V2' | 'P3V2'
}

// Note this is not an exhaustive list of SKUs.
@export()
@discriminator('tier')
type FunctionAppServicePlanSku = 
  | FunctionAppPlanSkuElastic
  | FunctionAppPlanSkuPremiumV2


type AppServicePlanSkuBasic = {
  tier: 'Basic'
  name: 'B1' | 'B2' | 'B3'
}

type AppServicePlanSkuStandard = {
  tier: 'Standard'
  name: 'S1' | 'S2' | 'S3'
}

type AppServicePlanSkuPremiumV2 = {
  tier: 'PremiumV2'
  name: 'P1V2' | 'P2V2' | 'P3V2'
}

// Note this is not an exhaustive list of SKUs.
@export()
@discriminator('tier')
type AppServicePlanSku = 
  | AppServicePlanSkuBasic
  | AppServicePlanSkuStandard
  | AppServicePlanSkuPremiumV2
