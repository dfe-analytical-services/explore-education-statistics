import {
  testComments,
  testCommentUser1,
  testCommentUser2,
} from '@admin/components/comments/__data__/testComments';
import EditableContentForm from '@admin/components/editable/EditableContentForm';
import { CommentsProvider } from '@admin/contexts/CommentsContext';
import { AuthContext, User } from '@admin/contexts/AuthContext';
import { GlobalPermissions } from '@admin/services/permissionService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('EditableContentForm', () => {
  const testUser1: User = {
    id: testCommentUser1.id,
    name: `testCommentUser1.firstName testCommentUser1.lastName`,
    permissions: {} as GlobalPermissions,
  };

  describe('editor form', () => {
    test('renders the form', () => {
      render(
        <EditableContentForm
          content=""
          id="block-id"
          label="Form label"
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      expect(
        screen.getByRole('textbox', { name: 'Form label' }),
      ).toBeInTheDocument();

      expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();
    });

    test('renders the form controls when autosave enabled', () => {
      render(
        <EditableContentForm
          autoSave
          content=""
          id="block-id"
          label="Form label"
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      expect(
        screen.getByRole('textbox', { name: 'Form label' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Save & close' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Cancel' }),
      ).not.toBeInTheDocument();
    });

    test('shows validation error if submit the form with no content', async () => {
      const handleSubmit = jest.fn();
      render(
        <EditableContentForm
          content=""
          id="block-id"
          label="Form label"
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Save' }));

      await waitFor(() => {
        expect(
          screen.getByText('Enter content', {
            selector: '#block-id-form-block-id-error',
          }),
        ).toBeInTheDocument();
        expect(handleSubmit).not.toHaveBeenCalled();
      });
    });

    test('calls handle submit with the content when submit the form', async () => {
      const handleSubmit = jest.fn();
      render(
        <EditableContentForm
          content="Test content"
          id="block-id"
          label="Form label"
          onCancel={noop}
          onSubmit={handleSubmit}
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Save' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith('Test content');
      });
    });
  });

  describe('comments list', () => {
    test('renders the comments list if allowComments', () => {
      render(
        <CommentsProvider
          value={{
            comments: testComments,
            onDeletePendingComment: jest.fn(),
            onSaveComment: jest.fn(),
            onSaveUpdatedComment: jest.fn(),
          }}
        >
          <EditableContentForm
            allowComments
            content="Test content"
            id="block-id"
            label="Form label"
            onCancel={noop}
            onSubmit={noop}
          />
        </CommentsProvider>,
      );
      const unresolvedComments = within(
        screen.getByTestId('unresolvedComments'),
      ).getAllByRole('listitem');
      expect(unresolvedComments).toHaveLength(3);
    });

    test('removes comment from the list when its delete button is clicked', async () => {
      render(
        <AuthContext.Provider
          value={{
            user: testUser1,
          }}
        >
          <CommentsProvider
            value={{
              comments: testComments,
              onDeletePendingComment: jest.fn(),
              onSaveComment: jest.fn(),
              onSaveUpdatedComment: jest.fn(),
            }}
          >
            <EditableContentForm
              allowComments
              content="Test content"
              id="block-id"
              label="Form label"
              onCancel={noop}
              onSubmit={noop}
            />
          </CommentsProvider>
        </AuthContext.Provider>,
      );

      const unresolvedComments = within(
        screen.getByTestId('unresolvedComments'),
      ).getAllByRole('listitem');
      expect(unresolvedComments).toHaveLength(3);
      expect(unresolvedComments[0]).toHaveTextContent('Comment 2 content');

      userEvent.click(
        within(unresolvedComments[0]).getByRole('button', {
          name: 'Delete',
        }),
      );

      await waitFor(() => {
        const updatedUnresolvedComments = within(
          screen.getByTestId('unresolvedComments'),
        ).getAllByRole('listitem');

        expect(updatedUnresolvedComments).toHaveLength(2);
        expect(updatedUnresolvedComments[0]).not.toHaveTextContent(
          'Comment 2 content',
        );
        expect(updatedUnresolvedComments[0]).toHaveTextContent(
          'Comment 3 content',
        );
      });
    });

    test('moves the comment to the resolved list when Resolve is clicked', async () => {
      const handleUpdateComment = jest.fn();
      const resolvedComment = {
        ...testComments[1],
        resolved: '2021-11-30T13:55',
        resolvedBy: testCommentUser2,
      };
      handleUpdateComment.mockResolvedValue(resolvedComment);
      render(
        <AuthContext.Provider
          value={{
            user: testUser1,
          }}
        >
          <CommentsProvider
            value={{
              comments: testComments,
              onDeletePendingComment: jest.fn(),
              onSaveComment: jest.fn(),
              onSaveUpdatedComment: handleUpdateComment,
            }}
          >
            <EditableContentForm
              allowComments
              content="Test content"
              id="block-id"
              label="Form label"
              onCancel={noop}
              onSubmit={noop}
            />
          </CommentsProvider>
        </AuthContext.Provider>,
      );

      const unresolvedComments = within(
        screen.getByTestId('unresolvedComments'),
      ).getAllByRole('listitem');

      expect(unresolvedComments).toHaveLength(3);

      userEvent.click(
        within(unresolvedComments[0]).getByRole('button', {
          name: 'Resolve',
        }),
      );

      await waitFor(() => {
        expect(
          within(screen.getByTestId('unresolvedComments')).getAllByRole(
            'listitem',
          ),
        ).toHaveLength(2);
      });

      expect(
        screen.getByRole('button', {
          name: 'Resolved comments (3)',
        }),
      ).toBeInTheDocument();

      userEvent.click(
        screen.getByRole('button', {
          name: 'Resolved comments (3)',
        }),
      );

      expect(
        within(screen.getByTestId('resolvedComments')).getAllByRole('listitem'),
      ).toHaveLength(3);
    });

    test('moves the comment to the unresolved list when Unresolve is clicked', async () => {
      const handleUpdateComment = jest.fn();
      const unresolvedComment = {
        ...testComments[0],
      };
      delete unresolvedComment.resolved;
      delete unresolvedComment.resolvedBy;

      handleUpdateComment.mockResolvedValue(unresolvedComment);

      render(
        <AuthContext.Provider
          value={{
            user: testUser1,
          }}
        >
          <CommentsProvider
            value={{
              comments: testComments,
              onDeletePendingComment: jest.fn(),
              onSaveComment: jest.fn(),
              onSaveUpdatedComment: handleUpdateComment,
            }}
          >
            <EditableContentForm
              allowComments
              content="Test content"
              id="block-id"
              label="Form label"
              onCancel={noop}
              onSubmit={noop}
            />
          </CommentsProvider>
        </AuthContext.Provider>,
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

      userEvent.click(
        within(resolvedComments[0]).getByRole('button', {
          name: 'Unresolve',
        }),
      );

      await waitFor(() => {
        expect(screen.getByText('Resolved comments (1)')).toBeInTheDocument();
      });

      expect(
        within(screen.getByTestId('resolvedComments')).getAllByRole('listitem'),
      ).toHaveLength(1);

      expect(
        within(screen.getByTestId('unresolvedComments')).getAllByRole(
          'listitem',
        ),
      ).toHaveLength(4);
    });
  });
});
