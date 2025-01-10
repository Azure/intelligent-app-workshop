metadata description = 'Creates Bing Search Service v7'
param name string
param location string = resourceGroup().location
param tags object = {}
param sku string = 'F1'

resource bingSearchService 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: name
  location: location
  tags: tags  
  kind: 'Bing.Search.v7'
  sku: {
    name: sku
  }
}

output serviceId string = bingSearchService.id
output apiKey string = listKeys(bingSearchService.id, '2021-04-30').key1
