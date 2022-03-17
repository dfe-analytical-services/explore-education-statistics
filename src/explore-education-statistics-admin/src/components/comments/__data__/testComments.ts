import { Comment, CommentUser } from '@admin/services/types/content';

export const testCommentUser1: CommentUser = {
  id: 'user-1',
  firstName: 'User',
  lastName: 'One',
  email: 'user.one@test.com',
};

export const testCommentUser2: CommentUser = {
  id: 'user-2',
  firstName: 'User',
  lastName: 'Two',
  email: 'user.two@test.com',
};

export const testComments: Comment[] = [
  {
    id: 'comment-1',
    content: 'Comment 1 content',
    createdBy: testCommentUser1,
    created: '2021-11-29T13:55',
    resolved: '2021-11-30T13:55',
    resolvedBy: testCommentUser2,
  },
  {
    id: 'comment-2',
    content: 'Comment 2 content',
    createdBy: testCommentUser1,
    created: '2021-11-30T10:00',
  },
  {
    id: 'comment-3',
    content: 'Comment 3 content',
    createdBy: testCommentUser2,
    created: '2021-11-30T13:55',
  },
  {
    id: 'comment-4',
    content: 'Comment 4 content',
    createdBy: testCommentUser2,
    created: '2021-11-30T13:55',
  },
  {
    id: 'comment-5',
    content: 'Comment 5 content',
    createdBy: testCommentUser2,
    created: '2021-11-30T13:55',
    resolved: '2021-11-3013:55',
    resolvedBy: testCommentUser2,
  },
];
