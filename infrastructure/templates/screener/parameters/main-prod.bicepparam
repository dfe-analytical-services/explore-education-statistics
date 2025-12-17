using '../main.bicep'

// Environment Params
param environmentName = 'Production'

param screenerFunctionAppSku = {
  name: 'EP3'
  tier: 'ElasticPremium'
  family: 'EP'
}
