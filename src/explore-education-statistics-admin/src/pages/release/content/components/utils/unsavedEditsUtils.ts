import { UnsavedEdit } from '@admin/contexts/EditingContext';
import { CommentsPendingDeletion } from '@admin/pages/release/content/contexts/ReleaseContentContext';

export const addUnsavedEdit = (
  edits: UnsavedEdit[],
  sectionId: string,
  blockId: string,
) => {
  if (edits.findIndex(edit => edit.sectionId === sectionId) === -1) {
    return [
      ...edits,
      {
        sectionId,
        blockIds: [blockId],
      },
    ];
  }

  return edits.map(edit =>
    edit.sectionId === sectionId
      ? {
          ...edit,
          blockIds:
            edit.blockIds.indexOf(blockId) === -1
              ? [...edit.blockIds, blockId]
              : edit.blockIds,
        }
      : edit,
  );
};

export const removeUnsavedEdit = (
  edits: UnsavedEdit[],
  sectionId: string,
  blockId: string,
) => {
  return edits
    .map(edit =>
      edit.sectionId === sectionId
        ? { ...edit, blockIds: edit.blockIds.filter(id => id !== blockId) }
        : edit,
    )
    .filter(edit => edit.blockIds.length);
};

export const getNumberOfUnsavedBlocks = (
  edits: UnsavedEdit[],
  commentsPendingDeletion?: CommentsPendingDeletion,
) => {
  let count = 0;
  edits.forEach(edit => {
    count += edit.blockIds.length;
  });

  if (commentsPendingDeletion) {
    const blocksWithPendingComments = Object.keys(commentsPendingDeletion);
    if (blocksWithPendingComments.length) {
      blocksWithPendingComments.forEach(blockId => {
        if (commentsPendingDeletion[blockId].length) {
          count += 1;
        }
      });
    }
  }

  return count;
};
