// These abbreviations are sourced generally from 
// https://github.com/Azure-Samples/todo-csharp-sql/blob/main/infra/abbreviations.json and
// https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-abbreviations.
//
// Non-standard abbreviations are highlighted with comments.

@export()
var abbreviations = {
  // TODO - remove the "-flexibleserver" suffix and change the suffix of our PSQL instance to "-01"
  dBforPostgreSQLServers: 'psql-flexibleserver'
}
