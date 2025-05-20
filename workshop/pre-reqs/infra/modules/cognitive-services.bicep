metadata description = 'Creates an Azure Cognitive Services instance.'

@description('The Azure region where all resources will be deployed')
param location string = resourceGroup().location
@description('Tags to apply to all resources')
param tags object = {}
@description('SKU for the Cognitive Services account')
param sku string = 'S0'
@description('Name of the Cognitive Services account')
param name string = 'cognitive-services-account'
@description('API version for Azure OpenAI API')
param ApiVersion string = '2024-11-20'
@description('Name of the GPT model deployment')
param gptModelDeploymentName string = 'gpt-4o'
@description('Capacity of the GPT model deployment')
param gptModelCapacity int = 1
@description('SKU name for the GPT model deployment')
param gptModelSkuName string = 'GlobalStandard'
@description('Format of the GPT model')
param gptModelFormat string = 'OpenAI'
@description('Version of the GPT model')
param gptModelVersion string = '2024-08-06'
@description('Name of the AI Foundry Hub')
param aiHubName string
@description('Display name of the AI Foundry Hub')
param aiHubFriendlyName string = 'AI Foundry Hub'
@description('Description of the AI Foundry Hub')
param aiHubDescription string = 'Azure AI Foundry Hub'
@description('Public network access setting for AI resources')
param publicNetworkAccess string = 'Enabled'
@description('Name of the AI Foundry Project')
param aiProjectName string
@description('Display name of the AI Foundry Project')
param aiProjectFriendlyName string = 'AI Foundry Project for Workshop Pre-Reqs'
@description('Description of the AI Foundry Project')
param aiProjectDescription string = 'AI Foundry Project for Workshop Pre-Reqs'
@description('Name of the OpenAI connection')
param openAiConnectionName string = 'openai-connection'

resource gpt4oModelDeployment 'Microsoft.CognitiveServices/accounts/deployments@2025-04-01-preview' = {
  parent: cognitiveServicesAccount
  name: gptModelDeploymentName
  sku: {
    capacity: gptModelCapacity
    name: gptModelSkuName
  }
  properties: {
    model: {
      format: gptModelFormat
      name: gptModelDeploymentName
      version: gptModelVersion
    }
  }
}

resource aiHub 'Microsoft.MachineLearningServices/workspaces@2024-07-01-preview' = {
  name: aiHubName
  location: location
  kind: 'hub'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    description: aiHubDescription
    friendlyName: aiHubFriendlyName
    publicNetworkAccess: publicNetworkAccess
  }
  tags: tags
}

// Create the AI Foundry Project
resource aiProject 'Microsoft.MachineLearningServices/workspaces@2024-07-01-preview' = {
  name: aiProjectName
  location: location
  kind: 'project'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    description: aiProjectDescription
    friendlyName: aiProjectFriendlyName
    hubResourceId: aiHub.id
    publicNetworkAccess: publicNetworkAccess
  }
  tags: tags
}

// Add Cognitive Services account of kind AIServices
resource cognitiveServicesAccount 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: name
  location: location
  tags: tags
  kind: 'OpenAI'
  sku: {
    name: sku
  }
  properties: {
    networkAcls: {
      defaultAction: 'Allow'
    }
    publicNetworkAccess: 'Enabled'
    apiProperties: {
      apiType: 'OpenAI'
      apiVersion: ApiVersion
    }
  }
}

// Connect the Azure OpenAI endpoint to the AI Foundry Project
resource aiServiceConnection 'Microsoft.MachineLearningServices/workspaces/connections@2023-08-01-preview' = {
  parent: aiProject
  name: openAiConnectionName
  properties: {
    category: 'AzureOpenAI'
    target: cognitiveServicesAccount.properties.endpoint
    authType: 'ApiKey'
    isSharedToAll: false
    credentials: {
      key: cognitiveServicesAccount.listKeys().key1
    }
    metadata: {
      resourceName: cognitiveServicesAccount.name
      ApiType: 'ApiKey'
      ApiVersion: ApiVersion
      Kind: 'OpenAI'
      AuthType: 'ApiKey'
    }
  }
}

output endpoint string = cognitiveServicesAccount.properties.endpoint
