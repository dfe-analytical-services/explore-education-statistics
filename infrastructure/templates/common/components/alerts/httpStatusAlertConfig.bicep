import { WindowSize, EvaluationFrequency, Severity } from 'types.bicep'

@export()
type HttpStatusAlertConfig = {
  @description('Strictly greater than this value. Total measures responses across the window; Minimum preserves the existing rule behaviour.')
  @minValue(0)
  threshold: int
  aggregation: 'Total' | 'Minimum'
  windowSize: WindowSize
  evaluationFrequency: EvaluationFrequency
  severity: Severity
}

@export()
type ServiceHttpStatusAlerts = {
  publicSite: HttpStatusAlertConfig?
  dataApi: HttpStatusAlertConfig?
  contentApi: HttpStatusAlertConfig?
  admin: HttpStatusAlertConfig?
  importer: HttpStatusAlertConfig?
  publisher: HttpStatusAlertConfig?
  notifier: HttpStatusAlertConfig?
}

@export()
@description('Static metric alert config overrides for an Http401 or Http5xx rule. Null preserves the existing rule.')
func httpStatusAlertOverride(config HttpStatusAlertConfig?) object =>
  config == null
    ? {}
    : {
        aggregation: config!.aggregation
        threshold: string(config!.threshold)
        windowSize: config!.windowSize
        evaluationFrequency: config!.evaluationFrequency
        severity: config!.severity
      }
