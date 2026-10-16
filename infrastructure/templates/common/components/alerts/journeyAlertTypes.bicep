import { Severity } from 'types.bicep'

@export()
type JourneyAlertPolicy = {
  @description('Stable rule name, unique within the resource group.')
  name: string
  enabled: bool
  @description('Owner, expected response, cohort and threshold rationale for notifications.')
  description: string
  kind: 'http5xx' | 'adminSignIn'
  appInsightsResourceId: string
  @description('Exact AppRoleName, including an empty string if verified for browser telemetry.')
  roleName: string
  windowSize: 'PT5M' | 'PT15M' | 'PT30M'
  severity: Severity
  @description('Either means count OR rate; count is independent of minimumRequests.')
  trigger: 'count' | 'rate' | 'either'
  @minValue(1)
  errorCountThreshold: int
  @description('Minimum errors required for the rate branch only.')
  @minValue(1)
  minimumErrors: int
  @minValue(1)
  minimumRequests: int
  @description('100 basis points = 1 percent. Used only by rate/either policies.')
  @minValue(1)
  @maxValue(10000)
  errorRateBasisPoints: int
  @description('Exact URL paths, ORed together. Empty selects all paths. Query strings are ignored.')
  paths: string[]
  @description('Exact AppRequests Name values, ORed together and ANDed with paths. Verify outer request names before enabling.')
  requestNames: string[]
}
