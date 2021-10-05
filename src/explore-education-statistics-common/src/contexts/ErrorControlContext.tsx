import noop from 'lodash/noop';
import React, { createContext, ReactNode, useContext } from 'react';

export interface ErrorPages {
  forbidden: () => void;
}

export interface ErrorControlState {
  handleError: (error: unknown) => void;
  errorPages: ErrorPages;
}

export const ErrorControlContext = createContext<ErrorControlState>({
  handleError: noop,
  errorPages: {
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
