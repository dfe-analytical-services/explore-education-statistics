import {
  testComments,
  testCommentUser1,
} from '@admin/components/comments/__data__/testComments';
import EditableContentForm from '@admin/components/editable/EditableContentForm';
import { CommentsContextProvider } from '@admin/contexts/CommentsContext';
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

    test('renders correct form controls when `onAutoSave` is set', () => {
      render(
        <EditableContentForm
          content=""
          id="block-id"
          label="Form label"
          onAutoSave={noop}
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

    test('shows validation error if the form is submitted with no content', async () => {
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
            selector: '#block-id-form-content-error',
          }),
        ).toBeInTheDocument();
      });

      expect(handleSubmit).not.toHaveBeenCalled();
    });

    test('calls `onSubmit` handler with the content when form is submitted', async () => {
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

    test('shows error if `onSubmit` throws error when submitting form', async () => {
      const handleSubmit = jest.fn();
      handleSubmit.mockRejectedValue(new Error('Something went wrong'));

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
        expect(
          screen.getByText('Could not save content', {
            selector: '#block-id-form-content-error',
          }),
        ).toBeInTheDocument();
      });

      expect(handleSubmit).toHaveBeenCalledTimes(1);
    });

    test('calls `onAction` handler when user performs action within form', () => {
      const handleAction = jest.fn();

      render(
        <EditableContentForm
          content="Test content"
          id="block-id"
          label="Form label"
          onAction={handleAction}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      expect(handleAction).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('textbox'));

      expect(handleAction).toHaveBeenCalledTimes(1);
    });

    test('calls `onAction` handler only once within the `actionThrottle` time', async () => {
      jest.useFakeTimers();

      const handleAction = jest.fn();

      render(
        <EditableContentForm
          actionThrottle={1_000}
          content="Test content"
          id="block-id"
          label="Form label"
          onAction={handleAction}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      expect(handleAction).not.toHaveBeenCalled();

      const textbox = screen.getByRole('textbox');

      userEvent.click(textbox);
      expect(handleAction).toHaveBeenCalledTimes(1);

      jest.advanceTimersByTime(500);

      await userEvent.type(textbox, 'Test');
      expect(handleAction).toHaveBeenCalledTimes(1);

      jest.advanceTimersByTime(500);
      expect(handleAction).toHaveBeenCalledTimes(1);

      jest.useRealTimers();
    });

    test('calls `onIdle` handler when user has been idle for specified `idleTimeout`', () => {
      jest.useFakeTimers();

      const handleIdle = jest.fn();

      render(
        <EditableContentForm
          content="Test content"
          id="block-id"
          idleTimeout={5000}
          label="Form label"
          onCancel={noop}
          onSubmit={noop}
          onIdle={handleIdle}
        />,
      );

      expect(handleIdle).not.toHaveBeenCalled();

      jest.advanceTimersByTime(5000);

      expect(handleIdle).toHaveBeenCalledTimes(1);

      jest.useRealTimers();
    });
  });

  describe('comments list', () => {
    test('renders comments correctly if `allowComments`', () => {
      render(
        <CommentsContextProvider
          comments={testComments}
          onDelete={noop}
          onCreate={jest.fn()}
          onUpdate={noop}
          onPendingDelete={noop}
          onPendingDeleteUndo={noop}
        >
          <EditableContentForm
            allowComments
            content="Test content"
            id="block-id"
            label="Form label"
            onCancel={noop}
            onSubmit={noop}
          />
        </CommentsContextProvider>,
      );

      const unresolvedComments = within(
        screen.getByTestId('unresolvedComments'),
      ).getAllByTestId('comment');

      expect(unresolvedComments).toHaveLength(3);
      expect(unresolvedComments[0]).toHaveTextContent('Comment 2 content');
      expect(unresolvedComments[1]).toHaveTextContent('Comment 3 content');
      expect(unresolvedComments[2]).toHaveTextContent('Comment 4 content');

      const resolvedComments = within(
        screen.getByTestId('resolvedComments'),
      ).getAllByTestId('comment');

      expect(resolvedComments).toHaveLength(2);
      expect(resolvedComments[0]).toHaveTextContent('Comment 1 content');
      expect(resolvedComments[1]).toHaveTextContent('Comment 5 content');
    });

    test('does not renders the comments if `allowComments = false`', () => {
      render(
        <CommentsContextProvider
          comments={testComments}
          onDelete={noop}
          onCreate={jest.fn()}
          onUpdate={noop}
          onPendingDelete={noop}
          onPendingDeleteUndo={noop}
        >
          <EditableContentForm
            content="Test content"
            id="block-id"
            label="Form label"
            onCancel={noop}
            onSubmit={noop}
          />
        </CommentsContextProvider>,
      );

      expect(
        screen.queryByTestId('unresolvedComments'),
      ).not.toBeInTheDocument();
      expect(screen.queryByTestId('resolvedComments')).not.toBeInTheDocument();
    });

    test('calls `onPendingDelete` handler when a comment delete button is clicked', async () => {
      const handlePendingDelete = jest.fn();

      render(
        <AuthContext.Provider
          value={{
            user: testUser1,
          }}
        >
          <CommentsContextProvider
            comments={testComments}
            onDelete={noop}
            onCreate={jest.fn()}
            onUpdate={noop}
            onPendingDelete={handlePendingDelete}
            onPendingDeleteUndo={noop}
          >
            <EditableContentForm
              allowComments
              content="Test content"
              id="block-id"
              label="Form label"
              onCancel={noop}
              onSubmit={noop}
            />
          </CommentsContextProvider>
        </AuthContext.Provider>,
      );

      expect(handlePendingDelete).not.toHaveBeenCalled();

      const unresolvedComments = within(
        screen.getByTestId('unresolvedComments'),
      ).getAllByTestId('comment');

      userEvent.click(
        within(unresolvedComments[0]).getByRole('button', {
          name: 'Delete',
        }),
      );

      await waitFor(() => {
        expect(handlePendingDelete).toHaveBeenCalledTimes(1);
        expect(handlePendingDelete).toHaveBeenCalledWith(testComments[1].id);
      });
    });

    test('calls `onUpdate` handler when a comment is edited', async () => {
      const handleUpdate = jest.fn();

      render(
        <AuthContext.Provider
          value={{
            user: testUser1,
          }}
        >
          <CommentsContextProvider
            comments={testComments}
            onDelete={noop}
            onCreate={jest.fn()}
            onUpdate={handleUpdate}
            onPendingDelete={noop}
            onPendingDeleteUndo={noop}
          >
            <EditableContentForm
              allowComments
              content="Test content"
              id="block-id"
              label="Form label"
              onCancel={noop}
              onSubmit={noop}
            />
          </CommentsContextProvider>
        </AuthContext.Provider>,
      );

      expect(handleUpdate).not.toHaveBeenCalled();

      const unresolvedComments = within(
        screen.getByTestId('unresolvedComments'),
      ).getAllByTestId('comment');

      const comment = within(unresolvedComments[0]);

      userEvent.click(comment.getByRole('button', { name: 'Edit' }));
      userEvent.clear(comment.getByRole('textbox'));
      await userEvent.type(
        comment.getByRole('textbox'),
        'Test updated content',
      );

      userEvent.click(comment.getByRole('button', { name: 'Update' }));

      await waitFor(() => {
        expect(handleUpdate).toHaveBeenCalledTimes(1);
        expect(handleUpdate).toHaveBeenCalledWith({
          ...testComments[1],
          content: 'Test updated content',
        });
      });
    });

    test('calls `onUpdate` handler when a comment is resolved', async () => {
      const handleUpdate = jest.fn();

      render(
        <AuthContext.Provider
          value={{
            user: testUser1,
          }}
        >
          <CommentsContextProvider
            comments={testComments}
            onDelete={noop}
            onCreate={jest.fn()}
            onUpdate={handleUpdate}
            onPendingDelete={noop}
            onPendingDeleteUndo={noop}
          >
            <EditableContentForm
              allowComments
              content="Test content"
              id="block-id"
              label="Form label"
              onCancel={noop}
              onSubmit={noop}
            />
          </CommentsContextProvider>
        </AuthContext.Provider>,
      );

      expect(handleUpdate).not.toHaveBeenCalled();

      const unresolvedComments = within(
        screen.getByTestId('unresolvedComments'),
      ).getAllByTestId('comment');

      const comment = within(unresolvedComments[0]);

      userEvent.click(comment.getByRole('button', { name: 'Resolve' }));

      await waitFor(() => {
        expect(handleUpdate).toHaveBeenCalledTimes(1);
        expect(handleUpdate).toHaveBeenCalledWith({
          ...testComments[1],
          setResolved: true,
        });
      });
    });

    test('calls `onUpdate` handler when a comment is unresolved', async () => {
      const handleUpdate = jest.fn();

      render(
        <AuthContext.Provider
          value={{
            user: testUser1,
          }}
        >
          <CommentsContextProvider
            comments={testComments}
            onDelete={noop}
            onCreate={jest.fn()}
            onUpdate={handleUpdate}
            onPendingDelete={noop}
            onPendingDeleteUndo={noop}
          >
            <EditableContentForm
              allowComments
              content="Test content"
              id="block-id"
              label="Form label"
              onCancel={noop}
              onSubmit={noop}
            />
          </CommentsContextProvider>
        </AuthContext.Provider>,
      );

      expect(handleUpdate).not.toHaveBeenCalled();

      const resolvedComments = within(
        screen.getByTestId('resolvedComments'),
      ).getAllByTestId('comment');

      const comment = within(resolvedComments[0]);

      userEvent.click(
        comment.getByRole('button', { name: 'Unresolve', hidden: true }),
      );

      await waitFor(() => {
        expect(handleUpdate).toHaveBeenCalledTimes(1);
        expect(handleUpdate).toHaveBeenCalledWith({
          ...testComments[0],
          setResolved: false,
        });
      });
    });
  });
});
