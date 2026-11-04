import { FrontDoorCertificateType } from '../common/components/front-door/types.bicep'
import { IpRange } from '../common/types.bicep'
import { StorageAccountSku } from '../common/components/storage/types.bicep'
import { AzureSqlDatabaseConfig } from '../common/components/azure-sql/types.bicep'
import { VNetAddressRanges } from 'application/virtual-network/types.bicep'
import { getResourceNamesForEnvironment } from '../common/resource-names.bicep'

@description('Environment : Subscription name. Used as a prefix for created resources.')
param subscription string = ''

@description('Environment : Specifies the location in which the Azure resources should be deployed.')
param location string = resourceGroup().location

@description('Tagging : Environment name e.g. Development. Used for tagging resources created by this infrastructure pipeline.')
param environmentName string

@description('Whether to use the naming convention for the DfE Enterprise Landing Zone (ELZ) subscriptions. Set to true by the param files in "parameters/elz".')
param elz bool = false

@description('The public site URL for use with Azure Front Door.')
param publicSiteUrl string = ''

@description('The public Content API URL for use with Azure Front Door.')
param contentApiUrl string

@description('FQDN of the service hosting the public site, rather than the public URL as used by custom domains.')
param publicSiteInternalServiceFqdn string

@description('The base public URL for accessing the public API and its documentation site. This is the base FaUAPI URL that users use to interact with the API.')
param publicApiPublicUrl string

@description('FQDN of the public API listener in Application Gateway. This is the FQDN that FaUAPI uses to proxy traffic to the public API.')
param publicApiApplicationGatewayFqdn string = ''

@description('Certificate type for Azure Front Door.')
param certificateType FrontDoorCertificateType = 'BringYourOwn'

@description('The minimum average response time from the public site (via Azure Front Door) before latency alerts fire.')
param averagePublicSiteResponseTimeAlertThresholdMillis int = 2500

@description('Specify if manual deletion of backups is allowed in Recovery Services Vault.')
param recoveryServicesVaultImmutable bool = false

@description('Retention of storage account blobs in days.')
param blobDeleteRetentionDays int = 90

@description('SKU of the core storage account, which sets its redundancy. Legacy accounts can\'t be converted to zone-redundant by a deploy, so only the param files in "parameters/elz" set a zone-redundant SKU.')
param coreStorageAccountSku StorageAccountSku = 'Standard_RAGRS'

@description('SKU of the public storage account, which sets its redundancy. Legacy accounts can\'t be converted to zone-redundant by a deploy, so only the param files in "parameters/elz" set a zone-redundant SKU.')
param publicStorageAccountSku StorageAccountSku = 'Standard_LRS'

@description('Whether the file share backup policy copies backups into the Recovery Services Vault, rather than keeping them as snapshots only. A policy can\'t move from the snapshot tier to the vault tier in place, so only the param files in "parameters/elz" turn this on.')
param vaultedFileShareBackups bool = false

@description('Whether Backup Vault backups (PostgreSQL and blobs) can be restored in the paired region.')
param backupVaultCrossRegionRestore bool = false

@description('Provides access to resources for specific IP address ranges used for service maintenance.')
param maintenanceIpRanges IpRange[] = []

@description('Whether or not to create role assignments necessary for performing certain backup actions.')
param deployBackupVaultReaderRoleAssignment bool = true

@description('Whether or not to deploy subnets.')
param deploySubnets bool = false

@description('CIDR address ranges for the VNet and each of its subnets.')
param vNetAddressRanges VNetAddressRanges

@description('Whether or not to deploy Private DNS Zones.')
param deployPrivateDnsZones bool = false

@description('Whether or not to deploy Azure Front Door.')
param deployAzureFrontDoor bool = false

@description('Whether or not to deploy Application Gateway and its configuration.')
param deployApplicationGateway bool = false

@description('Whether or not to deploy Data Factory.')
param deployDataFactory bool = false

@description('Whether or not to deploy Recovery Services Vault and its policies.')
param deployRecoveryServicesVault bool = false

@description('Whether or not to deploy the Container Registry.')
param deployContainerRegistry bool = false

