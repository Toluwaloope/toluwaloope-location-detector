# Toluwaloope Location Detector - Terraform Infrastructure

This directory contains Terraform Infrastructure as Code (IaC) for deploying the Toluwaloope Location Detector Azure Function App and its supporting resources in Azure.

## Overview

The Terraform stack provisions the following Azure resources:

- **Resource Group**: Per environment (dev/prod)
- **Storage Account**: For Function App runtime and Terraform backend state
- **App Service Plan**: Hosting plan for the Function App
- **Windows Function App**: .NET 8.0 isolated runtime
- **Application Insights**: Monitoring and diagnostics
- **Log Analytics Workspace**: Log aggregation and analysis
- **Diagnostic Settings**: Log streaming to Log Analytics

## File Structure

```
terraform/
├── provider.tf          # Provider configuration and backend setup (uses dev subscription for backend)
├── variables.tf         # Variable definitions
├── locals.tf            # Local values and naming conventions
├── main.tf              # Main resource definitions
├── outputs.tf           # Output values
├── dev.tfvars           # Development environment variables
└── prod.tfvars          # Production environment variables
```

## Prerequisites

1. **Terraform** >= 1.6.0 installed
2. **Azure CLI** installed and authenticated (OIDC preferred for CI/CD)
3. **Azure Subscriptions**: Dev (for backend and dev resources), Prod (for prod resources)
4. **Service Principal** with Contributor role (OIDC setup for GitHub Actions)

## Usage

### Local Deployment

1. Authenticate with Azure:

   ```sh
   az login
   az account set --subscription <SUBSCRIPTION_ID>
   ```

2. Initialize Terraform (always uses dev subscription for backend):

   ```sh
   export ARM_SUBSCRIPTION_ID=<AZURE_SUBSCRIPTION_ID>
   terraform init
   ```

3. Select or create workspace:

   ```sh
   terraform workspace select dev || terraform workspace new dev
   terraform workspace select prod || terraform workspace new prod
   ```

4. Plan and apply for the target environment:

   ```sh
   terraform plan -var-file=dev.tfvars
   terraform apply -var-file=dev.tfvars
   # or for prod
   terraform plan -var-file=prod.tfvars
   terraform apply -var-file=prod.tfvars
   ```

### CI/CD Deployment (GitHub Actions)

- The workflow `.github/workflows/deploy-function-app.yml` automates provisioning and deployment.
- The backend always uses the dev subscription for state, but resources are deployed to the selected environment's subscription.
- Secrets required: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_DEV_SUBSCRIPTION_ID`, `AZURE_PROD_SUBSCRIPTION_ID`

## Notes

- **Backend State**: The Terraform backend (state) is always stored in the dev subscription's storage account.
- **Workspaces**: Use `dev` and `prod` workspaces for environment isolation.
- **OIDC Authentication**: Recommended for CI/CD; see project `DEPLOYMENT_REQUIREMENTS.md` for setup.

## Outputs

- Resource group name
- Function app name
- Other resource identifiers for deployment and monitoring

## Troubleshooting

- Ensure correct subscription is set for resource deployment
- Ensure backend storage account exists in dev subscription
- Check workspace selection before plan/apply
- Review workflow logs for errors

## References

- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure Functions Documentation](https://learn.microsoft.com/en-us/azure/azure-functions/)
- [GitHub Actions OIDC with Azure](https://learn.microsoft.com/en-us/azure/active-directory/develop/workload-identity-federation-create-trust-github?tabs=azure-portal)
