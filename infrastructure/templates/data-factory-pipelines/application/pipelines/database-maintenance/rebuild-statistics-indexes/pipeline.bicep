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

@description('The tables to reindex.')
param fragmentationTables string

var pipelineName = 'pl_rebuild_statistics_indexes'

resource pipeline 'Microsoft.DataFactory/factories/pipelines@2018-06-01' = {
  name: '${dataFactoryName}/${pipelineName}'
  properties: {
    activities: [
      {
        name: 'Pause resumable index rebuilds first'
        description: 'This activity pauses any ongoing RESUMABLE index REBUILDs before starting the main pipeline.'
        type: 'SqlServerStoredProcedure'
        dependsOn: []
        policy: {
          timeout: '0.00:10:00'
          retry: 0
          retryIntervalInSeconds: 30
          secureOutput: false
          secureInput: false
        }
        userProperties: []
        typeProperties: {
          storedProcedureName: '[dbo].[PauseResumableIndexRebuilds]'
          storedProcedureParameters: {
            Tables: {
              value: '@pipeline().parameters.Tables'
              type: 'String'
            }
          }
        }
        linkedServiceName: {
          referenceName: statisticsDbLinkedServiceName
          type: 'LinkedServiceReference'
        }
      }
      {
        name: 'Kill index reorganizes first'
        description: 'This activity kills any ongoing index REORGANIZEs before starting the main pipeline.'
        type: 'SqlServerStoredProcedure'
        dependsOn: [
          {
            activity: 'Pause resumable index rebuilds first'
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
          storedProcedureName: '[dbo].[KillIndexReorganizations]'
          storedProcedureParameters: {
            Tables: {
              value: '@pipeline().parameters.Tables'
              type: 'String'
            }
          }
        }
        linkedServiceName: {
          referenceName: 'ls_sql_statistics'
          type: 'LinkedServiceReference'
        }
      }
      {
        name: 'Rebuild indexes'
        type: 'IfCondition'
        dependsOn: [
          {
            activity: 'Kill index reorganizes first'
            dependencyConditions: [
              'Succeeded'
            ]
          }
        ]
        typeProperties: {
          expression: {
            value: '@equals(pipeline().parameters.RunMode, \'Weekend\')'
            type: 'Expression'
          }
          ifTrueActivities: [
            {
              name: 'Rebuild indexes - weekend'
              type: 'SqlServerStoredProcedure'
              dependsOn: []
              policy: {
                timeout: '0.23:45:00'
                retry: 0
                retryIntervalInSeconds: 30
                secureOutput: false
                secureInput: false
              }
              userProperties: []
              typeProperties: {
                storedProcedureName: '[dbo].[RebuildIndexes]'
                storedProcedureParameters: {
                  Tables: {
                    value: {
                      value: '@pipeline().parameters.Tables'
                      type: 'Expression'
                    }
                    type: 'String'
                  }
                  StopInMinutes: {
                    value: {
                      value: '@pipeline().parameters.StoredProcedureStopInMinutes'
                      type: 'Expression'
                    }
                    type: 'String'
                  }
                  FragmentationThresholdReorganize: {
                    value: {
                      value: '@pipeline().parameters.FragmentationThresholdReorganize'
                      type: 'Expression'
                    }
                    type: 'String'
                  }
                  FragmentationThresholdRebuild: {
                    value: {
                      value: '@pipeline().parameters.FragmentationThresholdRebuild'
                      type: 'Expression'
                    }
                    type: 'String'
                  }
                }
              }
              linkedServiceName: {
                referenceName: 'ls_sql_statistics'
                type: 'LinkedServiceReference'
              }
            }
            {
              name: 'Get reason for index rebuild failure - weekend'
              type: 'SetVariable'
              dependsOn: [
                {
                  activity: 'Rebuild indexes - weekend'
                  dependencyConditions: [
                    'Failed'
                  ]
                }
              ]
              policy: {
                secureOutput: false
                secureInput: false
              }
              userProperties: []
              typeProperties: {
                variableName: 'storedProcedureActivityFailure'
                value: {
                  value: '@coalesce(\n  activity(\'Rebuild indexes - weekend\').error.errorCode,\n  activity(\'Rebuild indexes - weekend\').error.message,\n  \'Unknown\'\n)'
                  type: 'Expression'
                }
              }
            }
            {
              name: 'Get index rebuild failure message - weekend'
              type: 'SetVariable'
              dependsOn: [
                {
                  activity: 'Get reason for index rebuild failure - weekend'
                  dependencyConditions: [
                    'Succeeded'
                  ]
                }
              ]
              policy: {
                secureOutput: false
                secureInput: false
              }
              userProperties: []
              typeProperties: {
                variableName: 'storedProcedureActivityFailureMessage'
                value: {
                  value: '@activity(\'Rebuild indexes - weekend\').error.message'
                  type: 'Expression'
                }
              }
            }
          ]
          ifFalseActivities: [
            {
              name: 'Rebuild indexes - weekday'
              type: 'SqlServerStoredProcedure'
              dependsOn: []
              policy: {
                timeout: '0.09:30:00'
                retry: 0
                retryIntervalInSeconds: 30
                secureOutput: false
                secureInput: false
              }
              userProperties: []
              typeProperties: {
                storedProcedureName: '[dbo].[RebuildIndexes]'
                storedProcedureParameters: {
                  Tables: {
                    value: {
                      value: '@pipeline().parameters.Tables'
                      type: 'Expression'
                    }
                    type: 'String'
                  }
                  StopInMinutes: {
                    value: {
                      value: '@pipeline().parameters.StoredProcedureStopInMinutes'
                      type: 'Expression'
                    }
                    type: 'String'
                  }
                  FragmentationThresholdReorganize: {
                    value: {
                      value: '@pipeline().parameters.FragmentationThresholdReorganize'
                      type: 'Expression'
                    }
                    type: 'String'
                  }
                  FragmentationThresholdRebuild: {
                    value: {
                      value: '@pipeline().parameters.FragmentationThresholdRebuild'
                      type: 'Expression'
                    }
                    type: 'String'
                  }
                }
              }
              linkedServiceName: {
                referenceName: 'ls_sql_statistics'
                type: 'LinkedServiceReference'
              }
            }
            {
              name: 'Get reason for index rebuild failure - weekday'
              type: 'SetVariable'
              dependsOn: [
                {
                  activity: 'Rebuild indexes - weekday'
                  dependencyConditions: [
                    'Failed'
                  ]
                }
              ]
              policy: {
                secureOutput: false
                secureInput: false
              }
              userProperties: []
              typeProperties: {
                variableName: 'storedProcedureActivityFailure'
                value: {
                  value: '@coalesce(\n  activity(\'Rebuild indexes - weekday\').error.errorCode,\n  activity(\'Rebuild indexes - weekday\').error.message,\n  \'Unknown\'\n)'
                  type: 'Expression'
                }
              }
            }
            {
              name: 'Get index rebuild failure message - weekday'
              type: 'SetVariable'
              dependsOn: [
                {
                  activity: 'Get reason for index rebuild failure - weekday'
                  dependencyConditions: [
                    'Succeeded'
                  ]
                }
              ]
              policy: {
                secureOutput: false
                secureInput: false
              }
              userProperties: []
              typeProperties: {
                variableName: 'storedProcedureActivityFailureMessage'
                value: {
                  value: '@activity(\'Rebuild indexes - weekday\').error.message'
                  type: 'Expression'
                }
              }
            }
          ]
        }
      }
      {
        name: 'Check for rebuild indexes failure'
        description: 'This activity routes to the appropriate error handler based on the reason for the \'Rebuild indexes\' activity\'s reason for failure'
        type: 'Switch'
        dependsOn: [
          {
            activity: 'Rebuild indexes'
            dependencyConditions: [
              'Succeeded'
            ]
          }
        ]
        userProperties: []
        typeProperties: {
          on: {
            value: '@variables(\'storedProcedureActivityFailure\')'
            type: 'Expression'
          }
          cases: [
            {
              value: 'ActionTimedOut'
              activities: [
                {
                  name: 'Pause resumable index rebuilds after timeout'
                  description: 'This activity pauses any ongoing RESUMABLE index REBUILDs.'
                  type: 'SqlServerStoredProcedure'
                  dependsOn: []
                  policy: {
                    timeout: '0.00:10:00'
                    retry: 0
                    retryIntervalInSeconds: 30
                    secureOutput: false
                    secureInput: false
                  }
                  userProperties: []
                  typeProperties: {
                    storedProcedureName: '[dbo].[PauseResumableIndexRebuilds]'
                    storedProcedureParameters: {
                      Tables: {
                        value: '@pipeline().parameters.Tables'
                        type: 'String'
                      }
                    }
                  }
                  linkedServiceName: {
                    referenceName: 'ls_sql_statistics'
                    type: 'LinkedServiceReference'
                  }
                }
                {
                  name: 'Kill index reorganizes after timeout'
                  description: 'This activity kills any ongoing index REORGANIZEs after a timeout.'
                  type: 'SqlServerStoredProcedure'
                  dependsOn: [
                    {
                      activity: 'Pause resumable index rebuilds after timeout'
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
                    storedProcedureName: '[dbo].[KillIndexReorganizations]'
                    storedProcedureParameters: {
                      Tables: {
                        value: '@pipeline().parameters.Tables'
                        type: 'String'
                      }
                    }
                  }
                  linkedServiceName: {
                    referenceName: 'ls_sql_statistics'
                    type: 'LinkedServiceReference'
                  }
                }
                {
                  name: 'Report successful timeout'
                  description: 'Messages Slack to inform that the timeout has been handled successfully'
                  type: 'WebActivity'
                  dependsOn: [
                    {
                      activity: 'Kill index reorganizes after timeout'
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
                    method: 'POST'
                    headers: {
                      'Content-Type': 'application/json'
                      Authorization: 'Bearer ${slackAppToken}'
                    }
                    url: 'https://slack.com/api/chat.postMessage'
                    body: {
                      channel: slackAlertsChannel
                      text: 'Data Factory timeout handled successfully.'
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
                              title: 'Details'
                              value: 'Any resumable index rebuilds have been paused.\nAny reorganizations have been killed.'
                              short: true
                            }
                            {
                              title: 'Duration'
                              value: '@{activity(\'Rebuild indexes\').Duration}'
                              short: true
                            }
                          ]
                        }
                      ]
                    }
                  }
                }
                {
                  name: 'Report successful timeout to Teams'
                  type: 'WebActivity'
                  dependsOn: [
                    {
                      activity: 'Kill index reorganizes after timeout'
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
                                text: 'Data Factory timeout handled successfully.'
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
                                    title: 'Details'
                                    value: 'Any resumable index rebuilds have been paused.\nAny reorganizations have been killed.'
                                  }
                                  {
                                    title: 'Duration'
                                    value: '@{activity(\'Rebuild indexes\').Duration}'
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
                  name: 'Report rebuild pause error'
                  type: 'WebActivity'
                  dependsOn: [
                    {
                      activity: 'Pause resumable index rebuilds after timeout'
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
                    method: 'POST'
                    headers: {
                      'Content-Type': 'application/json'
                      Authorization: 'Bearer ${slackAppToken}'
                    }
                    url: 'https://slack.com/api/chat.postMessage'
                    body: {
                      channel: slackAlertsChannel
                      text: 'Data Factory failure! Problem encountered when pausing resumable index rebuilds.'
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
                              title: 'Activity'
                              value: '@{activity(\'Pause resumable index rebuilds after timeout\').Error.message}'
                              short: true
                            }
                            {
                              title: 'Duration'
                              value: '@{activity(\'Rebuild indexes\').Duration}'
                              short: true
                            }
                          ]
                        }
                      ]
                    }
                  }
                }
                {
                  name: 'Report rebuild pause error to Teams'
                  type: 'WebActivity'
                  dependsOn: [
                    {
                      activity: 'Pause resumable index rebuilds after timeout'
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
                                text: 'Data Factory failure! Problem encountered when pausing resumable index rebuilds.'
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
                                    title: 'Activity'
                                    value: '@{activity(\'Pause resumable index rebuilds after timeout\').Error.message}'
                                  }
                                  {
                                    title: 'Duration'
                                    value: '@{activity(\'Rebuild indexes\').Duration}'
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
                  name: 'Report index reorganization kill error'
                  type: 'WebActivity'
                  dependsOn: [
                    {
                      activity: 'Kill index reorganizes after timeout'
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
                    method: 'POST'
                    headers: {
                      'Content-Type': 'application/json'
                      Authorization: 'Bearer ${slackAppToken}'
                    }
                    url: 'https://slack.com/api/chat.postMessage'
                    body: {
                      channel: slackAlertsChannel
                      text: 'Data Factory failure! Problem encountered when killing index reorganizations.'
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
                              title: 'Activity'
                              value: '@{activity(\'Kill index reorganizes after timeout\').Error.message}'
                              short: true
                            }
                            {
                              title: 'Duration'
                              value: '@{activity(\'Rebuild indexes\').Duration}'
                              short: true
                            }
                          ]
                        }
                      ]
                    }
                  }
                }
                {
                  name: 'Report index reorganization kill error to Teams'
                  type: 'WebActivity'
                  dependsOn: [
                    {
                      activity: 'Kill index reorganizes after timeout'
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
                                text: 'Data Factory failure! Problem encountered when killing index reorganizations.'
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
                                    title: 'Activity'
                                    value: '@{activity(\'Kill index reorganizes after timeout\').Error.message}'
                                  }
                                  {
                                    title: 'Duration'
                                    value: '@{activity(\'Rebuild indexes\').Duration}'
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
            }
            {
              value: 'No failures'
              activities: [
                {
                  name: 'Report reindex success'
                  type: 'WebActivity'
                  dependsOn: []
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
                      text: 'Data Factory Success! All fragmented indexes rebuilt.'
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
                              value: '@{activity(\'Rebuild indexes\').Duration}'
                              short: true
                            }
                          ]
                        }
                      ]
                    }
                  }
                }
                {
                  name: 'Report reindex success to Teams'
                  type: 'WebActivity'
                  dependsOn: []
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
                                text: 'Data Factory Success! All fragmented indexes rebuilt.'
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
                                    value: '@{activity(\'Rebuild indexes\').Duration}'
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
            }
          ]
          defaultActivities: [
            {
              name: 'Report failure'
              type: 'WebActivity'
              dependsOn: []
              policy: {
                timeout: '0.00:10:00'
                retry: 0
                retryIntervalInSeconds: 30
                secureOutput: false
                secureInput: false
              }
              userProperties: []
              typeProperties: {
                method: 'POST'
                headers: {
                  'Content-Type': 'application/json'
                  Authorization: 'Bearer ${slackAppToken}'
                }
                url: 'https://slack.com/api/chat.postMessage'
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
                          value: '@variables(\'storedProcedureActivityFailureMessage\')'
                          short: true
                        }
                        {
                          title: 'Duration'
                          value: '@{activity(\'Rebuild indexes\').Duration}'
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
              dependsOn: []
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
                                value: '@variables(\'storedProcedureActivityFailureMessage\')'
                              }
                              {
                                title: 'Duration'
                                value: '@{activity(\'Rebuild indexes\').Duration}'
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
        }
      }
    ]
    parameters: {
      Tables: {
        type: 'string'
        defaultValue: fragmentationTables
      }
      StoredProcedureStopInMinutes: {
        type: 'string'
        defaultValue: '600'
      }
      FragmentationThresholdReorganize: {
        type: 'string'
        defaultValue: '5'
      }
      FragmentationThresholdRebuild: {
        type: 'string'
        defaultValue: '30'
      }
      RunMode: {
        type: 'string'
        defaultValue: 'Weekday'
      }
    }
    variables: {
      storedProcedureActivityFailure: {
        type: 'string'
        defaultValue: 'No failures'
      }
      storedProcedureActivityFailureMessage: {
        type: 'string'
        defaultValue: 'No failures'
      }
    }
    annotations: []
  }
}

output pipelineName string = pipelineName
