import {
  CommentsProvider,
  useCommentsContext,
} from '@admin/contexts/CommentsContext';
import {
  AddComment,
  UpdateComment,
} from '@admin/services/releaseContentCommentService';
import {
  testComments,
  testCommentUser1,
} from '@admin/components/comments/__data__/testComments';
import { Comment } from '@admin/services/types/content';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('CommentsContext', () => {
  const commentToAdd: AddComment = {
    content: 'Added Comment content',
  };
  const addedComment: Comment = {
    id: 'added-comment',
    content: 'Added Comment content',
    createdBy: testCommentUser1,
    created: '2021-11-30T10:00',
  };
  const commentToUpdate: UpdateComment = {
    content: 'Updated content',
    id: testComments[2].id,
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
  const unresolvedComment: Comment = { ...testComments[0] };
  delete unresolvedComment.resolved;
  delete unresolvedComment.resolvedBy;

  const handleSaveComment = jest.fn();
  const handleDeletePendingComment = jest.fn();
  const handleSaveUpdatedComment = jest.fn();

  const setUp = ({
    initialComments = testComments,
    initialPendingDeletions = [],
    returnedUpdatedComment = updatedComment,
  }: {
    initialComments?: Comment[];
    initialPendingDeletions?: Comment[];
    returnedUpdatedComment?: Comment;
  }) => {
    handleSaveComment.mockResolvedValue(addedComment);
    handleSaveUpdatedComment.mockResolvedValue(returnedUpdatedComment);

    const TestComponent = () => {
      const {
        comments,
        currentInteraction,
        pendingDeletions,
        addComment,
        removeComment,
        deletePendingComments,
        resolveComment,
        reAddComment,
        unresolveComment,
        updateComment,
      } = useCommentsContext();

      return (
        <>
          <button type="button" onClick={() => addComment(commentToAdd)}>
            Add comment
          </button>
          <button
            type="button"
            onClick={() => removeComment.current(testComments[2].id)}
          >
            Delete comment
          </button>
          <button type="button" onClick={() => deletePendingComments()}>
            Delete pendingDeletions
          </button>
          <button
            type="button"
            onClick={() => resolveComment.current(testComments[1].id, true)}
          >
            Resolve comment
          </button>
          <button
            type="button"
            onClick={() => reAddComment.current(testComments[2].id)}
          >
            Undelete comment
          </button>
          <button
            type="button"
            onClick={() => unresolveComment.current(testComments[0].id, true)}
          >
            Unresolve comment
          </button>
          <button type="button" onClick={() => updateComment(commentToUpdate)}>
            Update comment
          </button>
          <ul data-testid="comments">
            {comments.map(comment => (
              <li key={comment.id} id={comment.id}>
                {comment.content}
                {comment.resolved && <span>Resolved</span>}
              </li>
            ))}
          </ul>
          <ul data-testid="pendingDeletions">
            {pendingDeletions.map(comment => (
              <li key={comment.id} id={comment.id}>
                {comment.content}
              </li>
            ))}
          </ul>
          <div data-testid="currentInteraction">
            {currentInteraction?.type} {currentInteraction?.id}
          </div>
        </>
      );
    };

    return render(
      <CommentsProvider
        value={{
          comments: initialComments,
          pendingDeletions: initialPendingDeletions,
          onSaveComment: handleSaveComment,
          onDeletePendingComment: handleDeletePendingComment,
          onSaveUpdatedComment: handleSaveUpdatedComment,
        }}
      >
        <TestComponent />
      </CommentsProvider>,
    );
  };

  test('addComment calls the save method and adds the new comment to the comments array', async () => {
    setUp({});
    userEvent.click(screen.getByRole('button', { name: 'Add comment' }));

    await waitFor(() => {
      expect(handleSaveComment).toHaveBeenCalledWith(commentToAdd);
      const comments = within(screen.getByTestId('comments')).getAllByRole(
        'listitem',
      );
      expect(comments).toHaveLength(6);
      expect(comments[5]).toHaveAttribute('id', addedComment.id);
      expect(comments[5]).toHaveTextContent(addedComment.content);
      expect(screen.getByTestId('currentInteraction')).toHaveTextContent(
        `adding ${addedComment.id}`,
      );
    });
  });

  test('removeComment removes comment from the comments array and adds it to pendingDeletions', async () => {
    setUp({});
    userEvent.click(screen.getByRole('button', { name: 'Delete comment' }));

    await waitFor(() => {
      const comments = within(screen.getByTestId('comments')).getAllByRole(
        'listitem',
      );
      const pendingDeletions = within(
        screen.getByTestId('pendingDeletions'),
      ).getAllByRole('listitem');
      expect(comments).toHaveLength(4);
      expect(pendingDeletions).toHaveLength(1);
      expect(pendingDeletions[0]).toHaveAttribute('id', testComments[2].id);
      expect(pendingDeletions[0]).toHaveTextContent(testComments[2].content);
      expect(screen.getByTestId('currentInteraction')).toHaveTextContent(
        `removing ${testComments[2].id}`,
      );
    });
  });

  test('deletePendingComments calls the delete comment method for each pending and removes them from the pending array', async () => {
    setUp({
      initialComments: [testComments[0], testComments[1]],
      initialPendingDeletions: [
        testComments[2],
        testComments[3],
        testComments[4],
      ],
    });
    userEvent.click(
      screen.getByRole('button', { name: 'Delete pendingDeletions' }),
    );

    await waitFor(() => {
      expect(handleDeletePendingComment).toHaveBeenCalledTimes(3);
      expect(handleDeletePendingComment).toHaveBeenCalledWith(
        testComments[2].id,
      );
      expect(handleDeletePendingComment).toHaveBeenCalledWith(
        testComments[3].id,
      );
      expect(handleDeletePendingComment).toHaveBeenCalledWith(
        testComments[4].id,
      );
    });
    const pendingDeletions = within(
      screen.getByTestId('pendingDeletions'),
    ).queryAllByRole('listitem');
    expect(pendingDeletions).toHaveLength(0);

    const comments = within(screen.getByTestId('comments')).getAllByRole(
      'listitem',
    );
    expect(comments).toHaveLength(2);
  });

  test('resolveComment calls the update method and updates the comment in the array', async () => {
    setUp({ returnedUpdatedComment: resolvedComment });
    userEvent.click(screen.getByRole('button', { name: 'Resolve comment' }));
    await waitFor(() => {
      expect(handleSaveUpdatedComment).toHaveBeenCalledWith({
        ...testComments[1],
        setResolved: true,
      });
    });
    const comments = within(screen.getByTestId('comments')).getAllByRole(
      'listitem',
    );
    expect(comments[1]).toHaveAttribute('id', resolvedComment.id);
    expect(comments[1]).toHaveTextContent(resolvedComment.content);
    expect(comments[1]).toHaveTextContent('Resolved');
    expect(screen.getByTestId('currentInteraction')).toHaveTextContent(
      `resolving ${resolvedComment.id}`,
    );
  });

  test('unresolveComment calls the update method and updates the comment in the array', async () => {
    setUp({ returnedUpdatedComment: unresolvedComment });
    userEvent.click(screen.getByRole('button', { name: 'Unresolve comment' }));
    await waitFor(() => {
      expect(handleSaveUpdatedComment).toHaveBeenCalledWith({
        ...testComments[0],
        setResolved: false,
      });
    });
    const comments = within(screen.getByTestId('comments')).getAllByRole(
      'listitem',
    );
    expect(comments[0]).toHaveAttribute('id', unresolvedComment.id);
    expect(comments[0]).toHaveTextContent(unresolvedComment.content);
    expect(comments[0]).not.toHaveTextContent('Resolved');
    expect(screen.getByTestId('currentInteraction')).toHaveTextContent(
      `unresolving ${unresolvedComment.id}`,
    );
  });

  test('updateComment calls the update method and updates the comment in the array', async () => {
    setUp({});
    userEvent.click(screen.getByRole('button', { name: 'Update comment' }));
    await waitFor(() => {
      expect(handleSaveUpdatedComment).toHaveBeenCalledWith(commentToUpdate);
    });
    const comments = within(screen.getByTestId('comments')).getAllByRole(
      'listitem',
    );
    expect(comments[2]).toHaveAttribute('id', updatedComment.id);
    expect(comments[2]).toHaveTextContent(updatedComment.content);
  });

  test('reAddComment removes comment from the pendingDeletions array and adds it to comments', async () => {
    setUp({
      initialComments: [testComments[0], testComments[1]],
      initialPendingDeletions: [testComments[2], testComments[3]],
    });
    userEvent.click(screen.getByRole('button', { name: 'Undelete comment' }));

    await waitFor(() => {
      const comments = within(screen.getByTestId('comments')).getAllByRole(
        'listitem',
      );
      const pendingDeletions = within(
        screen.getByTestId('pendingDeletions'),
      ).getAllByRole('listitem');
      expect(comments).toHaveLength(3);
      expect(comments[2]).toHaveAttribute('id', testComments[2].id);
      expect(comments[2]).toHaveTextContent(testComments[2].content);
      expect(pendingDeletions).toHaveLength(1);
    });
  });
});
