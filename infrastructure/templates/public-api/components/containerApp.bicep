@description('Specifies the Resource Prefix')
param resourcePrefix string

@description('Specifies the location for all resources.')
param location string

@description('Specifies the login server from the registry.')
param acrLoginServer string

@description('Specifies the container image to deploy from the registry.')
param containerAppImageName string

@minLength(2)
@maxLength(32)
@description('Specifies the name of the container app.')
param containerAppName string

@description('Specifies the container port.')
param containerAppTargetPort int = 8080

@description('Number of CPU cores the container can use. Can be with a maximum of two decimals.')
@allowed([
  '1'
  '2'
  '3'
  '4'
])
param cpuCore string = '4'

@description('Amount of memory (in gibibytes, GiB) allocated to the container up to 4GiB. Can be with a maximum of two decimals. Ratio with CPU cores must be equal to 2.')
@allowed([
  '1'
  '2'
  '3'
  '4'
  '5'
  '6'
  '7'
  '8'
])
param memorySize string = '8'

@description('Minimum number of replicas that will be deployed')
@minValue(0)
@maxValue(25)
param minReplica int = 1

@description('Maximum number of replicas that will be deployed')
@minValue(0)
@maxValue(25)
param maxReplica int = 3

@description('Specifies the database connection string')
param appSettings {
  name: string
  @secure()
  value: string
}[]

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

@description('An existing Managed Identity\'s Resource Id with which to associate this Container App')
param userAssignedManagedIdentityId string

@description('Id of the owning Container App Environment')
param managedEnvironmentId string

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


var containerImageName = '${acrLoginServer}/${containerAppImageName}'
var containerApplicationName = toLower('${resourcePrefix}-ca-${containerAppName}')

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerApplicationName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedManagedIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: managedEnvironmentId
    configuration: {
      maxInactiveRevisions: 1
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: containerAppTargetPort
        allowInsecure: false
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
          identity: userAssignedManagedIdentityId
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
            cpu: json(cpuCore)
            memory: '${memorySize}Gi'
          }
          volumeMounts: volumeMounts
        }
      ]
      scale: {
        minReplicas: minReplica
        maxReplicas: maxReplica
        rules: [
          {
            name: 'http-requests'
            http: {
              metadata: {
                concurrentRequests: '10'
              }
            }
          }
        ]
      }
      volumes: volumes
    }
    workloadProfileName: 'Consumption'
  }
  tags: tagValues
}

output containerAppFQDN string = containerApp.properties.configuration.ingress.fqdn
output containerImage string = containerImageName
