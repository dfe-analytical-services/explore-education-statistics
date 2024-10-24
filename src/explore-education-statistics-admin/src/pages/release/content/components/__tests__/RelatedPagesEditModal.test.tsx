import RelatedPagesEditModal from '@admin/pages/release/content/components/RelatedPagesEditModal';
import { BasicLink } from '@common/services/publicationService';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('RelatedPagesEditModal', () => {
  const testRelatedPages: BasicLink[] = [
    { id: 'page-1', description: 'Page 1', url: 'https://gov.uk/1' },
    { id: 'page-2', description: 'Page 2', url: 'https://gov.uk/2' },
    { id: 'page-3', description: 'Page 3', url: 'https://gov.uk/3' },
  ];

  test('renders the table of related pages', async () => {
    const { user } = render(
      <RelatedPagesEditModal
        relatedPages={testRelatedPages}
        onUpdate={() => Promise.resolve()}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Edit pages' }));

    expect(
      await screen.findByRole('heading', { name: 'Edit related pages' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Reorder pages' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Close modal' }),
    ).toBeInTheDocument();

    const rows = screen.getAllByRole('row');

    const row1cells = within(rows[1]).getAllByRole('cell');
    expect(within(row1cells[0]).getByText('Page 1')).toBeInTheDocument();
    expect(
      within(row1cells[1]).getByRole('link', { name: 'https://gov.uk/1' }),
    ).toBeInTheDocument();
    expect(
      within(row1cells[2]).getByRole('button', { name: 'Edit Page 1' }),
    ).toBeInTheDocument();
    expect(
      within(row1cells[2]).getByRole('button', { name: 'Remove Page 1' }),
    ).toBeInTheDocument();

    const row2cells = within(rows[2]).getAllByRole('cell');
    expect(within(row2cells[0]).getByText('Page 2')).toBeInTheDocument();
    expect(
      within(row2cells[1]).getByRole('link', { name: 'https://gov.uk/2' }),
    ).toBeInTheDocument();
    expect(
      within(row2cells[2]).getByRole('button', { name: 'Edit Page 2' }),
    ).toBeInTheDocument();
    expect(
      within(row2cells[2]).getByRole('button', { name: 'Remove Page 2' }),
    ).toBeInTheDocument();

    const row3cells = within(rows[3]).getAllByRole('cell');
    expect(within(row3cells[0]).getByText('Page 3')).toBeInTheDocument();
    expect(
      within(row3cells[1]).getByRole('link', { name: 'https://gov.uk/3' }),
    ).toBeInTheDocument();
    expect(
      within(row3cells[2]).getByRole('button', { name: 'Edit Page 3' }),
    ).toBeInTheDocument();
    expect(
      within(row3cells[2]).getByRole('button', { name: 'Remove Page 3' }),
    ).toBeInTheDocument();
  });

  test('shows the edit form when click an edit button', async () => {
    const { user } = render(
      <RelatedPagesEditModal
        relatedPages={testRelatedPages}
        onUpdate={() => Promise.resolve()}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Edit pages' }));

    expect(
      await screen.findByRole('heading', { name: 'Edit related pages' }),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Edit Page 2' }));

    expect(screen.getByLabelText('Title')).toBeInTheDocument();
    expect(screen.getByLabelText('Title')).toHaveValue('Page 2');

    expect(screen.getByLabelText('Link URL')).toBeInTheDocument();
    expect(screen.getByLabelText('Link URL')).toHaveValue('https://gov.uk/2');

    expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('calls onUpdate when save an edited page', async () => {
    const handleUpdate = jest.fn();
    const { user } = render(
      <RelatedPagesEditModal
        relatedPages={testRelatedPages}
        onUpdate={handleUpdate}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Edit pages' }));

    expect(
      await screen.findByRole('heading', { name: 'Edit related pages' }),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Edit Page 2' }));

    await user.type(screen.getByLabelText('Title'), '-edited');

    expect(handleUpdate).not.toHaveBeenCalled();
    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(handleUpdate).toHaveBeenCalledTimes(1);
    expect(handleUpdate).toHaveBeenCalledWith([
      testRelatedPages[0],
      { id: 'page-2', description: 'Page 2-edited', url: 'https://gov.uk/2' },
      testRelatedPages[2],
    ]);
  });

  test('calls onUpdate when remove a page', async () => {
    const handleUpdate = jest.fn();
    const { user } = render(
      <RelatedPagesEditModal
        relatedPages={testRelatedPages}
        onUpdate={handleUpdate}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Edit pages' }));

    expect(
      await screen.findByRole('heading', { name: 'Edit related pages' }),
    ).toBeInTheDocument();

    expect(handleUpdate).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Remove Page 2' }));

    expect(handleUpdate).toHaveBeenCalledTimes(1);
    expect(handleUpdate).toHaveBeenCalledWith([
      testRelatedPages[0],
      testRelatedPages[2],
    ]);
  });

  test('shows the reordering UI when click the reorder button', async () => {
    const { user } = render(
      <RelatedPagesEditModal
        relatedPages={testRelatedPages}
        onUpdate={() => Promise.resolve()}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Edit pages' }));

    expect(
      await screen.findByRole('heading', { name: 'Edit related pages' }),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Reorder pages' }));

    expect(
      await screen.findByRole('heading', { name: 'Reorder pages' }),
    ).toBeInTheDocument();

    const reorderableList = screen.getAllByRole('listitem');

    expect(within(reorderableList[0]).getByText('Page 1')).toBeInTheDocument();
    expect(
      within(reorderableList[0]).getByRole('button', {
        name: 'Move Page 1 down',
      }),
    ).toBeInTheDocument();

    expect(within(reorderableList[1]).getByText('Page 2')).toBeInTheDocument();
    expect(
      within(reorderableList[1]).getByRole('button', {
        name: 'Move Page 2 up',
      }),
    ).toBeInTheDocument();
    expect(
      within(reorderableList[1]).getByRole('button', {
        name: 'Move Page 2 down',
      }),
    ).toBeInTheDocument();

    expect(within(reorderableList[2]).getByText('Page 3')).toBeInTheDocument();
    expect(
      within(reorderableList[2]).getByRole('button', {
        name: 'Move Page 3 up',
      }),
    ).toBeInTheDocument();
  });
});
