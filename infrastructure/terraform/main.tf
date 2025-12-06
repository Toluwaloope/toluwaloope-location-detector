resource "azurerm_resource_group" "this" {
  name       = "${local.resource_name}-rg"
  location   = var.location
  tags       = local.common_tags
}

resource "azurerm_storage_account" "this" {
  name                     = replace("st${local.resource_name}", "-", "")
  resource_group_name      = azurerm_resource_group.this.name
  location                 = azurerm_resource_group.this.location
  account_tier             = var.storage_account_tier
  account_replication_type = var.storage_account_replication_type
  
  tags = local.common_tags

  depends_on = [azurerm_resource_group.this]
}

resource "azurerm_service_plan" "this" {
  name                = "${local.resource_name}-asp"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  os_type             = "Windows"
  sku_name            = var.app_service_plan_sku

  tags = local.common_tags

  depends_on = [azurerm_resource_group.this]
}

resource "azurerm_windows_function_app" "this" {
  name                = "${local.resource_name}-func"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  
  service_plan_id     = azurerm_service_plan.this.id
  storage_account_name       = azurerm_storage_account.this.name
  storage_account_access_key = azurerm_storage_account.this.primary_access_key

  functions_extension_version = "~4"
  
  app_settings = {
    FUNCTIONS_WORKER_RUNTIME       = "dotnet-isolated"
    APPINSIGHTS_INSTRUMENTATIONKEY = azurerm_application_insights.this.instrumentation_key
    ApplicationInsightsAgent_EXTENSION_VERSION = "~3"
  }

  site_config {
    minimum_tls_version = "1.2"
    http2_enabled       = true
    
    cors {
      allowed_origins = ["*"]
    }

    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
  }

  identity {
    type = "SystemAssigned"
  }

  tags = local.common_tags

  depends_on = [
    azurerm_service_plan.this,
    azurerm_storage_account.this,
    azurerm_application_insights.this
  ]
}

resource "azurerm_application_insights" "this" {
  name                = "${local.resource_name}-ai"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  application_type    = "web"
  
  tags = local.common_tags

  depends_on = [azurerm_resource_group.this]
}

resource "azurerm_log_analytics_workspace" "this" {
  name                = "${local.resource_name}-law"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  sku                 = "PerGB2018"
  retention_in_days   = 30

  tags = local.common_tags

  depends_on = [azurerm_resource_group.this]
}

resource "azurerm_monitor_diagnostic_setting" "function_app" {
  name                       = "${local.resource_name}-diag"
  target_resource_id         = azurerm_windows_function_app.this.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.this.id

  enabled_log {
    category = "FunctionAppLogs"
  }

  enabled_metric {
    category = "AllMetrics"
  }

  depends_on = [
    azurerm_windows_function_app.this,
    azurerm_log_analytics_workspace.this
  ]
}
