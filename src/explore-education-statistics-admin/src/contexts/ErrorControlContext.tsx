import { AxiosErrorHandler } from '@common/services/api/Client';
import noop from 'lodash/noop';
import React, { createContext, ReactNode, useContext } from 'react';

export interface ManualErrorHandler {
  forbidden: () => void;
}

export interface ErrorControlState {
  handleApiErrors: AxiosErrorHandler;
  handleManualErrors: ManualErrorHandler;
  /**
   * Run a {@param callback} in a context where
   * the context's error handling is disabled and
   * errors are automatically re-thrown.
   * This is useful in situations where we want to
   * handle errors in a different way to the default.
   */
  withoutErrorHandling: (callback: () => void) => Promise<void>;
}

export const ErrorControlContext = createContext<ErrorControlState>({
  handleApiErrors: noop,
  handleManualErrors: {
    forbidden: noop,
  },
  withoutErrorHandling: async callback => {
    await callback();
  },
});

interface ProviderProps {
  children: ReactNode;
  value: ErrorControlState;
}

export const ErrorControlContextProvider = ({
  children,
  value,
}: ProviderProps) => {
  return (
    <ErrorControlContext.Provider value={value}>
      {children}
    </ErrorControlContext.Provider>
  );
};

export function useErrorControl(): ErrorControlState {
  return useContext(ErrorControlContext);
}
