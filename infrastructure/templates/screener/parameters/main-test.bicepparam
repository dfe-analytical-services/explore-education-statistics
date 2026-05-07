using '../main.bicep'

// Environment Params
param environmentName = 'Test'

// Allow screening results to be logged in the Screener API log stream on Test.
param logScreeningResults = true
