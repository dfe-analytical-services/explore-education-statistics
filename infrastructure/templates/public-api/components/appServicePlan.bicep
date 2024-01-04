@description('Specifies the location for all resources.')
param location string

//Specific parameters for the resources
@description('App Service Plan name')
param name string

@description('App Service Plan operating system')
@allowed([
  'Windows'
  'Linux'
])
param os string = 'Linux'


// Variables and created data
var reserved = os == 'Linux' ? true : false


//Resources
resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: name
  location: location
  kind: 'functionapp'
  sku: {
    name: 'Y1'
  }
  properties: {
    reserved: reserved
  }
}

//Output
output servicePlanId string = appServicePlan.id
