@description('Name of the Action Group.')
param actionGroupName string

@description('Specifies the name of the alerts logic app.')
param alertsLogicAppName string

resource actionGroup 'Microsoft.Insights/actionGroups@2019-06-01' = {
  name: actionGroupName
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
