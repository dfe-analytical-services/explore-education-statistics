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

@export()
type Tags = {
  @description('Tag Value - the cost centre identifying value provided by the Service Owner. Otherwise populate with Unknown.')
  CostCentre: string

  @description('Tag Value - the Department name tag value e.g. Data Directorate')
  Department: string

  @description('Tag Value - the solution name that the component is a part of e.g. EDAP, LDS, EES')
  Solution: string

  @description('Tag Value - the name of the Service or Application Owner in the SURNAME, Firstname format e.g. SINCLAIR, Paul / SHELBY, Laura')
  ServiceOwner: string

  @description('Tag Value - the name of the user who created these resources in the SURNAME, Firstname format e.g. RULER, Paul')
  CreatedBy: string

  @description('Tag Value - the URL of the repo containing the deployment script for the component')
  DeploymentRepoUrl: string
}
