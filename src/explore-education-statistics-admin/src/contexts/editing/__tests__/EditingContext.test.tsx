import {
  EditingContextState,
  editingReducer as originalEditingReducer,
} from '@admin/contexts/editing/EditingContext';
import { EditingDispatchAction } from '@admin/contexts/editing/EditingContextActionTypes';
import { produce } from 'immer';

const editingReducer = (
  initial: EditingContextState,
  action: EditingDispatchAction,
) => produce(initial, draft => originalEditingReducer(draft, action));

describe('EditingContext', () => {
  test('ADD_UNSAVED_BLOCK', () => {
    expect(
      editingReducer(
        {
          editingMode: 'edit',
          unresolvedComments: {},
          unsavedCommentDeletions: {},
          unsavedBlocks: [],
        },
        {
          type: 'ADD_UNSAVED_BLOCK',
          payload: { blockId: 'block-1' },
        },
      ),
    ).toEqual({
      editingMode: 'edit',
      unresolvedComments: {},
      unsavedCommentDeletions: {},
      unsavedBlocks: ['block-1'],
    });
  });

  test('REMOVE_UNSAVED_BLOCK', () => {
    expect(
      editingReducer(
        {
          editingMode: 'edit',
          unresolvedComments: {},
          unsavedCommentDeletions: {},
          unsavedBlocks: ['block-1', 'block-2', 'block-3'],
        },
        {
          type: 'REMOVE_UNSAVED_BLOCK',
          payload: { blockId: 'block-2' },
        },
      ),
    ).toEqual({
      editingMode: 'edit',
      unresolvedComments: {},
      unsavedCommentDeletions: {},
      unsavedBlocks: ['block-1', 'block-3'],
    });
  });

  test('SET_EDITING_MODE', () => {
    expect(
      editingReducer(
        {
          editingMode: 'edit',
          unresolvedComments: {},
          unsavedCommentDeletions: {},
          unsavedBlocks: [],
        },
        {
          type: 'SET_EDITING_MODE',
          payload: { editingMode: 'preview' },
        },
      ),
    ).toEqual({
      editingMode: 'preview',
      unresolvedComments: {},
      unsavedCommentDeletions: {},
      unsavedBlocks: [],
    });
  });

  test('UPDATE_UNRESOLVED_COMMENTS - stores unresolved comment by block', () => {
    expect(
      editingReducer(
        {
          editingMode: 'edit',
          unresolvedComments: {},
          unsavedCommentDeletions: {},
          unsavedBlocks: [],
        },
        {
          type: 'UPDATE_UNRESOLVED_COMMENTS',
          payload: { blockId: 'block-1', commentId: 'comment-1' },
        },
      ),
    ).toEqual({
      editingMode: 'edit',
      unresolvedComments: {
        'block-1': ['comment-1'],
      },
      unsavedCommentDeletions: {},
      unsavedBlocks: [],
    });
  });

  test('UPDATE_UNRESOLVED_COMMENTS - stores another unresolved comment on the same block', () => {
    expect(
      editingReducer(
        {
          editingMode: 'edit',
          unresolvedComments: { 'block-1': ['comment-1'] },
          unsavedCommentDeletions: {},
          unsavedBlocks: [],
        },
        {
          type: 'UPDATE_UNRESOLVED_COMMENTS',
          payload: { blockId: 'block-1', commentId: 'comment-2' },
        },
      ),
    ).toEqual({
      editingMode: 'edit',
      unresolvedComments: {
        'block-1': ['comment-1', 'comment-2'],
      },
      unsavedCommentDeletions: {},
      unsavedBlocks: [],
    });
  });

  test('UPDATE_UNRESOLVED_COMMENTS - stores another unresolved comment on a different block', () => {
    expect(
      editingReducer(
        {
          editingMode: 'edit',
          unresolvedComments: { 'block-1': ['comment-1', 'comment-2'] },
          unsavedCommentDeletions: {},
          unsavedBlocks: [],
        },
        {
          type: 'UPDATE_UNRESOLVED_COMMENTS',
          payload: { blockId: 'block-2', commentId: 'comment-3' },
        },
      ),
    ).toEqual({
      editingMode: 'edit',
      unresolvedComments: {
        'block-1': ['comment-1', 'comment-2'],
        'block-2': ['comment-3'],
      },
      unsavedCommentDeletions: {},
      unsavedBlocks: [],
    });
  });

  test('UPDATE_UNRESOLVED_COMMENTS - removes unresolved comment', () => {
    expect(
      editingReducer(
        {
          editingMode: 'edit',
          unresolvedComments: { 'block-1': ['comment-1', 'comment-2'] },
          unsavedCommentDeletions: {},
          unsavedBlocks: [],
        },
        {
          type: 'UPDATE_UNRESOLVED_COMMENTS',
          payload: { blockId: 'block-1', commentId: 'comment-1' },
        },
      ),
    ).toEqual({
      editingMode: 'edit',
      unresolvedComments: {
        'block-1': ['comment-2'],
      },
      unsavedCommentDeletions: {},
      unsavedBlocks: [],
    });
  });

  //
  test('UPDATE_UNSAVED_COMMENT_DELETIONS - stores unsaved comment deletion by block', () => {
    expect(
      editingReducer(
        {
          editingMode: 'edit',
          unresolvedComments: {},
          unsavedCommentDeletions: {},
          unsavedBlocks: [],
        },
        {
          type: 'UPDATE_UNSAVED_COMMENT_DELETIONS',
          payload: { blockId: 'block-1', commentId: 'comment-1' },
        },
      ),
    ).toEqual({
      editingMode: 'edit',
      unsavedCommentDeletions: {
        'block-1': ['comment-1'],
      },
      unresolvedComments: {},
      unsavedBlocks: [],
    });
  });

  test('UPDATE_UNSAVED_COMMENT_DELETIONS - stores another unsaved comment deletion on the same block', () => {
    expect(
      editingReducer(
        {
          editingMode: 'edit',
          unsavedCommentDeletions: { 'block-1': ['comment-1'] },
          unresolvedComments: {},
          unsavedBlocks: [],
        },
        {
          type: 'UPDATE_UNSAVED_COMMENT_DELETIONS',
          payload: { blockId: 'block-1', commentId: 'comment-2' },
        },
      ),
    ).toEqual({
      editingMode: 'edit',
      unsavedCommentDeletions: {
        'block-1': ['comment-1', 'comment-2'],
      },
      unresolvedComments: {},
      unsavedBlocks: [],
    });
  });

  test('UPDATE_UNSAVED_COMMENT_DELETIONS - stores another unsaved comment deletion on a different block', () => {
    expect(
      editingReducer(
        {
          editingMode: 'edit',
          unsavedCommentDeletions: { 'block-1': ['comment-1', 'comment-2'] },
          unresolvedComments: {},
          unsavedBlocks: [],
        },
        {
          type: 'UPDATE_UNSAVED_COMMENT_DELETIONS',
          payload: { blockId: 'block-2', commentId: 'comment-3' },
        },
      ),
    ).toEqual({
      editingMode: 'edit',
      unsavedCommentDeletions: {
        'block-1': ['comment-1', 'comment-2'],
        'block-2': ['comment-3'],
      },
      unresolvedComments: {},
      unsavedBlocks: [],
    });
  });

  test('UPDATE_UNSAVED_COMMENT_DELETIONS - removes unsaved comment deletion', () => {
    expect(
      editingReducer(
        {
          editingMode: 'edit',
          unsavedCommentDeletions: { 'block-1': ['comment-1', 'comment-2'] },
          unresolvedComments: {},
          unsavedBlocks: [],
        },
        {
          type: 'UPDATE_UNSAVED_COMMENT_DELETIONS',
          payload: { blockId: 'block-1', commentId: 'comment-1' },
        },
      ),
    ).toEqual({
      editingMode: 'edit',
      unsavedCommentDeletions: {
        'block-1': ['comment-2'],
      },
      unresolvedComments: {},
      unsavedBlocks: [],
    });
  });
});
