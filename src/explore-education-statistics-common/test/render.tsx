import { TenantContextProvider } from '@common/contexts/TenantContext';
import eesTenant from '@common/tenants/eesTenant';
import { Tenant } from '@common/tenants/types';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import {
  render as baseRender,
  RenderOptions,
  RenderResult,
} from '@testing-library/react';
import userEvent, { UserEvent } from '@testing-library/user-event';
import React, { FC, ReactNode } from 'react';

export interface CustomRenderResult extends RenderResult {
  user: UserEvent;
}

const createDefaultWrapper = (
  tenant: Tenant,
  CustomWrapper?: RenderOptions['wrapper'],
): FC => {
  const DefaultWrapper: FC = ({ children }: { children?: ReactNode }) => {
    if (CustomWrapper) {
      // A caller-supplied wrapper replaces the default QueryClientProvider,
      // as before tenants were added, but is still given the tenant.
      return (
        <TenantContextProvider tenant={tenant}>
          <CustomWrapper>{children}</CustomWrapper>
        </TenantContextProvider>
      );
    }

    const queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          gcTime: Infinity,
          retry: false,
        },
      },
    });

    return (
      <TenantContextProvider tenant={tenant}>
        <QueryClientProvider client={queryClient}>
          {children}
        </QueryClientProvider>
      </TenantContextProvider>
    );
  };

  return DefaultWrapper;
};

/**
 * Custom test render function that is pre-configured with any contexts
 * that would otherwise create unnecessary boilerplate.
 *
 * The tenant context is always provided, using `tenant` (EES by default),
 * including when a custom `wrapper` is passed.
 */
export default function render(
  ui: ReactNode,
  options?: Omit<RenderOptions, 'queries'> & { tenant?: Tenant },
): CustomRenderResult {
  const { tenant = eesTenant, wrapper, ...restOptions } = options ?? {};

  return {
    ...(baseRender(ui, {
      ...restOptions,
      wrapper: createDefaultWrapper(tenant, wrapper),
    }) as RenderResult),
    user: userEvent.setup(),
  };
}
