import { eesTenant } from '@common/tenants/eesTenant';
import { Tenant } from '@common/tenants/types';

/**
 * Returns the tenant for the current environment, outside React.
 *
 * For now this always returns the EES tenant. BP2-T2 will change this to
 * read the `TENANT` env var / app setting.
 */
export default function getCurrentTenant(): Tenant {
  return eesTenant;
}
