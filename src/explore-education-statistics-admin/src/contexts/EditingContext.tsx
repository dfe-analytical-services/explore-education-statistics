import React, {
  createContext,
  ReactNode,
  useContext,
  useMemo,
  useState,
} from 'react';
import noop from 'lodash/noop';

export interface UnsavedEdit {
  sectionId: string;
  blockIds: string[];
}

export type EditingMode = 'preview' | 'table-preview' | 'edit';

export interface EditingContextState {
  editingMode: EditingMode;
  setEditingMode: (mode: EditingMode) => void;
  unsavedEdits: UnsavedEdit[];
  setUnsavedEdits: (unsavedEdit: UnsavedEdit[]) => void;
}

export const EditingContext = createContext<EditingContextState>({
  editingMode: 'preview',
  setEditingMode: noop,
  unsavedEdits: [],
  setUnsavedEdits: noop,
});

export function useEditingContext() {
  return useContext(EditingContext);
}

export interface EditingContextProviderProps {
  children: ReactNode | ((state: EditingContextState) => ReactNode);
  initialEditingMode: EditingMode;
}

export const EditingContextProvider = ({
  children,
  initialEditingMode = 'preview',
}: EditingContextProviderProps) => {
  const [editingMode, setEditingMode] = useState<EditingMode>(
    initialEditingMode,
  );
  const [unsavedEdits, setUnsavedEdits] = useState<UnsavedEdit[]>([]);

  const state = useMemo<EditingContextState>(() => {
    return {
      editingMode,
      setEditingMode,
      unsavedEdits,
      setUnsavedEdits,
    };
  }, [editingMode, unsavedEdits]);

  return (
    <EditingContext.Provider value={state}>
      {typeof children === 'function' ? children(state) : children}
    </EditingContext.Provider>
  );
};
