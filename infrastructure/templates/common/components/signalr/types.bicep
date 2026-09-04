@export()
type SignalRSku = {
  name: 'Free_F1' | 'Standard_S1'
  
  @description('The number of SignalR units.')
  capacity: int
}
