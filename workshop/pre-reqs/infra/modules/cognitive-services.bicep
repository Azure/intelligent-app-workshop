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
param gptModelSkuName string = 'Standard'
@description('Format of the GPT model')
param gptModelFormat string = 'OpenAI'
@description('Version of the GPT model')
param gptModelVersion string = '2024-08-06'
@description('Name of the AI Foundry Project')
param aiProjectName string
@description('Description of the AI Foundry Project')
param aiProjectDescription string = 'AI Foundry Project for Workshop Pre-Reqs'

param publicNetworkAccess string = 'Enabled'
param allowedIpRules array = []
param networkAcls object = empty(allowedIpRules) ? {
  defaultAction: 'Allow'
} : {
  ipRules: allowedIpRules
  defaultAction: 'Deny'
}

// Add Cognitive Services account of kind AIServices
resource cognitiveServicesAccount 'Microsoft.CognitiveServices/accounts@2025-04-01-preview' = {
  name: name
  location: location
  tags: tags
  kind: 'AIServices'
  sku: {
    name: sku
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    customSubDomainName: 'custom-sub-domain-prereqs'
    allowProjectManagement: true
    publicNetworkAccess: publicNetworkAccess
    networkAcls: networkAcls
  }
}

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

// Create the AI Foundry Project
resource aiProject 'Microsoft.CognitiveServices/accounts/projects@2025-04-01-preview' = {
  parent: cognitiveServicesAccount
  name: aiProjectName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    description: aiProjectDescription
  }
  tags: tags
}

output endpoint string = cognitiveServicesAccount.properties.endpoint
