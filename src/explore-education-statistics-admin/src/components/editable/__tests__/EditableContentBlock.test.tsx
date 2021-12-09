import { testComments } from '@admin/components/comments/__data__/testComments';
import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import { CommentsProvider } from '@admin/contexts/CommentsContext';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('EditableContentBlock', () => {
  const testMarkdown = `
## Test heading
    
Test paragraph
`;

  test('renders editable version correctly', () => {
    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value="<p>Test content</p>"
        onSave={noop}
        onDelete={noop}
      />,
    );

    expect(
      screen.getByText('Test content', { selector: 'p' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Edit block' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove block' }),
    ).toBeInTheDocument();
  });

  test('renders editable version with markdown content correctly', () => {
    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value={testMarkdown}
        useMarkdown
        onSave={noop}
        onDelete={noop}
      />,
    );

    expect(
      screen.getByText('Test heading', { selector: 'h2' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('Test paragraph', { selector: 'p' }),
    ).toBeInTheDocument();
  });

  test('renders non-editable version correctly', () => {
    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value="<p>Test content</p>"
        editable={false}
        onSave={noop}
        onDelete={noop}
      />,
    );

    expect(
      screen.getByText('Test content', { selector: 'p' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Edit block' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove block' }),
    ).not.toBeInTheDocument();
  });

  test('renders non-editable version with markdown content correctly', () => {
    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value={testMarkdown}
        editable={false}
        useMarkdown
        onSave={noop}
        onDelete={noop}
      />,
    );

    expect(
      screen.getByText('Test heading', { selector: 'h2' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('Test paragraph', { selector: 'p' }),
    ).toBeInTheDocument();
  });

  test('clicking `Edit block` button shows editor', () => {
    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value="<p>Test content</p>"
        onSave={noop}
        onDelete={noop}
      />,
    );

    expect(screen.queryByLabelText('Block content')).not.toBeInTheDocument();

    userEvent.click(screen.getByRole('button', { name: 'Edit block' }));

    expect(screen.getByLabelText('Block content')).toBeInTheDocument();
  });

  test('clicking `Edit block` and `Save` buttons calls `onSave` handler', async () => {
    const handleSave = jest.fn();

    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value="<p>Test content</p>"
        onSave={handleSave}
        onDelete={noop}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Edit block' }));

    expect(handleSave).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSave).toHaveBeenCalledTimes(1);
      expect(handleSave).toHaveBeenCalledWith('<p>Test content</p>', undefined);
    });
  });

  test('clicking `Remove block` and `Confirm` buttons calls `onDelete` handler', async () => {
    const handleDelete = jest.fn();

    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value="<p>Test content</p>"
        onSave={noop}
        onDelete={handleDelete}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Remove block' }));

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();
    });

    expect(handleDelete).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(handleDelete).toHaveBeenCalledTimes(1);
      expect(handleDelete).toHaveBeenCalled();
    });
  });

  describe('comments allowed', () => {
    test('renders the view comments button with the number of unresolved comments', () => {
      render(
        <CommentsProvider
          value={{
            comments: testComments,
            onDeletePendingComment: jest.fn(),
            onSaveComment: jest.fn(),
            onSaveUpdatedComment: jest.fn(),
          }}
        >
          <EditableContentBlock
            allowComments
            id="test-id"
            label="Block content"
            value="<p>Test content</p>"
            onSave={noop}
            onDelete={noop}
          />
        </CommentsProvider>,
      );

      expect(
        screen.getByRole('button', { name: 'View comments (3 unresolved)' }),
      ).toBeInTheDocument();
    });

    test('clicking the view comments button opens the editor', () => {
      render(
        <CommentsProvider
          value={{
            comments: testComments,
            onDeletePendingComment: jest.fn(),
            onSaveComment: jest.fn(),
            onSaveUpdatedComment: jest.fn(),
          }}
        >
          <EditableContentBlock
            allowComments
            id="test-id"
            label="Block content"
            value="<p>Test content</p>"
            onSave={noop}
            onDelete={noop}
          />
        </CommentsProvider>,
      );

      userEvent.click(
        screen.getByRole('button', { name: 'View comments (3 unresolved)' }),
      );

      expect(screen.getByLabelText('Block content')).toBeInTheDocument();
    });
  });
});
