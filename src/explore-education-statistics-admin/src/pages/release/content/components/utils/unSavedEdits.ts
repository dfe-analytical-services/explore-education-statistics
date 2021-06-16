import { UnSavedEdit } from '@admin/contexts/EditingContext';

export const addUnSavedEdit = (
  edits: UnSavedEdit[],
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

export const removeUnSavedEdit = (
  edits: UnSavedEdit[],
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

export const getNumberOfUnSavedBlocks = (edits: UnSavedEdit[]) => {
  let count = 0;
  edits.forEach(edit => {
    count += edit.blockIds.length;
  });
  return count;
};
