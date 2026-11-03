using 'resource-names-outputs.bicep'

// These values are supplied through environment variables by the get-resource-names.yml pipeline task.
param environmentIdentifier = readEnvironmentVariable('RESOURCE_NAMES_ENVIRONMENT_IDENTIFIER', '')
param environmentName = readEnvironmentVariable('RESOURCE_NAMES_ENVIRONMENT_NAME', '')
param elz = bool(readEnvironmentVariable('RESOURCE_NAMES_ELZ', 'false'))
