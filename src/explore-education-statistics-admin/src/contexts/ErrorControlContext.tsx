import { AxiosResponse } from 'axios';
import React, { createContext, ReactNode, useContext } from 'react';

export type ApiErrorHandler = (error: AxiosResponse) => void;

export interface ManualErrorHandler {
  forbidden: () => void;
}

export interface ErrorControlState {
  handleApiErrors: ApiErrorHandler;
  handleManualErrors: ManualErrorHandler;
}

export const ErrorControlContext = createContext<ErrorControlState>({
  handleApiErrors: _ => {},
  handleManualErrors: {
    forbidden: () => {},
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
