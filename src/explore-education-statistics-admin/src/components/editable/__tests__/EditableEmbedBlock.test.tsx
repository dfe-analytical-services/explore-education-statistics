import EditableEmbedBlock from '@admin/components/editable/EditableEmbedBlock';
import { EmbedBlock } from '@common/services/types/blocks';
import {
  render as baseRender,
  RenderResult,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React, { ReactNode } from 'react';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';

describe('EditableEmbedBlock', () => {
  const testEmbedBlock: EmbedBlock = {
    id: 'embed-block-id',
    order: 0,
    title: 'Dashboard title',
    type: 'EmbedBlockLink',
    url: 'https://department-for-education.shinyapps.io/test-dashboard',
  };

  test('renders the iframe', () => {
    render(
      <EditableEmbedBlock
        block={testEmbedBlock}
        editable
        onDelete={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByTitle('Dashboard title')).toHaveAttribute(
      'src',
      'https://department-for-education.shinyapps.io/test-dashboard',
    );
  });

  test('renders the edit and delete buttons if editable', () => {
    render(
      <EditableEmbedBlock
        block={testEmbedBlock}
        editable
        onDelete={noop}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('button', { name: 'Edit embedded URL' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Remove block' }),
    ).toBeInTheDocument();
  });

  test('does not render the edit and delete buttons if not editable', () => {
    render(
      <EditableEmbedBlock
        block={testEmbedBlock}
        editable={false}
        onDelete={noop}
        onSubmit={noop}
      />,
    );

    expect(
      screen.queryByRole('button', { name: 'Edit embedded URL' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Remove block' }),
    ).not.toBeInTheDocument();
  });

  test('shows the edit modal when the edit button is clicked', () => {
    render(
      <EditableEmbedBlock
        block={testEmbedBlock}
        editable
        onDelete={noop}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Edit embedded URL' }));

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByRole('heading', { name: 'Edit embedded URL' }),
    ).toBeInTheDocument();
    expect(modal.getByLabelText('Title')).toBeInTheDocument();
    expect(modal.getByLabelText('URL')).toBeInTheDocument();
    expect(modal.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(modal.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('calls `onSubmit` with the updated values when save', async () => {
    const handleSubmit = jest.fn();
    render(
      <EditableEmbedBlock
        block={testEmbedBlock}
        editable
        onDelete={noop}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Edit embedded URL' }));

    const modal = within(screen.getByRole('dialog'));

    userEvent.type(modal.getByLabelText('Title'), '-edited');
    userEvent.type(modal.getByLabelText('URL'), '-edited');

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(modal.getByRole('button', { name: 'Save' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        id: 'embed-block-id',
        order: 0,
        title: 'Dashboard title-edited',
        type: 'EmbedBlockLink',
        url: 'https://department-for-education.shinyapps.io/test-dashboard-edited',
      });
    });
  });

  test('when clicking the remove block button it shows the modal and calls `onDelete` on confirm', async () => {
    const handleDelete = jest.fn();
    render(
      <EditableEmbedBlock
        block={testEmbedBlock}
        editable
        onDelete={handleDelete}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Remove block' }));

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByRole('heading', { name: 'Remove block' }),
    ).toBeInTheDocument();

    expect(handleDelete).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(handleDelete).toHaveBeenCalled();
    });
  });

  function render(child: ReactNode): RenderResult {
    return baseRender(
      <TestConfigContextProvider>{child}</TestConfigContextProvider>,
    );
  }
});
