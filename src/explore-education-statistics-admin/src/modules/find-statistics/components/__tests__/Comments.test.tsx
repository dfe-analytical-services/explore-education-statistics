import { AuthContext, User } from '@admin/contexts/AuthContext';
import ManageReleaseContext from '@admin/pages/release/ManageReleaseContext';
import { GlobalPermissions } from '@admin/services/permissionService';
import _releaseContentCommentService from '@admin/services/releaseContentCommentService';
import { Comment } from '@admin/services/types/content';
import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import Comments from '../Comments';

jest.mock('@admin/services/releaseContentCommentService');

const releaseContentCommentService = _releaseContentCommentService as jest.Mocked<
  typeof _releaseContentCommentService
>;

describe('Comments', () => {
  const testRelease = {
    releaseId: 'release-1',
    onChangeReleaseStatus: noop,
    publication: {
      id: 'publication-1',
      themeId: 'theme-1',
      title: 'Test publication',
    },
  };

  test('renders list of comments when comments are opened', () => {
    render(
      <ManageReleaseContext.Provider value={testRelease}>
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
                id: 'user-1',
                email: 'test2@test.com',
                firstName: 'Jane',
                lastName: 'Roberts',
              },
              created: '2020-06-06T11:00:00',
              updated: '2020-06-07T15:00:00',
            },
          ]}
          onChange={noop}
        />
      </ManageReleaseContext.Provider>,
    );

    fireEvent.click(
      screen.getByRole('button', {
        name: 'Add / View comments (2)',
      }),
    );

    const comments = screen.getAllByRole('listitem');

    expect(comments).toHaveLength(2);

    expect(comments[0]).toHaveTextContent('John Smith');
    expect(comments[0]).toHaveTextContent('6 June 2020');
    expect(comments[0]).toHaveTextContent('Test comment 1');

    expect(comments[1]).toHaveTextContent('Jane Roberts');
    expect(comments[1]).toHaveTextContent('Updated: 7 June 2020');
    expect(comments[1]).toHaveTextContent('Test comment 2');

    expect(screen.getByRole('list')).toMatchSnapshot();
  });

  test('adding comment calls the `onChange` handler with new comments list', async () => {
    const handleChange = jest.fn();

    render(
      <ManageReleaseContext.Provider value={testRelease}>
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
      </ManageReleaseContext.Provider>,
    );

    fireEvent.change(
      screen.getByRole('textbox', {
        name: 'Comment',
        hidden: true,
      }),
      {
        target: {
          value: 'New test comment',
        },
      },
    );

    releaseContentCommentService.addContentSectionComment.mockImplementation(
      () =>
        Promise.resolve<Comment>({
          id: 'comment-2',
          content: 'New test comment',
          createdBy: {
            id: 'user-2',
            email: 'test2@test.com',
            firstName: 'Bethany',
            lastName: 'Parker',
          },
          created: '2020-06-06T14:00:00',
        }),
    );

    fireEvent.click(
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
          <ManageReleaseContext.Provider value={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testComments}
              onChange={noop}
            />
          </ManageReleaseContext.Provider>
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
          <ManageReleaseContext.Provider value={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testComments}
              onChange={noop}
            />
          </ManageReleaseContext.Provider>
        </AuthContext.Provider>,
      );

      const comment = within(
        screen.getAllByRole('listitem', {
          hidden: true,
        })[1],
      );

      fireEvent.click(
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
          <ManageReleaseContext.Provider value={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testComments}
              onChange={noop}
            />
          </ManageReleaseContext.Provider>
        </AuthContext.Provider>,
      );

      const comment = within(
        screen.getAllByRole('listitem', {
          hidden: true,
        })[1],
      );

      fireEvent.click(
        comment.getByRole('button', {
          name: 'Edit',
          hidden: true,
        }),
      );

      fireEvent.click(
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
          <ManageReleaseContext.Provider value={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testComments}
              onChange={handleChange}
            />
          </ManageReleaseContext.Provider>
        </AuthContext.Provider>,
      );

      const comment = within(
        screen.getAllByRole('listitem', {
          hidden: true,
        })[1],
      );

      fireEvent.click(
        comment.getByRole('button', {
          name: 'Edit',
          hidden: true,
        }),
      );

      fireEvent.change(
        comment.getByRole('textbox', {
          name: 'Comment',
          hidden: true,
        }),
        {
          target: {
            value: 'Updated test comment',
          },
        },
      );

      releaseContentCommentService.updateContentSectionComment.mockImplementation(
        () =>
          Promise.resolve<Comment>({
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
          }),
      );

      fireEvent.click(
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
          <ManageReleaseContext.Provider value={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testComments}
              onChange={noop}
            />
          </ManageReleaseContext.Provider>
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
          <ManageReleaseContext.Provider value={testRelease}>
            <Comments
              blockId="test-block"
              sectionId="section-1"
              comments={testComments}
              onChange={handleChange}
            />
          </ManageReleaseContext.Provider>
        </AuthContext.Provider>,
      );

      releaseContentCommentService.deleteContentSectionComment.mockImplementation(
        () => Promise.resolve(),
      );

      fireEvent.click(
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
  });
});
