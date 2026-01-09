import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import {
  render as baseRender,
  RenderOptions,
  RenderResult,
} from '@testing-library/react';
import userEvent, { UserEvent } from '@testing-library/user-event';
import noop from 'lodash/noop';
import React, { FC, ReactNode } from 'react';

export interface CustomRenderResult extends RenderResult {
  user: UserEvent;
}

const DefaultWrapper: FC = ({ children }: { children?: ReactNode }) => {
  const queryClient = new QueryClient({
    logger: {
      // eslint-disable-next-line no-console
      warn: console.warn,
      // eslint-disable-next-line no-console
      log: console.log,
      error: noop,
    },
    defaultOptions: {
      queries: {
        cacheTime: Infinity,
        retry: false,
      },
    },
  });

  return (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

/**
 * Custom test render function that is pre-configured with any contexts
 * that would otherwise create unnecessary boilerplate.
 */
export default function render(
  ui: ReactNode,
  options?: Omit<RenderOptions, 'queries'>,
): CustomRenderResult {
  return {
    ...(baseRender(ui, {
      wrapper: DefaultWrapper,
      ...options,
    }) as RenderResult),
    user: userEvent.setup(),
  };
}
