import DataFileReplacementPlan from '@admin/pages/release/data/components/DataFileReplacementPlan';
import _dataReplacementService, {
  DataReplacementPlan,
} from '@admin/services/dataReplacementService';
import { render, screen, waitFor, within } from '@testing-library/react';
import React from 'react';

jest.mock('@admin/services/dataReplacementService');

const dataReplacementService = _dataReplacementService as jest.Mocked<
  typeof _dataReplacementService
>;

describe('DataReplacementPlan', () => {
  const testReplacementPlan: DataReplacementPlan = {
    dataBlocks: [
      {
        id: 'block-1',
        name: 'Data block 1',
        filters: {
          'filter-1': {
            id: 'filter-1',
            label: 'Filter 1',
            valid: false,
            groups: {
              'group-1': {
                id: 'group-1',
                label: 'Group 1',
                valid: false,
                filters: [
                  {
                    id: 'item-1',
                    label: 'Item 1',
                    valid: false,
                  },
                  {
                    id: 'item-2',
                    label: 'Item 2',
                    target: 'target-item-2',
                    valid: true,
                  },
                ],
              },
              'group-2': {
                id: 'group-2',
                label: 'Group 2',
                valid: true,
                filters: [
                  {
                    id: 'item-3',
                    label: 'Item 3',
                    target: 'target-item-3',
                    valid: true,
                  },
                ],
              },
            },
          },
          'filter-2': {
            id: 'filter-2',
            label: 'Filter 2',
            target: 'target-filter-2',
            valid: false,
            groups: {
              'group-3': {
                id: 'group-3',
                label: 'Group 3',
                target: 'target-group-3',
                valid: true,
                filters: [
                  {
                    id: 'item-4',
                    label: 'Item 4',
                    target: 'target-item-4',
                    valid: true,
                  },
                ],
              },
              'group-4': {
                id: 'group-4',
                label: 'Group 4',
                valid: false,
                filters: [
                  {
                    id: 'item-5',
                    label: 'Item 5',
                    valid: false,
                  },
                  {
                    id: 'item-6',
                    label: 'Item 6',
                    valid: false,
                  },
                ],
              },
            },
          },
        },
        indicatorGroups: {
          'indicator-group-1': {
            id: 'indicator-group-1',
            label: 'Indicator group 1',
            valid: true,
            indicators: [
              {
                id: 'indicator-1',
                label: 'Indicator 1',
                name: 'indicator_1',
                target: 'target-indicator-1',
                valid: true,
              },
            ],
          },
          'indicator-group-2': {
            id: 'indicator-group-2',
            label: 'Indicator group 2',
            valid: false,
            indicators: [
              {
                id: 'indicator-2',
                label: 'Indicator 2',
                name: 'indicator_2',
                target: 'target-indicator-2',
                valid: true,
              },
              {
                id: 'indicator-3',
                label: 'Indicator 3',
                name: 'indicator_3',
                valid: false,
              },
            ],
          },
          'indicator-group-3': {
            id: 'indicator-group-3',
            label: 'Indicator group 3',
            valid: false,
            indicators: [
              {
                id: 'indicator-4',
                label: 'Indicator 4',
                name: 'indicator_4',
                target: 'target-indicator-4',
                valid: false,
              },
            ],
          },
        },
        locations: {
          Country: {
            label: 'Country',
            valid: true,
            observationalUnits: [
              {
                code: 'england',
                label: 'England',
                target: 'england',
                valid: true,
              },
            ],
          },
          LocalAuthority: {
            label: 'Local Authority',
            valid: false,
            observationalUnits: [
              {
                code: 'barnsley',
                label: 'Barnsley',
                target: 'barnsley',
                valid: true,
              },
              {
                code: 'birmingham',
                label: 'Birmingham',
                valid: false,
              },
            ],
          },
        },
        timePeriods: {
          start: { code: 'CY', year: 2016, label: '2016', valid: false },
          end: { code: 'CY', year: 2018, label: '2018', valid: true },
          valid: false,
        },
        valid: false,
      },
      {
        id: 'block-2',
        name: 'Data block 2',
        filters: {},
        indicatorGroups: {},
        locations: {},
        valid: true,
      },
    ],
    footnotes: [
      {
        id: 'footnote-1',
        content: 'Footnote 1',
        valid: false,
        filters: [
          {
            id: 'filter-3',
            label: 'Filter 3',
            valid: true,
          },
          {
            id: 'filter-4',
            label: 'Filter 4',
            valid: false,
          },
        ],
        filterGroups: [
          {
            id: 'group-1',
            label: 'Group 1',
            valid: true,
            filterId: 'filter-1',
            filterLabel: 'Filter 1',
          },
          {
            id: 'group-4',
            label: 'Group 4',
            valid: false,
            filterId: 'filter-2',
            filterLabel: 'Filter 2',
          },
        ],
        filterItems: [
          {
            id: 'item-1',
            label: 'Item 1',
            valid: false,
            filterId: 'filter-1',
            filterLabel: 'Filter 1',
            filterGroupId: 'group-2',
            filterGroupLabel: 'Group 2',
          },
          {
            id: 'item-2',
            label: 'Item 2',
            valid: true,
            filterId: 'filter-1',
            filterLabel: 'Filter 1',
            filterGroupId: 'group-3',
            filterGroupLabel: 'Group 3',
          },
          {
            id: 'item-3',
            label: 'Item 3',
            valid: false,
            filterId: 'filter-1',
            filterLabel: 'Filter 1',
            filterGroupId: 'group-2',
            filterGroupLabel: 'Group 2',
          },
          {
            id: 'item-4',
            label: 'Item 4',
            valid: false,
            filterId: 'filter-2',
            filterLabel: 'Filter 2',
            filterGroupId: 'group-5',
            filterGroupLabel: 'Group 5',
          },
        ],
        indicatorGroups: {
          'indicator-group-1': {
            id: 'indicator-group-1',
            label: 'Indicator group 1',
            valid: true,
            indicators: [
              {
                id: 'indicator-1',
                label: 'Indicator 1',
                name: 'indicator_1',
                target: 'target-indicator-1',
                valid: true,
              },
            ],
          },
          'indicator-group-2': {
            id: 'indicator-group-2',
            label: 'Indicator group 2',
            valid: false,
            indicators: [
              {
                id: 'indicator-3',
                label: 'Indicator 3',
                name: 'indicator_3',
                valid: false,
              },
            ],
          },
        },
      },
      {
        id: 'footnote-2',
        content: 'Footnote 2',
        valid: true,
        filters: [],
        filterGroups: [],
        filterItems: [],
        indicatorGroups: {},
      },
    ],
    originalSubjectId: 'subject-1',
    replacementSubjectId: 'subject-2',
    valid: false,
  };

  test('renders correctly', async () => {
    dataReplacementService.getReplacementPlan.mockResolvedValue(
      testReplacementPlan,
    );

    render(
      <DataFileReplacementPlan fileId="file-1" replacementFileId="file-2" />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Data replacement in progress'),
      ).toBeInTheDocument();

      expect(screen.getByText('Data blocks: ERROR')).toBeInTheDocument();
      expect(screen.getByText('Footnotes: ERROR')).toBeInTheDocument();

      const details = screen.getAllByRole('group');

      expect(details).toHaveLength(4);

      const dataBlock1 = within(details[0]);

      expect(
        dataBlock1.getByRole('button', { name: /Data block 1/ }),
      ).toHaveTextContent('ERROR');

      expect(dataBlock1.getByTestId('Country')).toHaveTextContent('England');
      expect(dataBlock1.getByTestId('Country')).not.toHaveTextContent(
        'England not present',
      );

      expect(dataBlock1.getByTestId('Local Authority')).toHaveTextContent(
        'Barnsley',
      );
      expect(dataBlock1.getByTestId('Local Authority')).not.toHaveTextContent(
        'Barnsley not present',
      );

      expect(dataBlock1.getByTestId('Local Authority')).toHaveTextContent(
        'Birmingham not present',
      );
      expect(dataBlock1.getByTestId('Start date')).toHaveTextContent(
        '2016 not present',
      );

      expect(dataBlock1.getByTestId('End date')).toHaveTextContent('2018');
      expect(dataBlock1.getByTestId('End date')).not.toHaveTextContent(
        '2018 not present',
      );

      expect(dataBlock1.getByTestId('Filter 1')).toHaveTextContent('Group 1');
      expect(dataBlock1.getByTestId('Filter 1')).toHaveTextContent('Group 2');

      expect(dataBlock1.getByTestId('Group 1')).toHaveTextContent(
        'Item 1 not present',
      );
      expect(dataBlock1.getByTestId('Group 1')).toHaveTextContent('Item 2');
      expect(dataBlock1.getByTestId('Group 1')).not.toHaveTextContent(
        'Item 2 not present',
      );

      expect(dataBlock1.getByTestId('Group 2')).toHaveTextContent('Item 3');
      expect(dataBlock1.getByTestId('Group 2')).not.toHaveTextContent(
        'Item 3 not present',
      );

      expect(dataBlock1.getByTestId('Filter 2')).toHaveTextContent('Group 3');
      expect(dataBlock1.getByTestId('Filter 2')).toHaveTextContent('Group 4');

      expect(dataBlock1.getByTestId('Group 3')).toHaveTextContent('Item 4');
      expect(dataBlock1.getByTestId('Group 3')).not.toHaveTextContent(
        'Item 4 not present',
      );

      expect(dataBlock1.getByTestId('Group 4')).toHaveTextContent(
        'Item 5 not present',
      );
      expect(dataBlock1.getByTestId('Group 4')).toHaveTextContent(
        'Item 6 not present',
      );

      expect(dataBlock1.getByTestId('Indicator group 1')).toHaveTextContent(
        'Indicator 1',
      );
      expect(dataBlock1.getByTestId('Indicator group 1')).not.toHaveTextContent(
        'Indicator 1 not present',
      );

      expect(dataBlock1.getByTestId('Indicator group 2')).toHaveTextContent(
        'Indicator 2',
      );
      expect(dataBlock1.getByTestId('Indicator group 2')).not.toHaveTextContent(
        'Indicator 2 not present',
      );
      expect(dataBlock1.getByTestId('Indicator group 2')).toHaveTextContent(
        'Indicator 3 not present',
      );

      expect(dataBlock1.getByTestId('Indicator group 3')).toHaveTextContent(
        'Indicator 4 not present',
      );

      const dataBlock2 = within(details[1]);

      expect(
        dataBlock2.getByRole('button', { name: /Data block 2/ }),
      ).toHaveTextContent('OK');

      expect(
        dataBlock2.getByText(
          'This data block has no conflicts with the replacement data.',
        ),
      ).toBeInTheDocument();

      const footnote1 = within(details[2]);

      expect(
        footnote1.getByRole('button', { name: /Footnote 1/ }),
      ).toHaveTextContent('ERROR');

      expect(footnote1.getByTestId('Filter 1')).toHaveTextContent('Group 2');
      expect(footnote1.getByTestId('Filter 1')).toHaveTextContent('Group 3');
      expect(footnote1.getByTestId('Filter 1')).toHaveTextContent('Group 1');

      expect(footnote1.getByTestId('Group 2')).toHaveTextContent(
        'Item 1 not present',
      );
      expect(footnote1.getByTestId('Group 2')).toHaveTextContent(
        'Item 3 not present',
      );

      expect(footnote1.getByTestId('Group 3')).toHaveTextContent('Item 2');
      expect(footnote1.getByTestId('Group 3')).not.toHaveTextContent(
        'Item 2 not present',
      );

      expect(footnote1.getByTestId('Group 1')).toHaveTextContent(
        '(All selected)',
      );

      expect(footnote1.getByTestId('Filter 2')).toHaveTextContent('Group 5');
      expect(footnote1.getByTestId('Filter 2')).toHaveTextContent('Group 4');

      expect(footnote1.getByTestId('Group 5')).toHaveTextContent(
        'Item 4 not present',
      );
      expect(footnote1.getByTestId('Group 4')).toHaveTextContent(
        '(All selected)',
      );

      expect(footnote1.getByTestId('Filter 3')).toHaveTextContent(
        '(All selected)',
      );
      expect(footnote1.getByTestId('Filter 4')).toHaveTextContent(
        '(All selected)',
      );

      expect(footnote1.getByTestId('Indicator group 1')).toHaveTextContent(
        'Indicator 1',
      );
      expect(footnote1.getByTestId('Indicator group 1')).not.toHaveTextContent(
        'Indicator 1 not present',
      );
      expect(footnote1.getByTestId('Indicator group 2')).toHaveTextContent(
        'Indicator 3 not present',
      );

      const footnote2 = within(details[3]);

      expect(
        footnote2.getByRole('button', { name: /Footnote 2/ }),
      ).toHaveTextContent('OK');

      expect(
        footnote2.getByText(
          'This footnote has no conflicts with the replacement data.',
        ),
      ).toBeInTheDocument();
    });
  });

  test('renders error message if there is an error loading replacement plan', async () => {
    dataReplacementService.getReplacementPlan.mockRejectedValue(
      new Error('Something went wrong'),
    );

    render(
      <DataFileReplacementPlan fileId="file-1" replacementFileId="file-2" />,
    );

    await waitFor(() => {
      expect(
        screen.getByText(
          'There was a problem loading the data replacement details.',
        ),
      ).toBeInTheDocument();

      expect(
        screen.queryByText('Data replacement in progress'),
      ).not.toBeInTheDocument();
    });
  });
});
