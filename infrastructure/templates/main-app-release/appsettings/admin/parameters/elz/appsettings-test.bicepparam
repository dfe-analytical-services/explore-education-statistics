// ELZ version of ../appsettings-test.bicepparam, used instead of it when the environment's variable group sets "elz" to "true".
// It inherits every value from that file. Override values here only where the ELZ subscription differs.
using '../../admin-appsettings.bicep'
extends '../appsettings-test.bicepparam'

param elz = true
