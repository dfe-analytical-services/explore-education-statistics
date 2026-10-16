@export()
type AzureSqlDatabaseConfig = {
  sku: {
    name: string
    tier: string
    capacity: int
  }
  licenseType: 'BasePrice' | 'LicenseIncluded'
  maxSizeBytes: int
  minCapacity: string?
}

@export()
type SqlDatabaseDefinition = {

  @description('Name of the database, e.g. "statistics" or "content".')
  name: string

  @description('Configuration for the database.')
  config: AzureSqlDatabaseConfig

  @sealed()
  @discriminator('type')
  extendedConfig: {
    type: 'georeplica'

    @description('Resource id of the primary database that this geo-replica is created from.')
    geoReplicaSourceDatabaseId: string
  } | {
    type: 'primary'

    @description('Weekly long term backup retention, e.g. "P4W".')
    longTermWeeklyRetention: string

    @description('Monthly long term backup retention, e.g. "P12M" or "P3M".')
    longTermMonthlyRetention: string

    @description('Yearly long term backup retention, e.g. "P1Y".')
    longTermYearlyRetention: string

    @description('Week of the year that the yearly long term backup is taken.')
    longTermRetentionWeekOfYear: int
  }
}
