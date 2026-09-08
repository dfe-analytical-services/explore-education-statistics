@description('Name of the Data Factory instance that owns these triggers.')
param dataFactoryName string

@description('Name of the associated pipeline.')
param pipelineName string

@description('The tables to reindex.')
param fragmentationTables string

resource weekdayTrigger 'Microsoft.DataFactory/factories/triggers@2018-06-01' = {
  name: '${dataFactoryName}/rebuild_indexes_trigger'
  properties: {
    annotations: []
    pipelines: [
      {
        pipelineReference: {
          referenceName: pipelineName
          type: 'PipelineReference'
        }
        parameters: {
          Tables: fragmentationTables
          StoredProcedureStopInMinutes: '570'
          RunMode: 'Weekday'
        }
      }
    ]
    type: 'ScheduleTrigger'
    typeProperties: {
      recurrence: {
        frequency: 'Week'
        interval: 1
        startTime: '2023-03-03T02:00:00'
        timeZone: 'GMT Standard Time'
        schedule: {
          hours: [
            22
          ]
          minutes: [
            30
          ]
          weekDays: [
            'Sunday'
            'Monday'
            'Tuesday'
            'Wednesday'
            'Thursday'
          ]
        }
      }
    }
  }
}

resource weekendTrigger 'Microsoft.DataFactory/factories/triggers@2018-06-01' = {
  name: '${dataFactoryName}/rebuild_indexes_weekend_trigger'
  properties: {
    annotations: []
    pipelines: [
      {
        pipelineReference: {
          referenceName: pipelineName
          type: 'PipelineReference'
        }
        parameters: {
          Tables: fragmentationTables
          StoredProcedureStopInMinutes: '1425'
          RunMode: 'Weekend'
        }
      }
    ]
    type: 'ScheduleTrigger'
    typeProperties: {
      recurrence: {
        frequency: 'Week'
        interval: 1
        startTime: '2023-03-03T02:00:00'
        timeZone: 'GMT Standard Time'
        schedule: {
          hours: [
            22
          ]
          minutes: [
            30
          ]
          weekDays: [
            'Friday'
            'Saturday'
          ]
        }
      }
    }
  }
}
