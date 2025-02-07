import ReorderFiltersList, {
  UpdateFiltersRequest,
} from '@admin/pages/release/data/components/ReorderFiltersList';
import render from '@common-test/render';
import _tableBuilderService, {
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

jest.mock('@common/services/tableBuilderService');

const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

const testSubject = {
  id: 'subject-id',
  name: 'Subject Name',
} as Subject;

const testSubjectMeta: SubjectMeta = {
  filters: {
    Category1: {
      id: 'category-1-id',
      legend: 'Category 1',
      name: 'category-1',
      options: {
        Category1Group1: {
          id: 'category-1-group-1-id',
          label: 'Category 1 Group 1',
          options: [
            {
              label: 'Category 1 Group 1 Item 1',
              value: 'category-1-group-1-item-1',
            },
          ],
          order: 1,
        },
        Category1Group2: {
          id: 'category-1-group-2-id',
          label: 'Category 1 Group 2',
          options: [
            {
              label: 'Category 1 Group 2 Item 1',
              value: 'category-1-group-2-item-1',
            },
            {
              label: 'Category 1 Group 2 Item 2',
              value: 'category-1-group-2-item-2',
            },
            {
              label: 'Category 1 Group 2 Item 3',
              value: 'category-1-group-2-item-3',
            },
          ],
          order: 0,
        },
        Category1Group3: {
          id: 'category-1-group-3-id',
          label: 'Category 1 Group 3',
          options: [
            {
              label: 'Category 1 Group 3 Item 1',
              value: 'category-1-group-3-item-1',
            },
            {
              label: 'Category 1 Group 3 Item 2',
              value: 'category-1-group-3-item-2',
            },
          ],
          order: 2,
        },
      },
      order: 1,
    },
    Category2: {
      id: 'category-2-id',
      legend: 'Category 2',
      name: 'category-2',
      options: {
        Category2Group1: {
          id: 'category-2-group-1-id',
          label: 'Category 2 Group 1',
          options: [
            {
              label: 'Category 2 Group 1 Item 1',
              value: 'category-2-group-1-item-1',
            },
            {
              label: 'Category 2 Group 1 Item 2',
              value: 'category-2-group-1-item-2',
            },
          ],
          order: 0,
        },
      },
      order: 0,
    },
  },
  indicators: {},
  locations: {},
  timePeriod: {
    hint: '',
    legend: '',
    options: [],
  },
};

describe('ReorderFiltersList', () => {
  test('renders a list of filters for the subject ordered by the order property', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    render(
      <ReorderFiltersList
        releaseVersionId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );

    expect(
      await screen.findByText('Reorder filters for Subject Name'),
    ).toBeInTheDocument();

    const filters = within(screen.getByTestId('reorder-filters')).getAllByRole(
      'listitem',
    );

    expect(within(filters[0]).getByText('Category 2'));
    expect(
      within(filters[0]).getByRole('button', {
        name: 'Reorder options within Category 2',
      }),
    );
    expect(within(filters[1]).getByText('Category 1'));
    expect(
      within(filters[1]).getByRole('button', {
        name: 'Reorder options within Category 1',
      }),
    );
  });

  test('shows a message if there are no filters', async () => {
    const testNoFiltersSubjectMeta = {
      filters: {},
      indicators: {},
      locations: {},
      timePeriod: {
        hint: '',
        legend: '',
        options: [],
      },
    };
    tableBuilderService.getSubjectMeta.mockResolvedValue(
      testNoFiltersSubjectMeta,
    );
    render(
      <ReorderFiltersList
        releaseVersionId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );

    await waitFor(() => {
      expect(screen.getByText('No filters available.'));
    });

    expect(screen.queryByTestId('reorder-filters')).not.toBeInTheDocument();
  });

  test('clicking the `reorder options` button on a filter shows the filter groups for the filter ordered by the order property', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    const { user } = render(
      <ReorderFiltersList
        releaseVersionId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );
    expect(
      await screen.findByText('Reorder filters for Subject Name'),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Reorder options within Category 1' }),
    );
    expect(await screen.findByText('Category 1 Group 2'));

    const groups = within(
      screen.getByTestId('reorder-filters-children'),
    ).getAllByRole('listitem');
    expect(within(groups[0]).getByText('Category 1 Group 2'));
    expect(within(groups[1]).getByText('Category 1 Group 1'));
    expect(within(groups[2]).getByText('Category 1 Group 3'));
  });

  test('filter groups have a `reorder options` button if they have more than one filter item', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    const { user } = render(
      <ReorderFiltersList
        releaseVersionId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );
    expect(
      await screen.findByText('Reorder filters for Subject Name'),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Reorder options within Category 1' }),
    );
    expect(await screen.findByText('Category 1 Group 2'));

    await waitFor(() => {
      expect(screen.getByText('Category 1 Group 2'));
    });

    expect(
      screen.getByRole('button', {
        name: 'Reorder options within Category 1 Group 2',
      }),
    );
    expect(
      screen.queryByRole('button', {
        name: 'Reorder options within Category 1 Group 1',
      }),
    );
    expect(
      screen.getByRole('button', {
        name: 'Reorder options within Category 1 Group 3',
      }),
    );
  });

  test('clicking the `close` button on a filter hides the groups for the filter', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    const { user } = render(
      <ReorderFiltersList
        releaseVersionId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );
    expect(
      await screen.findByText('Reorder filters for Subject Name'),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Reorder options within Category 1' }),
    );
    expect(await screen.findByText('Category 1 Group 2'));

    await waitFor(() => {
      expect(screen.getByText('Category 1 Group 2'));
    });

    await user.click(screen.getByRole('button', { name: 'Close Category 1' }));
    expect(screen.queryByText('Category 1 Group 2')).not.toBeInTheDocument();
  });

  test('clicking the `reorder options` button on a filter group shows the options for that group', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    const { user } = render(
      <ReorderFiltersList
        releaseVersionId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );
    expect(
      await screen.findByText('Reorder filters for Subject Name'),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Reorder options within Category 1' }),
    );
    expect(await screen.findByText('Category 1 Group 2'));

    await user.click(
      screen.getByRole('button', {
        name: 'Reorder options within Category 1 Group 2',
      }),
    );
    expect(await screen.findByText('Category 1 Group 2 Item 1'));

    const options = within(
      screen.getByTestId('reorder-filters-children'),
    ).getAllByRole('listitem');
    expect(within(options[1]).getByText('Category 1 Group 2 Item 1'));
    expect(within(options[2]).getByText('Category 1 Group 2 Item 2'));
    expect(within(options[3]).getByText('Category 1 Group 2 Item 3'));
  });

  test('clicking the `close` button on a filter group hides the options for the group', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    const { user } = render(
      <ReorderFiltersList
        releaseVersionId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );
    expect(
      await screen.findByText('Reorder filters for Subject Name'),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Reorder options within Category 1' }),
    );
    expect(await screen.findByText('Category 1 Group 2'));

    await user.click(
      screen.getByRole('button', {
        name: 'Reorder options within Category 1 Group 2',
      }),
    );
    expect(await screen.findByText('Category 1 Group 2 Item 1'));

    await user.click(
      screen.getByRole('button', { name: 'Close Category 1 Group 2' }),
    );

    expect(
      screen.queryByText('Category 1 Group 2 Item 1'),
    ).not.toBeInTheDocument();
  });

  test('if the filter only has one group, clicking the `reorder options` button on the filter shows the options for the group', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    const { user } = render(
      <ReorderFiltersList
        releaseVersionId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );

    expect(
      await screen.findByText('Reorder filters for Subject Name'),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Reorder options within Category 2' }),
    );

    expect(await screen.findByText('Category 2 Group 1 Item 1'));

    const options = within(
      screen.getByTestId('reorder-filters-children'),
    ).getAllByRole('listitem');
    expect(within(options[0]).getByText('Category 2 Group 1 Item 1'));
    expect(within(options[1]).getByText('Category 2 Group 1 Item 2'));
  });

  test('clicking the `save` button calls handleSave with the reordered list formatted for the update request', async () => {
    const handleSave = jest.fn();
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    const { user } = render(
      <ReorderFiltersList
        releaseVersionId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={handleSave}
      />,
    );
    expect(
      await screen.findByText('Reorder filters for Subject Name'),
    ).toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: 'Save order' }));

    const expectedRequest: UpdateFiltersRequest = [
      {
        id: 'category-2-id',
        filterGroups: [
          {
            filterItems: [
              'category-2-group-1-item-1',
              'category-2-group-1-item-2',
            ],
            id: 'category-2-group-1-id',
          },
        ],
      },
      {
        id: 'category-1-id',
        filterGroups: [
          {
            id: 'category-1-group-2-id',
            filterItems: [
              'category-1-group-2-item-1',
              'category-1-group-2-item-2',
              'category-1-group-2-item-3',
            ],
          },
          {
            id: 'category-1-group-1-id',
            filterItems: ['category-1-group-1-item-1'],
          },
          {
            id: 'category-1-group-3-id',
            filterItems: [
              'category-1-group-3-item-1',
              'category-1-group-3-item-2',
            ],
          },
        ],
      },
    ];

    expect(handleSave).toHaveBeenCalledWith(testSubject.id, expectedRequest);
  });
});
