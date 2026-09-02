// Note this is not an exhaustive list of SKUs but ones that are commonly used in the service.
// If non-ElasticPremium types are added, a @discriminator can be used on "tier" to provide other
// combinations of valid name-tier-family combinations.
@export()
type FunctionAppServicePlanSku = {
  name: 'EP1' | 'EP2' | 'EP3'
  tier: 'ElasticPremium'
  family: 'EP'
}

type AppServicePlanSkuBasic = {
  tier: 'Basic'
  name: 'B1' | 'B2' | 'B3'
}

type AppServicePlanSkuStandard = {
  tier: 'Standard'
  name: 'S1' | 'S2' | 'S3'
}

type AppServicePlanSkuPremiumV2 = {
  tier: 'Premium'
  name: 'P1V2' | 'P2V2' | 'P3V2'
}

// Note this is not an exhaustive list of SKUs.
@export()
@discriminator('tier')
type AppServicePlanSku = 
  | AppServicePlanSkuBasic
  | AppServicePlanSkuStandard
  | AppServicePlanSkuPremiumV2
