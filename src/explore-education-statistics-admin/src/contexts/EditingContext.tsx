import { getTotalUnresolvedComments } from '@admin/pages/release/content/utils/getUnresolvedComments';
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
import uniq from 'lodash/uniq';

export type EditingMode = 'preview' | 'table-preview' | 'edit';
export type BlockCommentIds = Dictionary<string[]>;

export interface EditingContextState {
  addUnsavedBlock: (blockId: string) => void;
  clearUnsavedCommentDeletions: (blockId: string) => void;
  editingMode: EditingMode;
  removeUnsavedBlock: (blockId: string) => void;
  setEditingMode: (mode: EditingMode) => void;
  totalUnresolvedComments: number;
  totalUnsavedBlocks: number;
  unresolvedComments: BlockCommentIds;
  unsavedCommentDeletions: BlockCommentIds;
  unsavedBlocks: string[];
  updateUnresolvedComments: MutableRefObject<
    (blockId: string, commentId: string) => void
  >;
  updateUnsavedCommentDeletions: MutableRefObject<
    (blockId: string, commentId: string) => void
  >;
}

export const EditingContext = createContext<EditingContextState>({
  addUnsavedBlock: noop,
  clearUnsavedCommentDeletions: noop,
  editingMode: 'preview',
  removeUnsavedBlock: noop,
  setEditingMode: noop,
  totalUnsavedBlocks: 0,
  totalUnresolvedComments: 0,
  unresolvedComments: {},
  unsavedCommentDeletions: {},
  unsavedBlocks: [],
  updateUnresolvedComments: { current: noop },
  updateUnsavedCommentDeletions: { current: noop },
});

export function useEditingContext() {
  return useContext(EditingContext);
}

export interface EditingContextProviderProps {
  children: ReactNode | ((state: EditingContextState) => ReactNode);
  editingMode: EditingMode;
  unresolvedComments?: BlockCommentIds;
  unsavedCommentDeletions?: BlockCommentIds;
  unsavedBlocks?: string[];
}

export const EditingContextProvider = ({
  children,
  editingMode: initialEditingMode,
  unresolvedComments: initialUnresolvedComments = {},
  unsavedCommentDeletions: initialUnsavedCommentDeletions = {},
  unsavedBlocks: initialUnsavedBlocks = [],
}: EditingContextProviderProps) => {
  const [editingMode, setEditingMode] =
    useState<EditingMode>(initialEditingMode);
  const [unresolvedComments, setUnresolvedComments] = useState<BlockCommentIds>(
    initialUnresolvedComments,
  );
  const [unsavedCommentDeletions, setUnsavedCommentDeletions] =
    useState<BlockCommentIds>(initialUnsavedCommentDeletions);
  const [unsavedBlocks, setUnsavedBlocks] =
    useState<string[]>(initialUnsavedBlocks);

  const blocksWithCommentDeletions = Object.entries(unsavedCommentDeletions)
    .filter(([, deletions]) => deletions.length)
    .map(([blockId]) => blockId);
  const totalUnsavedBlocks = uniq([
    ...unsavedBlocks,
    ...blocksWithCommentDeletions,
  ]).length;

  const totalUnresolvedComments = getTotalUnresolvedComments(
    unresolvedComments,
    unsavedCommentDeletions,
  );

  const updateUnresolvedComments: EditingContextState['updateUnresolvedComments'] =
    useCallbackRef(
      (blockId, commentId) => {
        if (commentId === 'commentPlaceholder') {
          return;
        }

        const updated = { ...unresolvedComments };
        if (!updated[blockId]) {
          updated[blockId] = [commentId];
          setUnresolvedComments(updated);
          return;
        }

        const index = updated[blockId].indexOf(commentId);
        if (index !== -1) {
          const nextBlockComments = [...updated[blockId]];
          nextBlockComments.splice(index, 1);
          updated[blockId] = nextBlockComments;
        } else {
          updated[blockId] = [...updated[blockId], commentId];
        }

        setUnresolvedComments(updated);
      },
      [unresolvedComments],
    );

  const updateUnsavedCommentDeletions: EditingContextState['updateUnsavedCommentDeletions'] =
    useCallbackRef(
      (blockId, commentId) => {
        const updated = { ...unsavedCommentDeletions };
        if (!updated[blockId]) {
          updated[blockId] = [commentId];
          setUnsavedCommentDeletions(updated);
          updateUnresolvedComments.current(blockId, commentId);
          return;
        }

        const index = updated[blockId].indexOf(commentId);
        if (index !== -1) {
          const nextBlockComments = [...updated[blockId]];
          nextBlockComments.splice(index, 1);
          updated[blockId] = nextBlockComments;
        } else {
          updated[blockId] = [...updated[blockId], commentId];
        }

        setUnsavedCommentDeletions(updated);
        updateUnresolvedComments.current(blockId, commentId);
      },
      [unsavedCommentDeletions],
    );

  const state = useMemo<EditingContextState>(() => {
    const addUnsavedBlock: EditingContextState['addUnsavedBlock'] = blockId => {
      if (!unsavedBlocks.includes(blockId)) {
        setUnsavedBlocks([...unsavedBlocks, blockId]);
      }
    };

    const clearUnsavedCommentDeletions: EditingContextState['clearUnsavedCommentDeletions'] =
      blockId => {
        const updated = { ...unsavedCommentDeletions };
        delete updated[blockId];
        setUnsavedCommentDeletions(updated);
      };

    const removeUnsavedBlock: EditingContextState['removeUnsavedBlock'] =
      blockId => {
        const updated = unsavedBlocks.filter(block => block !== blockId);
        setUnsavedBlocks(updated);
      };

    return {
      addUnsavedBlock,
      editingMode,
      removeUnsavedBlock,
      clearUnsavedCommentDeletions,
      setEditingMode,
      totalUnresolvedComments,
      totalUnsavedBlocks,
      unresolvedComments,
      unsavedBlocks,
      unsavedCommentDeletions,
      updateUnresolvedComments,
      updateUnsavedCommentDeletions,
    };
  }, [
    editingMode,
    totalUnsavedBlocks,
    totalUnresolvedComments,
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
