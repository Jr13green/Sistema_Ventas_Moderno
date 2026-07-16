# ============================================================
# Terraform - Azure Infrastructure for Sistema Ventas
# ============================================================

terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
    }
  }

  backend "azurerm" {
    resource_group_name  = "tfstate-rg"
    storage_account_name = "tfstatesistemaventas"
    container_name       = "tfstate"
    key                  = "sistema-ventas.tfstate"
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = false
    }
  }
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "${var.prefix}-rg"
  location = var.location

  tags = local.common_tags
}

# Container Registry
resource "azurerm_container_registry" "main" {
  name                = "${replace(var.prefix, "-", "")}acr"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Basic"
  admin_enabled       = true

  tags = local.common_tags
}

# Key Vault for secrets
resource "azurerm_key_vault" "main" {
  name                = "${var.prefix}-kv"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"

  soft_delete_retention_days  = 7
  purge_protection_enabled    = false

  network_acls {
    bypass                     = "AzureServices"
    default_action             = "Allow"
  }

  tags = local.common_tags
}

# Container Instance for API
resource "azurerm_container_group" "api" {
  name                = "${var.prefix}-api"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  ip_address_type     = "Public"
  dns_name_label      = "${var.prefix}-api"
  os_type             = "Linux"

  container {
    name   = "api"
    image  = "${azurerm_container_registry.main.login_server}/sistema-ventas:latest"
    cpu    = "0.5"
    memory = "1.5"

    ports {
      port     = 5000
      protocol = "TCP"
    }

    environment_variables = {
      ASPNETCORE_ENVIRONMENT = "Production"
      ASPNETCORE_URLS        = "http://+:5000"
    }

    secure_environment_variables = {
      "Security__JwtSecretKey" = var.jwt_secret
    }
  }

  image_registry_credential {
    server   = azurerm_container_registry.main.login_server
    username = azurerm_container_registry.main.admin_username
    password = azurerm_container_registry.main.admin_password
  }

  tags = local.common_tags
}

# Redis Cache
resource "azurerm_redis_cache" "main" {
  name                = "${var.prefix}-redis"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  capacity            = 0
  family              = "C"
  sku_name            = "Basic"

  redis_configuration {}

  tags = local.common_tags
}

data "azurerm_client_config" "current" {}

locals {
  common_tags = {
    Project     = "SistemaVentas"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}
