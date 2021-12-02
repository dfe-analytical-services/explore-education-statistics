import {
  testComments,
  testCommentUser1,
  testCommentUser2,
} from '@admin/components/comments/__data__/testComments';
import EditableContentForm from '@admin/components/editable/EditableContentForm';
import { CommentsProvider } from '@admin/contexts/comments/CommentsContext';
import { AuthContext, User } from '@admin/contexts/AuthContext';
import { GlobalPermissions } from '@admin/services/permissionService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

// Mocking FormFieldEditor as CKEditor doesn't work in the tests so adding a mocked component to be able to test the interactions.
jest.mock('@admin/components/form/FormFieldEditor', () => {
  return {
    __esModule: true,
    default: ({
      onClickAddComment,
      onAutoSave,
      onCancelComment,
      onClickCommentMarker,
      onRemoveCommentMarker,
    }: {
      onClickAddComment: () => void;
      onAutoSave: (content: string) => void;
      onCancelComment: () => void;
      onClickCommentMarker: (id: string) => void;
      onRemoveCommentMarker: (id: string) => void;
    }) => {
      return (
        <div data-testid="mocked-form-editor">
          <button type="button" onClick={onClickAddComment}>
            Add comment clicked in editor
          </button>
          <button type="button" onClick={onCancelComment}>
            Cancel add comment in editor
          </button>
          <button
            type="button"
            onClick={() => onClickCommentMarker('comment-2')}
          >
            Marker clicked in editor
          </button>
          <button
            type="button"
            onClick={() => onRemoveCommentMarker('comment-2')}
          >
            Marker removed in editor
          </button>
          <button type="button" onClick={() => onAutoSave('Test content')}>
            AutoSave in editor
          </button>
        </div>
      );
    },
  };
});

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

      expect(screen.getByTestId('mocked-form-editor')).toBeInTheDocument();

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

      expect(screen.getByTestId('mocked-form-editor')).toBeInTheDocument();

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
        expect(screen.getByText('Enter content')).toBeInTheDocument();
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

  describe('add comment form', () => {
    test('renders the add comment form when onClickAddComment is called', async () => {
      render(
        <CommentsProvider
          value={{
            comments: testComments,
            pendingDeletions: [],
          }}
        >
          <EditableContentForm
            content="Test content"
            id="block-id"
            label="Form label"
            onCancel={noop}
            onSubmit={noop}
          />
        </CommentsProvider>,
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Add comment clicked in editor' }),
      );

      await waitFor(() => {
        expect(
          screen.getByRole('textbox', {
            name: 'Add comment',
          }),
        ).toBeInTheDocument();
      });
    });

    test('adds the new comment to the comments list', async () => {
      const handleOnAddComment = jest.fn();
      const newComment = {
        id: 'comment-5',
        content: 'I am a comment',
        createdBy: testCommentUser1,
        created: '2021-11-30T10:00',
      };
      handleOnAddComment.mockResolvedValue(newComment);
      render(
        <CommentsProvider
          value={{
            comments: testComments,
            pendingDeletions: [],
            onAddComment: handleOnAddComment,
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

      expect(
        within(screen.getByTestId('unresolvedComments')).getAllByRole(
          'listitem',
        ),
      ).toHaveLength(3);

      userEvent.click(
        screen.getByRole('button', { name: 'Add comment clicked in editor' }),
      );

      await waitFor(() => {
        expect(
          screen.getByRole('textbox', {
            name: 'Add comment',
          }),
        ).toBeInTheDocument();
      });

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
        const updatedComments = within(
          screen.getByTestId('unresolvedComments'),
        ).getAllByRole('listitem');
        expect(updatedComments).toHaveLength(4);
        expect(updatedComments[3]).toHaveTextContent('I am a comment');
      });
    });
  });

  describe('comments list', () => {
    test('renders the comments list if allowComments', () => {
      render(
        <CommentsProvider
          value={{
            comments: testComments,
            pendingDeletions: [],
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
              pendingDeletions: [],
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
      const handleToggleResolveComment = jest.fn();
      const resolvedComment = {
        ...testComments[1],
        resolved: '2021-11-30T13:55',
        resolvedBy: testCommentUser2,
      };
      handleToggleResolveComment.mockResolvedValue(resolvedComment);
      render(
        <AuthContext.Provider
          value={{
            user: testUser1,
          }}
        >
          <CommentsProvider
            value={{
              comments: testComments,
              pendingDeletions: [],
              onToggleResolveComment: handleToggleResolveComment,
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
      const handleToggleResolveComment = jest.fn();
      const unresolvedComment = {
        ...testComments[0],
      };
      delete unresolvedComment.resolved;
      delete unresolvedComment.resolvedBy;

      handleToggleResolveComment.mockResolvedValue(unresolvedComment);

      render(
        <AuthContext.Provider
          value={{
            user: testUser1,
          }}
        >
          <CommentsProvider
            value={{
              comments: testComments,
              pendingDeletions: [],
              onToggleResolveComment: handleToggleResolveComment,
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
        expect(
          screen.getByRole('button', {
            name: 'Resolved comments (1)',
          }),
        ).toBeInTheDocument();
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

  describe('editor interactions', () => {
    test('saves the form data when autoSaves', async () => {
      const handleSubmit = jest.fn();
      render(
        <CommentsProvider
          value={{
            comments: testComments,
            pendingDeletions: [],
          }}
        >
          <EditableContentForm
            allowComments
            autoSave
            content="Test content"
            id="block-id"
            label="Form label"
            onCancel={noop}
            onSubmit={handleSubmit}
          />
        </CommentsProvider>,
      );

      userEvent.click(
        screen.getByRole('button', { name: 'AutoSave in editor' }),
      );
      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith('Test content', true);
      });
    });

    test('closes the add comment form when cancel adding a comment from the editor', async () => {
      const handleSubmit = jest.fn();
      render(
        <CommentsProvider
          value={{
            comments: testComments,
            pendingDeletions: [],
          }}
        >
          <EditableContentForm
            allowComments
            autoSave
            content="Test content"
            id="block-id"
            label="Form label"
            onCancel={noop}
            onSubmit={handleSubmit}
          />
        </CommentsProvider>,
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Add comment clicked in editor' }),
      );
      await waitFor(() => {
        expect(
          screen.getByRole('textbox', {
            name: 'Add comment',
          }),
        ).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', { name: 'Cancel add comment in editor' }),
      );
      await waitFor(() => {
        expect(
          screen.queryByRole('textbox', {
            name: 'Add comment',
          }),
        ).not.toBeInTheDocument();
      });
    });

    test('selects the comment when its marker is clicked in the editor', async () => {
      const handleSubmit = jest.fn();
      render(
        <CommentsProvider
          value={{
            comments: testComments,
            pendingDeletions: [],
          }}
        >
          <EditableContentForm
            allowComments
            autoSave
            content="Test content"
            id="block-id"
            label="Form label"
            onCancel={noop}
            onSubmit={handleSubmit}
          />
        </CommentsProvider>,
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Marker clicked in editor' }),
      );

      const unresolvedComments = within(
        screen.getByTestId('unresolvedComments'),
      ).getAllByRole('listitem');

      await waitFor(() => {
        expect(
          within(unresolvedComments[0]).getByRole('button', {
            name: 'Comment',
          }),
        ).toHaveClass('comment active');
      });
    });

    test('removes the comment when its marker is removed in the editor', async () => {
      const handleSubmit = jest.fn();
      render(
        <CommentsProvider
          value={{
            comments: testComments,
            pendingDeletions: [],
          }}
        >
          <EditableContentForm
            allowComments
            autoSave
            content="Test content"
            id="block-id"
            label="Form label"
            onCancel={noop}
            onSubmit={handleSubmit}
          />
        </CommentsProvider>,
      );

      const unresolvedComments = within(
        screen.getByTestId('unresolvedComments'),
      ).getAllByRole('listitem');

      expect(unresolvedComments).toHaveLength(3);

      userEvent.click(
        screen.getByRole('button', { name: 'Marker removed in editor' }),
      );

      await waitFor(() => {
        const updatedUnresolvedComments = within(
          screen.getByTestId('unresolvedComments'),
        ).getAllByRole('listitem');
        expect(updatedUnresolvedComments).toHaveLength(2);
      });
    });
  });
});
