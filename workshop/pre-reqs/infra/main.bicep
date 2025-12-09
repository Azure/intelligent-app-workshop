targetScope = 'subscription'

@description('Name of the environment used to generate a short unique hash for resources.')
@minLength(1)
@maxLength(60)
param environmentName string

@description('Primary location for all resources')
param location string

@description('Tags to apply to the resources')
param tags string = ''

@description('Name of the resource group')
param resourceGroupName string = ''

var abbrs = loadJsonContent('./abbreviations.json')
var baseTags = { 'azd-env-name': environmentName }
var updatedTags = union(empty(tags) ? {} : base64ToJson(tags), baseTags)
var aiFoundryName = '${abbrs.cognitiveServicesAccounts}${environmentName}'

// Organize resources in a resource group
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: !empty(resourceGroupName) ? resourceGroupName : '${abbrs.resourcesResourceGroups}${environmentName}'
  location: location
  tags: updatedTags
}

module cognitiveServicesAccount 'modules/cognitive-services.bicep' = {
  name: '${abbrs.cognitiveServicesAccounts}${environmentName}'
  scope: resourceGroup
  params: {
    location: location
    tags: updatedTags
    name: aiFoundryName
    aiProjectName:  '${aiFoundryName}-proj'
  }
}

output aiFoundryEndpoint string = cognitiveServicesAccount.outputs.aiFoundryEndpoint
output aiFoundryName string = cognitiveServicesAccount.outputs.aiFoundryName
output aiProjectName string = cognitiveServicesAccount.outputs.aiProjectName
output modelDeploymentName string = cognitiveServicesAccount.outputs.modelDeploymentName
