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
  reverseOrderIds: string[];
  isReverse: boolean;
  setActiveGroup: (listName?: string) => void;
  setIsReverse: (reverse: boolean) => void;
  toggleExpandedList: (listName: string) => void;
  toggleGroupDraggingActive: (active: boolean) => void;
  toggleGroupDraggingEnabled: (enabled: boolean) => void;
  toggleMoveControlsActive: (groupId: string) => void;
  toggleReverseOrder: (reverse: string) => void;
}

const TableHeadersContext = createContext<TableHeadersContextState>({
  activeGroup: undefined,
  expandedLists: [],
  groupDraggingActive: false,
  groupDraggingEnabled: true,
  moveControlsActive: [],
  reverseOrderIds: [],
  isReverse: false,
  setActiveGroup: noop,
  setIsReverse: noop,
  toggleExpandedList: noop,
  toggleGroupDraggingActive: noop,
  toggleGroupDraggingEnabled: noop,
  toggleMoveControlsActive: noop,
  toggleReverseOrder: noop,
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
  const [reverseOrderIds, setReverseOrderIds] = useState<string[]>([]);
  const [isReverse, setIsReverse] = useState(false);

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

    const toggleReverseOrder = (groupId: string) => {
      setIsReverse(true);
      setReverseOrderIds(current =>
        current.includes(groupId) && current.length > 1
          ? current.filter(currentId => currentId !== groupId)
          : [groupId],
      );
    };

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
      reverseOrderIds,
      isReverse,
      setIsReverse,
      toggleReverseOrder,
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
    reverseOrderIds,
    isReverse,
    setIsReverse,
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
