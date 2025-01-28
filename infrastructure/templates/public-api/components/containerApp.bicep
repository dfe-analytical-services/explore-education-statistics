import { cpuPercentageConfig, memoryPercentageConfig, responseTimeConfig } from 'alerts/dynamicAlertConfig.bicep'
import { staticTotalGreaterThanZero } from 'alerts/staticAlertConfig.bicep'

import { EntraIdAuthentication } from '../types.bicep'

@description('Specifies the location for all resources.')
param location string

@description('Specifies the login server from the registry.')
@secure()
param acrLoginServer string

@description('Specifies the container image to deploy from the registry.')
param containerAppImageName string

@minLength(2)
@maxLength(32)
@description('Specifies the name of the Container App.')
param containerAppName string

@description('Specifies the container port.')
param containerAppTargetPort int = 8080

@description('The CORS policy to use for the Container App.')
param corsPolicy {
  allowedHeaders: string[]?
  allowedMethods: string[]?
  allowedOrigins: string[]?
}

@description('Name of the workload profile under which this Container App will be deployed.  Defaults to Consumption.')
param workloadProfileName string = 'Consumption'

@description('Number of CPU cores the container can use. Can be with a maximum of two decimals.')
@minValue(1)
@maxValue(8)
param cpuCores int = 4

@description('Amount of memory (in gibibytes, GiB) allocated to the container up to 32GiB. Can be with a maximum of two decimals. Ratio with CPU cores must be equal to 2.')
@minValue(1)
@maxValue(32)
param memoryGis int = 8

@description('Minimum number of replicas that will be deployed')
@minValue(0)
param minReplicas int = 1

@description('Maximum number of replicas that will be deployed')
@minValue(0)
@maxValue(1000)
param maxReplicas int = 3

@description('Number of concurrent requests required in order to trigger scaling out.')
@minValue(1)
param scaleAtConcurrentHttpRequests int?

@description('Specifies the database connection string')
param appSettings {
  name: string
  @secure()
  value: string
}[]

@description('An existing Managed Identity\'s Resource Id with which to associate this Container App')
param userAssignedManagedIdentityId string

@description('An existing Service Principal\'s Client Id with which this Container App can pull Docker images (using it as the ACR username)')
@secure()
param dockerPullManagedIdentityClientId string

@description('An existing Service Principal\'s Secret value with which this Container App can pull Docker images (using it as the ACR password)')
@secure()
param dockerPullManagedIdentitySecretValue string

@description('The id of the owning Container App Environment')
param environmentId string

@description('The IP address of the Container App Environment')
param environmentIpAddress string

@description('Deploy private DNS zone and records so other resources can communicate with Container App over private network using FQDNs')
param deployPrivateDns bool = true

@description('The vnet that the Container App will be connected to')
param vnetName string

@description('Volumes to mount within Containers - used in conjunction with "volumeMounts"')
param volumes {
  name: string
  storageType: string
  storageName: string
  mountOptions: string?
  secrets: {
    path: string
    secretRef: string
  }[]?
}[] = []

@description('Volume mount points within Containers - used in conjunction with "volumes"')
param volumeMounts {
  mountPath: string
  volumeName: string
}[] = []

@description('An existing App Registration registered with Entra ID that will be used to control access to this Container App')
param entraIdAuthentication EntraIdAuthentication?

@description('Whether to create or update Azure Monitor alerts during this deploy')
param alerts {
  restarts: bool
  responseTime: bool
  cpuPercentage: bool
  memoryPercentage: bool
  connectionTimeouts: bool
  requestRetries: bool
  requestTimeouts: bool
  alertsGroupName: string
}?

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

var containerImageName = '${acrLoginServer}/${containerAppImageName}'

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedManagedIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: environmentId
    configuration: {
      secrets: [
        {
          name: 'container-registry-password'
          value: dockerPullManagedIdentitySecretValue
        }
      ]
      maxInactiveRevisions: 1
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: containerAppTargetPort
        allowInsecure: false
        corsPolicy: any(corsPolicy)
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: [
        {
          server: acrLoginServer
          username: dockerPullManagedIdentityClientId
          passwordSecretRef: 'container-registry-password'
        }
      ]
    }
    template: {
      containers: [
        {
          name: containerAppName
          image: containerImageName
          env: appSettings
          resources: {
            cpu: json(string(cpuCores))
            memory: '${memoryGis}Gi'
          }
          volumeMounts: volumeMounts
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
        rules: scaleAtConcurrentHttpRequests != null ? [
          {
            name: 'http-requests'
            http: {
              metadata: {
                concurrentRequests: string(scaleAtConcurrentHttpRequests)
              }
            }
          }
        ] : []
      }
      volumes: volumes
    }
    workloadProfileName: workloadProfileName
  }
  tags: tagValues
}

