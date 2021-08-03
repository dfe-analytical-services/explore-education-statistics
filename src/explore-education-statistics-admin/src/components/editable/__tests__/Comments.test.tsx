import { AuthContext, User } from '@admin/contexts/AuthContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import { ReleaseContextProvider } from '@admin/pages/release/contexts/ReleaseContext';
import { GlobalPermissions } from '@admin/services/permissionService';
import _releaseContentCommentService from '@admin/services/releaseContentCommentService';
import { Comment } from '@admin/services/types/content';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';
import Comments from '../Comments';

jest.mock('@admin/services/releaseContentCommentService');

const releaseContentCommentService = _releaseContentCommentService as jest.Mocked<
  typeof _releaseContentCommentService
>;

describe('Comments', () => {
  test('renders list of comments when comments are opened', () => {
    render(
      <ReleaseContextProvider release={testRelease}>
        <Comments
          blockId="test-block"
          sectionId="section-1"
          comments={[
            {
              id: 'comment-1',
              content: 'Test comment 1',
              createdBy: {
                id: 'user-1',
                email: 'test@test.com',
                firstName: 'John',
                lastName: 'Smith',
              },
              created: '2020-06-06T12:00:00',
            },
            {
              id: 'comment-2',
              content: 'Test comment 2',
              createdBy: {
                id: 'user-2',
                email: 'test2@test.com',
                firstName: 'Jane',
                lastName: 'Roberts',
              },
              created: '2020-06-06T11:00:00',
              updated: '2020-06-07T15:00:00',
            },
            {
              id: 'comment-3',
              content: 'Test comment 3',
              createdBy: {
                id: 'user-3',
                email: 'test3@test.com',
                firstName: 'John',
                lastName: 'Cale',
              },
              created: '2020-06-08T12:00:00',
              resolved: '2020-06-10T12:00:00',
              resolvedBy: {
                id: 'user-1',
                email: 'test@test.com',
                firstName: 'John',
                lastName: 'Smith',
              },
            },
          ]}
          onChange={noop}
        />
      </ReleaseContextProvider>,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Add / View comments (2 unresolved)',
      }),
    );

    const comments = screen.getAllByRole('listitem');

    expect(comments).toHaveLength(3);

    expect(comments[0]).toHaveTextContent('Comment resolved');
    expect(comments[0]).toHaveTextContent('On: 10/06/20 12:00');
    expect(comments[0]).toHaveTextContent('By: John Smith');
    expect(comments[0]).toHaveTextContent('See comment');
    expect(comments[0]).toHaveTextContent('Created: 08/06/20 12:00');
    expect(comments[0]).not.toHaveTextContent('Updated:');
    expect(comments[0]).toHaveTextContent('Test comment 3');

    expect(comments[1]).toHaveTextContent('John Smith');
    expect(comments[1]).toHaveTextContent('Created: 06/06/20 12:00');
    expect(comments[1]).toHaveTextContent('Test comment 1');
    expect(comments[1]).not.toHaveTextContent('Updated:');
    expect(comments[1]).not.toHaveTextContent('Comment resolved');

    expect(comments[2]).toHaveTextContent('Jane Roberts');
    expect(comments[2]).toHaveTextContent('Created: 06/06/20 11:00');
    expect(comments[2]).toHaveTextContent('Updated: 07/06/20 15:00');
    expect(comments[2]).toHaveTextContent('Test comment 2');
    expect(comments[2]).not.toHaveTextContent('Comment resolved');
  });

  test('adding comment calls the `onChange` handler with new comments list', async () => {
    const handleChange = jest.fn();

    render(
      <ReleaseContextProvider release={testRelease}>
        <Comments
          blockId="test-block"
          sectionId="section-1"
          comments={[
            {
              id: 'comment-1',
              content: 'Test comment 1',
              createdBy: {
                id: 'user-1',
                email: 'test@test.com',
                firstName: 'John',
                lastName: 'Smith',
              },
              created: '2020-06-06T12:00:00',
            },
          ]}
          onChange={handleChange}
        />
      </ReleaseContextProvider>,
    );

    await userEvent.type(
      screen.getByRole('textbox', {
        name: 'Comment',
        hidden: true,
      }),
      'New test comment',
    );

    releaseContentCommentService.addContentSectionComment.mockResolvedValue({
      id: 'comment-2',
      content: 'New test comment',
      createdBy: {
        id: 'user-2',
        email: 'test2@test.com',
        firstName: 'Bethany',
        lastName: 'Parker',
      },
      created: '2020-06-06T14:00:00',
    });

    userEvent.click(
      screen.getByRole('button', {
        name: 'Add comment',
        hidden: true,
      }),
    );

    await waitFor(() => {
      expect(handleChange).toHaveBeenCalledWith('test-block', [
        {
          id: 'comment-2',
          content: 'New test comment',
          createdBy: {
            id: 'user-2',
            email: 'test2@test.com',
            firstName: 'Bethany',
            lastName: 'Parker',
          },
          created: '2020-06-06T14:00:00',
        },
        {
          id: 'comment-1',
          content: 'Test comment 1',
          createdBy: {
            id: 'user-1',
            email: 'test@test.com',
            firstName: 'John',
            lastName: 'Smith',
          },
          created: '2020-06-06T12:00:00',
        },
      ] as Comment[]);
    });
  });

  describe('updating comment', () => {
    const testComments = [
      {
        id: 'comment-1',
        content: 'Test comment 1',
        createdBy: {
          id: 'user-1',
          email: 'test@test.com',
          firstName: 'John',
          lastName: 'Smith',
        },
        created: '2020-06-06T12:00:00',
      },
      {
        id: 'comment-2',
        content: 'Test comment 2',
        createdBy: {
          id: 'user-2',
          email: 'test2@test.com',
          firstName: 'Jane',
          lastName: 'Roberts',
        },
        created: '2020-06-06T11:00:00',
      },
    ];

    const testUser: User = {
      id: 'user-2',
      name: 'Jane Roberts',
      permissions: {} as GlobalPermissions,
    };

    test('can only update comment belonging to the current user', () => {
      render(
        <AuthContext.Provider
          value={{
            user: testUser,
          }}
        >
          <ReleaseContextProvider release={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testComments}
              onChange={noop}
            />
          </ReleaseContextProvider>
        </AuthContext.Provider>,
      );

      const comments = screen.getAllByRole('listitem', {
        hidden: true,
      });

      expect(
        within(comments[0]).queryByRole('button', {
          name: 'Edit',
          hidden: true,
        }),
      ).toBeNull();

      expect(
        within(comments[1]).getByRole('button', {
          name: 'Edit',
          hidden: true,
        }),
      ).toBeInTheDocument();
    });

    test('clicking `Edit` button shows textbox and hides `Edit` button', () => {
      render(
        <AuthContext.Provider
          value={{
            user: testUser,
          }}
        >
          <ReleaseContextProvider release={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testComments}
              onChange={noop}
            />
          </ReleaseContextProvider>
        </AuthContext.Provider>,
      );

      const comment = within(
        screen.getAllByRole('listitem', {
          hidden: true,
        })[1],
      );

      userEvent.click(
        comment.getByRole('button', {
          name: 'Edit',
          hidden: true,
        }),
      );

      expect(
        comment.getByRole('textbox', {
          name: 'Comment',
          hidden: true,
        }),
      ).toBeInTheDocument();

      expect(
        comment.queryByRole('button', {
          name: 'Edit',
          hidden: true,
        }),
      ).not.toBeInTheDocument();
    });

    test('clicking `Cancel` button hides the textbox again', () => {
      render(
        <AuthContext.Provider
          value={{
            user: testUser,
          }}
        >
          <ReleaseContextProvider release={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testComments}
              onChange={noop}
            />
          </ReleaseContextProvider>
        </AuthContext.Provider>,
      );

      const comment = within(
        screen.getAllByRole('listitem', {
          hidden: true,
        })[1],
      );

      userEvent.click(
        comment.getByRole('button', {
          name: 'Edit',
          hidden: true,
        }),
      );

      userEvent.click(
        comment.getByRole('button', {
          name: 'Cancel',
          hidden: true,
        }),
      );

      expect(
        comment.queryByRole('textbox', {
          name: 'Comment',
          hidden: true,
        }),
      ).not.toBeInTheDocument();

      expect(
        comment.getByRole('button', {
          name: 'Edit',
          hidden: true,
        }),
      ).toBeInTheDocument();
    });

    test('updating comment calls the `onChange` handler with the updated comment', async () => {
      const handleChange = jest.fn();

      render(
        <AuthContext.Provider
          value={{
            user: testUser,
          }}
        >
          <ReleaseContextProvider release={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testComments}
              onChange={handleChange}
            />
          </ReleaseContextProvider>
        </AuthContext.Provider>,
      );

      const comment = within(
        screen.getAllByRole('listitem', {
          hidden: true,
        })[1],
      );

      userEvent.click(
        comment.getByRole('button', {
          name: 'Edit',
          hidden: true,
        }),
      );

      await userEvent.type(
        comment.getByRole('textbox', {
          name: 'Comment',
          hidden: true,
        }),
        'Updated test comment',
      );

      releaseContentCommentService.updateContentSectionComment.mockResolvedValue(
        {
          id: 'comment-2',
          content: 'Updated test comment',
          createdBy: {
            id: 'user-2',
            email: 'test2@test.com',
            firstName: 'Jane',
            lastName: 'Roberts',
          },
          created: '2020-06-06T11:00:00',
          updated: '2020-06-06T15:00:00',
        },
      );

      userEvent.click(
        comment.getByRole('button', {
          name: 'Update',
          hidden: true,
        }),
      );

      await waitFor(() => {
        expect(handleChange).toHaveBeenCalledWith('test-block', [
          {
            id: 'comment-1',
            content: 'Test comment 1',
            createdBy: {
              id: 'user-1',
              email: 'test@test.com',
              firstName: 'John',
              lastName: 'Smith',
            },
            created: '2020-06-06T12:00:00',
          },
          {
            id: 'comment-2',
            content: 'Updated test comment',
            createdBy: {
              id: 'user-2',
              email: 'test2@test.com',
              firstName: 'Jane',
              lastName: 'Roberts',
            },
            created: '2020-06-06T11:00:00',
            updated: '2020-06-06T15:00:00',
          },
        ] as Comment[]);
      });
    });
  });

  describe('resolving comment', () => {
    const testUser: User = {
      id: 'user-1',
      name: 'Lou Reed',
      permissions: {} as GlobalPermissions,
    };

    test('clicking resolve button calls the `onChange` handler with updated comments', async () => {
      const testComments: Comment[] = [
        {
          id: 'comment-1',
          content: 'Test comment 1',
          createdBy: {
            id: 'user-1',
            email: 'test@test.com',
            firstName: 'Lou',
            lastName: 'Reed',
          },
          created: '2020-06-06T12:00:00',
        },
      ];

      const handleChange = jest.fn();

      render(
        <AuthContext.Provider
          value={{
            user: testUser,
          }}
        >
          <ReleaseContextProvider release={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testComments}
              onChange={handleChange}
            />
          </ReleaseContextProvider>
        </AuthContext.Provider>,
      );

      const comments = screen.getAllByRole('listitem', {
        hidden: true,
      });

      expect(
        within(comments[0]).queryByRole('button', {
          name: 'Unresolve',
          hidden: true,
        }),
      ).not.toBeInTheDocument();

      expect(
        within(comments[0]).getByRole('button', {
          name: 'Resolve',
          hidden: true,
        }),
      ).toBeInTheDocument();

      const updateComment: Comment = {
        ...testComments[0],
        resolved: '2020-06-20T12:00:00',
        resolvedBy: {
          id: 'user-2',
          email: 'test@test.com',
          firstName: 'Jane',
          lastName: 'Roberts',
        },
        setResolved: true,
      };

      releaseContentCommentService.updateContentSectionComment.mockResolvedValue(
        updateComment,
      );

      userEvent.click(
        screen.getByRole('button', {
          name: 'Resolve',
          hidden: true,
        }),
      );

      await waitFor(() => {
        expect(handleChange).toHaveBeenCalledWith('test-block', [
          updateComment,
        ]);
      });
    });

    test('clicking unresolve button calls the `onChange` handler with the updated comments', async () => {
      const testComments: Comment[] = [
        {
          id: 'comment-2',
          content: 'Test comment 2',
          createdBy: {
            id: 'user-2',
            email: 'test2@test.com',
            firstName: 'Jane',
            lastName: 'Roberts',
          },
          created: '2020-06-06T11:00:00',
          updated: '2020-06-07T12:00:00',
          resolved: '2020-06-10T14:00:00',
          resolvedBy: {
            id: 'user-1',
            email: 'test@test.com',
            firstName: 'Lou',
            lastName: 'Reed',
          },
        },
      ];

      const handleChange = jest.fn();

      render(
        <AuthContext.Provider
          value={{
            user: testUser,
          }}
        >
          <ReleaseContextProvider release={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testComments}
              onChange={handleChange}
            />
          </ReleaseContextProvider>
        </AuthContext.Provider>,
      );

      const comments = screen.getAllByRole('listitem', {
        hidden: true,
      });

      expect(
        within(comments[0]).queryByRole('button', {
          name: 'Resolve',
          hidden: true,
        }),
      ).not.toBeInTheDocument();

      expect(
        within(comments[0]).getByRole('button', {
          name: 'Unresolve',
          hidden: true,
        }),
      ).toBeInTheDocument();

      const updatedComment: Comment = {
        ...testComments[0],
        resolved: undefined,
        resolvedBy: undefined,
        setResolved: false,
      };

      releaseContentCommentService.updateContentSectionComment.mockResolvedValue(
        updatedComment,
      );

      userEvent.click(
        screen.getByRole('button', {
          name: 'Unresolve',
          hidden: true,
        }),
      );

      await waitFor(() => {
        expect(handleChange).toHaveBeenCalledWith('test-block', [
          updatedComment,
        ]);
      });
    });
  });

  describe('deleting comment', () => {
    const testComments: Comment[] = [
      {
        id: 'comment-1',
        content: 'Test comment 1',
        createdBy: {
          id: 'user-1',
          email: 'test@test.com',
          firstName: 'John',
          lastName: 'Smith',
        },
        created: '2020-06-06T12:00:00',
      },
      {
        id: 'comment-2',
        content: 'Test comment 2',
        createdBy: {
          id: 'user-2',
          email: 'test2@test.com',
          firstName: 'Jane',
          lastName: 'Roberts',
        },
        created: '2020-06-06T11:00:00',
      },
    ];

    const testUser: User = {
      id: 'user-2',
      name: 'Jane Roberts',
      permissions: {} as GlobalPermissions,
    };

    test('can only delete comment belonging to the current user', () => {
      render(
        <AuthContext.Provider
          value={{
            user: testUser,
          }}
        >
          <ReleaseContextProvider release={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testComments}
              onChange={noop}
            />
          </ReleaseContextProvider>
        </AuthContext.Provider>,
      );

      const comments = screen.getAllByRole('listitem', {
        hidden: true,
      });

      expect(
        within(comments[0]).queryByRole('button', {
          name: 'Delete',
          hidden: true,
        }),
      ).toBeNull();

      expect(
        within(comments[1]).getByRole('button', {
          name: 'Delete',
          hidden: true,
        }),
      ).toBeInTheDocument();
    });

    test('deleting comment calls the `onChange` handler with it removed from comments', async () => {
      const handleChange = jest.fn();

      render(
        <AuthContext.Provider
          value={{
            user: testUser,
          }}
        >
          <ReleaseContextProvider release={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testComments}
              onChange={handleChange}
            />
          </ReleaseContextProvider>
        </AuthContext.Provider>,
      );

      releaseContentCommentService.deleteContentSectionComment.mockResolvedValue();

      userEvent.click(
        screen.getByRole('button', {
          name: 'Delete',
          hidden: true,
        }),
      );

      await waitFor(() => {
        expect(handleChange).toHaveBeenCalledWith('test-block', [
          {
            id: 'comment-1',
            content: 'Test comment 1',
            createdBy: {
              id: 'user-1',
              email: 'test@test.com',
              firstName: 'John',
              lastName: 'Smith',
            },
            created: '2020-06-06T12:00:00',
          },
        ] as Comment[]);
      });
    });

    test('deleting comment calls the `onChange` handler with it removed from comments when selecting one of multiple comments', async () => {
      const handleChange = jest.fn();

      const testCommentsMultiple = [
        ...testComments,
        {
          id: 'comment-3',
          content: 'Test comment 3',
          createdBy: {
            id: 'user-2',
            email: 'test2@test.com',
            firstName: 'Jane',
            lastName: 'Roberts',
          },
          created: '2020-06-08T11:00:00',
        },
      ];

      render(
        <AuthContext.Provider
          value={{
            user: testUser,
          }}
        >
          <ReleaseContextProvider release={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testCommentsMultiple}
              onChange={handleChange}
            />
          </ReleaseContextProvider>
        </AuthContext.Provider>,
      );

      releaseContentCommentService.deleteContentSectionComment.mockResolvedValue();

      userEvent.click(
        screen.getAllByRole('button', {
          name: 'Delete',
          hidden: true,
        })[0],
      );

      await waitFor(() => {
        expect(handleChange).toHaveBeenCalledWith('test-block', [
          {
            id: 'comment-1',
            content: 'Test comment 1',
            createdBy: {
              id: 'user-1',
              email: 'test@test.com',
              firstName: 'John',
              lastName: 'Smith',
            },
            created: '2020-06-06T12:00:00',
          },
          {
            id: 'comment-2',
            content: 'Test comment 2',
            createdBy: {
              id: 'user-2',
              email: 'test2@test.com',
              firstName: 'Jane',
              lastName: 'Roberts',
            },
            created: '2020-06-06T11:00:00',
          },
        ] as Comment[]);
      });
    });
  });
});
