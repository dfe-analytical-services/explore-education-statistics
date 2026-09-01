import { SignalRSku } from 'types.bicep'

@description('Name of the SignalR resource.')
param signalRName string

@description('The SignalR SKU.')
param sku SignalRSku

@description('The origins supported for CORS calls to this SignalR Service.')
param allowedOrigins string[]?

@description('The base URL for the hub event API calls.')
param hubEventBaseUrl string

resource signalR 'Microsoft.SignalRService/signalR@2025-01-01-preview' = {
  name: signalRName
  location: resourceGroup().location
  sku: sku
  kind: 'SignalR'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    tls: {
      clientCertEnabled: false
    }
    features: [
      {
        flag: 'ServiceMode'
        value: 'Default'
      }
      {
        flag: 'EnableConnectivityLogs'
        value: 'true'
      }
      {
        flag: 'EnableMessagingLogs'
        value: 'true'
      }
      {
        flag: 'EnableLiveTrace'
        value: 'true'
      }
    ]
    cors: {
      allowedOrigins: allowedOrigins
    }
    networkACLs: {
      defaultAction: 'deny'
      publicNetwork: {
        allow: [
          'ClientConnection'
          'ServerConnection'
          'RESTAPI'
          'Trace'
        ]
      }
    }
    upstream: {
      templates: [
        {
          categoryPattern: '*'
          eventPattern: '*'
          hubPattern: '*'
          urlTemplate: '${hubEventBaseUrl}/hubs/{hub}/{event}'
        }
      ]
    }
  }
}
