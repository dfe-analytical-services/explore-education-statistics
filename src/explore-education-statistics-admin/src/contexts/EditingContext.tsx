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
  isTablePreview: boolean;
  setTablePreview: (isTablePreview: boolean) => void;
  unSavedEdits: UnSavedEdit[];
  setUnSavedEdits: (unSavedEdits: UnSavedEdit[]) => void;
}

export const EditingContext = createContext<EditingContextState>({
  isEditing: false,
  setEditing: noop,
  isTablePreview: false,
  setTablePreview: noop,
  unSavedEdits: [],
  setUnSavedEdits: noop,
});

export function useEditingContext() {
  return useContext(EditingContext);
}

export interface EditingContextProviderProps {
  children: ReactNode | ((state: EditingContextState) => ReactNode);
  value?: {
    isEditing: boolean;
    isTablePreview?: boolean;
    unSavedEdits?: UnSavedEdit[];
  };
}

export const EditingContextProvider = ({
  children,
  value = {
    isEditing: true,
    isTablePreview: false,
  },
}: EditingContextProviderProps) => {
  const [isEditing, setEditing] = useState<boolean>(value.isEditing);
  const [isTablePreview, setTablePreview] = useState<boolean>(false);
  const [unSavedEdits, setUnSavedEdits] = useState<UnSavedEdit[]>([]);

  const state = useMemo<EditingContextState>(() => {
    return {
      isEditing,
      setEditing,
      isTablePreview,
      setTablePreview,
      unSavedEdits,
      setUnSavedEdits,
    };
  }, [isEditing, isTablePreview, unSavedEdits]);

  return (
    <EditingContext.Provider value={state}>
      {typeof children === 'function' ? children(state) : children}
    </EditingContext.Provider>
  );
};
