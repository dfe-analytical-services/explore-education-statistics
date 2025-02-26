@export()
type IpRange = {
  name: string
  cidr: string
}

@export()
type FirewallRule = {
  name: string
  cidr: string
  priority: int
  tag: string
}

@export()
type StorageAccountRole = 'Storage Blob Data Contributor' | 'Storage Blob Data Owner' | 'Storage Blob Data Reader'
