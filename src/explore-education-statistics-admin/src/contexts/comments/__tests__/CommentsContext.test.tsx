import {
  CommentsContextState,
  commentsReducer as originalCommentsReducer,
} from '@admin/contexts/comments/CommentsContext';
import { CommentsDispatchAction } from '@admin/contexts/comments/CommentsContextActionTypes';
import {
  testComments,
  testCommentUser1,
} from '@admin/components/comments/__data__/testComments';
import { Comment } from '@admin/services/types/content';
import { produce } from 'immer';

const commentsReducer = (
  initial: CommentsContextState,
  action: CommentsDispatchAction,
) => produce(initial, draft => originalCommentsReducer(draft, action));

describe('CommentsContext', () => {
  test('ADD_COMMENT', () => {
    const addedComment: Comment = {
      id: 'added-comment',
      content: 'Added Comment 2 content',
      createdBy: testCommentUser1,
      created: '2021-11-30T10:00',
    };
    expect(
      commentsReducer(
        {
          comments: testComments,
          pendingDeletions: [],
        },
        {
          type: 'ADD_COMMENT',
          payload: { comment: addedComment },
        },
      ),
    ).toEqual({
      comments: [...testComments, addedComment],
      pendingDeletions: [],
    });
  });

  test('DELETE_COMMENT', () => {
    expect(
      commentsReducer(
        {
          comments: testComments,
          pendingDeletions: [],
        },
        {
          type: 'DELETE_COMMENT',
          payload: { id: testComments[1].id },
        },
      ),
    ).toEqual({
      comments: [
        testComments[0],
        testComments[2],
        testComments[3],
        testComments[4],
      ],
      pendingDeletions: [testComments[1]],
    });
  });

  test('RESET_PENDING_DELETION', () => {
    expect(
      commentsReducer(
        {
          comments: [testComments[0]],
          pendingDeletions: [
            testComments[1],
            testComments[2],
            testComments[3],
            testComments[4],
          ],
        },
        {
          type: 'RESET_PENDING_DELETION',
        },
      ),
    ).toEqual({
      comments: [testComments[0]],
      pendingDeletions: [],
    });
  });

  test('SET_CURRENT_INTERACTION', () => {
    expect(
      commentsReducer(
        {
          comments: testComments,
          pendingDeletions: [],
        },
        {
          type: 'SET_CURRENT_INTERACTION',
          payload: { type: 'adding', id: 'adding-id' },
        },
      ),
    ).toEqual({
      comments: testComments,
      currentInteraction: { type: 'adding', id: 'adding-id' },
      pendingDeletions: [],
    });
  });

  test('UNDELETE_COMMENT', () => {
    expect(
      commentsReducer(
        {
          comments: [testComments[0]],
          pendingDeletions: [
            testComments[1],
            testComments[2],
            testComments[3],
            testComments[4],
          ],
        },
        {
          type: 'UNDELETE_COMMENT',
          payload: { id: testComments[2].id },
        },
      ),
    ).toEqual({
      comments: [testComments[0], testComments[2]],
      pendingDeletions: [testComments[1], testComments[3], testComments[4]],
    });
  });

  test('UPDATE_COMMENT', () => {
    const updatedComment: Comment = {
      ...testComments[1],
      content: 'I am updated',
    };
    expect(
      commentsReducer(
        {
          comments: testComments,
          pendingDeletions: [],
        },
        {
          type: 'UPDATE_COMMENT',
          payload: { comment: updatedComment },
        },
      ),
    ).toEqual({
      comments: [
        testComments[0],
        updatedComment,
        testComments[2],
        testComments[3],
        testComments[4],
      ],
      pendingDeletions: [],
    });
  });
});
