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

const createDefaultWrapper = (tenant: Tenant): FC => {
  const DefaultWrapper: FC = ({ children }: { children?: ReactNode }) => {
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
 */
export default function render(
  ui: ReactNode,
  options?: Omit<RenderOptions, 'queries'> & { tenant?: Tenant },
): CustomRenderResult {
  const { tenant = eesTenant, ...restOptions } = options ?? {};

  return {
    ...(baseRender(ui, {
      wrapper: createDefaultWrapper(tenant),
      ...restOptions,
    }) as RenderResult),
    user: userEvent.setup(),
  };
}
