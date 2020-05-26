import noop from 'lodash/noop';
import React, { createContext, ReactNode, useContext } from 'react';

export interface ManualErrorHandler {
  forbidden: () => void;
}

export interface ErrorControlState {
  handleApiErrors: (error: Error) => void;
  handleManualErrors: ManualErrorHandler;
}

export const ErrorControlContext = createContext<ErrorControlState>({
  handleApiErrors: noop,
  handleManualErrors: {
    forbidden: noop,
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
