@export()
type PublicApiConfig = {

  @description('The public URL for the public API (excluding "https://").')
  publicUrl: string?
}

var defaultConfig = {}

@export()
func mergePublicApiConfig(overridden PublicApiConfig) PublicApiConfig =>
  union(defaultConfig, overridden)
