import { EditableRelease } from '@admin/services/releaseContentService';
import { Comment, EditableBlock } from '@admin/services/types/content';
import { ContentSection } from '@common/services/publicationService';
import { CommentsPendingDeletion } from '@admin/pages/release/content/contexts/ReleaseContentContext';

const getContentSectionComments = (
  contentSection: ContentSection<EditableBlock>,
  commentsPendingDeletion?: CommentsPendingDeletion,
): Comment[] => {
  if (contentSection.content?.length) {
    return contentSection.content.reduce<Comment[]>(
      (allCommentsForSection, content) => {
        const blockCommentsPendingDeletion =
          (commentsPendingDeletion &&
            commentsPendingDeletion[`block-${content.id}`]) ??
          [];
        content.comments.forEach(comment => {
          if (!blockCommentsPendingDeletion.includes(comment.id)) {
            allCommentsForSection.push(comment);
          }
        });
        return allCommentsForSection;
      },
      [],
    );
  }

  return [];
};

const getUnresolvedComments = (
  release: EditableRelease,
  commentsPendingDeletion?: CommentsPendingDeletion,
) =>
  [
    ...getContentSectionComments(
      release.summarySection,
      commentsPendingDeletion,
    ),
    ...getContentSectionComments(
      release.keyStatisticsSection,
      commentsPendingDeletion,
    ),
    ...getContentSectionComments(
      release.headlinesSection,
      commentsPendingDeletion,
    ),
    ...release.content
      .filter(_ => _.content !== undefined)
      .reduce<Comment[]>(
        (allComments, contentSection) => [
          ...allComments,
          ...getContentSectionComments(contentSection, commentsPendingDeletion),
        ],
        [],
      ),
  ].filter(comment => comment !== undefined && !comment.resolved);

export default getUnresolvedComments;
