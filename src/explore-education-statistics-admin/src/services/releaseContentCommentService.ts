import { ExtendedComment } from '@admin/services/types/content';
import client from '@admin/services/utils/service';

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
    comment: ExtendedComment,
  ): Promise<ExtendedComment> {
    return client.post(
      `/release/${releaseId}/content/section/${sectionId}/block/${contentBlockId}/comments/add`,
      comment,
    );
  },

  updateContentSectionComment(
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
    comment: ExtendedComment,
  ): Promise<ExtendedComment> {
    return client.put(
      `/release/${releaseId}/content/section/${sectionId}/block/${contentBlockId}/comment/${comment.id}`,
      comment,
    );
  },

  deleteContentSectionComment(
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
    commentId: string,
  ): Promise<void> {
    return client.delete(
      `/release/${releaseId}/content/section/${sectionId}/block/${contentBlockId}/comment/${commentId}`,
    );
  },
};

export default releaseContentCommentService;
