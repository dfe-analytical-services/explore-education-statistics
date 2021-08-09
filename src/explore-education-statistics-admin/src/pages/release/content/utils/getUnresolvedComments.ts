import { EditableRelease } from '@admin/services/releaseContentService';
import { Comment, EditableBlock } from '@admin/services/types/content';
import { ContentSection } from '@common/services/publicationService';

const getContentSectionComments = (
  contentSection: ContentSection<EditableBlock>,
): Comment[] => {
  if (contentSection.content?.length) {
    return contentSection.content.reduce<Comment[]>(
      (allCommentsForSection, content) => {
        content.comments.forEach(comment =>
          allCommentsForSection.push(comment),
        );
        return allCommentsForSection;
      },
      [],
    );
  }

  return [];
};

const getUnresolvedComments = (release: EditableRelease) =>
  [
    ...getContentSectionComments(release.summarySection),
    ...getContentSectionComments(release.keyStatisticsSection),
    ...release.content
      .filter(_ => _.content !== undefined)
      .reduce<Comment[]>(
        (allComments, contentSection) => [
          ...allComments,
          ...getContentSectionComments(contentSection),
        ],
        [],
      ),
  ].filter(comment => comment !== undefined && !comment.resolved);

export default getUnresolvedComments;
