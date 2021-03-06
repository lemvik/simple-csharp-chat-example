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

  addon_profile {
    oms_agent {
      enabled = true
      log_analytics_workspace_id = azurerm_log_analytics_workspace.websocket_chat_logs.id
    }
  }

  tags = {
    Environment = "Production"
  }
}

resource "azurerm_role_assignment" "cluster_to_acr" {
  scope                = azurerm_container_registry.websocketchatacr.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_kubernetes_cluster.websocket_chat_cluster.kubelet_identity[0].object_id
}
