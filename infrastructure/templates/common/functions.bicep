import { abbreviations } from 'abbreviations.bicep'

@export()
func buildFullyQualifiedTopicName(resourcePrefix string, topicName string) string =>
  '${resourcePrefix}-${abbreviations.eventGridTopics}-${topicName}'

@export()
@description('Returns a percentage of a number.')
func percentage(number int, percent int) int => int((number * percent) / 100)

@export()
@description('Converts gigabytes into bytes.')
func gbsToBytes(gbs int) int => gbs * 1073741824

@export()
@description('Replaces multiple string occurrences within a given string.')
func replaceMultiple(input string, replacements { *: string }) string =>
  reduce(items(replacements), input, (cur, next) => replace(string(cur), next.key, next.value))

// Note that currently the "any()" around removals in the reducer below is necessary
// due to limitations in the Bicep type system. See https://github.com/Azure/bicep/issues/8971
// for more examples. 
@export()
@description('Removes multiple string occurrences within a given string.')
func removeMultiple(input string, removals string[]) string =>
  replaceMultiple(input, reduce(any(removals), {}, (cur, next) => {
    ...cur
    '${next}': ''
  }))

@export()
func keyVaultRef(vaultUri string, secretName string) string =>
  '@Microsoft.KeyVault(SecretUri=${vaultUri}secrets/${secretName}/)'

@export()
@description('''
Like keyVaultRef, but pins the reference to a specific secret version (e.g. a secret resource's
properties.secretUriWithVersion) rather than "latest". This ensures the resulting appsetting value
changes whenever the secret is rotated, because the appsettings update has a definite change
in values.
''')
func keyVaultRefFromVersionedUri(secretUriWithVersion string) string =>
  '@Microsoft.KeyVault(SecretUri=${secretUriWithVersion})'

@export()
@description('Builds a map of secret names to secret versioned URIs')
func secretRefsFromSecrets(secrets object[]) object =>
  toObject(secrets, s => last(split(s.id, '/')), s => keyVaultRefFromVersionedUri(s.properties.secretUriWithVersion))
