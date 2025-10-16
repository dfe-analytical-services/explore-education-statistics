@export()
type ResourceNames = {
  existingResources: {
    alertsGroup: string
    keyVault: string
    logAnalyticsWorkspace: string
    recoveryVault: string
    recoveryVaultFileShareBackupPolicy: string
    subnets: {
      analyticsFunctionApp: string
      storagePrivateEndpoints: string
    }
    vNet: string
  }
}
