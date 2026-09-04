using '../main.bicep'

// Environment Params
param environmentName = 'Production'

param averagePublicSiteResponseTimeAlertThresholdMillis = 15000

param publicSiteInternalServiceFqdn = 's101p01-ees-fde-hzgvd4b5effuaua2.a02.azurefd.net'

param publicApiApplicationGatewayFqdn = 'statistics.api.education.gov.uk'
param publicApiPublicUrl = 'https://api.education.gov.uk/statistics'

param slackAlertsChannels = ['C01MCTX47E3']

// Alerts are mirrored to the Hive workspace in production only. The other environments inherit the
// empty default in main.bicep, which stops the Logic App posting to Hive at all.
param hiveSlackAlertsChannels = ['C0BSTE3G6KY']

param recoveryServicesVaultImmutable = true
