@description('Name of the Data Factory instance that owns this pipeline.')
param dataFactoryName string

@description('Name of the Statistics database linked service.')
param statisticsDbLinkedServiceName string

@description('Slack channel to post Azure alerts to.')
param slackAlertsChannel string

@secure()
@description('Token to securely post to the Slack channel.')
param slackAppToken string

@secure()
@description('The Power Automate Webhook URL used to post messages to Teams.')
param teamsPowerAutomateWebhookUrl string

@description('Size of batch of Observations to delete.')
param removeSoftDeletedSubjectsObservationLimit int

@description('Size of batch of Observations to delete.')
param removeSoftDeletedSubjectsObservationCommitBatchSize int

@description('Size of batch of ObservationFilterItems to delete.')
param removeSoftDeletedSubjectsObservationFilterItemCommitBatchSize int

var pipelineName = 'pl_purge_subjects_statistics'

resource pipeline 'Microsoft.DataFactory/factories/pipelines@2018-06-01' = {
  name: '${dataFactoryName}/${pipelineName}'
  properties: {
    activities: [
      {
        name: 'sp_remove_soft_deleted_subjects'
        type: 'SqlServerStoredProcedure'
        dependsOn: []
        policy: {
          timeout: '0.12:00:00'
          retry: 0
          retryIntervalInSeconds: 30
          secureOutput: false
          secureInput: false
        }
        userProperties: []
        typeProperties: {
          storedProcedureName: '[dbo].[RemoveSoftDeletedSubjects]'
          storedProcedureParameters: {
            TotalObservationLimit: {
              value: {
                value: '@pipeline().parameters.TotalObservationLimit'
                type: 'Expression'
              }
              type: 'Int32'
            }
            ObservationCommitBatchSize: {
              value: {
                value: '@pipeline().parameters.ObservationCommitBatchSize'
                type: 'Expression'
              }
              type: 'Int32'
            }
            ObservationFilterItemCommitBatchSize: {
              value: {
                value: '@pipeline().parameters.ObservationFilterItemCommitBatchSize'
                type: 'Expression'
              }
              type: 'Int32'
            }
          }
        }
        linkedServiceName: {
          referenceName: statisticsDbLinkedServiceName
          type: 'LinkedServiceReference'
        }
      }
      {
        name: 'Report success'
        type: 'WebActivity'
        dependsOn: [
          {
            activity: 'sp_remove_soft_deleted_subjects'
            dependencyConditions: [
              'Succeeded'
            ]
          }
        ]
        policy: {
          timeout: '0.00:10:00'
          retry: 0
          retryIntervalInSeconds: 30
          secureOutput: false
          secureInput: false
        }
        userProperties: []
        typeProperties: {
          url: 'https://slack.com/api/chat.postMessage'
          method: 'POST'
          headers: {
            'Content-Type': 'application/json'
            Authorization: 'Bearer ${slackAppToken}'
          }
          body: {
            channel: slackAlertsChannel
            text: 'Data Factory Success!'
            attachments: [
              {
                color: 'good'
                fields: [
                  {
                    title: 'Data Factory'
                    value: '@{pipeline().DataFactory}'
                    short: true
                  }
                  {
                    title: 'Pipeline'
                    value: '@{pipeline().Pipeline}'
                    short: true
                  }
                  {
                    title: 'Duration'
                    value: '@{activity(\'sp_remove_soft_deleted_subjects\').Duration}'
                    short: true
                  }
                ]
              }
            ]
          }
        }
      }
      {
        name: 'Report success to Teams'
        type: 'WebActivity'
        dependsOn: [
          {
            activity: 'sp_remove_soft_deleted_subjects'
            dependencyConditions: [
              'Succeeded'
            ]
          }
        ]
        policy: {
          timeout: '0.00:10:00'
          retry: 0
          retryIntervalInSeconds: 30
          secureOutput: false
          secureInput: true
        }
        userProperties: []
        typeProperties: {
          url: teamsPowerAutomateWebhookUrl
          method: 'POST'
          headers: {
            'Content-Type': 'application/json'
          }
          body: {
            type: 'message'
            attachments: [
              {
                contentType: 'application/vnd.microsoft.card.adaptive'
                contentUrl: null
                content: {
                  '$schema': 'http://adaptivecards.io/schemas/adaptive-card.json'
                  type: 'AdaptiveCard'
                  version: '1.2'
                  body: [
                    {
                      type: 'TextBlock'
                      text: 'Data Factory Success!'
                      weight: 'Bolder'
                      size: 'Medium'
                      color: 'good'
                      wrap: true
                    }
                    {
                      type: 'FactSet'
                      facts: [
                        {
                          title: 'Data Factory'
                          value: '@{pipeline().DataFactory}'
                        }
                        {
                          title: 'Pipeline'
                          value: '@{pipeline().Pipeline}'
                        }
                        {
                          title: 'Duration'
                          value: '@{activity(\'sp_remove_soft_deleted_subjects\').Duration}'
                        }
                      ]
                    }
                  ]
                }
              }
            ]
          }
        }
      }
      {
        name: 'Report failure'
        type: 'WebActivity'
        dependsOn: [
          {
            activity: 'sp_remove_soft_deleted_subjects'
            dependencyConditions: [
              'Failed'
            ]
          }
        ]
        policy: {
          timeout: '0.00:10:00'
          retry: 0
          retryIntervalInSeconds: 30
          secureOutput: false
          secureInput: false
        }
        userProperties: []
        typeProperties: {
          url: 'https://slack.com/api/chat.postMessage'
          method: 'POST'
          headers: {
            'Content-Type': 'application/json'
            Authorization: 'Bearer ${slackAppToken}'
          }
          body: {
            channel: slackAlertsChannel
            text: 'Data Factory Failure!'
            attachments: [
              {
                color: 'warning'
                fields: [
                  {
                    title: 'Data Factory'
                    value: '@{pipeline().DataFactory}'
                    short: true
                  }
                  {
                    title: 'Pipeline'
                    value: '@{pipeline().Pipeline}'
                    short: true
                  }
                  {
                    title: 'Error'
                    value: '@{activity(\'sp_remove_soft_deleted_subjects\').Error.message}'
                    short: true
                  }
                  {
                    title: 'Duration'
                    value: '@{activity(\'sp_remove_soft_deleted_subjects\').Duration}'
                    short: true
                  }
                ]
              }
            ]
          }
        }
      }
      {
        name: 'Report failure to Teams'
        type: 'WebActivity'
        dependsOn: [
          {
            activity: 'sp_remove_soft_deleted_subjects'
            dependencyConditions: [
              'Failed'
            ]
          }
        ]
        policy: {
          timeout: '0.00:10:00'
          retry: 0
          retryIntervalInSeconds: 30
          secureOutput: false
          secureInput: true
        }
        userProperties: []
        typeProperties: {
          url: teamsPowerAutomateWebhookUrl
          method: 'POST'
          headers: {
            'Content-Type': 'application/json'
          }
          body: {
            type: 'message'
            attachments: [
              {
                contentType: 'application/vnd.microsoft.card.adaptive'
                contentUrl: null
                content: {
                  '$schema': 'http://adaptivecards.io/schemas/adaptive-card.json'
                  type: 'AdaptiveCard'
                  version: '1.2'
                  body: [
                    {
                      type: 'TextBlock'
                      text: 'Data Factory Failure!'
                      weight: 'Bolder'
                      size: 'Medium'
                      color: 'warning'
                      wrap: true
                    }
                    {
                      type: 'FactSet'
                      facts: [
                        {
                          title: 'Data Factory'
                          value: '@{pipeline().DataFactory}'
                        }
                        {
                          title: 'Pipeline'
                          value: '@{pipeline().Pipeline}'
                        }
                        {
                          title: 'Error'
                          value: '@{activity(\'sp_remove_soft_deleted_subjects\').Error.message}'
                        }
                        {
                          title: 'Duration'
                          value: '@{activity(\'sp_remove_soft_deleted_subjects\').Duration}'
                        }
                      ]
                    }
                  ]
                }
              }
            ]
          }
        }
      }
    ]
    parameters: {
      TotalObservationLimit: {
        type: 'int'
        defaultValue: removeSoftDeletedSubjectsObservationLimit
      }
      ObservationCommitBatchSize: {
        type: 'int'
        defaultValue: removeSoftDeletedSubjectsObservationCommitBatchSize
      }
      ObservationFilterItemCommitBatchSize: {
        type: 'int'
        defaultValue: removeSoftDeletedSubjectsObservationFilterItemCommitBatchSize
      }
    }
    annotations: []
  }
}

output pipelineName string = pipelineName
