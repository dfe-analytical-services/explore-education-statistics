@export()
type ConnectionString = {
  name: string
  connectionString: string
  type: 
    | 'SQLAzure'
    | 'Custom'
}
