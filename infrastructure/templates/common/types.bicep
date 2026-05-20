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
type PrivateDnsZone =
  | 'blobStorage'
  | 'eventGridTopic'
  | 'fileService'
  | 'postgres'
  | 'queue'
  | 'sites'
  | 'tableStorage'
  | 'custom'
