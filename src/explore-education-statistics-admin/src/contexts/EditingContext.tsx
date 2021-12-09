import { Dictionary } from '@admin/types';
import useCallbackRef from '@common/hooks/useCallbackRef';
import React, {
  createContext,
  MutableRefObject,
  ReactNode,
  useContext,
  useMemo,
  useState,
} from 'react';
import noop from 'lodash/noop';

export type EditingMode = 'preview' | 'table-preview' | 'edit';
export type BlockCommentIds = Dictionary<string[]>;

export interface EditingContextState {
  editingMode: EditingMode;
  setEditingMode: (mode: EditingMode) => void;
  unresolvedComments: BlockCommentIds;
  unsavedCommentDeletions: BlockCommentIds;
  unsavedBlocks: string[];
  addUnsavedBlock: (blockId: string) => void;
  removeUnsavedBlock: (blockId: string) => void;
  removeUnsavedDeletionsForBlock: (blockId: string) => void;
  updateUnresolvedComments: MutableRefObject<
    (blockId: string, commentId: string) => void
  >;
  updateUnsavedCommentDeletions: MutableRefObject<
    (blockId: string, commentId: string) => void
  >;
}

export const EditingContext = createContext<EditingContextState>({
  editingMode: 'preview',
  setEditingMode: noop,
  unresolvedComments: {},
  unsavedCommentDeletions: {},
  unsavedBlocks: [],
  addUnsavedBlock: noop,
  removeUnsavedBlock: noop,
  removeUnsavedDeletionsForBlock: noop,
  updateUnresolvedComments: { current: noop },
  updateUnsavedCommentDeletions: { current: noop },
});

export function useEditingContext() {
  return useContext(EditingContext);
}

export interface EditingContextProviderProps {
  children: ReactNode | ((state: EditingContextState) => ReactNode);
  value: {
    editingMode: EditingMode;
    unresolvedComments?: BlockCommentIds;
    unsavedCommentDeletions?: BlockCommentIds;
    unsavedBlocks?: string[];
  };
}

export const EditingProvider = ({
  children,
  value,
}: EditingContextProviderProps) => {
  const [editingMode, setEditingMode] = useState<EditingMode>(
    value.editingMode,
  );
  const [unresolvedComments, setUnresolvedComments] = useState<BlockCommentIds>(
    value.unresolvedComments ?? {},
  );
  const [unsavedCommentDeletions, setUnsavedCommentDeletions] = useState<
    BlockCommentIds
  >(value.unsavedCommentDeletions ?? {});
  const [unsavedBlocks, setUnsavedBlocks] = useState<string[]>(
    value.unsavedBlocks ?? [],
  );

  const updateUnresolvedComments = useCallbackRef(
    (blockId: string, commentId: string) => {
      const updated = { ...unresolvedComments };
      if (!unresolvedComments[blockId]) {
        updated[blockId] = [commentId];
        setUnresolvedComments(updated);
        return;
      }
      const index = updated[blockId].indexOf(commentId);
      if (index !== -1) {
        updated[blockId].splice(index, 1);
      } else {
        updated[blockId].push(commentId);
      }
      setUnresolvedComments(updated);
    },
    [unresolvedComments],
  );

  const updateUnsavedCommentDeletions = useCallbackRef(
    (blockId: string, commentId: string) => {
      const updated = { ...unsavedCommentDeletions };
      if (!unsavedCommentDeletions[blockId]) {
        updated[blockId] = [commentId];
        setUnsavedCommentDeletions(updated);
        updateUnresolvedComments.current(blockId, commentId);
        return;
      }
      const index = updated[blockId].indexOf(commentId);
      if (index !== -1) {
        updated[blockId].splice(index, 1);
      } else {
        updated[blockId].push(commentId);
      }
      setUnsavedCommentDeletions(updated);
      updateUnresolvedComments.current(blockId, commentId);
    },
    [unsavedCommentDeletions],
  );

  const state = useMemo<EditingContextState>(() => {
    const addUnsavedBlock = (blockId: string) => {
      setUnsavedBlocks([...unsavedBlocks, blockId]);
    };

    const removeUnsavedDeletionsForBlock = (blockId: string) => {
      const updated = { ...unsavedCommentDeletions };
      delete updated[blockId];
      setUnsavedCommentDeletions(updated);
    };

    const removeUnsavedBlock = (blockId: string) => {
      const updated = unsavedBlocks.filter(block => block !== blockId);
      setUnsavedBlocks(updated);
    };

    return {
      addUnsavedBlock,
      editingMode,
      removeUnsavedBlock,
      removeUnsavedDeletionsForBlock,
      setEditingMode,
      unresolvedComments,
      unsavedBlocks,
      unsavedCommentDeletions,
      updateUnresolvedComments,
      updateUnsavedCommentDeletions,
    };
  }, [
    editingMode,
    unresolvedComments,
    unsavedBlocks,
    unsavedCommentDeletions,
    updateUnresolvedComments,
    updateUnsavedCommentDeletions,
  ]);

  return (
    <EditingContext.Provider value={state}>
      {typeof children === 'function' ? children(state) : children}
    </EditingContext.Provider>
  );
};
