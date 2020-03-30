import React, { createContext, ReactNode, useContext } from 'react';

export interface EditingContextState {
  isEditing: boolean;
}

export const EditingContext = createContext<EditingContextState>({
  isEditing: false,
});

export function useEditingContext() {
  return useContext(EditingContext);
}

interface EditingContextProviderProps {
  children: ReactNode;
  value: EditingContextState;
}

export const EditingContextProvider = ({
  children,
  value,
}: EditingContextProviderProps) => {
  return (
    <EditingContext.Provider value={value}>{children}</EditingContext.Provider>
  );
};
