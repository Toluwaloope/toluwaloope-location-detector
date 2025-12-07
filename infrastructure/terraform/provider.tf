terraform {
  required_version = ">= 1.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "terraform-state-rg"
    storage_account_name = "toluwaloopetfstate"
    container_name       = "tfstate"
    key                  = "location-detector.tfstate"
    use_oidc             = true
  }
}

provider "azurerm" {
  features {}
}
