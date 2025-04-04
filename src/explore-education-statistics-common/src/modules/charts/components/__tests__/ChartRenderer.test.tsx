import render from '@common-test/render';
import React from 'react';
import { testChartTableData } from '@common/modules/charts/components/__tests__/__data__/testChartData';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/charts/components/ChartRenderer';
import { AxisConfiguration } from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { screen } from '@testing-library/react';

jest.mock('recharts/lib/util/LogUtils');

describe('ChartRenderer', () => {
  const testDataSets: DataSet[] = [
    {
      filters: ['male'],
      indicator: 'unauthorised-absence-sessions',
      order: 0,
    },
  ];

  const testAxisConfiguration: AxisConfiguration = {
    dataSets: testDataSets,
    groupBy: 'timePeriod',
    min: 0,
    referenceLines: [],
    showGrid: true,
    size: 50,
    sortAsc: true,
    sortBy: 'name',
    tickConfig: 'default',
    tickSpacing: 1,
    type: 'major',
    visible: true,
    unit: '',
    label: {
      text: '',
    },
  };
  const testFullTableMeta = mapFullTable(testChartTableData);
  const testMapChartRenderer: ChartRendererProps = {
    id: 'test-id',
    chart: {
      type: 'map',
      meta: testFullTableMeta.subjectMeta,
      data: testFullTableMeta.results,
      alt: '',
      height: 100,
      axes: {
        major: testAxisConfiguration,
      },
      legend: {
        items: [],
      },
      boundaryLevel: 1,
      map: { dataSetConfigs: [] },
      onBoundaryLevelChange: async () => undefined,
    },
  };

  test('renders auto-generated boundary level footnote successfully', async () => {
    render(<ChartRenderer {...testMapChartRenderer} />);
    const footnotes = screen.queryByTestId('footnotes');

    expect(footnotes).toBeInTheDocument();
    expect(footnotes).toHaveTextContent(
      'This map uses the boundary data Countries December 2017 Ultra Generalised Clipped Boundaries in UK',
    );
  });

  test('renders export button successfully', async () => {
    render(<ChartRenderer {...testMapChartRenderer} />);

    expect(screen.queryByText('Export options')).toBeInTheDocument();
  });
});
