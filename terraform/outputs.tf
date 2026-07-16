output "api_fqdn" {
  description = "FQDN of the API container"
  value       = azurerm_container_group.api.fqdn
}

output "api_ip" {
  description = "Public IP of the API container"
  value       = azurerm_container_group.api.ip_address
}

output "registry_login_server" {
  description = "Container registry login server"
  value       = azurerm_container_registry.main.login_server
}

output "redis_hostname" {
  description = "Redis cache hostname"
  value       = azurerm_redis_cache.main.hostname
  sensitive   = true
}

output "redis_primary_key" {
  description = "Redis cache primary access key"
  value       = azurerm_redis_cache.main.primary_access_key
  sensitive   = true
}
