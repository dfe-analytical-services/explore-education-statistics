import ApiDataSetChangelog from '@common/modules/data-catalogue/components/ApiDataSetChangelog';
import { render, screen, within } from '@testing-library/react';
import React from 'react';

describe('ApiDataSetChangelog', () => {
  test('renders major changes only', () => {
    render(
      <ApiDataSetChangelog
        version="2.0"
        majorChanges={{
          filters: [
            {
              previousState: {
                id: 'filter-1',
                column: 'filter_1',
                label: 'Filter 1',
                hint: '',
              },
            },
          ],
        }}
        minorChanges={{}}
      />,
    );

    const majorChanges = within(screen.getByTestId('major-changes'));

    expect(
      majorChanges.getByRole('heading', {
        name: 'Major changes for version 2.0',
      }),
    ).toBeInTheDocument();

    expect(
      majorChanges.getByRole('heading', { name: 'Deleted filters' }),
    ).toBeInTheDocument();

    const deletedFilters = within(
      majorChanges.getByTestId('deleted-filters'),
    ).getAllByRole('listitem');

    expect(deletedFilters).toHaveLength(1);
    expect(deletedFilters[0]).toHaveTextContent(
      'Filter 1 (id: filter-1, column: filter_1)',
    );

    // No minor changes
    expect(screen.queryByTestId('minor-changes')).not.toBeInTheDocument();
  });

  test('renders minor changes only', () => {
    render(
      <ApiDataSetChangelog
        version="2.0"
        majorChanges={{}}
        minorChanges={{
          filters: [
            {
              currentState: {
                id: 'filter-2',
                column: 'filter_2',
                label: 'Filter 2',
                hint: '',
              },
            },
          ],
        }}
      />,
    );

    // No major changes
    expect(screen.queryByTestId('major-changes')).not.toBeInTheDocument();

    const minorChanges = within(screen.getByTestId('minor-changes'));

    expect(
      minorChanges.getByRole('heading', {
        name: 'Minor changes for version 2.0',
      }),
    ).toBeInTheDocument();

    expect(
      minorChanges.getByRole('heading', { name: 'New filters' }),
    ).toBeInTheDocument();

    const addedFilters = within(
      minorChanges.getByTestId('added-filters'),
    ).getAllByRole('listitem');

    expect(addedFilters).toHaveLength(1);

    expect(addedFilters[0]).toHaveTextContent(
      'Filter 2 (id: filter-2, column: filter_2)',
    );
  });

  test('renders major and minor changes', () => {
    render(
      <ApiDataSetChangelog
        version="2.0"
        majorChanges={{
          filters: [
            {
              previousState: {
                id: 'filter-1',
                column: 'filter_1',
                label: 'Filter 1',
                hint: '',
              },
            },
          ],
        }}
        minorChanges={{
          filters: [
            {
              currentState: {
                id: 'filter-2',
                column: 'filter_2',
                label: 'Filter 2',
                hint: '',
              },
            },
          ],
        }}
      />,
    );

    const majorChanges = within(screen.getByTestId('major-changes'));

    expect(
      majorChanges.getByRole('heading', {
        name: 'Major changes for version 2.0',
      }),
    ).toBeInTheDocument();

    expect(
      majorChanges.getByRole('heading', { name: 'Deleted filters' }),
    ).toBeInTheDocument();

    const deletedFilters = within(
      majorChanges.getByTestId('deleted-filters'),
    ).getAllByRole('listitem');

    expect(deletedFilters).toHaveLength(1);
    expect(deletedFilters[0]).toHaveTextContent(
      'Filter 1 (id: filter-1, column: filter_1)',
    );

    const minorChanges = within(screen.getByTestId('minor-changes'));

    expect(
      minorChanges.getByRole('heading', {
        name: 'Minor changes for version 2.0',
      }),
    ).toBeInTheDocument();

    expect(
      minorChanges.getByRole('heading', { name: 'New filters' }),
    ).toBeInTheDocument();

    const addedFilters = within(
      minorChanges.getByTestId('added-filters'),
    ).getAllByRole('listitem');

    expect(addedFilters).toHaveLength(1);
    expect(addedFilters[0]).toHaveTextContent(
      'Filter 2 (id: filter-2, column: filter_2)',
    );
  });

  test('renders patch changes', () => {
    render(
      <ApiDataSetChangelog
        version="2.0.1"
        majorChanges={{}}
        minorChanges={{
          filters: [
            {
              currentState: {
                id: 'filter-2',
                column: 'filter_2',
                label: 'Filter 2',
                hint: '',
              },
            },
          ],
        }}
      />,
    );

    // No major changes
    expect(screen.queryByTestId('major-changes')).not.toBeInTheDocument();

    const minorChanges = within(screen.getByTestId('minor-changes'));

    expect(
      minorChanges.getByRole('heading', {
        name: 'Patch changes for version 2.0.1',
      }),
    ).toBeInTheDocument();

    expect(
      minorChanges.getByRole('heading', { name: 'New filters' }),
    ).toBeInTheDocument();

    const addedFilters = within(
      minorChanges.getByTestId('added-filters'),
    ).getAllByRole('listitem');

    expect(addedFilters).toHaveLength(1);

    expect(addedFilters[0]).toHaveTextContent(
      'Filter 2 (id: filter-2, column: filter_2)',
    );
  });
});
