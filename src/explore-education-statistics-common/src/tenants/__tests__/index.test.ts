import { eesTenant } from '@common/tenants/eesTenant';
import { getTenant, isTenantKey } from '@common/tenants/index';

describe('tenants', () => {
  describe('isTenantKey', () => {
    test('returns true for a known tenant key', () => {
      expect(isTenantKey('ees')).toBe(true);
    });

    test('returns false for an unknown tenant key', () => {
      expect(isTenantKey('not-a-tenant')).toBe(false);
    });

    test('returns false for non-string values', () => {
      expect(isTenantKey(undefined)).toBe(false);
      expect(isTenantKey(null)).toBe(false);
      expect(isTenantKey(123)).toBe(false);
    });
  });

  describe('getTenant', () => {
    test('returns the EES tenant when no key is given', () => {
      expect(getTenant()).toBe(eesTenant);
    });

    test('returns the EES tenant for an empty key', () => {
      expect(getTenant('')).toBe(eesTenant);
    });

    test('returns the EES tenant for the `ees` key', () => {
      expect(getTenant('ees')).toBe(eesTenant);
    });

    test('throws for an unknown key', () => {
      expect(() => getTenant('not-a-tenant')).toThrow(
        "Unknown tenant key: 'not-a-tenant'",
      );
    });
  });
});
