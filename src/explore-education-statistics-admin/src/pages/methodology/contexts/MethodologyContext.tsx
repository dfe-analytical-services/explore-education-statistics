import { BasicMethodologyVersion } from '@admin/services/methodologyService';
import noop from 'lodash/noop';
import React, { createContext, ReactNode, useContext, useMemo } from 'react';

export interface MethodologyContextState {
  methodology: BasicMethodologyVersion;
  methodologyId: string;
  onMethodologyChange: (nextMethodology: BasicMethodologyVersion) => void;
}

const MethodologyContext = createContext<MethodologyContextState | undefined>(
  undefined,
);

interface MethodologyContextProviderProps {
  children: ReactNode;
  methodology: BasicMethodologyVersion;
  onMethodologyChange?: (nextMethodology: BasicMethodologyVersion) => void;
}

export const MethodologyContextProvider = ({
  children,
  methodology,
  onMethodologyChange = noop,
}: MethodologyContextProviderProps) => {
  const value = useMemo<MethodologyContextState>(() => {
    return {
      methodology,
      methodologyId: methodology.id,
      onMethodologyChange,
    };
  }, [onMethodologyChange, methodology]);

  return (
    <MethodologyContext.Provider value={value}>
      {children}
    </MethodologyContext.Provider>
  );
};

export function useMethodologyContext() {
  const context = useContext(MethodologyContext);

  if (!context) {
    throw new Error(
      'useMethodologyContext must be used within a MethodologyContextProvider',
    );
  }
  return context;
}
