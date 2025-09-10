import { EinSummary } from '@admin/services/educationInNumbersService';
import noop from 'lodash/noop';
import React, { createContext, ReactNode, useContext, useMemo } from 'react';

export interface EducationInNumbersPageContextState {
  educationInNumbersPage: EinSummary;
  educationInNumbersPageId: string;
  onEducationInNumbersPageChange: (
    nextEducationInNumbersPage: EinSummary,
  ) => void;
}

const EducationInNumbersPageContext = createContext<
  EducationInNumbersPageContextState | undefined
>(undefined);

interface EducationInNumbersPageContextProviderProps {
  children: ReactNode;
  educationInNumbersPage: EinSummary;
  onEducationInNumbersPageChange?: (
    nextEducationInNumbersPage: EinSummary,
  ) => void;
}

export const EducationInNumbersPageContextProvider = ({
  children,
  educationInNumbersPage,
  onEducationInNumbersPageChange = noop,
}: EducationInNumbersPageContextProviderProps) => {
  const value = useMemo<EducationInNumbersPageContextState>(() => {
    return {
      educationInNumbersPage,
      educationInNumbersPageId: educationInNumbersPage.id,
      onEducationInNumbersPageChange,
    };
  }, [onEducationInNumbersPageChange, educationInNumbersPage]);

  return (
    <EducationInNumbersPageContext.Provider value={value}>
      {children}
    </EducationInNumbersPageContext.Provider>
  );
};

export function useEducationInNumbersPageContext() {
  const context = useContext(EducationInNumbersPageContext);

  if (!context) {
    throw new Error(
      'useEducationInNumbersPageContext must be used within a EducationInNumbersPageContextProvider',
    );
  }
  return context;
}
