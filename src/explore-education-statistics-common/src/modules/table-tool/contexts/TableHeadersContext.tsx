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
  moveControlsActive: string[];
  setActiveGroup: (listName?: string) => void;
  toggleExpandedList: (listName: string) => void;
  toggleGroupDraggingActive: (active: boolean) => void;
  toggleGroupDraggingEnabled: (enabled: boolean) => void;
  toggleMoveControlsActive: (groupId: string) => void;
}

const TableHeadersContext = createContext<TableHeadersContextState>({
  activeGroup: undefined,
  expandedLists: [],
  groupDraggingActive: false,
  groupDraggingEnabled: true,
  moveControlsActive: [],
  setActiveGroup: noop,
  toggleExpandedList: noop,
  toggleGroupDraggingActive: noop,
  toggleGroupDraggingEnabled: noop,
  toggleMoveControlsActive: noop,
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
  const [moveControlsActive, setMoveControlsActive] = useState<string[]>([]);

  const state = useMemo<TableHeadersContextState>(() => {
    const toggleExpandedList = (listName: string) =>
      setExpandedLists(current =>
        current.includes(listName)
          ? current.filter(currentName => currentName !== listName)
          : [...current, listName],
      );

    const toggleMoveControlsActive = (groupId: string) =>
      setMoveControlsActive(current =>
        current.includes(groupId)
          ? current.filter(currentId => currentId !== groupId)
          : [...current, groupId],
      );

    return {
      activeGroup,
      expandedLists,
      groupDraggingActive,
      groupDraggingEnabled,
      moveControlsActive,
      setActiveGroup,
      toggleExpandedList,
      toggleGroupDraggingActive,
      toggleGroupDraggingEnabled,
      toggleMoveControlsActive,
    };
  }, [
    activeGroup,
    expandedLists,
    groupDraggingActive,
    groupDraggingEnabled,
    moveControlsActive,
    setActiveGroup,
    toggleGroupDraggingActive,
    toggleGroupDraggingEnabled,
  ]);

  return (
    <TableHeadersContext value={state}>
      {typeof children === 'function' ? children(state) : children}
    </TableHeadersContext>
  );
};

export default function useTableHeadersContext() {
  const context = useContext(TableHeadersContext);

  if (!context) {
    throw new Error('Must have a parent TableHeadersContextProvider');
  }

  return context;
}
