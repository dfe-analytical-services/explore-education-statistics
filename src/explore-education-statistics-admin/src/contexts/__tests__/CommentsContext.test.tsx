import {
  CommentsContextProvider,
  CommentsContextProviderProps,
  useCommentsContext,
} from '@admin/contexts/CommentsContext';
import { AddComment } from '@admin/services/releaseContentCommentService';
import { OmitStrict } from '@common/types';
import {
  testComments,
  testCommentUser1,
} from '@admin/components/comments/__data__/testComments';
import { Comment } from '@admin/services/types/content';
import React, { FC } from 'react';
import { act, renderHook } from '@testing-library/react-hooks';

describe('CommentsContext', () => {
  type Props = OmitStrict<CommentsContextProviderProps, 'children'>;

  const wrapper: FC<Props> = ({ ...props }) => (
    <CommentsContextProvider {...props}>
      {props.children}
    </CommentsContextProvider>
  );

  const blockId = 'block-id';

  const commentToAdd: AddComment = {
    content: 'Added Comment content',
  };
  const addedComment: Comment = {
    id: 'added-comment',
    content: 'Added Comment content',
    createdBy: testCommentUser1,
    created: '2021-11-30T10:00',
  };
  const commentToUpdate: Comment = {
    ...testComments[2],
    content: 'Updated content',
  };
  const updatedComment: Comment = {
    ...testComments[2],
    content: 'Updated content',
  };
  const resolvedComment: Comment = {
    ...testComments[1],
    resolved: '2021-11-30T13:55',
    resolvedBy: testCommentUser1,
  };
  const unresolvedComment: Comment = {
    ...testComments[0],
    resolved: undefined,
    resolvedBy: undefined,
  };

  const handleSaveComment = jest.fn();
  const handleDeletePendingComment = jest.fn();
  const handleSaveUpdatedComment = jest.fn();
  const handleUpdateUnresolvedComments = jest.fn();
  const handleUnsavedCommentDeletion = jest.fn();

  const initialProps: Props = {
    comments: testComments,
    onSaveComment: handleSaveComment,
    onDeleteComment: handleDeletePendingComment,
    onSaveUpdatedComment: handleSaveUpdatedComment,
    onUpdateUnresolvedComments: {
      current: handleUpdateUnresolvedComments,
    },
    onUpdateUnsavedCommentDeletions: {
      current: handleUnsavedCommentDeletion,
    },
  };

  test('addComment calls the save method and adds the new comment to the comments array', async () => {
    handleSaveComment.mockResolvedValue(addedComment);
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper,
      initialProps: {
        ...initialProps,
        comments: [],
      },
    });
    expect(result.current.comments).toEqual([]);

    await act(async () => {
      await result.current.addComment(blockId, commentToAdd);
    });
    expect(handleSaveComment).toHaveBeenCalledWith(commentToAdd);
    expect(handleUpdateUnresolvedComments).toHaveBeenCalledWith(
      blockId,
      addedComment.id,
    );
    expect(result.current.comments).toEqual([addedComment]);
    expect(result.current.currentInteraction).toEqual({
      type: 'adding',
      id: addedComment.id,
    });
  });

  test('removeComment removes comment from the comments array and adds it to pendingDeletions', async () => {
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper,
      initialProps,
    });
    expect(result.current.comments).toEqual(testComments);
    expect(result.current.pendingDeletions).toEqual([]);

    await act(async () => {
      await result.current.removeComment.current(blockId, testComments[1].id);
    });

    expect(result.current.comments).toEqual([
      testComments[0],
      testComments[2],
      testComments[3],
      testComments[4],
    ]);
    expect(result.current.pendingDeletions).toEqual([testComments[1]]);
    expect(result.current.currentInteraction).toEqual({
      type: 'removing',
      id: testComments[1].id,
    });
    expect(handleUnsavedCommentDeletion).toHaveBeenCalledWith(
      blockId,
      testComments[1].id,
    );
  });

  test('clearPendingDeletions calls the delete comment method for each pending and removes them from the pending array', async () => {
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper,
      initialProps: {
        ...initialProps,
        comments: [testComments[0], testComments[1]],
        pendingDeletions: [testComments[2], testComments[3], testComments[4]],
      },
    });
    expect(result.current.comments).toEqual([testComments[0], testComments[1]]);
    expect(result.current.pendingDeletions).toEqual([
      testComments[2],
      testComments[3],
      testComments[4],
    ]);
    await act(async () => {
      await result.current.clearPendingDeletions();
    });

    expect(handleDeletePendingComment).toHaveBeenCalledTimes(3);
    expect(handleDeletePendingComment).toHaveBeenCalledWith(testComments[2].id);
    expect(handleDeletePendingComment).toHaveBeenCalledWith(testComments[3].id);
    expect(handleDeletePendingComment).toHaveBeenCalledWith(testComments[4].id);

    expect(result.current.comments).toEqual([testComments[0], testComments[1]]);
    expect(result.current.pendingDeletions).toEqual([]);
  });

  test('resolveComment calls the update method and updates the comment in the array', async () => {
    handleSaveUpdatedComment.mockResolvedValue(resolvedComment);
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper,
      initialProps,
    });
    expect(result.current.comments[1].resolved).toBeUndefined();

    await act(async () => {
      await result.current.resolveComment.current(
        blockId,
        testComments[1].id,
        true,
      );
    });

    expect(result.current.comments[1]).toEqual(resolvedComment);
    expect(handleUpdateUnresolvedComments).toHaveBeenCalledWith(
      blockId,
      resolvedComment.id,
    );
    expect(result.current.currentInteraction).toEqual({
      type: 'resolving',
      id: resolvedComment.id,
    });
  });

  test('unresolveComment calls the update method and updates the comment in the array', async () => {
    handleSaveUpdatedComment.mockResolvedValue(unresolvedComment);
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper,
      initialProps,
    });
    expect(result.current.comments[0].resolved).toEqual('2021-11-30T13:55');

    await act(async () => {
      await result.current.unresolveComment.current(
        blockId,
        testComments[0].id,
        true,
      );
    });

    expect(result.current.comments[0]).toEqual(unresolvedComment);
    expect(handleUpdateUnresolvedComments).toHaveBeenCalledWith(
      blockId,
      unresolvedComment.id,
    );
    expect(result.current.currentInteraction).toEqual({
      type: 'unresolving',
      id: unresolvedComment.id,
    });
  });

  test('updateComment calls the update method and updates the comment in the array', async () => {
    handleSaveUpdatedComment.mockResolvedValue(updatedComment);
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper,
      initialProps,
    });
    expect(result.current.comments[2].content).toEqual(testComments[2].content);

    await act(async () => {
      await result.current.updateComment(commentToUpdate);
    });

    expect(handleSaveUpdatedComment).toHaveBeenCalledWith(commentToUpdate);
    expect(result.current.comments[2].content).toEqual(updatedComment.content);
  });

  test('reAddComment removes comment from the pendingDeletions array and adds it to comments', async () => {
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper,
      initialProps: {
        ...initialProps,
        comments: [testComments[0], testComments[1]],
        pendingDeletions: [testComments[2], testComments[3], testComments[4]],
      },
    });

    await act(async () => {
      await result.current.reAddComment.current(blockId, testComments[3].id);
    });

    expect(result.current.pendingDeletions).toEqual([
      testComments[2],
      testComments[4],
    ]);
    expect(result.current.comments).toEqual([
      testComments[0],
      testComments[1],
      testComments[3],
    ]);

    expect(handleUnsavedCommentDeletion).toHaveBeenLastCalledWith(
      blockId,
      testComments[3].id,
    );
  });
});
