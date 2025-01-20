@export()
@description('Returns a percentage of a number.')
func percentage(number int, percent int) int => int((number * percent) / 100)

@export()
@description('Converts gigabytes into bytes.')
func gbsToBytes(gbs int) int => gbs * 1073741824
