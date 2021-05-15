resource "azurerm_container_registry" "websocketchatacr" {
  name                = "websocketchatacr"
  resource_group_name = azurerm_resource_group.websocket_chat_group.name
  location            = azurerm_resource_group.websocket_chat_group.location
  sku                 = "Basic"
  admin_enabled       = false
}
