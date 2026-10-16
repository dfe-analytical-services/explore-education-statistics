@export()
type Tags = {
  @description('Tag Value - the cost centre identifying value provided by the Service Owner. Otherwise populate with Unknown.')
  CostCentre: string

  @description('Tag Value - the Department name tag value e.g. Data Directorate')
  Department: string

  @description('Tag Value - the solution name that the component is a part of e.g. EDAP, LDS, EES')
  Solution: string

  @description('Tag Value - the name of the Service or Application Owner in the SURNAME, Firstname format e.g. SINCLAIR, Paul / SHELBY, Laura')
  ServiceOwner: string

  @description('Tag Value - the name of the user who created these resources in the SURNAME, Firstname format e.g. RULER, Paul')
  CreatedBy: string

  @description('Tag Value - the URL of the repo containing the deployment script for the component')
  DeploymentRepoUrl: string
}
