# Deployment Requirements

This document outlines all prerequisites and setup steps required to deploy the Toluwaloope Location Detector Azure Function App with OIDC authentication.

## Prerequisites

### 1. Azure Subscriptions
- **Development Subscription**: For staging deployments
- **Production Subscription**: For production deployments

### 2. GitHub Repository
- Repository must be accessible with Actions enabled
- Repository must be a public repository (required for OIDC with GitHub Actions)

### 3. Local Tools
- **Git**: For version control
- **Azure CLI**: For manual Azure operations (optional, used by GitHub Actions)
- **.NET 8.0 SDK**: For local development and testing
- **Terraform CLI**: For local Terraform validation (v1.6.0 or higher)

---

## Azure Configuration

### 1. Create Microsoft Entra ID (Azure AD) Application

#### Step 1: Register Application in Each Subscription

For **both Development and Production** subscriptions:

1. Navigate to Azure Portal → Microsoft Entra ID → App registrations
2. Click **New registration**
3. Enter application name: `toluwaloope-location-detector-app` 
4. Select **Accounts in this organizational directory only**
5. Click **Register**

#### Step 2: Configure Federated Credentials for OIDC

For each registered application:

1. Go to **Certificates & secrets** → **Federated credentials**
2. Click **Add credential**
3. Select **GitHub Actions deploying Azure resources**
4. Configure the following fields:
   - **Organization**: Your GitHub organization (e.g., `yourusername`)
   - **Repository**: `toluwaloope-location-detector`
   - **Entity type**: Select **Branch**
   - **GitHub Branch name**: Select **main**
5. Enter a descriptive name and click **Add**

#### Step 3: Note Application Credentials

From each app registration, note these values (these will be used for GitHub Secrets):

- **Application (Client) ID**: Found in Overview tab
- **Directory (Tenant) ID**: Found in Overview tab (same for both dev and prod if in same tenant)

### 2. Create Azure Storage Account for Terraform State (Optional, for Production)

To enable remote state management:

1. Create a storage account in your production subscription
2. Create a blob container named `terraform-state`
3. Note the storage account name and container name

Then uncomment the `backend` block in `infrastructure/terraform/provider.tf`:

```hcl
terraform {
  backend "azurerm" {
    resource_group_name  = "your-rg-name"
    storage_account_name = "your-storage-account"
    container_name       = "terraform-state"
    key                  = "prod.tfstate"
  }
}
```

---

## GitHub Configuration

### 1. Create GitHub Environments

In your GitHub repository settings:

1. Go to **Settings** → **Environments**
2. Create two environments:
   - **development**: For dev deployments
   - **production**: For prod deployments

### 2. Configure GitHub Secrets

Add the following secrets to your GitHub repository (repository-level secrets):

#### Shared Secrets (same for both dev and prod):

| Secret Name | Value | Notes |
|---|---|---|
| `AZURE_CLIENT_ID` | Application (Client) ID | From Microsoft Entra ID app registration |
| `AZURE_TENANT_ID` | Directory (Tenant) ID | From Microsoft Entra ID app registration |
| `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` | Publish profile content | Generated below |

#### Environment-Specific Secrets:

**Development Environment Secrets:**
| Secret Name | Value | Notes |
|---|---|---|
| `AZURE_DEV_SUBSCRIPTION_ID` | Subscription ID | From Azure Portal → Subscriptions |

**Production Environment Secrets:**
| Secret Name | Value | Notes |
|---|---|---|
| `AZURE_PROD_SUBSCRIPTION_ID` | Subscription ID | From Azure Portal → Subscriptions |

### 3. Generate Publish Profile

The publish profile is generated after the first Terraform deployment creates the Azure Function App.

**Option A: Generate via Azure Portal (After First Deployment)**

1. Navigate to Azure Portal → Function Apps → `toluwaloope-location-detector-dev` (or prod)
2. Click **Get publish profile** in the top menu
3. Copy the downloaded XML content
4. Add as GitHub Secret `AZURE_FUNCTIONAPP_PUBLISH_PROFILE`

**Option B: Generate via Azure CLI**

```bash
# For Development
az functionapp deployment list-publishing-profiles \
  --name toluwaloope-location-detector-dev \
  --resource-group rg-toluwaloope-location-detector-dev \
  --subscription <AZURE_DEV_SUBSCRIPTION_ID> \
  --output json

# For Production
az functionapp deployment list-publishing-profiles \
  --name toluwaloope-location-detector-prod \
  --resource-group rg-toluwaloope-location-detector-prod \
  --subscription <AZURE_PROD_SUBSCRIPTION_ID> \
  --output json
```

---

## Azure Role Assignment

### Grant Required Permissions

The registered applications need permissions to create and manage resources in each subscription.

#### Using Azure Portal:

For **each subscription** (dev and prod):

1. Navigate to **Subscriptions** → Your subscription
2. Click **Access control (IAM)** → **Add** → **Add role assignment**
3. Select role: **Contributor** (allows creating/managing resources)
4. Go to **Members** tab, select **User, group, or service principal**
5. Search for your app registration by name (e.g., `toluwaloope-location-detector-dev`)
6. Select it and click **Review + assign**

