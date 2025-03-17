import {
  testFullTable,
  testTableData,
} from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartBuilderTabSection from '@admin/pages/release/datablocks/components/chart/ChartBuilderTabSection';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ReleaseDataBlock } from '@admin/services/dataBlockService';
import render from '@common-test/render';
import { Chart } from '@common/modules/charts/types/chart';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import _tableBuilderService, {
  LocationGeoJsonOption,
  TableDataQuery,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import { screen } from '@testing-library/react';
import noop from 'lodash/noop';

jest.mock('@common/services/tableBuilderService');
const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('ChartBuilderTabSection', () => {
  const testFormState: ChartBuilderForms = {
    options: {
      isValid: true,
      submitCount: 0,
      id: 'options',
      title: 'Options',
    },
    dataSets: {
      isValid: true,
      submitCount: 0,
      id: 'dataSets',
      title: 'Data sets',
    },
  };

  const testChart: Chart = {
    type: 'map',
    boundaryLevel: 2,
    map: {
      dataSetConfigs: [],
    },
    title: 'Data block title',
    subtitle: '',
    alt: 'd',
    height: 600,
    includeNonNumericData: false,
    axes: {
      major: {
        type: 'major',
        groupBy: 'locations',
        groupByFilter: '',
        groupByFilterGroups: false,
        sortBy: 'name',
        sortAsc: false,
        dataSets: [
          {
            order: 0,
            indicator: 'overall-absence-sessions',
            filters: ['state-funded-primary'],
            timePeriod: '2014_AY',
          },
        ],
        referenceLines: [],
        visible: true,
        unit: '',
        showGrid: false,
        label: {
          text: '',
          rotated: false,
        },
        min: 0,
        size: 50,
        tickConfig: 'default',
        tickSpacing: 1,
      },
    },
    legend: { items: [] },
  };

  const testDataBlock: ReleaseDataBlock = {
    id: 'data-block-1',
    dataBlockParentId: 'data-block-parent-1',
    dataSetId: 'data-set-1',
    dataSetName: 'Test data set',
    name: 'Test block',
    highlightName: 'Test highlight name',
    source: '',
    heading: '',
    table: {
      tableHeaders: {
        columnGroups: [],
        rowGroups: [],
        columns: [],
        rows: [],
      },
      indicators: [],
    },
    charts: [testChart],
    query: {
      subjectId: 'subject-1',
      indicators: ['authorised-absence-sessions'],
      filters: ['state-funded-primary', 'state-funded-secondary'],
      timePeriod: {
        startYear: 2014,
        startCode: 'AY',
        endYear: 2014,
        endCode: 'AY',
      },
      locationIds: ['barnet', 'barnsley'],
    },
  };
  const testQuery: TableDataQuery = {
    subjectId: 'subject-id',
    filters: ['filter-id'],
    indicators: ['indicator-id'],
    locationIds: ['location-id'],
  };

  const testFullTableWithBoundaryLevels: FullTable = {
    ...testFullTable,
    subjectMeta: {
      ...testFullTable.subjectMeta,
      boundaryLevels: [
        {
          id: 1,
          label: 'Boundary level 1',
        },
        {
          id: 2,
          label: 'Boundary level 2',
        },
      ],
    },
  };

  test('renders the ChartBuilder', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartBuilderTabSection
          dataBlock={testDataBlock}
          releaseVersionId="release-1"
          query={testQuery}
          table={testFullTableWithBoundaryLevels}
          onDataBlockSave={noop}
          onTableUpdate={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(
      screen.getByRole('button', { name: 'Chart preview' }),
    ).toBeInTheDocument();

    const tabs = screen.getAllByRole('tab');

    expect(tabs).toHaveLength(5);
  });

  test('calls `getTableData` and `onTableUpdate` when boundary level changed', async () => {
    tableBuilderService.getTableData.mockResolvedValue({
      ...testTableData,
      subjectMeta: { ...testTableData.subjectMeta, locations: {} },
    });
    tableBuilderService.getDataBlockGeoJson.mockResolvedValue(
      testTableData.subjectMeta.locations as Dictionary<
        LocationGeoJsonOption[]
      >,
    );

    const handleUpdate = jest.fn();

    const { user } = render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartBuilderTabSection
          dataBlock={testDataBlock}
          releaseVersionId="release-1"
          query={testQuery}
          table={testFullTableWithBoundaryLevels}
          onDataBlockSave={noop}
          onTableUpdate={handleUpdate}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(
      screen.getByRole('tab', { name: 'Boundary levels' }),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('tab', { name: 'Boundary levels' }));

    await user.selectOptions(screen.getByLabelText('Default boundary level'), [
      '1',
    ]);

    expect(tableBuilderService.getTableData).toHaveBeenCalledWith(
      { ...testQuery, boundaryLevel: 1 }, // @MarkFix boundaryLevel
      'release-1',
    );

    expect(tableBuilderService.getDataBlockGeoJson).toHaveBeenCalledWith(
      'release-1',
      'data-block-parent-1',
      1,
    );

    expect(handleUpdate).toHaveBeenCalledWith({
      table: testFullTable,
      query: { ...testQuery, boundaryLevel: 1 },
    });
  });
});