module azureAuthentication 'containerAppAzureAuthentication.bicep' = if (entraIdAuthentication != null) {
  name: '${containerAppName}AzureAuthentication'
  params: {
    clientId: entraIdAuthentication!.appRegistrationClientId
    containerAppName: containerApp.name
    allowedClientIds: entraIdAuthentication!.allowedClientIds
    allowedPrincipalIds: entraIdAuthentication!.allowedPrincipalIds
    requireAuthentication: entraIdAuthentication!.requireAuthentication
  }
}

var containerAppFqdn = containerApp.properties.configuration.ingress.fqdn

module privateDns 'containerAppPrivateDns.bicep' = if (deployPrivateDns) {
  name: '${containerAppName}PrivateDns'
  params: {
    domain: substring(containerAppFqdn, indexOf(containerAppFqdn, '.') + 1)
    ipAddress: environmentIpAddress
    vnetName: vnetName
    tagValues: tagValues
  }
}

module containerAppRestartsAlert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.restarts) {
  name: '${containerAppName}RestartsAlertModule'
  params: {
    resourceName: containerAppName
    resourceMetric: {
      resourceType: 'Microsoft.App/containerApps'
      metric: 'RestartCount'
    }
    config: {
      ...staticTotalGreaterThanZero
      nameSuffix: 'restarts'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
  dependsOn: [
    containerApp
  ]
}

module responseTimeAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.responseTime) {
  name: '${containerAppName}ResponseTimeDeploy'
  params: {
    resourceName: containerAppName
    resourceMetric: {
      resourceType: 'Microsoft.App/containerApps'
      metric: 'ResponseTime'
    }
    config: responseTimeConfig
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
  dependsOn: [
    containerApp
  ]
}

module cpuPercentageAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.cpuPercentage) {
  name: '${containerAppName}CpuPercentageDeploy'
  params: {
    resourceName: containerAppName
    resourceMetric: {
      resourceType: 'Microsoft.App/containerApps'
      metric: 'CpuPercentage'
    }
    config: cpuPercentageConfig
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
  dependsOn: [
    containerApp
  ]
}

module memoryPercentageAlert 'alerts/dynamicMetricAlert.bicep' = if (alerts != null && alerts!.memoryPercentage) {
  name: '${containerAppName}MemoryPercentageDeploy'
  params: {
    resourceName: containerAppName
    resourceMetric: {
      resourceType: 'Microsoft.App/containerApps'
      metric: 'MemoryPercentage'
    }
    config: memoryPercentageConfig
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
  dependsOn: [
    containerApp
  ]
}

module connectionTimeoutsAlert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.connectionTimeouts) {
  name: '${containerAppName}ConnectionTimeoutsAlertModule'
  params: {
    resourceName: containerAppName
    resourceMetric: {
      resourceType: 'Microsoft.App/containerApps'
      metric: 'ResiliencyConnectTimeouts'
    }
    config: {
      ...staticTotalGreaterThanZero
      nameSuffix: 'connection-timeouts'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
  dependsOn: [
    containerApp
  ]
}

module requestRetriesAlert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.requestRetries) {
  name: '${containerAppName}RequestRetriesAlertModule'
  params: {
    resourceName: containerAppName
    resourceMetric: {
      resourceType: 'Microsoft.App/containerApps'
      metric: 'ResiliencyRequestRetries'
    }
    config: {
      ...staticTotalGreaterThanZero
      nameSuffix: 'request-retries'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
  dependsOn: [
    containerApp
  ]
}

module requestTimeoutsAlert 'alerts/staticMetricAlert.bicep' = if (alerts != null && alerts!.requestTimeouts) {
  name: '${containerAppName}RequestTimeoutsAlertModule'
  params: {
    resourceName: containerAppName
    resourceMetric: {
      resourceType: 'Microsoft.App/containerApps'
      metric: 'ResiliencyRequestTimeouts'
    }
    config: {
      ...staticTotalGreaterThanZero
      nameSuffix: 'request-timeouts'
    }
    alertsGroupName: alerts!.alertsGroupName
    tagValues: tagValues
  }
  dependsOn: [
    containerApp
  ]
}

output containerAppFqdn string = containerAppFqdn
output containerImage string = containerImageName
output containerAppName string = containerApp.name
