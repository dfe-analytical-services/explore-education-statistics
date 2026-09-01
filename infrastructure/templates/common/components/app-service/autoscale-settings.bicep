@description('Name of the App Service to allow autoscaling on.')
param appServiceName string

@description('Id of the App Service Plan to allow autoscaling on.')
param appServicePlanId string

@description('Whether or not the autoscale settings are currently enabled.')
param autoscaleEnabled bool

@description('Minimum, maximum and default number of instances to run when autoscaling is enabled.')
param instances {
  min: int
  max: int
  default: int
} = {
  min: 2
  max: 10
  default: 2
}

resource autoscaleSettings 'Microsoft.Insights/autoscalesettings@2022-10-01' = {
  name: '${appServiceName}-autoscale'
  location: resourceGroup().location
  tags: {}
  properties: {
    name: '${appServiceName}-autoscale'
    enabled: autoscaleEnabled
    targetResourceUri: appServicePlanId
    profiles: [
      {
        name: 'Auto created scale condition'
        capacity: {
          minimum: '${instances.min}'
          maximum: '${instances.max}'
          default: '${instances.default}'
        }
        rules: [
          {
            scaleAction: {
              direction: 'Increase'
              type: 'ChangeCount'
              value: '1'
              cooldown: 'PT5M'
            }
            metricTrigger: {
              metricName: 'CpuPercentage'
              metricNamespace: 'microsoft.web/serverfarms'
              metricResourceUri: appServicePlanId
              operator: 'GreaterThan'
              statistic: 'Average'
              threshold: 70
              timeAggregation: 'Average'
              timeGrain: 'PT1M'
              timeWindow: 'PT10M'
              dimensions: []
              dividePerInstance: false
            }
          }
          {
            scaleAction: {
              direction: 'Decrease'
              type: 'ChangeCount'
              value: '1'
              cooldown: 'PT5M'
            }
            metricTrigger: {
              metricName: 'CpuPercentage'
              metricNamespace: 'microsoft.web/serverfarms'
              metricResourceUri: appServicePlanId
              operator: 'LessThan'
              statistic: 'Average'
              threshold: 30
              timeAggregation: 'Average'
              timeGrain: 'PT1M'
              timeWindow: 'PT10M'
              dimensions: []
              dividePerInstance: false
            }
          }
        ]
      }
    ]
    notifications: []
    targetResourceLocation: ''
  }
}
