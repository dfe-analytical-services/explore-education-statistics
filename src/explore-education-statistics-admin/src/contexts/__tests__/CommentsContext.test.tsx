import {
  CommentsContextProvider,
  CommentsContextProviderProps,
  useCommentsContext,
} from '@admin/contexts/CommentsContext';
import { CommentCreate } from '@admin/services/releaseContentCommentService';
import { OmitStrict } from '@common/types';
import {
  testComments,
  testCommentUser1,
} from '@admin/components/comments/__data__/testComments';
import { Comment } from '@admin/services/types/content';
import React, { ReactNode } from 'react';
import { act, renderHook } from '@testing-library/react';

describe('CommentsContext', () => {
  interface Props extends OmitStrict<CommentsContextProviderProps, 'children'> {
    children?: ReactNode;
  }

  const commentToAdd: CommentCreate = {
    content: 'Added Comment content',
  };
  const addedComment: Comment = {
    id: 'added-comment',
    content: 'Added Comment content',
    createdBy: testCommentUser1,
    created: '2021-11-30T10:00',
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

  const handleCreate = jest.fn();
  const handleDelete = jest.fn();
  const handleUpdate = jest.fn();
  const handlePendingDelete = jest.fn();
  const handlePendingDeleteUndo = jest.fn();

  const initialProps: Props = {
    comments: testComments,
    onCreate: handleCreate,
    onDelete: handleDelete,
    onUpdate: handleUpdate,
    onPendingDelete: handlePendingDelete,
    onPendingDeleteUndo: handlePendingDeleteUndo,
  };

  // initialProps are not passed to the wrapper component, so we can't test updating
  // them using rerender.
  // https://testing-library.com/docs/react-testing-library/api/#renderhook-options-initialprops
  test.skip('updating `comments` prop updates the returned comments', async () => {
    const { result, rerender } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper({ ...initialProps, comments: [] }),
    });

    expect(result.current.comments).toEqual([]);

    rerender({
      ...initialProps,
      comments: testComments,
    });

    expect(result.current.comments).toEqual(testComments);
  });

  test('filters out returned `comments` that are pending deletion', () => {
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper({
        ...initialProps,
        pendingDeletions: [testComments[1], testComments[2]],
      }),
    });

    expect(result.current.comments).toEqual([
      testComments[0],
      testComments[3],
      testComments[4],
    ]);
  });

  test('calling `addComment` updates state correctly', async () => {
    handleCreate.mockResolvedValue(addedComment);

    const { result } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper({ ...initialProps, comments: [] }),
    });

    expect(result.current.currentInteraction).toBeUndefined();

    await act(async () => {
      await result.current.addComment(commentToAdd);
    });

    expect(result.current.currentInteraction).toEqual({
      type: 'adding',
      id: addedComment.id,
    });
  });

  test('calling `addComment` calls `onSaveComment` handler', async () => {
    handleCreate.mockResolvedValue(addedComment);

    const { result } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper({ ...initialProps, comments: [] }),
    });

    expect(handleCreate).not.toHaveBeenCalled();

    await act(async () => {
      await result.current.addComment(commentToAdd);
    });

    expect(handleCreate).toHaveBeenCalledWith(commentToAdd);
  });

  test('calling `removeComment` updates state correctly', async () => {
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper(initialProps),
    });

    expect(result.current.pendingDeletions).toEqual([]);

    await act(async () => {
      await result.current.removeComment.current(testComments[1].id);
    });

    expect(result.current.pendingDeletions).toEqual([testComments[1]]);
    expect(result.current.currentInteraction).toEqual({
      type: 'removing',
      id: testComments[1].id,
    });
  });

  test('calling `removeComment` calls the `onPendingDelete` handler', async () => {
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper(initialProps),
    });

    expect(handlePendingDelete).not.toHaveBeenCalled();

    await act(async () => {
      await result.current.removeComment.current(testComments[1].id);
    });

    expect(handlePendingDelete).toHaveBeenCalledWith(testComments[1].id);
  });

  test('calling `clearPendingDeletions` updates state correctly', async () => {
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper({
        ...initialProps,
        comments: [testComments[0], testComments[1]],
        pendingDeletions: [testComments[2], testComments[3], testComments[4]],
      }),
    });

    expect(result.current.pendingDeletions).toEqual([
      testComments[2],
      testComments[3],
      testComments[4],
    ]);

    await act(async () => {
      await result.current.clearPendingDeletions();
    });

    expect(result.current.pendingDeletions).toEqual([]);
  });

  test('calling `clearPendingDeletions` calls the `onDelete` handler for each pending deletion', async () => {
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper({
        ...initialProps,
        comments: [testComments[0], testComments[1]],
        pendingDeletions: [testComments[2], testComments[3], testComments[4]],
      }),
    });

    expect(handleDelete).not.toHaveBeenCalled();

    await act(async () => {
      await result.current.clearPendingDeletions();
    });

    expect(handleDelete).toHaveBeenCalledTimes(3);
    expect(handleDelete).toHaveBeenCalledWith(testComments[2].id);
    expect(handleDelete).toHaveBeenCalledWith(testComments[3].id);
    expect(handleDelete).toHaveBeenCalledWith(testComments[4].id);
  });

  test('calling `resolveComment` updates state correctly', async () => {
    handleUpdate.mockResolvedValue(resolvedComment);

    const { result } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper(initialProps),
    });

    expect(result.current.currentInteraction).toBeUndefined();

    await act(async () => {
      await result.current.resolveComment.current(testComments[1].id, true);
    });

    expect(result.current.currentInteraction).toEqual({
      type: 'resolving',
      id: resolvedComment.id,
    });
  });

  test('calling `resolveComment` calls the `onUpdate` handler', async () => {
    handleUpdate.mockResolvedValue(resolvedComment);

    const { result } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper(initialProps),
    });

    expect(handleUpdate).not.toHaveBeenCalled();

    await act(async () => {
      await result.current.resolveComment.current(testComments[1].id, true);
    });

    expect(handleUpdate).toHaveBeenCalledWith({
      ...testComments[1],
      setResolved: true,
    });
  });

  test('calling `unresolveComment` updates state correctly', async () => {
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper(initialProps),
    });

    expect(result.current.currentInteraction).toBeUndefined();

    await act(async () => {
      await result.current.unresolveComment.current(testComments[0].id, true);
    });

    expect(result.current.currentInteraction).toEqual({
      type: 'unresolving',
      id: unresolvedComment.id,
    });
  });

  test('calling `unresolveComment` calls the `onUpdate` handler', async () => {
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper(initialProps),
    });

    expect(handleUpdate).not.toHaveBeenCalled();

    await act(async () => {
      await result.current.unresolveComment.current(testComments[0].id, true);
    });

    expect(handleUpdate).toHaveBeenCalledWith({
      ...testComments[0],
      setResolved: false,
    });
  });

  test('calling `updateComment` calls `onUpdate` handler', async () => {
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper(initialProps),
    });

    const updatedComment: Comment = {
      ...testComments[2],
      content: 'Updated content',
    };

    expect(handleUpdate).not.toHaveBeenCalled();

    await act(async () => {
      await result.current.updateComment(updatedComment);
    });

    expect(handleUpdate).toHaveBeenCalledWith(updatedComment);
  });

  test('calling `reAddComment` updates state correctly', async () => {
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper({
        ...initialProps,
        comments: [testComments[0], testComments[1]],
        pendingDeletions: [testComments[2], testComments[3], testComments[4]],
      }),
    });

    await act(async () => {
      await result.current.reAddComment.current(testComments[3].id);
    });

    expect(result.current.pendingDeletions).toEqual([
      testComments[2],
      testComments[4],
    ]);
  });

  test('calling `reAddComment` calls `onPendingDeleteUndo` handler', async () => {
    const { result } = renderHook(() => useCommentsContext(), {
      wrapper: createWrapper({
        ...initialProps,
        comments: [testComments[0], testComments[1]],
        pendingDeletions: [testComments[2], testComments[3], testComments[4]],
      }),
    });

    expect(handlePendingDeleteUndo).not.toHaveBeenCalled();

    await act(async () => {
      await result.current.reAddComment.current(testComments[3].id);
    });

    expect(handlePendingDeleteUndo).toHaveBeenLastCalledWith(
      testComments[3].id,
    );
  });

  function createWrapper(props: Props) {
    return function CreatedWrapper({ children }: { children: ReactNode }) {
      return (
        <CommentsContextProvider {...props}>{children}</CommentsContextProvider>
      );
    };
  }
});
