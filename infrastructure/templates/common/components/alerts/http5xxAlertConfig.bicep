import { WindowSize, EvaluationFrequency, Severity } from 'types.bicep'

@export()
type Http5xxAlertConfig = {
  @description('Strictly greater than this value. Total measures errors across the window; Minimum preserves the existing rule behaviour.')
  @minValue(0)
  threshold: int
  aggregation: 'Total' | 'Minimum'
  windowSize: WindowSize
  evaluationFrequency: EvaluationFrequency
  severity: Severity
}

@export()
type ServiceHttp5xxAlerts = {
  publicSite: Http5xxAlertConfig?
  dataApi: Http5xxAlertConfig?
  contentApi: Http5xxAlertConfig?
  admin: Http5xxAlertConfig?
  importer: Http5xxAlertConfig?
  publisher: Http5xxAlertConfig?
  notifier: Http5xxAlertConfig?
}
