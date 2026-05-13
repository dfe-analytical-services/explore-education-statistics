using '../main.bicep'

// Environment Params
param environmentName = 'Pre-Production'

// Allow screening results to be logged in the Screener API log stream on Pre-Prod.
param logScreeningResults = true

// Don't include any worker pool in Plumber yet for Pre-Prod.
param concurrentRWorkers = 0