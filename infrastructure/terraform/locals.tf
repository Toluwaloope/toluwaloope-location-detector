locals {
  environment_suffix = var.environment == "prod" ? "-prod" : "-dev"
  resource_name      = "${var.project_name}${local.environment_suffix}"
  
  common_tags = merge(
    var.tags,
    {
      Environment = var.environment
      ManagedBy   = "Terraform"
      CreatedAt   = timestamp()
    }
  )
}

