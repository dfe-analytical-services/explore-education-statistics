import { abbreviations } from '../../../common/abbreviations.bicep'

@description('Resource prefix for all resources.')
param subscription string

@description('Specifies the name of the alerts logic app.')
param alertsLogicAppName string

var alertsActionGroupName = '${subscription}-${abbreviations.insightsActionGroups}-ees-alertedusers'

resource actionGroup 'Microsoft.Insights/actionGroups@2019-06-01' = {
  name: alertsActionGroupName
  location: 'global'
  properties: {
    groupShortName: 'alertAG'
    enabled: true
    logicAppReceivers: [
      {
        name: alertsLogicAppName
        resourceId: resourceId('Microsoft.Logic/workflows', alertsLogicAppName)
        callbackUrl: listCallbackUrl('${resourceId('Microsoft.Logic/workflows', alertsLogicAppName)}/triggers/manual', '2017-07-01').value
        useCommonAlertSchema: true
      }
    ]
  }
}

output actionGroupName string = actionGroup.name
