import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import {
  render as baseRender,
  RenderOptions,
  RenderResult,
} from '@testing-library/react';
import noop from 'lodash/noop';
import React, { FC, ReactElement } from 'react';

const DefaultWrapper: FC = ({ children }) => {
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
  ui: ReactElement,
  options?: Omit<RenderOptions, 'queries'>,
): RenderResult {
  return baseRender(ui, {
    wrapper: DefaultWrapper,
    ...options,
  });
}
