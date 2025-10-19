param namePrefix string
param location string
param storageName string
param vnetId string
param peSubnetId string
param tags object

@minLength(3)
@maxLength(24)
var stgName = toLower(replace(storageName, '-', ''))

resource stg 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: stgName
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    publicNetworkAccess: 'Disabled'
    supportsHttpsTrafficOnly: true
    networkAcls: {
      bypass: 'None'
      defaultAction: 'Deny'
    }
  }
}

// Blob service + containers
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  name: '${stg.name}/default'
}

var containers = [
  'boundary'
  'batch-metadata'
  'points-bucket-0'
  'points-bucket-1'
  'points-bucket-2'
  'points-bucket-3'
  'points-bucket-4'
  'points-bucket-5'
  'points-bucket-6'
  'points-bucket-7'
  'points-bucket-8'
  'points-bucket-9'
]

resource blobContainers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = [for c in containers: {
  name: '${stg.name}/default/${c}'
  properties: {
    publicAccess: 'None'
  }
}]

// Private Endpoint to Blob subresource
resource pe 'Microsoft.Network/privateEndpoints@2023-09-01' = {
  name: '${namePrefix}-stg-blob-pe'
  location: location
  properties: {
    subnet: {
      id: peSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: 'blob-connection'
        properties: {
          privateLinkServiceId: stg.id
          groupIds: [ 'blob' ]
          requestMessage: 'Private Endpoint for blob'
        }
      }
    ]
  }
}

// Private DNS zone group linking storage private endpoint to zone defined in network module
resource pdnsZoneGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-09-01' = {
  name: '${pe.name}/blob-dns'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'blob'
        properties: {
          privateDnsZoneId: resourceId('Microsoft.Network/privateDnsZones', 'privatelink.blob.core.windows.net')
        }
      }
    ]
  }
}

output storageAccountName string = stg.name
output storageId string = stg.id
output privateEndpointId string = pe.id
