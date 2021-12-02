import Comment from '@admin/components/comments/Comment';
import {
  testComments,
  testCommentUser1,
  testCommentUser2,
} from '@admin/components/comments/__data__/testComments';
import { AuthContext, User } from '@admin/contexts/AuthContext';
import { GlobalPermissions } from '@admin/services/permissionService';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('Comment', () => {
  const testUser1: User = {
    id: testCommentUser1.id,
    name: `testCommentUser1.firstName testCommentUser1.lastName`,
    permissions: {} as GlobalPermissions,
  };
  const testUser2: User = {
    id: testCommentUser2.id,
    name: `testCommentUser2.firstName testCommentUser2.lastName`,
    permissions: {} as GlobalPermissions,
  };

  test('renders an unresolved comment correctly', () => {
    render(
      <Comment
        active={false}
        comment={testComments[2]}
        onRemove={noop}
        onResolve={noop}
        onSelect={noop}
        onUpdate={noop}
      />,
    );

    const comment = screen.getByRole('button', {
      name: 'Comment',
    });

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
        <Comment
          active={false}
          comment={testComments[2]}
          onRemove={noop}
          onResolve={noop}
          onSelect={noop}
          onUpdate={noop}
        />
        ,
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
        <Comment
          active={false}
          comment={testComments[2]}
          onRemove={noop}
          onResolve={noop}
          onSelect={noop}
          onUpdate={noop}
        />
        ,
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
    render(
      <Comment
        active={false}
        comment={updatedComment}
        onRemove={noop}
        onResolve={noop}
        onSelect={noop}
        onUpdate={noop}
      />,
    );

    const comment = screen.getByRole('button', {
      name: 'Comment',
    });

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
        <Comment
          active={false}
          comment={testComments[0]}
          onRemove={noop}
          onResolve={noop}
          onSelect={noop}
          onUpdate={noop}
        />
      </AuthContext.Provider>,
    );

    const comment = screen.getByRole('listitem');
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

  test('resolving a comment', () => {
    const handleCommentResolved = jest.fn();

    render(
      <AuthContext.Provider
        value={{
          user: testUser2,
        }}
      >
        <Comment
          active={false}
          comment={testComments[2]}
          onRemove={noop}
          onResolve={handleCommentResolved}
          onSelect={noop}
          onUpdate={noop}
        />
        ,
      </AuthContext.Provider>,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Resolve',
      }),
    );

    expect(handleCommentResolved).toHaveBeenCalledWith({
      comment: testComments[2],
    });
  });

  test('unresolving a comment', () => {
    const handleCommentResolved = jest.fn();

    render(
      <AuthContext.Provider
        value={{
          user: testUser2,
        }}
      >
        <Comment
          active={false}
          comment={testComments[0]}
          onRemove={noop}
          onResolve={handleCommentResolved}
          onSelect={noop}
          onUpdate={noop}
        />
        ,
      </AuthContext.Provider>,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Unresolve',
      }),
    );

    expect(handleCommentResolved).toHaveBeenCalledWith({
      comment: testComments[0],
    });
  });

  test('deleting a comment', () => {
    const handleCommentRemoved = jest.fn();
    render(
      <AuthContext.Provider
        value={{
          user: testUser2,
        }}
      >
        <Comment
          active={false}
          comment={testComments[2]}
          onRemove={handleCommentRemoved}
          onResolve={noop}
          onSelect={noop}
          onUpdate={noop}
        />
        ,
      </AuthContext.Provider>,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Delete',
      }),
    );

    expect(handleCommentRemoved).toHaveBeenCalledWith(testComments[2].id);
  });

  test('selecting a comment', () => {
    const handleCommentSelected = jest.fn();
    render(
      <AuthContext.Provider
        value={{
          user: testUser2,
        }}
      >
        <Comment
          active={false}
          comment={testComments[2]}
          onRemove={noop}
          onResolve={noop}
          onSelect={handleCommentSelected}
          onUpdate={noop}
        />
        ,
      </AuthContext.Provider>,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Comment',
      }),
    );

    expect(handleCommentSelected).toHaveBeenCalledWith(testComments[2].id);
  });

  test('clicking `Edit` button shows the edit form and hides `Edit` button', () => {
    render(
      <AuthContext.Provider
        value={{
          user: testUser2,
        }}
      >
        <Comment
          active={false}
          comment={testComments[2]}
          onRemove={noop}
          onResolve={noop}
          onSelect={noop}
          onUpdate={noop}
        />
        ,
      </AuthContext.Provider>,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Edit',
      }),
    );

    expect(
      screen.getByRole('textbox', {
        name: 'Edit comment',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'Edit',
      }),
    ).not.toBeInTheDocument();
  });
});
