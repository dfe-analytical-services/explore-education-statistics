import KeyStat from '@common/modules/find-statistics/components/KeyStat';
import _tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';

jest.mock('@common/services/tableBuilderService');

const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('KeyStat', () => {
  const testTableDataResponse: TableDataResponse = {
    subjectMeta: {
      publicationName: 'Test publication',
      subjectName: 'Test subject',
      geoJsonAvailable: false,
      filters: {
        Filter1: {
          legend: 'Filter 1',
          name: 'filter1',
          options: {
            FilterGroup1: {
              label: 'Filter group 1',
              options: [
                {
                  label: 'Filter 1',
                  value: 'filter-1',
                },
              ],
            },
          },
        },
      },
      locations: {
        country: [
          {
            id: 'england-id',
            label: 'England',
            value: 'england',
          },
        ],
      },
      timePeriodRange: [{ code: 'AY', label: '2020/21', year: 2020 }],
      indicators: [
        {
          label: 'Number of applications received',
          name: 'applications_received',
          unit: '',
          value: 'indicator-1',
        },
      ],
      boundaryLevels: [],
      footnotes: [],
    },
    results: [
      {
        filters: ['filter-1'],
        geographicLevel: 'country',
        locationId: 'england-id',
        timePeriod: '2020_AY',
        measures: {
          'indicator-1': '608180',
        },
      },
    ],
  };

  test('renders correctly with summary', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <KeyStat
        releaseId="release-1"
        dataBlockId="block-1"
        summary={{
          dataSummary: ['Down from 620,330 in 2017'],
          dataDefinitionTitle: ['What is the number of applications received?'],
          dataDefinition: [
            'Total number of applications received for places at primary and secondary schools.',
          ],
          dataKeys: [],
        }}
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );

      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );

      expect(screen.getByTestId('keyStat-value')).toHaveTextContent('608,180');

      expect(screen.getByTestId('keyStat-summary')).toHaveTextContent(
        'Down from 620,330 in 2017',
      );

      expect(
        screen.getByRole('button', {
          name: 'What is the number of applications received?',
        }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('keyStat-definition')).toHaveTextContent(
        'Total number of applications received for places at primary and secondary schools.',
      );
    });
  });

  test('renders correctly without summary', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(<KeyStat releaseId="release-1" dataBlockId="block-1" />);

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );

      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'Number of applications received',
      );

      expect(screen.getByTestId('keyStat-value')).toHaveTextContent('608,180');

      expect(screen.queryByTestId('keyStat-summary')).not.toBeInTheDocument();
      expect(screen.queryByRole('button')).not.toBeInTheDocument();
      expect(
        screen.queryByTestId('keyStat-definition'),
      ).not.toBeInTheDocument();
    });
  });

  test('does not render if there was an error fetching the table data', async () => {
    tableBuilderService.getDataBlockTableData.mockRejectedValue(
      new Error('Something went wrong'),
    );

    render(
      <KeyStat
        releaseId="release-1"
        dataBlockId="block-1"
        summary={{
          dataSummary: ['Down from 620,330 in 2017'],
          dataDefinitionTitle: ['What is the number of applications received?'],
          dataDefinition: [
            'Total number of applications received for places at primary and secondary schools.',
          ],
          dataKeys: [],
        }}
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );
      expect(screen.queryByTestId('keyStat-title')).not.toBeInTheDocument();
      expect(screen.queryByTestId('keyStat-value')).not.toBeInTheDocument();
      expect(screen.queryByTestId('keyStat-summary')).not.toBeInTheDocument();
      expect(screen.queryByRole('button')).not.toBeInTheDocument();
      expect(
        screen.queryByTestId('keyStat-definition'),
      ).not.toBeInTheDocument();
    });
  });

  test('does not render if there is no matching result in the response', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue({
      ...testTableDataResponse,
      subjectMeta: {
        ...testTableDataResponse.subjectMeta,
        indicators: [
          {
            label: 'Number of applications received',
            name: 'applications_received',
            unit: '',
            value: 'indicator-1',
          },
        ],
      },
      results: [],
    });

    render(
      <KeyStat
        releaseId="release-1"
        dataBlockId="block-1"
        summary={{
          dataSummary: ['Down from 620,330 in 2017'],
          dataDefinitionTitle: ['What is the number of applications received?'],
          dataDefinition: [
            'Total number of applications received for places at primary and secondary schools.',
          ],
          dataKeys: [],
        }}
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );
      expect(screen.queryByTestId('keyStat-title')).not.toBeInTheDocument();
      expect(screen.queryByTestId('keyStat-value')).not.toBeInTheDocument();
      expect(screen.queryByTestId('keyStat-summary')).not.toBeInTheDocument();
      expect(screen.queryByRole('button')).not.toBeInTheDocument();
      expect(
        screen.queryByTestId('keyStat-definition'),
      ).not.toBeInTheDocument();
    });
  });
});
