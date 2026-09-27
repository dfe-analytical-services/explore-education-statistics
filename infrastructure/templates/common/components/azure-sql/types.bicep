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

  @description('Monthly long term backup retention for this database, e.g. "P12M" or "P3M".')
  longTermMonthlyRetention: string
}

@export()
type SubnetReference = {
  name: string
  id: string
}
