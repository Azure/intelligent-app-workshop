targetScope = 'subscription'

@description('Name of the environment used to generate a short unique hash for resources.')
@minLength(1)
@maxLength(60)
param environmentName string

@description('Primary location for all resources')
// Locations that support gpt-4o
@allowed([ 'australiaeast', 'brazilsouth', 'canadaeast', 'eastus', 'eastus2', 'francecentral', 'germanywestcentral', 'japaneast', 'koreacentral', 'northcentralus', 'norwayeast', 'polandcentral', 'switzerlandnorth', 'southafricanorth','southcentralus', 'southindia', 'spaincentral', 'swedencentral', 'switzerlandnorth', 'uaenorth', 'uksouth', 'westeurope', 'westus', 'westus3' ])
param location string

@description('Tags to apply to the resources')
param tags string = ''

@description('Name of the resource group')
param resourceGroupName string = ''

var abbrs = loadJsonContent('./abbreviations.json')
var baseTags = { 'azd-env-name': environmentName }
var updatedTags = union(empty(tags) ? {} : base64ToJson(tags), baseTags)
param name string = 'cognitive-services-account'

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
    name: name
    aiProjectName:  '${abbrs.cognitiveServicesAccounts}${environmentName}-pro'
  }
}

output endpoint string = cognitiveServicesAccount.outputs.endpoint
