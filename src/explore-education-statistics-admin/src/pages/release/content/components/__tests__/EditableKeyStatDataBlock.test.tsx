import EditableKeyStatDataBlock from '@admin/pages/release/content/components/EditableKeyStatDataBlock';
import render from '@common-test/render';
import { KeyStatisticDataBlock } from '@common/services/publicationService';
import _tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';

jest.mock('@admin/services/keyStatisticService');
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
      isCroppedTable: false,
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

  const testKeyStat: KeyStatisticDataBlock = {
    type: 'KeyStatisticDataBlock',
    id: 'keyStatDataBlock-1',
    trend: 'DataBlock trend',
    guidanceTitle: 'DataBlock guidance title',
    guidanceText: 'DataBlock guidance text',
    dataBlockParentId: 'block-1',
  };

  test('renders correctly when `isEditing` is false', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStatDataBlock
        releaseVersionId="release-1"
        keyStat={testKeyStat}
        keyStats={[testKeyStat]}
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    expect(await screen.findByText('DataBlock indicator')).toBeInTheDocument();

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
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: /Remove/ }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly when `isEditing` is true', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStatDataBlock
        isEditing
        releaseVersionId="release-1"
        keyStat={testKeyStat}
        keyStats={[testKeyStat]}
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    expect(await screen.findByText('DataBlock indicator')).toBeInTheDocument();

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

    expect(screen.getByRole('button', { name: /Edit/ })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Remove/ })).toBeInTheDocument();
  });

  test('does not render the trend when `trend` is undefined', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStatDataBlock
        releaseVersionId="release-1"
        keyStat={{ ...testKeyStat, trend: undefined }}
        keyStats={[testKeyStat]}
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    expect(await screen.findByText('DataBlock indicator')).toBeInTheDocument();

    expect(screen.queryByTestId('keyStat-trend')).not.toBeInTheDocument();
  });

  test('renders the default guidance title when `guidanceTitle` is undefined', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStatDataBlock
        releaseVersionId="release-1"
        keyStat={{ ...testKeyStat, guidanceTitle: undefined }}
        keyStats={[testKeyStat]}
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    expect(await screen.findByText('DataBlock indicator')).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Help for DataBlock indicator',
      }),
    ).toBeInTheDocument();
    expect(screen.getByTestId('keyStat-guidanceText')).toHaveTextContent(
      'DataBlock guidance text',
    );
  });

  test('does not render the guidance when `guidanceText` is undefined', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStatDataBlock
        releaseVersionId="release-1"
        keyStat={{ ...testKeyStat, guidanceText: undefined }}
        keyStats={[testKeyStat]}
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    expect(await screen.findByText('DataBlock indicator')).toBeInTheDocument();

    expect(
      screen.queryByRole('button', {
        name: 'DataBlock guidance title',
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('keyStat-guidanceText'),
    ).not.toBeInTheDocument();
  });

  test('clicking the remove button calls the `onRemove` handler', async () => {
    const user = userEvent.setup();
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    const onRemove = jest.fn();

    render(
      <EditableKeyStatDataBlock
        releaseVersionId="release-1"
        keyStat={testKeyStat}
        keyStats={[testKeyStat]}
        isEditing
        onRemove={onRemove}
        onSubmit={noop}
      />,
    );

    expect(await screen.findByText('DataBlock indicator')).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', {
        name: 'Remove key statistic: DataBlock indicator',
      }),
    );

    await waitFor(() => {
      expect(onRemove).toHaveBeenCalledTimes(1);
    });
  });

  test('clicking the edit button shows the form', async () => {
    const user = userEvent.setup();
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    render(
      <EditableKeyStatDataBlock
        releaseVersionId="release-1"
        keyStat={testKeyStat}
        keyStats={[testKeyStat]}
        isEditing
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    expect(await screen.findByText('DataBlock indicator')).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', {
        name: 'Edit key statistic: DataBlock indicator',
      }),
    );

    expect(await screen.findByLabelText('Trend')).toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: /Edit/ }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: /Remove/ }),
    ).not.toBeInTheDocument();
  });

  test('clicking the cancel button toggles back to preview', async () => {
    tableBuilderService.getDataBlockTableData.mockResolvedValue(
      testTableDataResponse,
    );

    const user = userEvent.setup();

    render(
      <EditableKeyStatDataBlock
        releaseVersionId="release-1"
        keyStat={testKeyStat}
        keyStats={[testKeyStat]}
        isEditing
        onRemove={noop}
        onSubmit={noop}
      />,
    );

    expect(await screen.findByText('DataBlock indicator')).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', {
        name: 'Edit key statistic: DataBlock indicator',
      }),
    );

    expect(await screen.findByLabelText('Trend')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(
      await screen.findByRole('button', {
        name: 'Edit key statistic: DataBlock indicator',
      }),
    ).toBeInTheDocument();

    expect(screen.getByRole('button', { name: /Remove/ })).toBeInTheDocument();

    expect(screen.queryByLabelText('Trend')).not.toBeInTheDocument();
  });

  test('can click the remove button when data block fails to load', async () => {
    tableBuilderService.getDataBlockTableData.mockRejectedValue(
      new Error('Something went wrong'),
    );

    const user = userEvent.setup();
    const handleRemove = jest.fn();

    render(
      <EditableKeyStatDataBlock
        releaseVersionId="release-1"
        keyStat={testKeyStat}
        keyStats={[testKeyStat]}
        onRemove={handleRemove}
        onSubmit={noop}
      />,
    );

    expect(
      await screen.findByText('Could not load key statistic'),
    ).toBeInTheDocument();

    expect(screen.queryByTestId('keyStat-title')).not.toBeInTheDocument();
    expect(screen.queryByTestId('keyStat-statistic')).not.toBeInTheDocument();
    expect(screen.queryByTestId('keyStat-trend')).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('keyStat-guidanceText'),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Edit' }),
    ).not.toBeInTheDocument();

    expect(handleRemove).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', {
        name: 'Remove key statistic',
      }),
    );

    expect(handleRemove).toHaveBeenCalledTimes(1);
  });
});
