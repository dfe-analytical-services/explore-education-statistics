import CommentAddForm from '@admin/components/comments/CommentAddForm';
import { testComments } from '@admin/components/comments/__data__/testComments';
import { CommentsContextProvider } from '@admin/contexts/CommentsContext';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React, { createRef } from 'react';

describe('CommentAddForm', () => {
  const blockId = 'block-id';
  const containerRef = createRef<HTMLDivElement>();

  test('renders the add comment form correctly', () => {
    render(
      <CommentAddForm
        blockId={blockId}
        containerRef={containerRef}
        onCancel={noop}
        onSave={noop}
      />,
    );

    expect(
      screen.getByRole('textbox', {
        name: 'Comment',
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
    const handleSave = jest.fn();
    const handleSaveComment = jest.fn();
    handleSaveComment.mockResolvedValue(testComments[1]);

    render(
      <CommentsContextProvider
        comments={[]}
        onDeleteComment={jest.fn()}
        onSaveComment={handleSaveComment}
        onSaveUpdatedComment={jest.fn()}
        onUpdateUnresolvedComments={{ current: jest.fn() }}
        onUpdateUnsavedCommentDeletions={{ current: jest.fn() }}
      >
        <CommentAddForm
          blockId={blockId}
          containerRef={containerRef}
          onCancel={noop}
          onSave={handleSave}
        />
      </CommentsContextProvider>,
    );

    await userEvent.type(
      screen.getByRole('textbox', {
        name: 'Comment',
      }),
      'I am a comment',
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Add comment',
      }),
    );

    await waitFor(() => {
      expect(handleSaveComment).toHaveBeenCalledWith({
        content: 'I am a comment',
      });

      expect(handleSave).toHaveBeenCalled();
    });
  });

  test('shows validation error and does not submit if no comment given', async () => {
    const handleSave = jest.fn();

    render(
      <CommentAddForm
        blockId={blockId}
        containerRef={containerRef}
        onCancel={noop}
        onSave={handleSave}
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

      expect(handleSave).not.toHaveBeenCalled();
    });
  });

  test('calls the onCancel handler when the cancel button is clicked', () => {
    const handleCancel = jest.fn();

    render(
      <CommentAddForm
        blockId={blockId}
        containerRef={containerRef}
        onCancel={handleCancel}
        onSave={noop}
      />,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Cancel',
      }),
    );

    expect(handleCancel).toHaveBeenCalled();
  });
});
