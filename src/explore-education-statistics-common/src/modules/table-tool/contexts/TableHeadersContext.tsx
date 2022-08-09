import useToggle from '@common/hooks/useToggle';
import { noop } from 'lodash';
import React, {
  createContext,
  ReactNode,
  useContext,
  useMemo,
  useState,
} from 'react';

export interface TableHeadersContextState {
  activeList: string | undefined;
  defaultNumberOfItems: number;
  expandedLists: string[];
  groupDraggingActive: boolean;
  groupDraggingEnabled: boolean;
  setActiveList: (listName?: string) => void;
  toggleExpandedList: (listName: string) => void;
  toggleGroupDraggingActive: () => void;
  toggleGroupDraggingEnabled: () => void;
}

const TableHeadersContext = createContext<TableHeadersContextState>({
  activeList: undefined,
  defaultNumberOfItems: 2,
  expandedLists: [],
  groupDraggingActive: false,
  groupDraggingEnabled: true,
  setActiveList: noop,
  toggleExpandedList: noop,
  toggleGroupDraggingActive: noop,
  toggleGroupDraggingEnabled: noop,
});

export interface TableHeadersContextProviderProps {
  children: ReactNode | ((state: TableHeadersContextState) => ReactNode);
  activeList?: string;
  expandedLists?: string[];
  groupDraggingActive?: boolean;
  groupDraggingEnabled?: boolean;
}

export const TableHeadersContextProvider = ({
  children,
  activeList: initialActiveList,
  expandedLists: initialExpandedLists,
  groupDraggingActive: initialGroupDraggingActive,
  groupDraggingEnabled: initialGroupDraggingEnabled,
}: TableHeadersContextProviderProps) => {
  const defaultNumberOfItems = 2;
  const [activeList, setActiveList] = useState<string | undefined>(
    initialActiveList ?? undefined,
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
      activeList,
      defaultNumberOfItems,
      expandedLists,
      groupDraggingActive,
      groupDraggingEnabled,
      setActiveList,
      toggleExpandedList,
      toggleGroupDraggingActive,
      toggleGroupDraggingEnabled,
    };
  }, [
    activeList,
    defaultNumberOfItems,
    expandedLists,
    groupDraggingActive,
    groupDraggingEnabled,
    setActiveList,
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
