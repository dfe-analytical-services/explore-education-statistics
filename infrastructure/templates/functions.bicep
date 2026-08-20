@export()
func keyVaultRef(vaultUri string, secretName string) string => 
  '@Microsoft.KeyVault(SecretUri=${vaultUri}secrets/${secretName})'
