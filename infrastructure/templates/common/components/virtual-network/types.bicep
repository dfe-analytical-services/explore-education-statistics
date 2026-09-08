@export()
type Subnet = {
  name: string
  properties: {
    addressPrefix: string
    serviceEndpoints: ('Microsoft.Sql' | 'Microsoft.Storage')[]?
    delegations: ('webapp' | 'environment')[]?
  }
}
