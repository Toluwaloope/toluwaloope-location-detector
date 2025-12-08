# Deployment Requirements

This document outlines all prerequisites and setup steps required to deploy the Toluwaloope Location Detector Azure Function App using OIDC authentication and GitHub Actions CI/CD.

## Prerequisites

### 1. Azure Subscriptions
- **Development Subscription**: Used for staging and backend Terraform state
- **Production Subscription**: Used for production deployments

### 2. GitHub Repository
- Actions must be enabled
- Repository can be public or private (OIDC works for both)

### 3. Local Tools
- **Git**
- **Azure CLI** (optional, used by GitHub Actions)
- **.NET 8.0 SDK**
- **Terraform CLI** (v1.6.0+)

---

## Azure Configuration

### 1. Create Microsoft Entra ID (Azure AD) Application

#### Step 1: Register Application in Each Subscription

For **both Development and Production** subscriptions:

1. Go to Azure Portal → Microsoft Entra ID → App registrations
2. Click **New registration**
3. Name: `toluwaloope-location-detector-app`
4. Supported account types: **Accounts in this organizational directory only**
5. Click **Register**

#### Step 2: Configure Federated Credentials for OIDC

For each registered application:
- Go to **Certificates & secrets → Federated credentials**
- Add a credential for your GitHub repository (see [Microsoft Docs](https://learn.microsoft.com/en-us/azure/active-directory/develop/workload-identity-federation-create-trust-github?tabs=azure-portal))

#### Step 3: Assign Roles
- Assign **Contributor** role to the app registration in both subscriptions

---

## GitHub Secrets

Set the following secrets in your repository:
- `AZURE_CLIENT_ID` (Service Principal Client ID)
- `AZURE_TENANT_ID` (Azure Tenant ID)
- `AZURE_DEV_SUBSCRIPTION_ID` (Dev Subscription ID)
- `AZURE_PROD_SUBSCRIPTION_ID` (Prod Subscription ID)

---

## Workflow Usage

The main workflow is `.github/workflows/deploy-function-app.yml`.

### Steps:
1. **Provision Infrastructure**
   - Uses Terraform to provision resources in the target subscription
   - Always uses the dev subscription for backend state
2. **Build and Publish**
   - Builds and publishes the Function App
3. **Deploy**
   - Deploys to either dev or prod environment
4. **Post-Deployment Tests**
   - Runs health checks on the API endpoint
5. **Notify**
   - Summarizes deployment status

### Triggering Deployment
- Go to GitHub Actions and run the workflow
- Select `dev` or `prod` environment

---

## API Endpoint
After deployment, call:
```
GET https://<function-app-name>.azurewebsites.net/api/GetCallerInformation
```

---

## Local Development
- Code: `src/Toluwaloope.LocationDetector.Api`
- Infra: `infrastructure/terraform`
- Build: `dotnet build src/Toluwaloope.LocationDetector.Api`
- Run: `func start`

---

## Troubleshooting
- Ensure all secrets are set
- Ensure app registration has Contributor role
- Check workflow logs for errors

---

## References
- [GitHub Actions OIDC with Azure](https://learn.microsoft.com/en-us/azure/active-directory/develop/workload-identity-federation-create-trust-github?tabs=azure-portal)
- [Azure Functions Documentation](https://learn.microsoft.com/en-us/azure/azure-functions/)
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
