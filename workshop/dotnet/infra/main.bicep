targetScope = 'subscription'

@description('Name of the environment used to generate a short unique hash for resources.')
@minLength(1)
@maxLength(64)
param environmentName string

@description('Primary location for all resources')
@allowed([ 'centralus', 'eastus2', 'eastasia', 'westus', 'westeurope', 'westus2', 'australiaeast', 'eastus', 'francecentral', 'japaneast', 'nortcentralus', 'swedencentral', 'switzerlandnorth', 'uksouth' ])
param location string
param tags string = ''

@description('Location for the OpenAI resource group')
@allowed([ 'canadaeast', 'westus', 'eastus', 'eastus2', 'francecentral', 'swedencentral', 'switzerlandnorth', 'uksouth', 'japaneast', 'northcentralus', 'australiaeast' ])
@metadata({
  azd: {
    type: 'location'
  }
})
param openAiResourceGroupLocation string

@description('Name of the chat GPT model. Default: gpt-35-turbo')
@allowed([ 'gpt-35-turbo', 'gpt-4', 'gpt-4o', 'gpt-4o-mini', 'gpt-35-turbo-16k', 'gpt-4-16k' ])
param azureOpenAIChatGptModelName string = 'gpt-4o-mini'

@description('Name of the chat GPT model. Default: 0613 for gpt-35-turbo, or choose 2024-07-18 for gpt-4o-mini')
@allowed([ '0613', '2024-07-18' ])
param azureOpenAIChatGptModelVersion string ='2024-07-18'

@description('Defines if the process will deploy an Azure Application Insights resource')
param useApplicationInsights bool = true

// @description('Name of the Azure Application Insights dashboard')
param applicationInsightsDashboardName string = ''

@description('Name of the Azure Application Insights resource')
param applicationInsightsName string = ''

@description('Name of the Azure App Service Plan')
param appServicePlanName string = ''

@description('Capacity of the chat GPT deployment. Default: 10')
param chatGptDeploymentCapacity int = 8

@description('Name of the chat GPT deployment')
param azureChatGptDeploymentName string = 'chat'

@description('Name of the container apps environment')
param containerAppsEnvironmentName string = ''

@description('Name of the Azure container registry')
param containerRegistryName string = ''

@description('Name of the resource group for the Azure container registry')
param containerRegistryResourceGroupName string = ''

@description('Name of the Azure Log Analytics workspace')
param logAnalyticsName string = ''

@description('Name of the resource group for the OpenAI resources')
param openAiResourceGroupName string = ''

@description('Name of the OpenAI service')
param openAiServiceName string = ''

@description('SKU name for the OpenAI service. Default: S0')
param openAiSkuName string = 'S0'

@description('ID of the principal')
param principalId string = ''

@description('Type of the principal. Valid values: User,ServicePrincipal')
param principalType string = 'User'

@description('Name of the resource group')
param resourceGroupName string = ''

@description('Name of the storage account')
param storageAccountName string = ''

@description('Name of the storage container. Default: content')
param storageContainerName string = 'content'

@description('Location of the resource group for the storage account')
param storageResourceGroupLocation string = location

@description('Name of the resource group for the storage account')
param storageResourceGroupName string = ''

@description('Bing Search Service name')
param bingSearchServiceName string = ''

@description('Grounding with Bing resource name')
param bingSearchGroundingName string = ''

@description('Specifies if the web app exists')
param webAppExists bool = false

@description('Specifies if the api app exists')
param apiAppExists bool = false

@description('Name of the api app container')
param apiContainerAppName string = ''

@description('Name of the web app container')
param webContainerAppName string = ''

@description('Name of the web app image')
param webImageName string = ''

@description('Name of the api app image')
param apiImageName string = ''

@description('Use Azure OpenAI service')
param useAOAI bool

@description('OpenAI API Key, leave empty to provision a new Azure OpenAI instance')
param openAIApiKey string

@description('OpenAI Deployment name')
param openAiChatGptDeployment string

@description('OpenAI Endpoint')
param openAiEndpoint string

@description('Stock Service API Key from Polygon.io')
param stockServiceApiKey string

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))

var baseTags = { 'azd-env-name': environmentName }
var updatedTags = union(empty(tags) ? {} : base64ToJson(tags), baseTags)

// Organize resources in a resource group
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: !empty(resourceGroupName) ? resourceGroupName : '${abbrs.resourcesResourceGroups}${environmentName}'
  location: location
  tags: updatedTags
}

resource azureOpenAiResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = if (!empty(openAiResourceGroupName) && useAOAI) {
  name: !empty(openAiResourceGroupName) ? openAiResourceGroupName : resourceGroup.name
}


resource storageResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = if (!empty(storageResourceGroupName)) {
  name: !empty(storageResourceGroupName) ? storageResourceGroupName : resourceGroup.name
}


