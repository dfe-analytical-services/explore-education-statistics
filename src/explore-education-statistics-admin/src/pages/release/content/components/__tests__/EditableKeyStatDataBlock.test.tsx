import EditableKeyStatDataBlock from '@admin/pages/release/content/components/EditableKeyStatDataBlock';
import { KeyStatDataBlockFormValues } from '@admin/pages/release/content/components/EditableKeyStatDataBlockForm';
import _keyStatisticService from '@admin/services/keyStatisticService';
import { KeyStatisticDataBlock } from '@common/services/publicationService';
import _tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

jest.mock('@admin/services/keyStatisticService');
const keyStatisticService = _keyStatisticService as jest.Mocked<
  typeof _keyStatisticService
>;

jest.mock('@common/services/tableBuilderService');
const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('EditableKeyStatDataBlock', () => {
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
              id: 'filter-group-1',
              label: 'Filter group 1',
              options: [{ label: 'Filter 1', value: 'filter-1' }],
              order: 1,
            },
          },
          order: 0,
        },
      },
      locations: {
        country: [{ label: 'England', value: 'england' }],
      },
      timePeriodRange: [{ code: 'AY', label: '2020/21', year: 2020 }],
      indicators: [
        {
          label: 'DataBlock indicator',
          name: 'datablock_title',
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
        locationId: 'england',
        timePeriod: '2020_AY',
        measures: {
          'indicator-1': '608180',
        },
      },
    ],
  };

  const keyStatDataBlock: KeyStatisticDataBlock = {
    type: 'KeyStatisticDataBlock',
    id: 'keyStatDataBlock-1',
    trend: 'DataBlock trend',
    guidanceTitle: 'DataBlock guidance title',
    guidanceText: 'DataBlock guidance text',
    order: 0,
    created: '2023-01-01',
    dataBlockId: 'block-1',
  };

  test('renders preview correctly', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStatDataBlock
        releaseId="release-1"
        keyStat={keyStatDataBlock}
        onSubmit={noop}
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'DataBlock indicator',
      );
    });

    expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
      '608,180',
    );
    expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
      'DataBlock trend',
    );

    expect(
      screen.getByRole('button', {
        name: 'DataBlock guidance title',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
      'DataBlock guidance text',
    );

    expect(
      screen.queryByRole('button', { name: /Remove/ }),
    ).not.toBeInTheDocument();
  });

  test('renders preview correctly without trend', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStatDataBlock
        releaseId="release-1"
        keyStat={{ ...keyStatDataBlock, trend: undefined }}
        onSubmit={noop}
      />,
    );

    await waitFor(() => {
      expect(tableBuilderService.getDataBlockTableData).toHaveBeenCalledTimes(
        1,
      );
    });

    expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
      'DataBlock indicator',
    );
    expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
      '608,180',
    );
    expect(screen.queryByTestId('keyStat-trend')).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'DataBlock guidance title',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
      'DataBlock guidance text',
    );

    expect(
      screen.queryByRole('button', { name: /Remove/ }),
    ).not.toBeInTheDocument();
  });

  test('renders preview correctly without guidanceTitle', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStatDataBlock
        releaseId="release-1"
        keyStat={{ ...keyStatDataBlock, guidanceTitle: undefined }}
        onSubmit={noop}
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'DataBlock indicator',
      );
    });

    expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
      '608,180',
    );
    expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
      'DataBlock trend',
    );

    expect(
      screen.getByRole('button', {
        name: 'Help for DataBlock indicator',
      }),
    ).toBeInTheDocument();

    expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
      'DataBlock guidance text',
    );

    expect(
      screen.queryByRole('button', { name: /Remove/ }),
    ).not.toBeInTheDocument();
  });

  test('renders preview correctly without guidanceText', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStatDataBlock
        releaseId="release-1"
        keyStat={{ ...keyStatDataBlock, guidanceText: undefined }}
        onSubmit={noop}
      />,
    );

    await waitFor(() => {
      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'DataBlock indicator',
      );
    });

    expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
      '608,180',
    );
    expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
      'DataBlock trend',
    );

    expect(
      screen.queryByRole('button', {
        name: 'DataBlock guidance title',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByTestId('keyStat-guidanceText'),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: /Remove/ }),
    ).not.toBeInTheDocument();
  });

  test('clicking `Remove` button calls `onRemove` handler', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    const onRemove = jest.fn();

    render(
      <EditableKeyStatDataBlock
        releaseId="release-1"
        keyStat={keyStatDataBlock}
        isEditing
        onRemove={onRemove}
        onSubmit={noop}
      />,
    );

    await waitFor(() => {
      expect(screen.getByText(/Remove/)).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', {
        name: 'Remove key statistic: DataBlock indicator',
      }),
    );

    await waitFor(() => {
      expect(onRemove).toHaveBeenCalledTimes(1);
    });
  });

  describe('when editing', () => {
    test('renders edit form correctly', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      render(
        <EditableKeyStatDataBlock
          releaseId="release-1"
          keyStat={keyStatDataBlock}
          isEditing
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(screen.getByText(/Edit/)).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', {
          name: 'Edit key statistic: DataBlock indicator',
        }),
      );

      await waitFor(() => {
        expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
          'DataBlock indicator',
        );
      });

      expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
        '608,180',
      );

      expect(screen.getByLabelText('Trend')).toHaveValue('DataBlock trend');

      expect(screen.getByLabelText('Guidance title')).toHaveValue(
        'DataBlock guidance title',
      );

      expect(screen.getByLabelText('Guidance text')).toHaveTextContent(
        'DataBlock guidance text',
      );

      expect(screen.getByRole('button', { name: /Save/ })).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: /Cancel/ }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('button', {
          name: 'Edit key statistic: Text title',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: /Remove/ }),
      ).not.toBeInTheDocument();
    });

    test('submitting edit form calls `onSubmit` handler with updated values', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      const onSubmit = jest.fn();

      render(
        <EditableKeyStatDataBlock
          releaseId="release-1"
          keyStat={keyStatDataBlock}
          isEditing
          onSubmit={onSubmit}
        />,
      );

      await waitFor(() => {
        expect(screen.getByText(/Edit/)).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', {
          name: 'Edit key statistic: DataBlock indicator',
        }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Trend')).toBeInTheDocument();
      });

      userEvent.clear(screen.getByLabelText('Trend'));
      await userEvent.type(screen.getByLabelText('Trend'), 'New trend');

      userEvent.clear(screen.getByLabelText('Guidance title'));
      await userEvent.type(
        screen.getByLabelText('Guidance title'),
        'New guidance title',
      );

      userEvent.clear(screen.getByLabelText('Guidance text'));
      await userEvent.type(
        screen.getByLabelText('Guidance text'),
        'New guidance text',
      );

      userEvent.click(screen.getByRole('button', { name: 'Save' }));

      await waitFor(() => {
        expect(onSubmit).toHaveBeenCalledWith<KeyStatDataBlockFormValues[]>({
          trend: 'New trend',
          guidanceTitle: 'New guidance title',
          guidanceText: 'New guidance text',
        });
      });
    });

    test('submitting edit form calls `onSubmit` handler with trimmed updated guidance title', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      const onSubmit = jest.fn();

      render(
        <EditableKeyStatDataBlock
          releaseId="release-1"
          keyStat={keyStatDataBlock}
          isEditing
          onSubmit={onSubmit}
        />,
      );

      await waitFor(() => {
        expect(screen.getByText(/Edit/)).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', {
          name: 'Edit key statistic: DataBlock indicator',
        }),
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Trend')).toBeInTheDocument();
      });

      userEvent.clear(screen.getByLabelText('Guidance title'));
      await userEvent.type(
        screen.getByLabelText('Guidance title'),
        '  New guidance title  ',
      );

      userEvent.click(screen.getByRole('button', { name: 'Save' }));

      await waitFor(() => {
        expect(onSubmit).toHaveBeenCalledWith<KeyStatDataBlockFormValues[]>({
          trend: 'DataBlock trend',
          guidanceTitle: 'New guidance title',
          guidanceText: 'DataBlock guidance text',
        });
      });
    });

    test('clicking `Cancel` button toggles back to preview', async () => {
      tableBuilderService.getDataBlockTableData.mockResolvedValue(
        testTableDataResponse,
      );

      render(
        <EditableKeyStatDataBlock
          releaseId="release-1"
          keyStat={keyStatDataBlock}
          isEditing
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(screen.getByText(/Edit/)).toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', {
          name: 'Edit key statistic: DataBlock indicator',
        }),
      );

      await waitFor(() => {
        expect(screen.getByText('Cancel')).toBeInTheDocument();
      });

      userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

      await waitFor(() => {
        expect(screen.queryByText('Cancel')).not.toBeInTheDocument();
      });

      expect(screen.queryByLabelText('Trend')).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Guidance title')).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Guidance text')).not.toBeInTheDocument();

      expect(screen.getByTestId('keyStat-title')).toHaveTextContent(
        'DataBlock indicator',
      );
      expect(screen.getByTestId('keyStat-statistic')).toHaveTextContent(
        '608,180',
      );
      expect(screen.getByTestId('keyStat-trend')).toHaveTextContent(
        'DataBlock trend',
      );

      expect(
        screen.getByRole('button', {
          name: 'DataBlock guidance title',
        }),
      ).toBeInTheDocument();
      expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
        'DataBlock guidance text',
      );

      expect(
        screen.queryByRole('button', { name: /Edit/ }),
      ).toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: /Remove/ }),
      ).not.toBeInTheDocument();

      expect(
        keyStatisticService.updateKeyStatisticDataBlock,
      ).not.toHaveBeenCalled();
    });

    test('can click `Remove` button when data block fails to load', async () => {
      tableBuilderService.getDataBlockTableData.mockRejectedValue(
        new Error('Something went wrong'),
      );

      const onRemove = jest.fn();

      render(
        <EditableKeyStatDataBlock
          releaseId="release-1"
          keyStat={keyStatDataBlock}
          onRemove={onRemove}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(
          screen.getByText('Could not load key statistic'),
        ).toBeInTheDocument();
      });

      expect(screen.queryByTestId('keyStat-title')).not.toBeInTheDocument();
      expect(screen.queryByTestId('keyStat-statistic')).not.toBeInTheDocument();
      expect(screen.queryByTestId('keyStat-trend')).not.toBeInTheDocument();
      expect(
        screen.queryByTestId('keyStat-guidanceText'),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Edit' }),
      ).not.toBeInTheDocument();

      expect(onRemove).not.toHaveBeenCalled();

      userEvent.click(
        screen.getByRole('button', {
          name: 'Remove key statistic',
        }),
      );

      expect(onRemove).toBeCalledTimes(1);
    });
  });
});
