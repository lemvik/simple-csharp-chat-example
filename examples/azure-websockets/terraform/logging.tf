resource "azurerm_log_analytics_workspace" "websocket_chat_logs" {
  name                = "websocket-chat-logs"
  location            = azurerm_resource_group.websocket_chat_group.location
  resource_group_name = azurerm_resource_group.websocket_chat_group.name
  sku                 = "Free"
  retention_in_days   = 7
}
