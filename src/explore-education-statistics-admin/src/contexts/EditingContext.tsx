import React, {
  createContext,
  ReactNode,
  useContext,
  useMemo,
  useState,
} from 'react';
import noop from 'lodash/noop';

export interface EditingContextState {
  isEditing: boolean;
  setEditing: (isEditing: boolean) => void;
}

export const EditingContext = createContext<EditingContextState>({
  isEditing: false,
  setEditing: noop,
});

export function useEditingContext() {
  return useContext(EditingContext);
}

interface EditingContextProviderProps {
  children: ReactNode | ((state: EditingContextState) => ReactNode);
  value?: {
    isEditing: boolean;
  };
}

export const EditingContextProvider = ({
  children,
  value = {
    isEditing: true,
  },
}: EditingContextProviderProps) => {
  const [isEditing, setEditing] = useState<boolean>(value.isEditing);

  const state = useMemo<EditingContextState>(() => {
    return {
      isEditing,
      setEditing,
    };
  }, [isEditing]);

  return (
    <EditingContext.Provider value={state}>
      {typeof children === 'function' ? children(state) : children}
    </EditingContext.Provider>
  );
};
