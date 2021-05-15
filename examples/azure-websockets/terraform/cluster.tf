resource "azurerm_kubernetes_cluster" "websocket_chat_cluster" {
  name                = "websocket-chat-cluster"
  location            = azurerm_resource_group.websocket_chat_group.location
  resource_group_name = azurerm_resource_group.websocket_chat_group.name
  dns_prefix          = "lemvicwschat"

  default_node_pool {
    name       = "default"
    node_count = 2
    vm_size    = "Standard_B2s"
  }

  identity {
    type = "SystemAssigned"
  }

  tags = {
    Environment = "Production"
  }
}