// Create a user assigned identity
module identity './app/user-assigned-identity.bicep' = {
  name: 'identity'
  scope: resourceGroup
  params: {
    name: 'sk-app-identity'
  }
}

module bingSearch './core/bing/bing-search.bicep' = {
  name: 'bingSearch'
  scope: resourceGroup
  params: {
    tags: updatedTags
    name: !empty(bingSearchServiceName) ? bingSearchServiceName : '${abbrs.bingSearchServices}${resourceToken}'
  }
}

module bingSearchGrounding './core/bing/bing-grounding.bicep' = {
  name: 'bingSearchGrounding'
  scope: resourceGroup
  params: {
    tags: updatedTags
    name: !empty(bingSearchGroundingName) ? bingSearchGroundingName : '${abbrs.bingSearchGroundings}${resourceToken}'
  }
}

// Container apps host (including container registry)
module containerApps 'core/host/container-apps.bicep' = {
  name: 'container-apps'
  scope: resourceGroup
  params: {
    name: 'app'
    containerAppsEnvironmentName: !empty(containerAppsEnvironmentName) ? containerAppsEnvironmentName : '${abbrs.appManagedEnvironments}${resourceToken}'
    containerRegistryName: !empty(containerRegistryName) ? containerRegistryName : '${abbrs.containerRegistryRegistries}${resourceToken}'
    containerRegistryResourceGroupName: !empty(containerRegistryResourceGroupName) ? containerRegistryResourceGroupName : resourceGroup.name
    location: location
    logAnalyticsWorkspaceName: monitoring.outputs.logAnalyticsWorkspaceName
  }
}


// App api
module api './app/api.bicep' = {
  name: 'api'
  scope: resourceGroup
  params: {
    name: !empty(apiContainerAppName) ? apiContainerAppName : '${abbrs.appContainerApps}api-${resourceToken}'
    location: location
    tags: updatedTags
    imageName: apiImageName
    identityName: identity.outputs.name
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    containerAppsEnvironmentName: containerApps.outputs.environmentName
    containerRegistryName: containerApps.outputs.registryName
    userAssignedManagedIdentity: {
      resourceId: identity.outputs.resourceId
      clientId: identity.outputs.clientId
    }
    exists: apiAppExists
    openAiApiKey: useAOAI ? '' : openAIApiKey
    openAiEndpoint: useAOAI ? azureOpenAi.outputs.endpoint : openAiEndpoint
    stockServiceApiKey: stockServiceApiKey
    bingSearchApiKey: bingSearch.outputs.apiKey
    //bingSearchGroundingApiKey: bingSearchGrounding.outputs.apiKey
    openAiChatGptDeployment: useAOAI ? azureChatGptDeploymentName : openAiChatGptDeployment
    serviceBinds: []
  }
}

// App web
module web './app/web.bicep' = {
  name: 'web'
  scope: resourceGroup
  params: {
    name: !empty(webContainerAppName) ? webContainerAppName : '${abbrs.appContainerApps}web-${resourceToken}'
    location: location
    tags: updatedTags
    imageName: webImageName
    identityName: identity.outputs.name
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    containerAppsEnvironmentName: containerApps.outputs.environmentName
    containerRegistryName: containerApps.outputs.registryName
    userAssignedManagedIdentity: {
      resourceId: identity.outputs.resourceId
      clientId: identity.outputs.clientId
    }
    exists: webAppExists
    apiEndpoint: '${api.outputs.SERVICE_API_URI}/chat'
    serviceBinds: []
  }
}


// Monitor application with Azure Monitor
module monitoring 'core/monitor/monitoring.bicep' = {
  name: 'monitoring'
  scope: resourceGroup
  params: {
    location: location
    tags: updatedTags
    includeApplicationInsights: true
    logAnalyticsName: !empty(logAnalyticsName) ? logAnalyticsName : '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsName: !empty(applicationInsightsName) ? applicationInsightsName : '${abbrs.insightsComponents}${resourceToken}'
    applicationInsightsDashboardName: !empty(applicationInsightsDashboardName) ? applicationInsightsDashboardName : '${abbrs.portalDashboards}${resourceToken}'    
  }
}

module azureOpenAi 'core/ai/cognitiveservices.bicep' = if (useAOAI) {
  name: 'openai'
  scope: azureOpenAiResourceGroup
  params: {
    name: !empty(openAiServiceName) ? openAiServiceName : '${abbrs.cognitiveServicesAccounts}${resourceToken}'
    location: openAiResourceGroupLocation
    tags: updatedTags
    sku: {
      name: openAiSkuName
    }
    deployments: [
      {
        name: azureChatGptDeploymentName
        model: {
          format: 'OpenAI'
          name: azureOpenAIChatGptModelName
          version: azureOpenAIChatGptModelVersion
        }
        sku: {
          name: 'Standard'
          capacity: chatGptDeploymentCapacity
        }
      }
    ]
  }
}


