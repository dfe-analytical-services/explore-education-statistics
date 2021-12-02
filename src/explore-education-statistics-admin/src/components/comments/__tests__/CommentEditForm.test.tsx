import CommentEditForm from '@admin/components/comments/CommentEditForm';
import { testComments } from '@admin/components/comments/__data__/testComments';
import { CommentsProvider } from '@admin/contexts/comments/CommentsContext';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('CommentEditForm', () => {
  test('renders the form', () => {
    render(
      <CommentEditForm
        comment={testComments[2]}
        id="block-id"
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('textbox', {
        name: 'Edit comment',
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

  test('successfully submitting the for updates the comment and calls the onSubmit handler', async () => {
    const updatedComment = {
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
      <CommentsProvider
        value={{
          comments: [],
          pendingDeletions: [],
          onUpdateComment: handleUpdateComment,
        }}
      >
        <CommentEditForm
          comment={testComments[2]}
          id="block-id"
          onCancel={noop}
          onSubmit={handleSubmit}
        />
      </CommentsProvider>,
    );

    await userEvent.type(
      screen.getByRole('textbox', {
        name: 'Edit comment',
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

      expect(handleSubmit).toHaveBeenCalledWith({
        ...updatedComment,
        updated: '2021-11-29T14:00',
      });
    });
  });
});
