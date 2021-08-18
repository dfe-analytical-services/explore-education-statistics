import { BasicMethodology } from '@admin/services/methodologyService';
import noop from 'lodash/noop';
import React, { createContext, ReactNode, useContext, useMemo } from 'react';

export interface MethodologyContextState {
  methodology: BasicMethodology;
  methodologyId: string;
  onMethodologyChange: (nextMethodology: BasicMethodology) => void;
}

const MethodologyContext = createContext<MethodologyContextState | undefined>(
  undefined,
);

interface MethodologyContextProviderProps {
  children: ReactNode;
  methodology: BasicMethodology;
  onMethodologyChange?: (nextMethodology: BasicMethodology) => void;
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
