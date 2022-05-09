import ReorderIndicatorsList from '@admin/pages/release/data/components/ReorderIndicatorsList';
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
  filters: {},
  indicators: {
    Category1: {
      id: 'category-1-id',
      label: 'Category 1',
      options: [
        {
          value: 'category-1-item-1',
          label: 'Category 1 Item 1',
          unit: '',
          name: 'category_1_item_1',
        },
        {
          value: 'category-1-item-2',
          label: 'Category 1 Item 2',
          unit: '',
          name: 'category_1_item_2',
        },
        {
          value: 'category-1-item-3',
          label: 'Category 1 Item 3',
          unit: '',
          name: 'category_1_item_3',
        },
      ],
      order: 1,
    },
    Category2: {
      id: 'category-2-id',
      label: 'Category 2',
      options: [
        {
          value: 'category-2-item-1',
          label: 'Category 2 Item 1',
          unit: '',
          name: 'category_2_item_1',
        },
      ],
      order: 0,
    },
  },
  locations: {},
  timePeriod: {
    hint: '',
    legend: '',
    options: [],
  },
};

describe('ReorderIndicatorsList', () => {
  test('renders a list of indicators for the subject ordered by the order property', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    render(
      <ReorderIndicatorsList
        releaseId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Reorder indicators for Subject Name',
      }),
    );

    await waitFor(() => {
      expect(screen.getByTestId('reorder-list'));
    });

    // the listitems are given role 'button' by react-dnd
    const indicators = within(screen.getByTestId('reorder-list')).getAllByRole(
      'button',
    );
    expect(within(indicators[0]).getByText('Category 2'));
    expect(within(indicators[1]).getByText('Category 1'));
  });

  test('shows a message if there are no indicators', async () => {
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
      <ReorderIndicatorsList
        releaseId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );

    await waitFor(() => {
      expect(screen.getByText('No indicators available.'));
      expect(screen.queryByTestId('reorder-list')).not.toBeInTheDocument();
    });
  });

  test('indicators have a `reorder options` button if they have more than one item', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    render(
      <ReorderIndicatorsList
        releaseId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );
    await waitFor(() => {
      expect(screen.getByTestId('reorder-list'));
    });
    const indicatorsList = screen.getByTestId('reorder-list');

    const buttons = within(indicatorsList).getAllByRole('button');

    expect(within(buttons[0]).getByText('Category 2'));
    expect(
      within(buttons[0]).queryByRole('button', {
        name: 'Reorder options within this group',
      }),
    ).not.toBeInTheDocument();

    expect(within(buttons[1]).getByText('Category 1'));
    expect(
      within(buttons[1]).getByRole('button', {
        name: 'Reorder options within this group',
      }),
    );
  });

  test('clicking the `reorder options` button on a indicator shows the items for the indicator', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    render(
      <ReorderIndicatorsList
        releaseId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );
    await waitFor(() => {
      expect(screen.getByTestId('reorder-list'));
    });
    const indicatorsList = screen.getByTestId('reorder-list');
    userEvent.click(
      screen.getAllByRole('button', {
        name: 'Reorder options within this group',
      })[0],
    );

    await waitFor(() => {
      expect(screen.getByText('Category 1 Item 1'));
    });
    const itemsList = within(indicatorsList).getByRole('list');
    const items = within(itemsList).getAllByRole('button');
    expect(within(items[0]).getByText('Category 1 Item 1'));
    expect(within(items[1]).getByText('Category 1 Item 2'));
    expect(within(items[2]).getByText('Category 1 Item 3'));
  });

  test('clicking the `done` button on a indicator hides the items for the indicator', async () => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    render(
      <ReorderIndicatorsList
        releaseId="release-1"
        subject={testSubject}
        onCancel={noop}
        onSave={noop}
      />,
    );
    await waitFor(() => {
      expect(screen.getByTestId('reorder-list'));
    });
    const indicatorsList = screen.getByTestId('reorder-list');
    userEvent.click(
      screen.getAllByRole('button', {
        name: 'Reorder options within this group',
      })[0],
    );

    await waitFor(() => {
      expect(screen.getByText('Category 1 Item 1'));
    });

    expect(within(indicatorsList).getByRole('list'));
    userEvent.click(screen.getByRole('button', { name: 'Done' }));
    expect(screen.queryByText('Category 1 Item 1')).not.toBeInTheDocument();
    expect(within(indicatorsList).queryByRole('list')).not.toBeInTheDocument();
  });

  test('clicking the `save` button calls handleSave with the reordered list formatted for the update request', async () => {
    const handleSave = jest.fn();
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    render(
      <ReorderIndicatorsList
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
        indicators: ['category-2-item-1'],
      },
      {
        id: 'category-1-id',
        indicators: [
          'category-1-item-1',
          'category-1-item-2',
          'category-1-item-3',
        ],
      },
    ];

    expect(handleSave).toHaveBeenCalledWith(testSubject.id, expectedRequest);
  });
});
