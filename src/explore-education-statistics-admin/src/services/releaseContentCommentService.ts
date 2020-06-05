import { ExtendedComment } from '@admin/services/types/content';
import client from '@admin/services/utils/service';

export type AddExtendedComment = Pick<ExtendedComment, 'content'>;
export type UpdateExtendedComment = Pick<ExtendedComment, 'id' | 'content'>;

const releaseContentCommentService = {
  getContentSectionComments(
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
  ): Promise<ExtendedComment[]> {
    return client.get(
      `/release/${releaseId}/content/section/${sectionId}/block/${contentBlockId}/comments`,
    );
  },

  addContentSectionComment(
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
    comment: AddExtendedComment,
  ): Promise<ExtendedComment> {
    return client.post(
      `/release/${releaseId}/content/section/${sectionId}/block/${contentBlockId}/comments/add`,
      comment,
    );
  },

  updateContentSectionComment(
    comment: UpdateExtendedComment,
  ): Promise<ExtendedComment> {
    return client.put(`/comment/${comment.id}`, comment);
  },

  deleteContentSectionComment(commentId: string): Promise<void> {
    return client.delete(`/comment/${commentId}`);
  },
};

export default releaseContentCommentService;
