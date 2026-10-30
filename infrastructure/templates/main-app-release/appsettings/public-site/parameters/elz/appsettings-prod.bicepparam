// ELZ version of ../appsettings-prod.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../public-site-appsettings.bicep'
extends '../appsettings-prod.bicepparam'

param elz = true
