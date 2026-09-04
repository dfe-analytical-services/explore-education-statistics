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
type DayOfWeek = 'Monday' | 'Tuesday' | 'Wednesday' | 'Thursday' | 'Friday' | 'Saturday' | 'Sunday'

@export()
type WeekOfMonth = 'First' | 'Second' | 'Third' | 'Fourth' | 'Last'

@export()
type MonthOfYear =
  | 'January'
  | 'February'
  | 'March'
  | 'April'
  | 'May'
  | 'June'
  | 'July'
  | 'August'
  | 'September'
  | 'October'
  | 'November'
  | 'December'

@export()
type ConnectionString = {
  name: string
  connectionString: string
  type: 
    | 'SQLAzure'
    | 'Custom'
}