#### Using Azure CLI:

```bash
# For Development
az role assignment create \
  --role Contributor \
  --assignee <AZURE_CLIENT_ID> \
  --subscription <AZURE_DEV_SUBSCRIPTION_ID>

# For Production
az role assignment create \
  --role Contributor \
  --assignee <AZURE_CLIENT_ID> \
  --subscription <AZURE_PROD_SUBSCRIPTION_ID>
```

---

## Deployment Workflow

### Initial Setup Checklist

- [ ] Create Microsoft Entra ID applications for dev and prod
- [ ] Configure federated credentials for OIDC authentication
- [ ] Document Client ID, Tenant ID, and Subscription IDs
- [ ] Assign Contributor role to applications in respective subscriptions
- [ ] Create GitHub environments (development and production)
- [ ] Add all required secrets to GitHub:
  - [ ] `AZURE_CLIENT_ID`
  - [ ] `AZURE_TENANT_ID`
  - [ ] `AZURE_DEV_SUBSCRIPTION_ID`
  - [ ] `AZURE_PROD_SUBSCRIPTION_ID`
  - [ ] `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` (can be generated after first deployment)
- [ ] (Optional) Create storage account and configure Terraform remote state backend

### Triggering Deployments

Once all prerequisites are met:

1. Navigate to GitHub repository
2. Go to **Actions** → **Build and Deploy to Azure Function App**
3. Click **Run workflow**
4. Select environment: **dev** or **prod**
5. Click **Run workflow**

The workflow will:
1. Provision Azure infrastructure using Terraform (resource group, storage account, app service plan, function app, monitoring)
2. Build the .NET project
3. Deploy to the selected Azure Function App
4. Run health checks
5. Send deployment notification

### Monitoring Deployments

- View workflow progress in **Actions** tab
- Check deployment logs for each job
- View post-deployment test results
- Review deployment summary in workflow annotations

---

## Troubleshooting

### OIDC Authentication Failures

**Error**: `The actor is not authorized to perform the requested action`

**Solution**:
1. Verify federated credentials are configured correctly
2. Ensure GitHub environment names match exactly (case-sensitive)
3. Confirm repository is public (required for OIDC)
4. Check that role assignments are in place

### Terraform State Issues

**Error**: `Error: Error acquiring the state lock`

**Solution**:
1. Ensure only one workflow run is executing Terraform at a time
2. Wait for previous workflow to complete
3. If stuck, manually remove the state lock in Azure Storage (use Azure Portal → Storage Account → Containers → terraform-state)

### Function App Deployment Failures

**Error**: `Deployment failed for function app`

**Solution**:
1. Check publish profile is valid and up-to-date
2. Verify .NET build completed successfully
3. Review Azure Function App logs in Azure Portal
4. Ensure storage account connection is valid

### Health Check Failures

**Error**: `Health check failed with status code: 404`

**Solution**:
1. Wait for function app to warm up (may take 1-2 minutes after deployment)
2. Verify endpoint `/api/GetCallerInformation` exists in codebase
3. Check Application Insights logs for errors
4. Review Azure Function App runtime logs

---

## Environment Variables & Configuration

### Function App Application Settings

These are automatically configured by Terraform and set in the Azure Function App:

| Setting | Purpose |
|---|---|
| `APPINSIGHTS_INSTRUMENTATIONKEY` | Application Insights monitoring |
| `AzureWebJobsStorage` | Storage account connection for function runtime |
| `FUNCTIONS_EXTENSION_VERSION` | Azure Functions runtime version (~4) |
| `FUNCTIONS_WORKER_RUNTIME` | Language runtime (dotnet-isolated) |

### Local Development

To test locally with the emulator:

1. Download and install [Azure Cosmos DB Emulator](https://learn.microsoft.com/azure/cosmos-db/emulator) (if using Cosmos DB)
2. Update `local.settings.json` with emulator connection strings
3. Run: `func start`

---

## Security Considerations

### OIDC Benefits Over Secrets

- **No long-lived secrets**: Eliminates need to rotate credentials
- **Short-lived tokens**: Each deployment gets a token valid for 15 minutes
- **Audit trail**: Each deployment is traceable to a specific GitHub action run
- **Revocable**: Can be revoked by removing federated credentials

### Best Practices

1. Keep application registrations private
2. Use separate applications for dev and prod
3. Regularly audit role assignments
4. Enable Azure Monitor alerts for failed deployments
5. Review GitHub Actions logs periodically
6. Use branch protection rules to prevent unintended deployments to prod

---

## Additional Resources

- [Azure Function App Documentation](https://learn.microsoft.com/azure/azure-functions/)
- [GitHub Actions OIDC with Azure](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure)
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Microsoft Entra ID Applications](https://learn.microsoft.com/en-us/azure/active-directory/develop/app-objects-and-service-principals)
