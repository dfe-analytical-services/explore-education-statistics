using '../main.bicep'

// Environment Params
param environmentName = 'Development'

param publicSiteInternalServiceFqdn = 's101d01-ees-fde-eve8c8hmd6gxgqcr.a03.azurefd.net'

param publicApiApplicationGatewayFqdn = 'dev.statistics.api.education.gov.uk'
param publicApiPublicUrl = 'https://pp-api.education.gov.uk/statistics-dev'

param slackAlertsChannels = ['C067Z1K68UD']

// TEMPORARY: exercises the secondary-workspace token selection before it reaches production, where
// it would otherwise run for the first time. Revert once verified.
param secondarySlackAlertsChannels = ['C0C13TPGB53']
