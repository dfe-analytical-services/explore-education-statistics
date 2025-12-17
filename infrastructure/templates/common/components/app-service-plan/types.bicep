// Note this is not an exhaustive list of SKUs but ones that are commonly used in the service.
// If non-ElasticPremium types are added, a @discriminator can be used on "tier" to provide other
// combinations of valid name-tier-family combinations.
@export()
type AppServicePlanSku = {
  name: 'EP1' | 'EP2' | 'EP3'
  tier: 'ElasticPremium'
  family: 'EP'
}
