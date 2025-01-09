@export()
type EvaluationFrequency = 'PT1M' | 'PT5M' | 'PT15M' | 'PT30M' | 'PT1H'

@export()
type WindowSize = 'PT5M' | 'PT15M' | 'PT30M' | 'PT1H'

@export()
type TimeAggregation = 'Average' | 'Count' | 'Maximum' | 'Minimum' | 'Total'

@export()
type Severity = 'Critical' | 'Error' | 'Warning' | 'Informational' | 'Verbose'

@export()
type Sensitivity = 'High' | 'Medium' | 'Low'

@export()
var severityMapping = {
  Critical: 0
  Error: 1
  Warning: 2
  Informational: 3
  Verbose: 4
}
