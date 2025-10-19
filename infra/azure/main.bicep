// Buddhabrot Authentication & Access Infrastructure (Azure Bicep)
// Provisions storage (private), user-assigned managed identity, RBAC, VNet, private endpoint, and DNS.

param namePrefix string
@allowed([ 'eastus' 'eastus2' 'westus' 'westus2' 'westus3' 'centralus' 'northcentralus' 'southcentralus' 'westeurope' 'northeurope' 'uksouth' 'ukwest' 'swedencentral' 'switzerlandnorth' 'francecentral' 'germanywestcentral' ])
param location string = resourceGroup().location
param vnetAddressSpace string = '10.30.0.0/16'
param privateEndpointSubnetPrefix string = '10.30.1.0/24'
param tags object = {
  project: 'buddhabrot'
  component: 'auth-infra'
}

// Resource name seeds
var storageName = toLower(format('{0}stg', take(namePrefix, 20)))
var vnetName = format('{0}-vnet', namePrefix)
var peSubnetName = 'snet-private-endpoints'
var uamiName = format('{0}-backend-mi', namePrefix)

module net './modules/network.bicep' = {
  name: 'network'
  params: {
    namePrefix: namePrefix
    location: location
    vnetName: vnetName
    addressSpace: vnetAddressSpace
    peSubnetName: peSubnetName
    peSubnetPrefix: privateEndpointSubnetPrefix
    tags: tags
  }
}

module stg './modules/storage.bicep' = {
  name: 'storage'
  params: {
    namePrefix: namePrefix
    location: location
    storageName: storageName
    vnetId: net.outputs.vnetId
    peSubnetId: net.outputs.peSubnetId
    tags: tags
  }
}

module id './modules/identity.bicep' = {
  name: 'identity'
  params: {
    namePrefix: namePrefix
    location: location
    uamiName: uamiName
    storageId: stg.outputs.storageId
    tags: tags
  }
}

output storageAccountName string = stg.outputs.storageAccountName
output storageId string = stg.outputs.storageId
output userAssignedIdentityResourceId string = id.outputs.userAssignedIdentityResourceId
output vnetId string = net.outputs.vnetId
output privateEndpointId string = stg.outputs.privateEndpointId
