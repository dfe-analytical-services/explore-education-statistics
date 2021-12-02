import CommentAddForm from '@admin/components/comments/CommentAddForm';
import { testComments } from '@admin/components/comments/__data__/testComments';
import { CommentsProvider } from '@admin/contexts/comments/CommentsContext';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('CommentAddForm', () => {
  const blockId = 'block-id';

  test('renders the add comment form correctly', () => {
    render(<CommentAddForm blockId={blockId} onCancel={noop} onSave={noop} />);

    expect(
      screen.getByRole('textbox', {
        name: 'Add comment',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Add comment',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Cancel',
      }),
    ).toBeInTheDocument();
  });

  test('adds the comment and calls the onSave handler when the form is submitted', async () => {
    const handleOnSave = jest.fn();
    const handleOnAddComment = jest.fn();
    handleOnAddComment.mockResolvedValue(testComments[1]);

    render(
      <CommentsProvider
        value={{
          comments: [],
          pendingDeletions: [],
          onAddComment: handleOnAddComment,
        }}
      >
        <CommentAddForm
          blockId={blockId}
          onCancel={noop}
          onSave={handleOnSave}
        />
      </CommentsProvider>,
    );

    await userEvent.type(
      screen.getByRole('textbox', {
        name: 'Add comment',
      }),
      'I am a comment',
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Add comment',
      }),
    );

    await waitFor(() => {
      expect(handleOnAddComment).toHaveBeenCalledWith({
        content: 'I am a comment',
      });

      expect(handleOnSave).toHaveBeenCalledWith(testComments[1]);
    });
  });

  test('shows validation error and does not submit if no comment given', async () => {
    const handleOnSave = jest.fn();

    render(
      <CommentAddForm
        blockId={blockId}
        onCancel={noop}
        onSave={handleOnSave}
      />,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Add comment',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByText('Enter a comment', {
          selector: '#block-id-addCommentForm-content-error',
        }),
      ).toBeInTheDocument();

      expect(handleOnSave).not.toHaveBeenCalled();
    });
  });

  test('calls the onCancel handler when the cancel button is clicked', () => {
    const handleOnCancel = jest.fn();

    render(
      <CommentAddForm
        blockId={blockId}
        onCancel={handleOnCancel}
        onSave={noop}
      />,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Cancel',
      }),
    );

    expect(handleOnCancel).toHaveBeenCalled();
  });
});
