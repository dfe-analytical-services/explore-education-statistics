import { testComments } from '@admin/components/comments/__data__/testComments';
import EditableContentBlock from '@admin/components/editable/EditableContentBlock';
import { CommentsContextProvider } from '@admin/contexts/CommentsContext';
import { getDescribedBy } from '@common-test/queries';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('EditableContentBlock', () => {
  const testMarkdown = `
## Test heading
    
Test paragraph
`;

  const testOrphanedCommentHtml = `
<p>
  Test <comment-start name="comment-1"></comment-start>unresolved<comment-end name="comment-1"></comment-end> and 
  <resolvedcomment-start name="comment-2"></resolvedcomment-start>resolved<resolvedcomment-end name="comment-2"></resolvedcomment-end>
</p>`;

  test('renders editable version correctly', () => {
    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value="<p>Test content</p>"
        onEditing={noop}
        onCancel={noop}
        onSubmit={noop}
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

  test('renders editable version without orphaned comments', () => {
    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value={testOrphanedCommentHtml}
        onEditing={noop}
        onCancel={noop}
        onSubmit={noop}
        onDelete={noop}
      />,
    );

    const paragraph = screen.getByText('Test unresolved and resolved', {
      selector: 'p',
    });

    // Markup should not contain comment elements
    expect(paragraph.innerHTML).toMatchInlineSnapshot(`
      "
        Test unresolved and 
        resolved
      "
    `);
  });

  test('renders editable version with markdown content correctly', () => {
    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value={testMarkdown}
        useMarkdown
        onEditing={noop}
        onCancel={noop}
        onSubmit={noop}
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
        onEditing={noop}
        onCancel={noop}
        onSubmit={noop}
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

  test('renders non-editable version without orphaned comments', () => {
    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value={testOrphanedCommentHtml}
        editable={false}
        useMarkdown
        onEditing={noop}
        onCancel={noop}
        onSubmit={noop}
        onDelete={noop}
      />,
    );

    const paragraph = screen.getByText('Test unresolved and resolved', {
      selector: 'p',
    });

    // Markup should not contain comment elements
    expect(paragraph.innerHTML).toMatchInlineSnapshot(`
      "
        Test unresolved and 
        resolved
      "
    `);
  });

  test('renders locked version correctly', () => {
    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value="<p>Test content</p>"
        locked="2022-02-16T12:00:00Z"
        lockedBy={{
          displayName: 'Jane Doe',
          email: 'jane@test.com',
          id: 'user-1',
        }}
        onEditing={noop}
        onCancel={noop}
        onSubmit={noop}
        onDelete={noop}
      />,
    );

    expect(
      screen.getByText('Test content', { selector: 'p' }),
    ).toBeInTheDocument();

    const editButton = screen.getByRole('button', { name: 'Edit block' });

    expect(editButton).toBeAriaDisabled();
    expect(getDescribedBy(editButton)).toHaveTextContent(
      'This block is being edited by Jane Doe',
    );

    const removeButton = screen.getByRole('button', { name: 'Remove block' });

    expect(removeButton).toBeAriaDisabled();
    expect(getDescribedBy(removeButton)).toHaveTextContent(
      'This block is being edited by Jane Doe',
    );

    expect(
      screen.getByText(
        'Jane Doe (jane@test.com) is currently editing this block (last updated 12:00)',
      ),
    );

    expect(screen.getByText('Jane Doe is editing')).toBeInTheDocument();
  });

  test('renders non-editable version with markdown content correctly', () => {
    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value={testMarkdown}
        editable={false}
        useMarkdown
        onEditing={noop}
        onCancel={noop}
        onSubmit={noop}
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

  test('clicking `Edit block` button calls `onEditing` handler', () => {
    const handleEditing = jest.fn();

    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value="<p>Test content</p>"
        onEditing={handleEditing}
        onCancel={noop}
        onSubmit={noop}
        onDelete={noop}
      />,
    );

    expect(screen.queryByLabelText('Block content')).not.toBeInTheDocument();
    expect(handleEditing).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Edit block' }));

    expect(handleEditing).toHaveBeenCalledTimes(1);
  });

  test('clicking `Save` buttons calls `onSave` handler', async () => {
    const handleSave = jest.fn();

    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value="<p>Test content</p>"
        isEditing
        onCancel={noop}
        onEditing={noop}
        onSubmit={handleSave}
        onDelete={noop}
      />,
    );
    expect(handleSave).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSave).toHaveBeenCalledTimes(1);
      expect(handleSave).toHaveBeenCalledWith('<p>Test content</p>');
    });
  });

  test('clicking `Remove block` and `Confirm` buttons calls `onDelete` handler', async () => {
    const handleDelete = jest.fn();

    render(
      <EditableContentBlock
        id="test-id"
        label="Block content"
        value="<p>Test content</p>"
        onCancel={noop}
        onEditing={noop}
        onSubmit={noop}
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
    });
  });

  describe('comments allowed', () => {
    test('renders the `View comments` button with the number of unresolved comments', () => {
      render(
        <CommentsContextProvider
          comments={testComments}
          onDelete={noop}
          onCreate={jest.fn()}
          onUpdate={noop}
          onPendingDelete={noop}
          onPendingDeleteUndo={noop}
        >
          <EditableContentBlock
            allowComments
            id="test-id"
            label="Block content"
            value="<p>Test content</p>"
            onCancel={noop}
            onEditing={noop}
            onSubmit={noop}
            onDelete={noop}
          />
        </CommentsContextProvider>,
      );

      expect(
        screen.getByRole('button', { name: 'View comments (3 unresolved)' }),
      ).toBeInTheDocument();
    });

    test('clicking the `View comments` button calls the `onEditing` handler', () => {
      const handleEditing = jest.fn();

      render(
        <CommentsContextProvider
          comments={testComments}
          onDelete={noop}
          onCreate={jest.fn()}
          onUpdate={noop}
          onPendingDelete={noop}
          onPendingDeleteUndo={noop}
        >
          <EditableContentBlock
            allowComments
            id="test-id"
            label="Block content"
            value="<p>Test content</p>"
            onEditing={handleEditing}
            onCancel={noop}
            onSubmit={noop}
            onDelete={noop}
          />
        </CommentsContextProvider>,
      );

      userEvent.click(
        screen.getByRole('button', { name: 'View comments (3 unresolved)' }),
      );

      expect(handleEditing).toHaveBeenCalledTimes(1);
    });
  });
});
