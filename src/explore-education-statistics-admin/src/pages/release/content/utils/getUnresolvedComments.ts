import { EditableRelease } from '@admin/services/releaseContentService';
import { EditableBlock } from '@admin/services/types/content';
import { BlockCommentIds } from '@admin/contexts/EditingContext';

export const getTotalUnresolvedComments = (
  unresolvedComments: BlockCommentIds,
  unsavedCommentDeletions: BlockCommentIds,
): number => {
  const allUnresolved = Object.values(unresolvedComments).flat();
  const allUnsaved = Object.values(unsavedCommentDeletions).flat();
  return allUnresolved.filter(id => !allUnsaved.includes(id)).length;
};

const getContentSectionComments = (
  blocks: EditableBlock[],
): BlockCommentIds => {
  if (!blocks || !blocks.length) {
    return {};
  }

  return blocks.reduce<BlockCommentIds>((acc, block) => {
    if (!acc[block.id]) {
      acc[block.id] = [];
    }

    block.comments.forEach(comment => {
      if (!comment.resolved) {
        acc[block.id].push(comment.id);
      }
    });

    return acc;
  }, {});
};

const getUnresolvedComments = (release: EditableRelease): BlockCommentIds => {
  return {
    ...getContentSectionComments(release.summarySection.content),
    ...getContentSectionComments(release.headlinesSection.content),
    ...release.content
      .filter(section => section.content !== undefined)
      .reduce((acc, section) => {
        return { ...acc, ...getContentSectionComments(section.content) };
      }, {}),
  };
};

export default getUnresolvedComments;
