Buddhabrot Cloud Backend — Authentication Infrastructure as Code (Azure)

This folder contains Azure Bicep templates that provision the authentication and access-control foundation for the cloud backend described in docs/requirements.md.

What this deploys
- Storage account for boundary file, points, and metadata with all public access disabled.
- Pre-created blob containers:
  - boundary (stores the boundary file, private)
  - batch-metadata (stores batch metadata objects, private)
  - points-bucket-0 .. points-bucket-9 (escape-time buckets)
- User Assigned Managed Identity (UAMI) for the backend to authenticate to Azure resources.
- RBAC assignments for the UAMI on the storage account:
  - Storage Blob Data Contributor (read/write blobs)
  - Storage Blob Delegator (required to request User Delegation SAS)
- Private networking for storage access:
  - A Virtual Network and a subnet dedicated to Private Endpoints
  - A Private Endpoint to the storage account’s Blob service
  - A Private DNS zone privatelink.blob.core.windows.net linked to the VNet

How this maps to requirements
- 4.1.1 Access Control and Networking: All endpoints private; Azure components authenticate via Managed Identity; non-Azure access via SAS. This deployment disables public network access to the storage account, sets up private endpoint + DNS, and provisions a UAMI with proper roles to issue User Delegation SAS.
- 4.1.2/4.1.3/4.1.4: Creates containers for boundary, points (10 buckets), and batch metadata; exact data formats are handled by application code. Idempotent uploads and counters are application-level but this infra enables private, role-based access.

Prerequisites
- Azure subscription and permissions to create resource groups and assign roles
- Azure CLI >= 2.56 with Bicep support (az bicep version)
- A resource group (or create one below)

Quick start
1) Choose environment variables:
   SUBSCRIPTION_ID="<your-subscription-guid>"
   RESOURCE_GROUP="rg-buddhabrot-dev"
   LOCATION="eastus"
   NAME_PREFIX="buddhabrotdev"

2) Create RG if needed:
   az account set --subscription "$SUBSCRIPTION_ID"
   az group create -n "$RESOURCE_GROUP" -l "$LOCATION"

3) Deploy:
   az deployment group create \
     -g "$RESOURCE_GROUP" \
     -f main.bicep \
     -p namePrefix="$NAME_PREFIX" location="$LOCATION"

Outputs
- storageAccountName: Name of the storage account
- userAssignedIdentityResourceId: Resource ID of the UAMI
- vnetId and privateEndpointId: Network resources for private access

Post-deploy notes for application/backend
- When running in Azure (e.g., Azure Functions, Container Apps, VMs), assign the created UAMI to your compute resource. Your backend should use DefaultAzureCredential to authenticate to Storage.
- To issue SAS for non-Azure clients, use the backend with the UAMI:
  - Acquire a User Delegation Key via BlobServiceClient.GetUserDelegationKeyAsync
  - Construct user-delegation SAS tokens with minimal scope/permissions per the requirements.
- The storage account blocks public access and only resolves via the private endpoint from within the VNet. Ensure your compute runs in or can reach this VNet (via private endpoints, VNet integration, or Private Link service chaining).

Parameters
- namePrefix: String used to name resources (letters/numbers). Keep it short (<= 12 chars) to satisfy naming rules.
- location: Azure region (e.g., eastus)
- vnetAddressSpace: CIDR for the VNet (default 10.30.0.0/16)
- privateEndpointSubnetPrefix: CIDR for the Private Endpoint subnet (default 10.30.1.0/24)
- tags: Object of resource tags

Security defaults
- Storage: public network access disabled, blob public access disabled
- Private Endpoint + Private DNS for blob service
- RBAC least privilege for backend: Data Contributor + Delegator

Cleanup
- Deleting the resource group removes all resources (including data) — be careful.
