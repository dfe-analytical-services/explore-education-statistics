@description('Name of the Data Factory instance that owns this pipeline.')
param dataFactoryName string

@description('Name of the associated pipeline.')
param pipelineName string

@description('Size of batch of Observations to delete.')
param removeSoftDeletedSubjectsObservationLimit int

@description('Size of batch of Observations to delete.')
param removeSoftDeletedSubjectsObservationCommitBatchSize int

@description('Size of batch of ObservationFilterItems to delete.')
param removeSoftDeletedSubjectsObservationFilterItemCommitBatchSize int

resource trigger 'Microsoft.DataFactory/factories/triggers@2018-06-01' = {
  name: '${dataFactoryName}/purge_soft_deleted_subjects_trigger'
  properties: {
    annotations: []
    pipelines: [
      {
        pipelineReference: {
          referenceName: pipelineName
          type: 'PipelineReference'
        }
        parameters: {
          TotalObservationLimit: removeSoftDeletedSubjectsObservationLimit
          ObservationCommitBatchSize: removeSoftDeletedSubjectsObservationCommitBatchSize
          ObservationFilterItemCommitBatchSize: removeSoftDeletedSubjectsObservationFilterItemCommitBatchSize
        }
      }
    ]
    type: 'ScheduleTrigger'
    typeProperties: {
      recurrence: {
        frequency: 'Day'
        interval: 1
        startTime: '2020-11-23T19:00:00'
        timeZone: 'GMT Standard Time'
        schedule: {
          hours: [
            19
          ]
          minutes: [
            0
          ]
        }
      }
    }
  }
}
