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
  // Expired tokens and silent reauthentication make some Admin 401s routine. Use the
  // dedicated adminSignIn journey policy, not this rule, to detect users unable to sign in.
  admin: {
    threshold: 20
    aggregation: 'Total'
    windowSize: 'PT15M'
    evaluationFrequency: 'PT5M'
    severity: 'Warning'
  }
  // Same keys as http5xxAlerts. Omitted services retain their existing rules.
}

var sample = {
  enabled: false
  appInsightsResourceId: '/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/REPLACE/providers/Microsoft.Insights/components/REPLACE'
  roleName: 'REPLACE-with-verified-production-role'
  windowSize: 'PT5M'
  severity: 'Error'
  trigger: 'either'
  errorCountThreshold: 10
  minimumErrors: 3
  minimumRequests: 20
  errorRateBasisPoints: 500 // 5%. Three errors out of 20 triggers the rate branch.
  requestNames: []
}

param journeyAlerts = [
  {
    ...sample
    name: 'example-public-data-tables'
    description: 'Public site: opening data tables. Owner and response to be agreed.'
    kind: 'http5xx'
    paths: ['/data-tables']
    // Set requestNames to observed OUTER HTTP request names for Next.js;
    // do not count nested rendering spans as additional user requests.
    requestNames: ['REPLACE-with-observed-outer-request-name']
  }
  {
    ...sample
    name: 'example-statistics-saved-query'
    description: 'Statistics/Data API: an agreed important saved table. Choose a real data block ID.'
    kind: 'http5xx'
    // This is a real route pattern. Replace the sample UUID with an agreed data block.
    paths: ['/api/tablebuilder/fast-track/00000000-0000-0000-0000-000000000000']
    trigger: 'count'
    errorCountThreshold: 1 // Low-volume critical query: no traffic floor on the count branch.
  }
  {
    ...sample
    name: 'example-public-data-api'
    description: 'External Public Data API consumers. Select its own App Insights resource and an agreed endpoint.'
    kind: 'http5xx'
    paths: ['/REPLACE-with-agreed-public-data-api-path']
    // Automated API clients are included; a bot user agent is not a reason to exclude them.
  }
  {
    ...sample
    name: 'example-admin-sign-in'
    description: 'Admin users unable to finish sign-in. Check identity provider, token, sign-in API and permissions stages.'
    kind: 'adminSignIn'
    // Select the BROWSER App Insights resource and its observed role, possibly ''.
    paths: []
    requestNames: []
    trigger: 'count'
    errorCountThreshold: 1 // Proposed sensitive signal, independent of general Admin HTTP noise.
  }
  {
    ...sample
    name: 'example-admin-sign-in-server-errors'
    description: 'Admin sign-in endpoint server failures, including first-time invited users not recognised by browser history.'
    kind: 'http5xx'
    // Select Admin API SERVER telemetry, not the browser component/role above.
    paths: ['/api/sign-in']
    trigger: 'count'
    errorCountThreshold: 1
  }
]
