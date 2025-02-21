param name string
param location string = resourceGroup().location
param tags object = {}

@description('The name of the identity')
param identityName string

@description('The name of the Application Insights')
param applicationInsightsName string

@description('The name of the container apps environment')
param containerAppsEnvironmentName string

@description('The name of the container registry')
param containerRegistryName string

@description('The name of the service')
param serviceName string = 'api'

@description('The name of the image')
param imageName string = ''

@description('Specifies if the resource exists')
param exists bool

@description('The OpenAI endpoint')
param openAiEndpoint string

@description('The OpenAI ChatGPT deployment name')
param openAiChatGptDeployment string

@description('The OpenAI API key')
param openAiApiKey string

@description('The Stock Service API key')
param stockServiceApiKey string

@description('The user-assigned role client id')
param userAssignedRoleClientId string

@description('AI Foundry Project connection string')
param aiFoundryProjectConnectionString string

@description('Grounding with Bing connection id')
param groundingWithBingConnectionId string

@description('An array of service binds')
param serviceBinds array

type managedIdentity = {
  resourceId: string
  clientId: string
}

@description('Unique identifier for user-assigned managed identity.')
param userAssignedManagedIdentity managedIdentity

module app '../core/host/container-app-upsert.bicep' = {
  name: '${serviceName}-container-app'
  params: {
    name: name
    location: location
    tags: union(tags, { 'azd-service-name': serviceName })
    identityName: identityName
    imageName: imageName
    exists: exists
    serviceBinds: serviceBinds
    containerAppsEnvironmentName: containerAppsEnvironmentName
    containerRegistryName: containerRegistryName
    secrets: {
        'open-ai-api-key': openAiApiKey
        'stock-service-api-key': stockServiceApiKey
        'azure-managed-identity-client-id':  userAssignedManagedIdentity.clientId
        'user-assigned-role-client-id': userAssignedRoleClientId
        'ai-foundry-project-connection-string': aiFoundryProjectConnectionString
        'grounding-with-bing-connection-id': groundingWithBingConnectionId
      }
    env: [
      {
        name: 'AZURE_MANAGED_IDENTITY_CLIENT_ID'
        value: 'azure-managed-identity-client-id'
      }
      {
        name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
        value: !empty(applicationInsightsName) ? applicationInsights.properties.ConnectionString : ''
      }
      {
        name: 'ManagedIdentity__ClientId'
        secretREf: 'azure-managed-identity-client-id'
      }
      {
        name: 'AIFoundryProject__Endpoint'
        value: openAiEndpoint
      }
      {
        name: 'AIFoundryProject__DeploymentName'
        value: openAiChatGptDeployment
      }
      {
        name: 'AIFoundryProject__ApiKey'
        secretRef: 'open-ai-api-key'
      }
      {
        name: 'AIFoundryProject__ConnectionString'
        secretRef: 'ai-foundry-project-connection-string'
      }
      {
        name: 'AIFoundryProject__GroundingWithBingConnectionId'
        secretRef: 'grounding-with-bing-connection-id'
      }
      {
        name: 'AIFoundryProject__UserAssignedRoleClientId'
        secretRef: 'user-assigned-role-client-id'
      }
      {
        name: 'StockService__ApiKey'
        secretRef: 'stock-service-api-key'
      }
    ]
    targetPort: 8080
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = if (!empty(applicationInsightsName)) {
  name: applicationInsightsName
}

output SERVICE_API_IDENTITY_NAME string = identityName
output SERVICE_API_IMAGE_NAME string = app.outputs.imageName
output SERVICE_API_NAME string = app.outputs.name
output SERVICE_API_URI string = app.outputs.uri
output SERVICE_API_PRINCIPAL_ID string = app.outputs.principalId
