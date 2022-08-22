import ReorderFiltersList from '@admin/pages/release/data/components/ReorderFiltersList';
import _tableBuilderService, {
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
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
        releaseId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Reorder filters for Subject Name' }),
    );

    await waitFor(() => {
      expect(screen.getByTestId('reorder-list'));
    });

    const filters = within(screen.getByTestId('reorder-list')).getAllByRole(
      'button',
    );

    // the listitems are given role 'button' by react-dnd
    expect(within(filters[0]).getByText('Category 2'));
    expect(
      within(filters[0]).getByRole('button', {
        name: 'Reorder options within this group',
      }),
    );
    expect(within(filters[2]).getByText('Category 1'));
    expect(
      within(filters[2]).getByRole('button', {
        name: 'Reorder options within this group',
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
        releaseId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );

    await waitFor(() => {
      expect(screen.getByText('No filters available.'));
      expect(screen.queryByTestId('reorder-list')).not.toBeInTheDocument();
    });
  });

  test('clicking the `reorder options` button on a filter shows the filter groups for the filter ordered by the order property', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    render(
      <ReorderFiltersList
        releaseId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );
    await waitFor(() => {
      expect(screen.getByTestId('reorder-list'));
    });
    const filtersList = screen.getByTestId('reorder-list');
    userEvent.click(
      screen.getAllByRole('button', {
        name: 'Reorder options within this group',
      })[1],
    );

    await waitFor(() => {
      expect(screen.getByText('Category 1 Group 2'));
    });
    const groupsList = within(filtersList).getByRole('list');
    const groups = within(groupsList).getAllByRole('button');
    expect(within(groups[0]).getByText('Category 1 Group 2'));
    expect(within(groups[2]).getByText('Category 1 Group 1'));
    expect(within(groups[3]).getByText('Category 1 Group 3'));
  });

  test('filter groups have a `reorder options` button if they have more than one filter item', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    render(
      <ReorderFiltersList
        releaseId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );
    await waitFor(() => {
      expect(screen.getByTestId('reorder-list'));
    });
    const filtersList = screen.getByTestId('reorder-list');
    userEvent.click(
      screen.getAllByRole('button', {
        name: 'Reorder options within this group',
      })[1],
    );

    await waitFor(() => {
      expect(screen.getByText('Category 1 Group 2'));
    });

    const groupsList = within(filtersList).getByRole('list');
    const groups = within(groupsList).getAllByRole('button');
    expect(within(groups[0]).getByText('Category 1 Group 2'));
    expect(
      within(groups[0]).getByRole('button', {
        name: 'Reorder options within this group',
      }),
    );
    expect(within(groups[2]).getByText('Category 1 Group 1'));
    expect(
      within(groups[2]).queryByRole('button', {
        name: 'Reorder options within this group',
      }),
    ).not.toBeInTheDocument();

    expect(within(groups[3]).getByText('Category 1 Group 3'));
    expect(
      within(groups[3]).getByRole('button', {
        name: 'Reorder options within this group',
      }),
    );
  });

  test('clicking the `done` button on a filter hides the groups for the filter', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    render(
      <ReorderFiltersList
        releaseId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );
    await waitFor(() => {
      expect(screen.getByTestId('reorder-list'));
    });
    const filtersList = screen.getByTestId('reorder-list');
    userEvent.click(
      screen.getAllByRole('button', {
        name: 'Reorder options within this group',
      })[1],
    );

    await waitFor(() => {
      expect(screen.getByText('Category 1 Group 2'));
    });

    expect(within(filtersList).getByRole('list'));
    userEvent.click(screen.getByRole('button', { name: 'Done' }));
    expect(screen.queryByText('Category 1 Group 2')).not.toBeInTheDocument();
    expect(within(filtersList).queryByRole('list')).not.toBeInTheDocument();
  });

  test('clicking the `reorder options` button on a filter group shows the options for that group', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    render(
      <ReorderFiltersList
        releaseId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );
    await waitFor(() => {
      expect(screen.getByTestId('reorder-list'));
    });
    const filtersList = screen.getByTestId('reorder-list');
    userEvent.click(
      screen.getAllByRole('button', {
        name: 'Reorder options within this group',
      })[1],
    );

    await waitFor(() => {
      expect(screen.getByText('Category 1 Group 2'));
    });
    const buttons = within(within(filtersList).getByRole('list')).getAllByRole(
      'button',
    );
    userEvent.click(
      within(buttons[0]).getByRole('button', {
        name: 'Reorder options within this group',
      }),
    );

    const options = within(buttons[0]).getAllByRole('button');
    expect(within(options[1]).getByText('Category 1 Group 2 Item 1'));
    expect(within(options[2]).getByText('Category 1 Group 2 Item 2'));
    expect(within(options[3]).getByText('Category 1 Group 2 Item 3'));
  });

  test('clicking the `done` button on a filter group hides the options for the group', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    render(
      <ReorderFiltersList
        releaseId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );
    await waitFor(() => {
      expect(screen.getByTestId('reorder-list'));
    });
    const filtersList = screen.getByTestId('reorder-list');
    userEvent.click(
      screen.getAllByRole('button', {
        name: 'Reorder options within this group',
      })[1],
    );

    await waitFor(() => {
      expect(screen.getByText('Category 1 Group 2'));
    });

    const groupsList = within(filtersList).getByRole('list');
    const groups = within(groupsList).getAllByRole('button');

    userEvent.click(
      within(groups[0]).getByRole('button', {
        name: 'Reorder options within this group',
      }),
    );

    const options = within(groups[0]).getAllByRole('button');

    expect(within(options[1]).getByText('Category 1 Group 2 Item 1'));

    // Waiting to make sure the button has changed,
    // otherwise it's clicked too quickly and thinks it's a double-click.
    await waitFor(() => {
      expect(within(groups[0]).getByText('Done')).toBeInTheDocument();
    });

    userEvent.click(within(groups[0]).getByRole('button', { name: 'Done' }));

    expect(
      screen.queryByText('Category 1 Group 2 Item 1'),
    ).not.toBeInTheDocument();
  });

  test('if the filter only has one group, clicking the `reorder options` button on the filter shows the options for the group', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    render(
      <ReorderFiltersList
        releaseId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );
    await waitFor(() => {
      expect(screen.getByTestId('reorder-list'));
    });
    const filtersList = screen.getByTestId('reorder-list');
    userEvent.click(
      screen.getAllByRole('button', {
        name: 'Reorder options within this group',
      })[0],
    );

    await waitFor(() => {
      expect(screen.getByText('Category 2 Group 1 Item 1'));
    });

    const optionsList = within(filtersList).getByRole('list');
    const options = within(optionsList).getAllByRole('button');
    expect(within(options[0]).getByText('Category 2 Group 1 Item 1'));
    expect(within(options[1]).getByText('Category 2 Group 1 Item 2'));
  });

  test('clicking the `save` button calls handleSave with the reordered list formatted for the update request', async () => {
    const handleSave = jest.fn();
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    render(
      <ReorderFiltersList
        releaseId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={handleSave}
      />,
    );
    await waitFor(() => {
      expect(screen.getByTestId('reorder-list'));
    });
    userEvent.click(screen.getByRole('button', { name: 'Save order' }));

    const expectedRequest = [
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
