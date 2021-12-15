import CommentsList from '@admin/components/comments/CommentsList';
import { testComments } from '@admin/components/comments/__data__/testComments';
import { CommentsProvider } from '@admin/contexts/CommentsContext';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('CommentsList', () => {
  const testMarkersOrder: string[] = ['comment-4', 'comment-2', 'comment-3'];
  const blockId = 'block-id';

  test('displays unresolved comments ordered by marker position', () => {
    render(
      <CommentsProvider
        comments={testComments}
        markersOrder={testMarkersOrder}
        onDeleteComment={jest.fn()}
        onSaveComment={jest.fn()}
        onSaveUpdatedComment={jest.fn()}
        onUpdateUnresolvedComments={{ current: jest.fn() }}
        onUpdateUnsavedCommentDeletions={{ current: jest.fn() }}
      >
        <CommentsList blockId={blockId} />
      </CommentsProvider>,
    );

    const unresolvedComments = within(
      screen.getByTestId('unresolvedComments'),
    ).getAllByRole('listitem');
    expect(unresolvedComments).toHaveLength(3);
    expect(unresolvedComments[0]).toHaveTextContent('Comment 4');
    expect(unresolvedComments[1]).toHaveTextContent('Comment 2');
    expect(unresolvedComments[2]).toHaveTextContent('Comment 3');
  });

  test('displays resolved comments', () => {
    render(
      <CommentsProvider
        comments={testComments}
        markersOrder={testMarkersOrder}
        onDeleteComment={jest.fn()}
        onSaveComment={jest.fn()}
        onSaveUpdatedComment={jest.fn()}
        onUpdateUnresolvedComments={{ current: jest.fn() }}
        onUpdateUnsavedCommentDeletions={{ current: jest.fn() }}
      >
        <CommentsList blockId={blockId} />
      </CommentsProvider>,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Resolved comments (2)',
      }),
    );

    const resolvedComments = within(
      screen.getByTestId('resolvedComments'),
    ).getAllByRole('listitem');
    expect(resolvedComments).toHaveLength(2);
    expect(resolvedComments[0]).toHaveTextContent('Comment 1');
    expect(resolvedComments[1]).toHaveTextContent('Comment 5');
  });
});
