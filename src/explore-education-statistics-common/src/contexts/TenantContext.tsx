import { Tenant } from '@common/tenants/types';
import React, { createContext, ReactNode, useContext } from 'react';

const TenantContext = createContext<Tenant | undefined>(undefined);

export interface TenantContextProviderProps {
  tenant: Tenant;
  children?: ReactNode;
}

export function TenantContextProvider({
  tenant,
  children,
}: TenantContextProviderProps) {
  return (
    <TenantContext.Provider value={tenant}>{children}</TenantContext.Provider>
  );
}

export function useTenant(): Tenant {
  const tenant = useContext(TenantContext);

  if (!tenant) {
    throw new Error('Must be used within a TenantContextProvider');
  }

  return tenant;
}
