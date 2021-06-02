import React, {
  createContext,
  ReactNode,
  useContext,
  useMemo,
  useState,
} from 'react';
import noop from 'lodash/noop';

export interface UnSavedEdit {
  sectionId: string;
  blockIds: string[];
}

export interface EditingContextState {
  isEditing: boolean;
  setEditing: (isEditing: boolean) => void;
  unSavedEdits: UnSavedEdit[];
  setUnSavedEdits: (unSavedEdits: UnSavedEdit[]) => void;
}

export const EditingContext = createContext<EditingContextState>({
  isEditing: false,
  setEditing: noop,
  unSavedEdits: [],
  setUnSavedEdits: noop,
});

export function useEditingContext() {
  return useContext(EditingContext);
}

interface EditingContextProviderProps {
  children: ReactNode | ((state: EditingContextState) => ReactNode);
  value?: {
    isEditing: boolean;
    unSavedEdits?: UnSavedEdit[];
  };
}

export const EditingContextProvider = ({
  children,
  value = {
    isEditing: true,
  },
}: EditingContextProviderProps) => {
  const [isEditing, setEditing] = useState<boolean>(value.isEditing);
  const [unSavedEdits, setUnSavedEdits] = useState<UnSavedEdit[]>([]);

  const state = useMemo<EditingContextState>(() => {
    return {
      isEditing,
      setEditing,
      unSavedEdits,
      setUnSavedEdits,
    };
  }, [isEditing, unSavedEdits]);

  return (
    <EditingContext.Provider value={state}>
      {typeof children === 'function' ? children(state) : children}
    </EditingContext.Provider>
  );
};
