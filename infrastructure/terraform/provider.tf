terraform {
  required_version = ">= 1.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "<resource_group_name>"
    storage_account_name = "<storage_account_name>"
    container_name       = "<container_name>"
    key                  = "location-detector.tfstate"
    use_oidc             = true
  }
}

provider "azurerm" {
  features {}
}
