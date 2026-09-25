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
