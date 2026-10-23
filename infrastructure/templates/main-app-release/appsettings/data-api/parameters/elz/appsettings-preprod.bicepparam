// ELZ version of ../appsettings-preprod.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../data-api-appsettings.bicep'
extends '../appsettings-preprod.bicepparam'

param elz = true
