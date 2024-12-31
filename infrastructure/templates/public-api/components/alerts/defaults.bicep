@export()
var cpu = {
  alertNameSuffix: 'cpu-percentage'
  aggregation: 'Average'
  operator: 'GreaterThan'
  evaluationFrequency: 'PT5M'
  windowSize: 'PT15M'
  numberOfEvaluationPeriods: 5
  minFailingPeriodsToAlert: 5
}
