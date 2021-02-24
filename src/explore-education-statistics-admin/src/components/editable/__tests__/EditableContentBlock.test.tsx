import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';
import EditableContentBlock from '../EditableContentBlock';

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
});
