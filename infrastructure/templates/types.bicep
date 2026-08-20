@export()
type MemoryCacheConfig = {

  @description('The frequency of scans to evict expired entries from the in-memory cache.')
  expirationScanFrequencySeconds: int
  
  @description('Max size of in-memory cache in MBs.  This is an approximation based on the size of the cached objects in JSON notation.')
  maxCacheSizeMb: int
  
  @description('Override duration in seconds for all entities cached in memory')
  overridesDurationInSeconds: int?

  @description('Override cron expression for all entities cached in memory')
  overridesExpirySchedule: string?
}
