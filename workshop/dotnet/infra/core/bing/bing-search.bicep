metadata description = 'Creates Bing Search Service v7'
param name string
param location string = 'global'
param tags object = {}
param sku string = 'F1'

resource bingSearchService 'Microsoft.Bing/accounts@2020-06-10' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
  }
  kind: 'Bing.Search.v7'
}

output serviceId string = bingSearchService.id
output apiKey string = bingSearchService.listKeys().key1
