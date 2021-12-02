import { EditableRelease } from '@admin/services/releaseContentService';
import { EditableBlock } from '@admin/services/types/content';
import { BlockCommentIds } from '@admin/contexts/editing/EditingContext';

export const getTotalUnresolvedComments = (
  unresolvedComments: BlockCommentIds,
  unsavedCommentDeletions: BlockCommentIds,
) => {
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
  const unresolvedComments: BlockCommentIds = {};

  blocks.forEach(block => {
    if (!unresolvedComments[block.id]) {
      unresolvedComments[block.id] = [];
    }
    block.comments.forEach(comment => {
      if (!comment.resolved) {
        unresolvedComments[block.id].push(comment.id);
      }
    });
  });
  return unresolvedComments;
};

const getUnresolvedComments = (release: EditableRelease) => {
  return {
    ...getContentSectionComments(release.summarySection.content),
    ...getContentSectionComments(release.keyStatisticsSection.content),
    ...getContentSectionComments(release.headlinesSection.content),
    ...release.content
      .filter(_ => _.content !== undefined)
      .reduce((acc, section) => {
        getContentSectionComments(section.content);
        return { ...acc, ...getContentSectionComments(section.content) };
      }, {}),
  };
};

export default getUnresolvedComments;
