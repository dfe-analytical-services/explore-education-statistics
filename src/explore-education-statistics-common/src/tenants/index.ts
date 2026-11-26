import { eesTenant } from '@common/tenants/eesTenant';
import { Tenant, TenantKey } from '@common/tenants/types';

export const defaultTenantKey: TenantKey = 'ees';

export const tenants: Partial<Record<TenantKey, Tenant>> = {
  ees: eesTenant,
};

export function isTenantKey(value: unknown): value is TenantKey {
  return typeof value === 'string' && Object.hasOwn(tenants, value);
}

export function getTenant(key?: string): Tenant {
  if (!key) {
    return tenants[defaultTenantKey] as Tenant;
  }

  if (!isTenantKey(key)) {
    throw new Error(`Unknown tenant key: '${key}'`);
  }

  return tenants[key] as Tenant;
}

export { eesTenant };
export * from '@common/tenants/types';