module storage 'core/storage/storage-account.bicep' = {
  name: 'storage'
  scope: storageResourceGroup
  params: {
    name: !empty(storageAccountName) ? storageAccountName : '${abbrs.storageStorageAccounts}${resourceToken}'
    location: storageResourceGroupLocation
    tags: updatedTags
    sku: {
      name: 'Standard_LRS'
    }
    deleteRetentionPolicy: {
      enabled: true
      days: 2
    }
    containers: [
      {
        name: storageContainerName
      }
    ]
  }
}

// USER ROLES
module azureOpenAiRoleUser 'core/security/role.bicep' = if (useAOAI) {
  scope: azureOpenAiResourceGroup
  name: 'openai-role-user'
  params: {
    principalId: principalId
    roleDefinitionId: '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
    principalType: principalType
  }
}

// Assign storage blob data contributor to the user for local runs
module storageRoleUser 'core/security/role.bicep' = {
  scope: storageResourceGroup
  name: 'storage-role-user'
  params: {
    principalId: principalId 
    roleDefinitionId: '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1' // built-in role definition id for storage blob data reader
    principalType: principalType
  }
}

// Assign storage blob data contributor to the identity
module storageContribRoleUser 'core/security/role.bicep' = {
  scope: storageResourceGroup
  name: 'storage-contribrole-user'
  params: {
    principalId: principalId
    roleDefinitionId: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
    principalType: principalType
  }
}

// SYSTEM IDENTITIES
module azureOpenAiRoleApi 'core/security/role.bicep' = if (useAOAI) {
  scope: azureOpenAiResourceGroup
  name: 'openai-role-api'
  params: {
    principalId: identity.outputs.principalId
    roleDefinitionId: '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
    principalType: 'ServicePrincipal'
  }
}

module storageRoleApi 'core/security/role.bicep' = {
  scope: storageResourceGroup
  name: 'storage-role-api'
  params: {
    principalId: identity.outputs.principalId
    roleDefinitionId: '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
    principalType: 'ServicePrincipal'
  }
}

module storageContribRoleApi 'core/security/role.bicep' = {
  scope: storageResourceGroup
  name: 'storage-contribrole-api'
  params: {
    principalId: identity.outputs.principalId
    roleDefinitionId: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
    principalType: 'ServicePrincipal'
  }
}

output APPLICATIONINSIGHTS_CONNECTION_STRING string = monitoring.outputs.applicationInsightsConnectionString
output APPLICATIONINSIGHTS_NAME string = monitoring.outputs.applicationInsightsName
output AZURE_USE_APPLICATION_INSIGHTS bool = useApplicationInsights
output AZURE_CONTAINER_ENVIRONMENT_NAME string = containerApps.outputs.environmentName
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerApps.outputs.registryLoginServer
output AZURE_CONTAINER_REGISTRY_NAME string = containerApps.outputs.registryName
output AZURE_CONTAINER_REGISTRY_RESOURCE_GROUP string = containerApps.outputs.registryName
output AZURE_LOCATION string = location
output AZURE_OPENAI_RESOURCE_LOCATION string = openAiResourceGroupLocation
output AZURE_OPENAI_CHATGPT_DEPLOYMENT string = azureChatGptDeploymentName
output AZURE_OPENAI_ENDPOINT string = useAOAI? azureOpenAi.outputs.endpoint : ''
output AZURE_OPENAI_RESOURCE_GROUP string = useAOAI ? azureOpenAiResourceGroup.name : ''
output AZURE_OPENAI_SERVICE string = useAOAI ? azureOpenAi.outputs.name : ''
output AZURE_RESOURCE_GROUP string = resourceGroup.name
output AZURE_STORAGE_ACCOUNT string = storage.outputs.name
output AZURE_STORAGE_BLOB_ENDPOINT string = storage.outputs.primaryEndpoints.blob
output AZURE_STORAGE_CONTAINER string = storageContainerName
output AZURE_STORAGE_RESOURCE_GROUP string = storageResourceGroup.name
output AZURE_TENANT_ID string = tenant().tenantId
output SERVICE_WEB_IDENTITY_NAME string = web.outputs.SERVICE_WEB_IDENTITY_NAME
output SERVICE_WEB_NAME string = web.outputs.SERVICE_WEB_NAME
output SERVICE_API_IDENTITY_NAME string = api.outputs.SERVICE_API_IDENTITY_NAME
output SERVICE_API_NAME string = api.outputs.SERVICE_API_NAME
output USE_AOAI bool = useAOAI
output AZURE_OPENAI_CHATGPT_MODEL_VERSION string = azureOpenAIChatGptModelVersion
output AZURE_OPENAI_CHATGPT_MODEL_NAME string = azureOpenAIChatGptModelName