// TODO EES-7502 - remove once every environment has moved to the ELZ, along with the core grants it controls and the
// lines that turn it off in "parameters/elz".
@description('Whether to grant the Admin App Service\'s and Publisher Function App\'s system-assigned identities permission to send Event Grid events, and the Publisher\'s permission to purge the Front Door cache. In ELZ, the release pipeline grants these roles to their user-assigned identities instead, so the param files in "parameters/elz" turn this off.')
param deploySystemAssignedIdentityRoleAssignments bool = true

@description('Do Azure Monitor alerts need creating or updating?')
param deployAlerts bool = false

@description('The admin user of the Core and Public SQL Servers.')
param sqlAdministratorLogin string = ''

@description('The login name of the Entra ID admin for the Core and Public SQL Servers.')
param sqlAzureAdministratorLogin string = ''

@description('The object id of the Entra ID admin for the Core and Public SQL Servers.')
param sqlAzureAdministratorSid string = ''

@description('Whether or not public access is enabled for the Core and Public SQL Servers.')
param sqlServerPublicNetworkAccess 'Enabled' | 'Disabled' = 'Disabled'

@description('Email addresses to notify for SQL security alerts and vulnerability assessment scans.')
param teamEmailAddresses string[] = []

@description('Number of days to retain SQL database audit logs for in blob storage.')
param databaseAuditBlobRetentionDays int = 365

@description('Configuration for the Content database.')
param contentDbConfig AzureSqlDatabaseConfig = {
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    capacity: 1
  }
  licenseType: 'LicenseIncluded'
  maxSizeBytes: 1073741824
}

@description('Configuration for the Statistics database and its geo-replica.')
param statisticsDbConfig AzureSqlDatabaseConfig = {
  sku: {
    name: 'GP_Gen5'
    tier: 'GeneralPurpose'
    capacity: 2
  }
  licenseType: 'LicenseIncluded'
  maxSizeBytes: 268435456000
}

@description('Tagging : Used for tagging resources created by this infrastructure pipeline.')
param resourceTags {
  CostCentre: string
  Department: string
  Solution: string
  ServiceOwner: string
  CreatedBy: string
  DeploymentRepoUrl: string
}?

@description('Tagging : Date Provisioned. Used for tagging resources created by this infrastructure pipeline.')
param dateProvisioned string = utcNow('u')

var tagValues = union(resourceTags ?? {}, {
  Environment: environmentName
  DateProvisioned: dateProvisioned
})

var resourceNames = getResourceNamesForEnvironment({
  environmentIdentifier: subscription
  environmentName: environmentName
  elz: elz
})

module vNetModule 'application/virtual-network/virtual-network.bicep' = {
  name: 'vNetModuleDeploy'
  params: {
    vnetNames: resourceNames.vnet
    deploySubnets: deploySubnets
    addressRanges: vNetAddressRanges
    tagValues: tagValues
  }
}

module privateDnsZonesModule 'application/virtual-network/private-dns-zones.bicep' = if (deployPrivateDnsZones) {
  name: 'privateDnsZonesApplicationModuleDeploy'
  params: {
    vnetName: vNetModule.outputs.vNetName
    tagValues: tagValues
  }
}

module keyVaultModule 'application/keyVault/keyVault.bicep' = {
  name: 'keyVaultModuleDeploy'
  params: {
    keyVaultName: resourceNames.keyVault.keyVault
    tagValues: tagValues
  }
}

module logAnalyticsWorkspaceModule 'application/log-analytics-workspace/log-analytics-workspace.bicep' = {
  name: 'logAnalyticsWorkspaceApplicationModuleDeploy'
  params: {
    workspaceName: resourceNames.logAnalyticsWorkspace
    location: location
    tagValues: tagValues
  }
}

module alertsModule 'application/alerts/alerts.bicep' = {
  name: 'alertsModuleDeploy'
  params: {
    subscription: subscription
    actionGroupName: resourceNames.alertsGroup
    alertsLogicAppName: resourceNames.alertsLogicApp
    keyVaultName: keyVaultModule.outputs.keyVaultName
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
  }
}

