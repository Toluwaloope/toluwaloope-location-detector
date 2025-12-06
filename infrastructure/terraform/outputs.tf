output "resource_group_name" {
  description = "Name of the created resource group"
  value       = azurerm_resource_group.this.name
}

output "resource_group_id" {
  description = "ID of the created resource group"
  value       = azurerm_resource_group.this.id
}

output "function_app_id" {
  description = "ID of the Function App"
  value       = azurerm_windows_function_app.this.id
}

output "function_app_name" {
  description = "Name of the Function App"
  value       = azurerm_windows_function_app.this.name
}

output "function_app_default_hostname" {
  description = "Default hostname of the Function App"
  value       = azurerm_windows_function_app.this.default_hostname
}

output "storage_account_name" {
  description = "Name of the storage account"
  value       = azurerm_storage_account.this.name
}

output "storage_account_id" {
  description = "ID of the storage account"
  value       = azurerm_storage_account.this.id
}

output "app_insights_instrumentation_key" {
  description = "Instrumentation key for Application Insights"
  value       = azurerm_application_insights.this.instrumentation_key
  sensitive   = true
}

output "service_plan_id" {
  description = "ID of the App Service Plan"
  value       = azurerm_service_plan.this.id
}

output "function_app_identity_principal_id" {
  description = "Principal ID of the Function App's managed identity"
  value       = azurerm_windows_function_app.this.identity[0].principal_id
}
