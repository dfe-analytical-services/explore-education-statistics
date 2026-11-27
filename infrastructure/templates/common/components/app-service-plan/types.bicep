type FunctionAppPlanSkuElastic = {
  name: 'EP1' | 'EP2' | 'EP3'
  tier: 'ElasticPremium'
  family: 'EP'
}

type FunctionAppPlanSkuPremiumV2 = {
  tier: 'PremiumV2'
  name: 'P1V2' | 'P2V2' | 'P3V2'
}

type FunctionAppPlanSkuBasic = {
  tier: 'Basic'
  name: 'B1' | 'B2' | 'B3'
}

type FunctionAppPlanSkuStandard = {
  tier: 'Standard'
  name: 'S1' | 'S2' | 'S3'
}

// Note this is not an exhaustive list of SKUs.
@export()
@discriminator('tier')
type FunctionAppServicePlanSku =
  | FunctionAppPlanSkuElastic
  | FunctionAppPlanSkuPremiumV2
  | FunctionAppPlanSkuBasic
  | FunctionAppPlanSkuStandard

type AppServicePlanSkuStandard = {
  tier: 'Standard'
  name: 'S1' | 'S2' | 'S3'
}

type AppServicePlanSkuPremiumV2 = {
  tier: 'PremiumV2'
  name: 'P1V2' | 'P2V2' | 'P3V2'
}

type AppServicePlanSkuPremiumV3 = {
  tier: 'PremiumV3'
  name: 'P0V3' | 'P1V3' | 'P2V3' | 'P3V3'
}

// Note this is not an exhaustive list of SKUs. Basic SKUs are deliberately excluded, as they don't support the
// deployment slots that our App Services rely upon for slot swapping during deploys.
@export()
@discriminator('tier')
type AppServicePlanSku =
  | AppServicePlanSkuStandard
  | AppServicePlanSkuPremiumV2
  | AppServicePlanSkuPremiumV3
