using '../main.bicep'

// Environment Params
param environmentName = 'Production'

param screenerFunctionAppSku = {
  name: 'EP3'
  tier: 'ElasticPremium'
  family: 'EP'
}

// Currently explicitly setting "includeDataDictionaryChecks" to false for Prod whilst no
// BAU override is in place to allow a data set that failed screening to be an API data set.
//
// In the future, this can be switched back to true to allow Prod to properly enforce this
// quality check.
param includeDataDictionaryChecks bool = false