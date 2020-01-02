/* eslint-disable no-bitwise */
import { ExtendedComment } from '@admin/services/publicationService';
import { Dictionary } from '@common/types';
import * as realService from './service';

const key = (
  releaseId: string,
  sectionId: string,
  contentBlockId: string,
  commentId: string,
) => {
  return `${releaseId}_${sectionId}_${contentBlockId}_${commentId}`;
};

const genId = () => {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
    const r = Math.random() * 16;
    const v = c === 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
};

const commentsCache: Dictionary<ExtendedComment> = {};

const service = {
  ...realService.default,

  getContentSectionComments(
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
  ): Promise<ExtendedComment[]> {
    const blockKey = key(releaseId, sectionId, contentBlockId, '');

    return Promise.resolve(
      Object.entries(commentsCache)
        .filter(([k]) => k.indexOf(blockKey) === 0)
        .map(([, v]) => v),
    );
  },

  addContentSectionComment(
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
    comment: ExtendedComment,
  ): Promise<ExtendedComment> {
    const commentId = genId();
    const blockKey = key(releaseId, sectionId, contentBlockId, commentId);

    const newComment = {
      ...comment,
      id: commentId,
    };

    commentsCache[blockKey] = newComment;

    return Promise.resolve(newComment);
  },

  updateContentSectionComment(
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
    comment: ExtendedComment,
  ): Promise<ExtendedComment> {
    const blockKey = key(releaseId, sectionId, contentBlockId, comment.id);

    commentsCache[blockKey] = { ...comment };
    return Promise.resolve({ ...commentsCache[blockKey] });
  },

  deleteContentSectionComment(
    releaseId: string,
    sectionId: string,
    contentBlockId: string,
    commentId: string,
  ): Promise<void> {
    const blockKey = key(releaseId, sectionId, contentBlockId, commentId);

    delete commentsCache[blockKey];

    return Promise.resolve();
  },
};

export default service;
