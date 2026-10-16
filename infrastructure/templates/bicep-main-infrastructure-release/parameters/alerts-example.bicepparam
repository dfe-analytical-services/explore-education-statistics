// Type-checked examples, NOT production settings. Do not deploy this file.
// Copy approved policies into the appropriate environment's parameter file.
using '../main.bicep'

param http5xxAlerts = {
  // Illustrative count override: strictly more than 4 errors (i.e. 5+) in 5 minutes.
  // The public site is most important: retain broad detection alongside targeted rules.
  publicSite: {
    threshold: 4
    aggregation: 'Total'
    windowSize: 'PT5M'
    evaluationFrequency: 'PT1M'
    severity: 'Error'
  }
  // Other keys: dataApi (Statistics), contentApi, admin, importer, publisher, notifier.
  // Omitted services retain their existing rules.
}

param http401Alerts = {
  // Illustrative count override: strictly more than 20 unauthorised responses in 15 minutes.
  // Expired tokens and silent reauthentication make some Admin 401s routine. A 401 count
  // does not measure users unable to sign in.
  admin: {
    threshold: 20
    aggregation: 'Total'
    windowSize: 'PT15M'
    evaluationFrequency: 'PT5M'
    severity: 'Warning'
  }
  // Same keys as http5xxAlerts. Omitted services retain their existing rules.
}

