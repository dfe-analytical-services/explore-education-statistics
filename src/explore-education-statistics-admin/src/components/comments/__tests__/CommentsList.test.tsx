import React from 'react';
import CommentsList from '@admin/components/comments/CommentsList';
import { testComments } from '@admin/components/comments/__data__/testComments';
import { CommentsContextProvider } from '@admin/contexts/CommentsContext';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';

describe('CommentsList', () => {
  const testMarkersOrder: string[] = ['comment-4', 'comment-2', 'comment-3'];

  test('displays unresolved comments ordered by marker position', () => {
    render(
      <CommentsContextProvider
        comments={testComments}
        markersOrder={testMarkersOrder}
        onDelete={noop}
        onCreate={jest.fn()}
        onUpdate={noop}
        onPendingDelete={noop}
        onPendingDeleteUndo={noop}
      >
        <CommentsList />
      </CommentsContextProvider>,
    );

    const unresolvedComments = within(
      screen.getByTestId('unresolvedComments'),
    ).getAllByTestId('comment');

    expect(unresolvedComments).toHaveLength(3);
    expect(unresolvedComments[0]).toHaveTextContent('Comment 4');
    expect(unresolvedComments[1]).toHaveTextContent('Comment 2');
    expect(unresolvedComments[2]).toHaveTextContent('Comment 3');
  });

  test('displays resolved comments', () => {
    render(
      <CommentsContextProvider
        comments={testComments}
        markersOrder={testMarkersOrder}
        onDelete={noop}
        onCreate={jest.fn()}
        onUpdate={noop}
        onPendingDelete={noop}
        onPendingDeleteUndo={noop}
      >
        <CommentsList />
      </CommentsContextProvider>,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Resolved comments (2)',
      }),
    );

    expect(
      screen.getByRole('button', { name: 'Resolved comments (2)' }),
    ).toBeInTheDocument();

    const resolvedComments = within(
      screen.getByTestId('resolvedComments'),
    ).getAllByTestId('comment');

    expect(resolvedComments).toHaveLength(2);
    expect(resolvedComments[0]).toHaveTextContent('Comment 1');
    expect(resolvedComments[1]).toHaveTextContent('Comment 5');
  });
});
