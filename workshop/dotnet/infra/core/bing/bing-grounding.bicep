metadata description = 'Creates Grounding with Bing Search'
param name string
param location string = 'global'
param tags object = {}
param sku string = 'G1'

resource bingSearchGrounding 'Microsoft.Bing/accounts@2020-06-10' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
  }
  kind: 'Bing.Grounding'
}

output serviceId string = bingSearchGrounding.id
output apiKey string = bingSearchGrounding.listKeys().key1
