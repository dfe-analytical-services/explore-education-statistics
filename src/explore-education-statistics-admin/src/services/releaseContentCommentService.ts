import { Comment } from '@admin/services/types/content';
import client from '@admin/services/utils/service';

export type CommentCreate = Pick<Comment, 'content'>;
export type CommentUpdate = Pick<Comment, 'id' | 'content'>;

const releaseContentCommentService = {
  getContentSectionComments(
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
  ): Promise<Comment[]> {
    return client.get(
      `/release/${releaseId}/content/section/${sectionId}/block/${contentBlockId}/comments`,
    );
  },

  addContentSectionComment(
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
    comment: CommentCreate,
  ): Promise<Comment> {
    return client.post(
      `/release/${releaseId}/content/section/${sectionId}/block/${contentBlockId}/comments/add`,
      comment,
    );
  },

  updateContentSectionComment(comment: Comment): Promise<Comment> {
    return client.put(`/comment/${comment.id}`, comment);
  },

  deleteContentSectionComment(commentId: string): Promise<void> {
    return client.delete(`/comment/${commentId}`);
  },
};

export default releaseContentCommentService;
