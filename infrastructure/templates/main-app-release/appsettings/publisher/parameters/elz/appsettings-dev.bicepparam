// ELZ version of ../appsettings-dev.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../publisher-appsettings.bicep'
extends '../appsettings-dev.bicepparam'

param elz = true
