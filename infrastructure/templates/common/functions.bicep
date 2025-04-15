import { abbreviations } from 'abbreviations.bicep'

@export()
func buildFullyQualifiedTopicName(resourcePrefix string, topicName string) string =>
  '${resourcePrefix}-${abbreviations.eventGridTopics}-${topicName}'
