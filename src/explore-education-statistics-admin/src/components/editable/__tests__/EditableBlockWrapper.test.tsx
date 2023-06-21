import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { MemoryRouter } from 'react-router';

describe('EditableBlockWrapper', () => {
  test('renders the child block', () => {
    render(
      <EditableBlockWrapper>
        <div>Child block</div>
      </EditableBlockWrapper>,
    );

    expect(screen.getByText('Child block')).toBeInTheDocument();
  });

  test('renders the edit block button and calls `onEdit` when clicked when `onEdit` is provided', () => {
    const handleEdit = jest.fn();
    render(
      <EditableBlockWrapper onEdit={handleEdit}>
        <div>Child block</div>
      </EditableBlockWrapper>,
    );

    expect(handleEdit).not.toHaveBeenCalled();
    userEvent.click(screen.getByRole('button', { name: 'Edit block' }));
    expect(handleEdit).toHaveBeenCalled();
  });

  test('does not render the edit block button when `onEdit` is not provided', () => {
    render(
      <EditableBlockWrapper>
        <div>Child block</div>
      </EditableBlockWrapper>,
    );

    expect(
      screen.queryByRole('button', { name: 'Edit block' }),
    ).not.toBeInTheDocument();
  });

  test('renders the edit data block link when `dataBlockEditLink` is provided', () => {
    render(
      <MemoryRouter>
        <EditableBlockWrapper dataBlockEditLink="/test-url">
          <div>Child block</div>
        </EditableBlockWrapper>
      </MemoryRouter>,
    );

    const link = screen.getByRole('link', { name: 'Edit data block' });
    expect(link).toHaveAttribute('href', '/test-url');
  });

  test('does not render the edit data block link when `dataBlockEditLink` is not provided', () => {
    render(
      <EditableBlockWrapper>
        <div>Child block</div>
      </EditableBlockWrapper>,
    );

    expect(
      screen.queryByRole('link', { name: 'Edit data block' }),
    ).not.toBeInTheDocument();
  });

  test('renders the Edit embedded URL button when `onEmbedBlockEdit` is provided', () => {
    const handleEmbedBlockEdit = jest.fn();
    render(
      <EditableBlockWrapper onEmbedBlockEdit={handleEmbedBlockEdit}>
        <div>Child block</div>
      </EditableBlockWrapper>,
    );

    expect(
      screen.getByRole('button', { name: 'Edit embedded URL' }),
    ).toBeInTheDocument();
  });

  test('calls `onEmbedBlockEdit` when the edit dashboard button is clicked', () => {
    const handleEmbedBlockEdit = jest.fn();
    render(
      <EditableBlockWrapper onEmbedBlockEdit={handleEmbedBlockEdit}>
        <div>Child block</div>
      </EditableBlockWrapper>,
    );

    expect(handleEmbedBlockEdit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Edit embedded URL' }));

    expect(handleEmbedBlockEdit).toHaveBeenCalled();
  });

  test('does not render the Edit embedded URL button when `onEmbedBlockEdit` is not provided', () => {
    render(
      <EditableBlockWrapper>
        <div>Child block</div>
      </EditableBlockWrapper>,
    );

    expect(
      screen.queryByRole('button', { name: 'Edit embedded URL' }),
    ).not.toBeInTheDocument();
  });

  test('renders the remove block button when `onDelete` is provided', () => {
    const handleDelete = jest.fn();
    render(
      <EditableBlockWrapper onDelete={handleDelete}>
        <div>Child block</div>
      </EditableBlockWrapper>,
    );

    expect(
      screen.getByRole('button', { name: 'Remove block' }),
    ).toBeInTheDocument();
  });

  test('when the remove block button is clicked a modal is shown which calls `onDelete` on confirm', async () => {
    const handleDelete = jest.fn();
    render(
      <EditableBlockWrapper onDelete={handleDelete}>
        <div>Child block</div>
      </EditableBlockWrapper>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Remove block' }));

    const modal = within(screen.getByRole('dialog'));

    await waitFor(() => {
      expect(modal.getByText('Remove block')).toBeInTheDocument();
    });

    expect(handleDelete).not.toHaveBeenCalled();
    userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
    expect(handleDelete).toHaveBeenCalled();
  });

  test('does not render the remove block button when `onDelete` is not provided', () => {
    render(
      <EditableBlockWrapper>
        <div>Child block</div>
      </EditableBlockWrapper>,
    );

    expect(
      screen.queryByRole('button', { name: 'Remove block' }),
    ).not.toBeInTheDocument();
  });
});
