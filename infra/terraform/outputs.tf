output "resource_group_name" {
  value = azurerm_resource_group.example.id
}

output "workspace_name" {
  value = azurerm_ai_services.example.name
}