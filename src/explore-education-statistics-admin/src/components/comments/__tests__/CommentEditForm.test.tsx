import React from 'react';
import CommentEditForm from '@admin/components/comments/CommentEditForm';
import { testComments } from '@admin/components/comments/__data__/testComments';
import { CommentsContextProvider } from '@admin/contexts/CommentsContext';
import { Comment } from '@admin/services/types/content';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';

describe('CommentEditForm', () => {
  test('renders the form', () => {
    render(
      <CommentEditForm
        comment={testComments[2]}
        id="comment-id"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('textbox', {
        name: 'Comment',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Update',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Cancel',
      }),
    ).toBeInTheDocument();
  });

  test('clicking cancel button calls the onCancel handler', () => {
    const handleCancel = jest.fn();
    render(
      <CommentEditForm
        comment={testComments[2]}
        id="block-id"
        onCancel={handleCancel}
        onSubmit={noop}
      />,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Cancel',
      }),
    );

    expect(handleCancel).toHaveBeenCalled();
  });

  test('shows validation error and does not submit if the comment is deleted', async () => {
    const handleSubmit = jest.fn();

    render(
      <CommentEditForm
        comment={testComments[2]}
        id="block-id"
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await userEvent.clear(
      screen.getByRole('textbox', {
        name: 'Comment',
      }),
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Update',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Enter a comment', {
          selector: '#block-id-editCommentForm-content-error',
        }),
      ).toBeInTheDocument();

      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('successfully submitting the for updates the comment and calls the onSubmit handler', async () => {
    const updatedComment: Comment = {
      ...testComments[2],
      content: 'Comment 3 content updated',
    };
    const handleSubmit = jest.fn();
    const handleUpdateComment = jest.fn();
    handleUpdateComment.mockResolvedValue({
      ...updatedComment,
      updated: '2021-11-29T14:00',
    });

    render(
      <CommentsContextProvider
        comments={[]}
        onDelete={noop}
        onCreate={jest.fn()}
        onUpdate={handleUpdateComment}
        onPendingDelete={noop}
        onPendingDeleteUndo={noop}
      >
        <CommentEditForm
          comment={testComments[2]}
          id="block-id"
          onCancel={noop}
          onSubmit={handleSubmit}
        />
      </CommentsContextProvider>,
    );

    userEvent.type(
      screen.getByRole('textbox', {
        name: 'Comment',
      }),
      ' updated',
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Update',
      }),
    );

    await waitFor(() => {
      expect(handleUpdateComment).toHaveBeenCalledWith(updatedComment);
      expect(handleSubmit).toHaveBeenCalled();
    });
  });
});
