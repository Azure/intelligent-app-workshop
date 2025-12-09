metadata description = 'Creates an Azure AI Foundry instance with project and model deployment.'

@description('The Azure region where all resources will be deployed')
param location string = resourceGroup().location
@description('Tags to apply to all resources')
param tags object = {}
@description('Name of the AI Foundry account (must be globally unique)')
param name string
@description('Name of the AI Foundry Project')
param aiProjectName string = '${name}-proj'
@description('Name of the GPT model deployment')
param gptModelDeploymentName string = 'gpt-4.1'
@description('Capacity of the GPT model deployment')
param gptModelCapacity int = 100
@description('SKU name for the GPT model deployment')
param gptModelSkuName string = 'GlobalStandard'

/*
  An AI Foundry resource is a variant of a CognitiveServices/account resource type.
*/
resource aiFoundry 'Microsoft.CognitiveServices/accounts@2025-04-01-preview' = {
  name: name
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  sku: {
    name: 'S0'
  }
  kind: 'AIServices'
  properties: {
    // Required to work in AI Foundry
    allowProjectManagement: true
    // Defines developer API endpoint subdomain
    customSubDomainName: name
    disableLocalAuth: false
    publicNetworkAccess: 'Enabled'
  }
  tags: tags
}

/*
  Developer APIs are exposed via a project, which groups in- and outputs that
  relate to one use case, including files.
  It's advisable to create one project right away, so development teams can
  directly get started.
  Projects may be granted individual RBAC permissions and identities on top of
  what account provides.
*/
resource aiProject 'Microsoft.CognitiveServices/accounts/projects@2025-04-01-preview' = {
  name: aiProjectName
  parent: aiFoundry
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {}
  tags: tags
}

/*
  Optionally deploy a model to use in playground, agents and other tools.
*/
resource modelDeployment 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: aiFoundry
  name: gptModelDeploymentName
  sku: {
    capacity: gptModelCapacity
    name: gptModelSkuName
  }
  properties: {
    model: {
      name: gptModelDeploymentName
      format: 'OpenAI'
    }
  }
}

output aiFoundryId string = aiFoundry.id
output aiFoundryName string = aiFoundry.name
output aiFoundryEndpoint string = aiFoundry.properties.endpoint
output aiProjectId string = aiProject.id
output aiProjectName string = aiProject.name
output modelDeploymentName string = modelDeployment.name
