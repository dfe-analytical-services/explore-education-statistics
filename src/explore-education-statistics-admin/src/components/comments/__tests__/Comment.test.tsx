import Comment from '@admin/components/comments/Comment';
import {
  testComments,
  testCommentUser1,
  testCommentUser2,
} from '@admin/components/comments/__data__/testComments';
import { AuthContext, User } from '@admin/contexts/AuthContext';
import { GlobalPermissions } from '@admin/services/permissionService';
import { render, screen } from '@testing-library/react';
import React from 'react';

describe('Comment', () => {
  const testUser1: User = {
    id: testCommentUser1.id,
    name: `${testCommentUser1.firstName} ${testCommentUser1.lastName}`,
    permissions: {} as GlobalPermissions,
  };
  const testUser2: User = {
    id: testCommentUser2.id,
    name: `${testCommentUser2.firstName} ${testCommentUser2.lastName}`,
    permissions: {} as GlobalPermissions,
  };

  test('renders an unresolved comment correctly', () => {
    render(<Comment comment={testComments[2]} />);

    const comment = screen.getByTestId('comment');
    expect(comment).toHaveTextContent('Comment 3');
    expect(comment).toHaveTextContent('User Two');
    expect(comment).toHaveTextContent('30 Nov 2021, 13:55');

    expect(screen.getByRole('button', { name: 'Resolve' })).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Unresolve' }),
    ).not.toBeInTheDocument();
  });

  test('renders the edit and delete buttons if the user created the comment', () => {
    render(
      <AuthContext.Provider
        value={{
          user: testUser2,
        }}
      >
        <Comment comment={testComments[2]} />,
      </AuthContext.Provider>,
    );

    expect(screen.getByRole('button', { name: 'Edit' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Delete' })).toBeInTheDocument();
  });

  test('does not render the edit and delete buttons if the user did not create the comment', () => {
    render(
      <AuthContext.Provider
        value={{
          user: testUser1,
        }}
      >
        <Comment comment={testComments[2]} />,
      </AuthContext.Provider>,
    );

    expect(
      screen.queryByRole('button', { name: 'Edit' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Delete' }),
    ).not.toBeInTheDocument();
  });

  test('renders an updated comment correctly', () => {
    const updatedComment = {
      ...testComments[2],
      updated: '2021-11-30T14:00',
    };
    render(<Comment comment={updatedComment} />);

    const comment = screen.getByTestId('comment');
    expect(comment).toHaveTextContent('Comment 3');
    expect(comment).toHaveTextContent(
      '30 Nov 2021, 13:55(Updated 30 Nov 2021, 14:00)',
    );
  });

  test('renders a resolved comment correctly', () => {
    render(
      <AuthContext.Provider
        value={{
          user: testUser1,
        }}
      >
        <Comment comment={testComments[0]} />
      </AuthContext.Provider>,
    );

    const comment = screen.getByTestId('comment');
    expect(comment).toHaveTextContent('Comment 1');
    expect(comment).toHaveTextContent('User One');
    expect(comment).toHaveTextContent('29 Nov 2021, 13:55');
    expect(comment).toHaveTextContent(
      'Resolved by User Two on 30 Nov 2021, 13:55',
    );

    expect(
      screen.getByRole('button', { name: 'Unresolve' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Resolve' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Edit' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Delete' }),
    ).not.toBeInTheDocument();
  });
});
