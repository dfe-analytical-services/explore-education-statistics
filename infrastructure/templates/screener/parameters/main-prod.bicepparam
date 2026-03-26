using '../main.bicep'

// Environment Params
param environmentName = 'Production'

param screenerFunctionAppSku = {
  name: 'EP3'
  tier: 'ElasticPremium'
  family: 'EP'
}

// Ensure that the Screener correctly carries out data dictionary checks
// in Prod to determine whether or not a data set is API compatible or not.
param includeDataDictionaryChecks bool = true