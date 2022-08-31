import useToggle from '@common/hooks/useToggle';
import noop from 'lodash/noop';
import React, {
  createContext,
  ReactNode,
  useContext,
  useMemo,
  useState,
} from 'react';

export interface TableHeadersContextState {
  activeGroup?: string;
  expandedLists: string[];
  groupDraggingActive: boolean;
  groupDraggingEnabled: boolean;
  setActiveGroup: (listName?: string) => void;
  toggleExpandedList: (listName: string) => void;
  toggleGroupDraggingActive: (active: boolean) => void;
  toggleGroupDraggingEnabled: (enabled: boolean) => void;
}

const TableHeadersContext = createContext<TableHeadersContextState>({
  activeGroup: undefined,
  expandedLists: [],
  groupDraggingActive: false,
  groupDraggingEnabled: true,
  setActiveGroup: noop,
  toggleExpandedList: noop,
  toggleGroupDraggingActive: noop,
  toggleGroupDraggingEnabled: noop,
});

export interface TableHeadersContextProviderProps {
  children: ReactNode | ((state: TableHeadersContextState) => ReactNode);
  activeGroup?: string;
  expandedLists?: string[];
  groupDraggingActive?: boolean;
  groupDraggingEnabled?: boolean;
}

export const TableHeadersContextProvider = ({
  children,
  activeGroup: initialActiveGroup,
  expandedLists: initialExpandedLists,
  groupDraggingActive: initialGroupDraggingActive,
  groupDraggingEnabled: initialGroupDraggingEnabled,
}: TableHeadersContextProviderProps) => {
  const [activeGroup, setActiveGroup] = useState<string | undefined>(
    initialActiveGroup ?? undefined,
  );
  const [expandedLists, setExpandedLists] = useState<string[]>(
    initialExpandedLists ?? [],
  );
  const [groupDraggingActive, toggleGroupDraggingActive] = useToggle(
    initialGroupDraggingActive ?? false,
  );
  const [groupDraggingEnabled, toggleGroupDraggingEnabled] = useToggle(
    initialGroupDraggingEnabled ?? true,
  );

  const state = useMemo<TableHeadersContextState>(() => {
    const toggleExpandedList = (listName: string) =>
      setExpandedLists(current =>
        current.includes(listName)
          ? current.filter(currentName => currentName !== listName)
          : [...current, listName],
      );

    return {
      activeGroup,
      expandedLists,
      groupDraggingActive,
      groupDraggingEnabled,
      setActiveGroup,
      toggleExpandedList,
      toggleGroupDraggingActive,
      toggleGroupDraggingEnabled,
    };
  }, [
    activeGroup,
    expandedLists,
    groupDraggingActive,
    groupDraggingEnabled,
    setActiveGroup,
    toggleGroupDraggingActive,
    toggleGroupDraggingEnabled,
  ]);

  return (
    <TableHeadersContext.Provider value={state}>
      {typeof children === 'function' ? children(state) : children}
    </TableHeadersContext.Provider>
  );
};

export default function useTableHeadersContext() {
  const context = useContext(TableHeadersContext);

  if (!context) {
    throw new Error('Must have a parent TableHeadersContextProvider');
  }

  return context;
}
