import {
  TenantContextProvider,
  useTenant,
} from '@common/contexts/TenantContext';
import { eesTenant } from '@common/tenants/eesTenant';
import { Tenant } from '@common/tenants/types';
import { renderHook } from '@testing-library/react';
import React, { FC, ReactNode } from 'react';

describe('useTenant', () => {
  test('throws when there is no provider', () => {
    // Suppress the error React logs when a hook throws during render.
    jest.spyOn(console, 'error').mockImplementation(jest.fn());

    expect(() => renderHook(() => useTenant())).toThrow(
      'Must be used within a TenantContextProvider',
    );
  });

  test('returns the tenant supplied by the provider', () => {
    const egsTenant = { ...eesTenant, key: 'egs' } as Tenant;

    const wrapper: FC<{ children: ReactNode }> = ({ children }) => (
      <TenantContextProvider tenant={egsTenant}>
        {children}
      </TenantContextProvider>
    );

    const { result } = renderHook(() => useTenant(), { wrapper });

    expect(result.current).toBe(egsTenant);
  });
});
