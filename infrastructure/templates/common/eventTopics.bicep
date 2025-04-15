@description('Event topics used for handling communication between different services in the system.')
@export()
var eventTopics = {
  publicationChanged: 'publication-changed'
  releaseChanged: 'release-changed'
  releaseVersionChanged: 'release-version-changed'
  themeChanged: 'theme-changed'
}
