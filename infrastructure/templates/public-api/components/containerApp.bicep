import { entraIdAuthenticationType } from '../types.bicep'

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

@description('An existing Managed Identity\'s Resource Id with which to associate this Container App')
param userAssignedManagedIdentityId string

@description('An existing Service Principal\'s Client Id with which this Container App can pull Docker images (using it as the ACR username)')
@secure()
param dockerPullManagedIdentityClientId string

@description('An existing Service Principal\'s Secret value with which this Container App can pull Docker images (using it as the ACR password)')
@secure()
param dockerPullManagedIdentitySecretValue string

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

@description('An existing App Registration registered with Entra ID that will be used to control access to this Container App')
param entraIdAuthentication entraIdAuthenticationType?

@description('A set of tags with which to tag the resource in Azure')
param tagValues object

var containerImageName = '${acrLoginServer}/${containerAppImageName}'

resource containerApp 'Microsoft.App/containerApps@2023-11-02-preview' = {
  name: containerAppName
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
        corsPolicy: corsPolicy
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

output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
output containerImage string = containerImageName
output containerAppName string = containerApp.name
