param namePrefix string
param location string
param uamiName string
param storageId string
param tags object

resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: uamiName
  location: location
  tags: tags
}

// RBAC role assignments for the UAMI on the storage account
// Built-in role IDs:
// Storage Blob Data Contributor: ba92f5b4-2d11-453d-a403-e96b0029c9fe
// Storage Blob Delegator: 2a2b9908-6ea1-4ae2-8e65-a410df84e7d1

resource roleDataContrib 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageId, uami.id, 'blob-data-contributor')
  scope: storageResource
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource roleDelegator 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageId, uami.id, 'blob-delegator')
  scope: storageResource
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1')
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Get storage resource to use as the scope for the assignments
resource storageResource 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: last(split(storageId, '/'))
}

output userAssignedIdentityResourceId string = uami.id
