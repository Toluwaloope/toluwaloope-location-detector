# Toluwaloope Location Detector - Terraform Infrastructure

This directory contains Terraform Infrastructure as Code (IaC) for deploying the Toluwaloope Location Detector Azure Function App.

## Overview

The Terraform stack creates the following resources in Azure:

- **Resource Group**: Dedicated resource group per environment
- **Storage Account**: For Function App runtime
- **App Service Plan**: Windows-based hosting plan
- **Windows Function App**: .NET 8.0 isolated runtime
- **Application Insights**: Monitoring and diagnostics
- **Log Analytics Workspace**: Log aggregation and analysis
- **Diagnostic Settings**: Log streaming to Log Analytics

## File Structure

```
terraform/
├── provider.tf          # Provider configuration and backend setup
├── variables.tf         # Variable definitions
├── locals.tf           # Local values and naming conventions
├── main.tf             # Main resource definitions
├── outputs.tf          # Output values
├── dev.tfvars          # Development environment variables
└── prod.tfvars         # Production environment variables
```

## Prerequisites

1. **Terraform** >= 1.0 installed
2. **Azure CLI** installed and authenticated
3. **Azure Subscription** with appropriate permissions
4. **Service Principal** (for CI/CD deployments)

## Usage

### Local Deployment

#### Initialize Terraform

```bash
cd infrastructure/terraform

# Initialize Terraform working directory
terraform init
```

#### Plan Infrastructure

```bash
# For development environment
terraform plan -var-file="dev.tfvars" -out=plan.dev

# For production environment
terraform plan -var-file="prod.tfvars" -out=plan.prod
```

#### Apply Infrastructure

```bash
# For development environment
terraform apply plan.dev

# For production environment
terraform apply plan.prod
```

### CI/CD Deployment

The infrastructure is automatically deployed via GitHub Actions in the deployment workflow. Terraform is executed with appropriate credentials and variables based on the target environment.

## Variables

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `environment` | Environment name (dev or prod) | N/A | Yes |
| `location` | Azure region | `East US` | No |
| `project_name` | Project name | `toluwaloope-location-detector` | No |
| `app_service_plan_sku` | App Service Plan SKU | Dev: `Y1`, Prod: `B1` | No |
| `storage_account_tier` | Storage tier | `Standard` | No |
| `storage_account_replication_type` | Storage replication | `LRS` | No |
| `tags` | Resource tags | See tfvars | No |

## Outputs

After applying, the following outputs are available:

- `resource_group_name`: Name of the created resource group
- `function_app_name`: Name of the Function App
- `function_app_default_hostname`: Default hostname (URL)
- `storage_account_name`: Storage account name
- `app_insights_instrumentation_key`: Application Insights key
- `service_plan_id`: App Service Plan ID
- `function_app_identity_principal_id`: Managed Identity Principal ID

## State Management

By default, Terraform state is stored locally. For production deployments, use remote state:

1. Create an Azure Storage Account for Terraform state
2. Uncomment the `backend` block in `provider.tf`
3. Update the storage account name and container details

## Environment SKUs

- **Development**: `Y1` (Consumption plan - free tier)
- **Production**: `B1` (Basic plan - for guaranteed performance)

## Destroying Resources

**Warning**: This will delete all resources in the resource group.

```bash
# For development environment
terraform destroy -var-file="dev.tfvars"

# For production environment
terraform destroy -var-file="prod.tfvars"
```

## Troubleshooting

### Authentication Issues

Ensure you're authenticated with Azure:

```bash
az login
```

### State Lock

If Terraform state is locked, you may need to:

```bash
terraform force-unlock <LOCK_ID>
```

### Import Existing Resources

If resources already exist:

```bash
terraform import azurerm_resource_group.this /subscriptions/<subscription-id>/resourceGroups/<rg-name>
```

## Cost Considerations

- **Development**: Consumption plan (Y1) - Pay per execution
- **Production**: Basic plan (B1) - Approximately $10-15/month
- **Storage**: Minimal cost for function runtime storage
- **Application Insights**: Free tier (1 GB/month)

## Security Best Practices

1. Never commit sensitive values to version control
2. Use Azure Managed Identities for authentication
3. Enable diagnostic logging for audit trails
4. Restrict CORS origins in production
5. Use resource locks in production

## Contributing

When modifying Terraform configurations:

1. Run `terraform fmt` to format code
2. Run `terraform validate` to check syntax
3. Test in dev environment first
4. Review all resource changes before apply

## Support

For issues or questions, please refer to:

- [Azure Terraform Provider Documentation](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure Function App Documentation](https://learn.microsoft.com/en-us/azure/azure-functions/)