module backupsModule 'application/backups/backups.bicep' = {
  name: 'backupsModuleDeploy'
  params: {
    location: location
    backupsNames: resourceNames.backups
    deployBackupVaultReaderRoleAssignment: deployBackupVaultReaderRoleAssignment
    deployRecoveryServicesVault: deployRecoveryServicesVault
    recoveryServicesVaultImmutable: recoveryServicesVaultImmutable
    vaultedFileShareBackups: vaultedFileShareBackups
    backupVaultCrossRegionRestore: backupVaultCrossRegionRestore
    tagValues: tagValues
  }
}

module eventMessagingModule 'application/eventMessaging/eventMessaging.bicep' = {
  name: 'eventMessagingModuleDeploy'
  params: {
    location: location
    eventGridNames: resourceNames.eventGrid
    resourceNames: {
      adminApp: resourceNames.admin.appService
      alertsGroup: alertsModule.outputs.actionGroupName
      publisherFunction: resourceNames.publisher.functionApp
      vNet: vNetModule.outputs.vNetName
      subnets: {
        eventGridCustomTopicPrivateEndpoints: vNetModule.outputs.eventGridCustomTopicPrivateEndpointsSubnetName
      }
    }
    deploySystemAssignedIdentityRoleAssignments: deploySystemAssignedIdentityRoleAssignments
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

module frontDoorModule 'application/frontDoor/frontDoor.bicep' = if (deployAzureFrontDoor) {
  name: 'frontDoorModuleDeploy'
  params: {
    frontDoorNames: resourceNames.frontDoor
    publicSiteCertificateName: resourceNames.keyVault.certificates.publicSite
    contentApiCertificateName: resourceNames.keyVault.certificates.contentApiFrontDoor
    publicSiteAppServiceName: resourceNames.publicSite.appService
    contentApiAppServiceName: resourceNames.contentApi.appService
    alertsGroupName: alertsModule.outputs.actionGroupName
    keyVaultName: keyVaultModule.outputs.keyVaultName
    publicSiteUrl: publicSiteUrl
    contentApiUrl: contentApiUrl
    publisherFunctionName: resourceNames.publisher.functionApp
    deploySystemAssignedIdentityRoleAssignments: deploySystemAssignedIdentityRoleAssignments
    certificateType: certificateType
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
    averagePublicSiteResponseTimeAlertThresholdMillis: averagePublicSiteResponseTimeAlertThresholdMillis
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

module appGatewayModule 'application/applicationGateway/appGateway.bicep' = if (deployApplicationGateway) {
  name: 'appGatewayModuleDeploy'
  params: {
    location: location
    resourceNames: resourceNames
    subnetName: vNetModule.outputs.applicationGatewaySubnetName
    alertsGroupName: alertsModule.outputs.actionGroupName
    keyVaultName: keyVaultModule.outputs.keyVaultName
    vnetName: vNetModule.outputs.vNetName
    publicApiAppGatewayFqdn: publicApiApplicationGatewayFqdn
    publicApiPublicUrl: publicApiPublicUrl
    publicSiteFqdn: replace(publicSiteUrl, 'https://', '')
    publicSiteInternalServiceFqdn: publicSiteInternalServiceFqdn
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

module dataFactoryModule 'application/data-factory/data-factory.bicep' = if (deployDataFactory) {
  name: 'dataFactoryModuleDeploy'
  params: {
    dataFactoryName: resourceNames.dataFactory
    alertsGroupName: alertsModule.outputs.actionGroupName
    keyVaultName: keyVaultModule.outputs.keyVaultName
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

module containerRegistryModule 'application/container-registry/container-registry.bicep' = if (deployContainerRegistry && environmentName == 'Development') {
  name: 'containerRegistryModuleDeploy'
  params: {
    containerRegistryName: resourceNames.acr.serverName
    tagValues: tagValues
  }
}

module loggingStorageAccountModule 'application/logging-storage-account/logging-storage-account.bicep' = {
  name: 'loggingStorageAccountModuleDeploy'
  params: {
    storageAccountName: resourceNames.storageAccounts.logging
    alertsGroupName: alertsModule.outputs.actionGroupName
    blobDeleteRetentionDays: blobDeleteRetentionDays
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

module coreStorageAccountModule 'application/core-storage-account/core-storage-account.bicep' = {
  name: 'coreStorageAccountModuleDeploy'
  params: {
    storageAccountName: resourceNames.storageAccounts.core
    sku: coreStorageAccountSku
    connectionStringSecretName: resourceNames.keyVault.secrets.coreStorageAccountConnectionString
    subnets: vNetModule.outputs.subnets
    keyVaultName: keyVaultModule.outputs.keyVaultName
    backupVaultName: backupsModule.outputs.backupVaultName
    backupBlobsPolicyName: backupsModule.outputs.backupVaultBlobsPolicyName
    alertsGroupName: alertsModule.outputs.actionGroupName
    blobDeleteRetentionDays: blobDeleteRetentionDays
    firewallRules: maintenanceIpRanges
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

module publicStorageAccountModule 'application/public-storage-account/public-storage-account.bicep' = {
  name: 'publicStorageAccountModuleDeploy'
  params: {
    storageAccountName: resourceNames.storageAccounts.public
    sku: publicStorageAccountSku
    connectionStringSecretName: resourceNames.keyVault.secrets.publicStorageAccountConnectionString
    subnets: vNetModule.outputs.subnets
    keyVaultName: keyVaultModule.outputs.keyVaultName
    backupVaultName: backupsModule.outputs.backupVaultName
    backupBlobsPolicyName: backupsModule.outputs.backupVaultBlobsPolicyName
    alertsGroupName: alertsModule.outputs.actionGroupName
    blobDeleteRetentionDays: blobDeleteRetentionDays
    firewallRules: maintenanceIpRanges
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

module coreSqlServerModule 'application/core-sql-server/sql-server.bicep' = {
  name: 'coreSqlServerModuleDeploy'
  params: {
    serverName: resourceNames.databases.coreSqlServer
    location: location
    sqlAdministratorLogin: sqlAdministratorLogin
    keyVaultName: keyVaultModule.outputs.keyVaultName
    sqlAzureAdministratorLogin: sqlAzureAdministratorLogin
    sqlAzureAdministratorSid: sqlAzureAdministratorSid
    sqlServerPublicNetworkAccess: sqlServerPublicNetworkAccess
    subnets: vNetModule.outputs.subnets
    firewallRules: maintenanceIpRanges
    teamEmailAddresses: teamEmailAddresses
    databaseAuditBlobRetentionDays: databaseAuditBlobRetentionDays
    loggingStorageAccountName: loggingStorageAccountModule.outputs.storageAccountName
    contentDbConfig: contentDbConfig
    statisticsDbConfig: statisticsDbConfig
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
    alertsGroupName: alertsModule.outputs.actionGroupName
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}

module publicSqlServerModule 'application/public-sql-server/sql-server.bicep' = {
  name: 'publicSqlServerModuleDeploy'
  params: {
    serverName: resourceNames.databases.publicSqlServer
    location: location
    sqlAdministratorLogin: sqlAdministratorLogin
    keyVaultName: keyVaultModule.outputs.keyVaultName
    sqlAzureAdministratorLogin: sqlAzureAdministratorLogin
    sqlAzureAdministratorSid: sqlAzureAdministratorSid
    sqlServerPublicNetworkAccess: sqlServerPublicNetworkAccess
    subnets: vNetModule.outputs.subnets
    firewallRules: maintenanceIpRanges
    teamEmailAddresses: teamEmailAddresses
    databaseAuditBlobRetentionDays: databaseAuditBlobRetentionDays
    loggingStorageAccountName: loggingStorageAccountModule.outputs.storageAccountName
    statisticsDbConfig: statisticsDbConfig
    statisticsPrimaryDatabaseId: coreSqlServerModule.outputs.statisticsDatabaseId
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
    alertsGroupName: alertsModule.outputs.actionGroupName
    deployAlerts: deployAlerts
    tagValues: tagValues
  }
}
