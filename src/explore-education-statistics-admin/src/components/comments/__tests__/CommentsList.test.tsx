import CommentsList from '@admin/components/comments/CommentsList';
import { testComments } from '@admin/components/comments/__data__/testComments';
import { SelectedComment } from '@admin/components/editable/EditableContentForm';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('CommentsList', () => {
  const testMarkersOrder: string[] = ['comment-4', 'comment-2', 'comment-3'];
  const testSelectedComment: SelectedComment = {
    commentId: '',
    fromEditor: false,
  };

  test('displays unresolved comments ordered by marker position', () => {
    render(
      <CommentsList
        comments={testComments}
        markersOrder={testMarkersOrder}
        selectedComment={testSelectedComment}
        onRemove={noop}
        onResolve={noop}
        onSelect={noop}
        onUpdate={noop}
      />,
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
      <CommentsList
        comments={testComments}
        markersOrder={testMarkersOrder}
        selectedComment={testSelectedComment}
        onRemove={noop}
        onResolve={noop}
        onSelect={noop}
        onUpdate={noop}
      />,
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
