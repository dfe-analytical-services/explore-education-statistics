metadata description = '''
Evaluates the resource names for an environment and returns them as an output, so that
pipelines can use the same names as the Bicep templates rather than building them again
manually. It is evaluated locally with "bicep snapshot" rather than deployed, so that
we don't have to wait 40 seconds for it to spin up a service connection it doesn't need.
'''

import { getResourceNamesForEnvironment, ResourceNames } from 'resource-names.bicep'

@description('Identifier for resources in this environment, used as a prefix for all resources e.g. s101d01.')
param environmentIdentifier string

@description('Name of this environment e.g. Development, Test.')
param environmentName string

output resourceNames ResourceNames = getResourceNamesForEnvironment({
  environmentIdentifier: environmentIdentifier
  environmentName: environmentName
})
