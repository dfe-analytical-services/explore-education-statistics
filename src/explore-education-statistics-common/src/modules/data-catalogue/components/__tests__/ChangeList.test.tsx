import render from '@common-test/render';
import ChangeList from '@common/modules/data-catalogue/components/ChangeList';
import { Change } from '@common/services/types/apiDataSetChanges';
import { FilterOption } from '@common/services/types/apiDataSetMeta';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('ChangeList', () => {
  test('renders nothing if there are no changes', () => {
    const { container } = render(
      <ChangeList changes={[]} metaType="filterOptions" />,
    );

    expect(container).toBeEmptyDOMElement();
  });

  test('renders a list of additions', () => {
    const testAdditions: Change<FilterOption>[] = [
      { currentState: { id: 'filter-opt-1', label: 'Filter option 1' } },
      { currentState: { id: 'filter-opt-2', label: 'Filter option 2' } },
      { currentState: { id: 'filter-opt-3', label: 'Filter option 3' } },
    ];

    render(<ChangeList changes={testAdditions} metaType="filterOptions" />);

    expect(
      screen.getByRole('heading', { name: 'New filter options', level: 4 }),
    ).toBeInTheDocument();

    const added = within(
      screen.getByTestId('added-filterOptions'),
    ).getAllByRole('listitem');

    expect(added[0]).toHaveTextContent('Filter option 1 (id: filter-opt-1)');
    expect(added[1]).toHaveTextContent('Filter option 2 (id: filter-opt-2)');
    expect(added[2]).toHaveTextContent('Filter option 3 (id: filter-opt-3)');

    expect(
      screen.queryByRole('heading', { name: 'Deleted filter options' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Updated filter options' }),
    ).not.toBeInTheDocument();
  });

  test('renders a list of deletions', () => {
    const testChanges: Change<FilterOption>[] = [
      { previousState: { id: 'filter-opt-1', label: 'Filter option 1' } },
      { previousState: { id: 'filter-opt-2', label: 'Filter option 2' } },
      { previousState: { id: 'filter-opt-3', label: 'Filter option 3' } },
    ];

    render(<ChangeList changes={testChanges} metaType="filterOptions" />);

    expect(
      screen.getByRole('heading', { name: 'Deleted filter options', level: 4 }),
    ).toBeInTheDocument();

    const deleted = within(
      screen.getByTestId('deleted-filterOptions'),
    ).getAllByRole('listitem');

    expect(deleted[0]).toHaveTextContent('Filter option 1 (id: filter-opt-1)');
    expect(deleted[1]).toHaveTextContent('Filter option 2 (id: filter-opt-2)');
    expect(deleted[2]).toHaveTextContent('Filter option 3 (id: filter-opt-3)');

    expect(
      screen.queryByRole('heading', { name: 'New filter options' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'Updated filter options' }),
    ).not.toBeInTheDocument();
  });

  test('renders a list of updates', () => {
    const testChanges: Change<FilterOption>[] = [
      {
        previousState: { id: 'filter-opt-1', label: 'Filter option 1' },
        currentState: { id: 'filter-opt-1', label: 'Filter option 1 updated' },
      },
      {
        previousState: { id: 'filter-opt-2', label: 'Filter option 2' },
        currentState: { id: 'filter-opt-2-updated', label: 'Filter option 2' },
      },
      {
        previousState: { id: 'filter-opt-3', label: 'Filter option 3' },
        currentState: {
          id: 'filter-opt-3',
          label: 'Filter option 3 updated',
          isAutoSelect: true,
        },
      },
    ];

    render(<ChangeList changes={testChanges} metaType="filterOptions" />);

    expect(
      screen.getByRole('heading', { name: 'Updated filter options', level: 4 }),
    ).toBeInTheDocument();

    const updates = within(
      screen.getByTestId('updated-filterOptions'),
    ).getAllByTestId('updated-item');

    expect(updates[0]).toHaveTextContent('Filter option 1 (id: filter-opt-1)');
    expect(updates[0]).toHaveTextContent(
      'label changed to: Filter option 1 updated',
    );

    expect(updates[1]).toHaveTextContent('Filter option 2 (id: filter-opt-2)');
    expect(updates[1]).toHaveTextContent('id changed to: filter-opt-2-updated');

    expect(updates[2]).toHaveTextContent('Filter option 3 (id: filter-opt-3)');
    expect(updates[2]).toHaveTextContent('changed to be the default option');

    expect(
      screen.queryByRole('heading', { name: 'Deleted filter options' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('heading', { name: 'New filter options' }),
    ).not.toBeInTheDocument();
  });

  test('renders separate lists of additions and updates', () => {
    const testChanges: Change<FilterOption>[] = [
      { currentState: { id: 'filter-opt-1', label: 'Filter option 1' } },
      { currentState: { id: 'filter-opt-2', label: 'Filter option 2' } },
      {
        previousState: { id: 'filter-opt-3', label: 'Filter option 3' },
        currentState: { id: 'filter-opt-3', label: 'Filter option 3 updated' },
      },
      {
        previousState: { id: 'filter-opt-4', label: 'Filter option 4' },
        currentState: { id: 'filter-opt-4-updated', label: 'Filter option 4' },
      },
    ];

    render(<ChangeList changes={testChanges} metaType="filterOptions" />);

    expect(
      screen.getByRole('heading', {
        name: 'Changes to filter options',
        level: 4,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Updated', level: 5 }),
    ).toBeInTheDocument();

    const updated = within(
      screen.getByTestId('updated-filterOptions'),
    ).getAllByTestId('updated-item');

    expect(updated).toHaveLength(2);

    expect(updated[0]).toHaveTextContent('Filter option 3 (id: filter-opt-3)');
    expect(updated[0]).toHaveTextContent(
      'label changed to: Filter option 3 updated',
    );

    expect(updated[1]).toHaveTextContent('Filter option 4 (id: filter-opt-4)');
    expect(updated[1]).toHaveTextContent('id changed to: filter-opt-4-updated');

    expect(
      screen.getByRole('heading', { name: 'New', level: 5 }),
    ).toBeInTheDocument();

    const added = within(
      screen.getByTestId('added-filterOptions'),
    ).getAllByRole('listitem');

    expect(added).toHaveLength(2);

    expect(added[0]).toHaveTextContent('Filter option 1 (id: filter-opt-1)');
    expect(added[1]).toHaveTextContent('Filter option 2 (id: filter-opt-2)');

    expect(
      screen.queryByRole('heading', { name: 'Deleted' }),
    ).not.toBeInTheDocument();
  });

  test('renders separate lists of updates and deletions', () => {
    const testChanges: Change<FilterOption>[] = [
      {
        previousState: { id: 'filter-opt-3', label: 'Filter option 3' },
        currentState: { id: 'filter-opt-3', label: 'Filter option 3 updated' },
      },
      {
        previousState: { id: 'filter-opt-4', label: 'Filter option 4' },
        currentState: { id: 'filter-opt-4-updated', label: 'Filter option 4' },
      },
      { previousState: { id: 'filter-opt-5', label: 'Filter option 5' } },
      { previousState: { id: 'filter-opt-6', label: 'Filter option 6' } },
    ];

    render(<ChangeList changes={testChanges} metaType="filterOptions" />);

    expect(
      screen.getByRole('heading', {
        name: 'Changes to filter options',
        level: 4,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Deleted', level: 5 }),
    ).toBeInTheDocument();

    const deleted = within(
      screen.getByTestId('deleted-filterOptions'),
    ).getAllByRole('listitem');

    expect(deleted).toHaveLength(2);

    expect(deleted[0]).toHaveTextContent('Filter option 5 (id: filter-opt-5)');
    expect(deleted[1]).toHaveTextContent('Filter option 6 (id: filter-opt-6)');

    expect(
      screen.getByRole('heading', { name: 'Updated', level: 5 }),
    ).toBeInTheDocument();

    const updated = within(
      screen.getByTestId('updated-filterOptions'),
    ).getAllByTestId('updated-item');

    expect(updated).toHaveLength(2);

    expect(updated[0]).toHaveTextContent('Filter option 3 (id: filter-opt-3)');
    expect(updated[0]).toHaveTextContent(
      'label changed to: Filter option 3 updated',
    );

    expect(updated[1]).toHaveTextContent('Filter option 4 (id: filter-opt-4)');
    expect(updated[1]).toHaveTextContent('id changed to: filter-opt-4-updated');

    expect(
      screen.queryByRole('heading', { name: 'New' }),
    ).not.toBeInTheDocument();
  });

  test('renders separate lists of additions, updates and deletions', () => {
    const testChanges: Change<FilterOption>[] = [
      { currentState: { id: 'filter-opt-1', label: 'Filter option 1' } },
      { currentState: { id: 'filter-opt-2', label: 'Filter option 2' } },
      {
        previousState: { id: 'filter-opt-3', label: 'Filter option 3' },
        currentState: { id: 'filter-opt-3', label: 'Filter option 3 updated' },
      },
      {
        previousState: { id: 'filter-opt-4', label: 'Filter option 4' },
        currentState: { id: 'filter-opt-4-updated', label: 'Filter option 4' },
      },
      { previousState: { id: 'filter-opt-5', label: 'Filter option 5' } },
      { previousState: { id: 'filter-opt-6', label: 'Filter option 6' } },
    ];

    render(<ChangeList changes={testChanges} metaType="filterOptions" />);

    expect(
      screen.getByRole('heading', {
        name: 'Changes to filter options',
        level: 4,
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Deleted', level: 5 }),
    ).toBeInTheDocument();

    const deleted = within(
      screen.getByTestId('deleted-filterOptions'),
    ).getAllByRole('listitem');

    expect(deleted).toHaveLength(2);

    expect(deleted[0]).toHaveTextContent('Filter option 5 (id: filter-opt-5)');
    expect(deleted[1]).toHaveTextContent('Filter option 6 (id: filter-opt-6)');

    expect(
      screen.getByRole('heading', { name: 'Updated', level: 5 }),
    ).toBeInTheDocument();

    const updated = within(
      screen.getByTestId('updated-filterOptions'),
    ).getAllByTestId('updated-item');

    expect(updated).toHaveLength(2);

    expect(updated[0]).toHaveTextContent('Filter option 3 (id: filter-opt-3)');
    expect(updated[0]).toHaveTextContent(
      'label changed to: Filter option 3 updated',
    );

    expect(updated[1]).toHaveTextContent('Filter option 4 (id: filter-opt-4)');
    expect(updated[1]).toHaveTextContent('id changed to: filter-opt-4-updated');

    expect(
      screen.getByRole('heading', { name: 'New', level: 5 }),
    ).toBeInTheDocument();

    const added = within(
      screen.getByTestId('added-filterOptions'),
    ).getAllByRole('listitem');

    expect(added).toHaveLength(2);

    expect(added[0]).toHaveTextContent('Filter option 1 (id: filter-opt-1)');
    expect(added[1]).toHaveTextContent('Filter option 2 (id: filter-opt-2)');
  });

  test('renders correct heading for custom `metaTypeLabel` and only additions', () => {
    const testChanges: Change<FilterOption>[] = [
      { currentState: { id: 'filter-opt-1', label: 'Filter option 1' } },
    ];

    render(
      <ChangeList
        changes={testChanges}
        metaType="filterOptions"
        metaTypeLabel="Test filter options"
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'New Test filter options',
        level: 4,
      }),
    ).toBeInTheDocument();
  });

  test('renders correct heading for custom `metaTypeLabel` and only deletions', () => {
    const testChanges: Change<FilterOption>[] = [
      { previousState: { id: 'filter-opt-1', label: 'Filter option 1' } },
    ];

    render(
      <ChangeList
        changes={testChanges}
        metaType="filterOptions"
        metaTypeLabel="Test filter options"
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Deleted Test filter options',
        level: 4,
      }),
    ).toBeInTheDocument();
  });

  test('renders correct heading for custom `metaTypeLabel` and only updates', () => {
    const testChanges: Change<FilterOption>[] = [
      {
        previousState: { id: 'filter-opt-1', label: 'Filter option 1' },
        currentState: { id: 'filter-opt-1', label: 'Filter option 1 updated' },
      },
    ];

    render(
      <ChangeList
        changes={testChanges}
        metaType="filterOptions"
        metaTypeLabel="Test filter options"
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Updated Test filter options',
        level: 4,
      }),
    ).toBeInTheDocument();
  });

  test('renders correct heading for custom `metaTypeLabel` and multiple change types', () => {
    const testChanges: Change<FilterOption>[] = [
      { currentState: { id: 'filter-opt-1', label: 'Filter option 1' } },
      {
        previousState: { id: 'filter-opt-2', label: 'Filter option 2' },
        currentState: { id: 'filter-opt-2', label: 'Filter option 2 updated' },
      },
      { previousState: { id: 'filter-opt-3', label: 'Filter option 3' } },
    ];

    render(
      <ChangeList
        changes={testChanges}
        metaType="filterOptions"
        metaTypeLabel="Test filter options"
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Changes to Test filter options',
        level: 4,
      }),
    ).toBeInTheDocument();
  });
});
