using '../main.bicep'

// Environment Params
param environmentName = 'Development'

param publicUrls = {
  contentApi: 'https://content.dev.explore-education-statistics.service.gov.uk'
  publicSite: 'https://dev.explore-education-statistics.service.gov.uk'
  publicApi: 'https://pp-api.education.gov.uk/statistics-dev'
  publicApiAppGateway: 'https://dev.statistics.api.education.gov.uk'
}

param publicApiContainerAppConfig = {
  cpuCores: 4
  memoryGis: 8
  minReplicas: 1
  maxReplicas: 100
  scaleAtConcurrentHttpRequests: 10
  workloadProfileName: 'Consumption'
}

param publicApiContainerAppWorkloadProfiles = [{
  name: 'D8'
  workloadProfileType: 'D8'
  minimumCount: 0
  maximumCount: 10
}]

param enableThemeDeletion = true
param enableSwagger = true
