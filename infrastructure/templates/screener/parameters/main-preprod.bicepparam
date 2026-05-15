using '../main.bicep'

// Environment Params
param environmentName = 'Pre-Production'

// Allow screening results to be logged in the Screener API log stream on Pre-Prod.
param logScreeningResults = true

// Don't include any worker pool in Plumber yet for Pre-Prod.
param concurrentRWorkers = 0

// Temporary run with 2 instances in Pre-Prod to test the autoscaling config works before reaching Prod.
// Can be removed in a subsequent PR for https://dfedigital.atlassian.net/browse/EES-7139.
param minimumInstanceCount = 2
param maximumInstanceCount = 4
