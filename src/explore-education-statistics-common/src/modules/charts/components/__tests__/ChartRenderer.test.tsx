import {
  testChartConfiguration,
  testChartTableData,
} from '@common/modules/charts/components/__tests__/__data__/testChartData';
import { expectTicks } from '@common/modules/charts/components/__tests__/testUtils';
import HorizontalBarBlock, {
  HorizontalBarProps,
} from '@common/modules/charts/components/HorizontalBarBlock';
import {
  verticalBarBlockDefinition,
  VerticalBarProps,
} from '@common/modules/charts/components/VerticalBarBlock';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { TableDataResponse } from '@common/services/tableBuilderService';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/charts/components/ChartRenderer';
import { AxisConfiguration } from '@common/modules/charts/types/chart';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { DataSet } from '@common/modules/charts/types/dataSet';
import produce from 'immer';

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
  };

  test('renders auto-generated boundary level footnote successfully', async () => {
    render(<ChartRenderer id="id" {...testMapChartRenderer} />);
    const footnotes = screen.queryByTestId('footnotes');
    expect(footnotes).toBeInTheDocument();
    expect(footnotes).toHaveTextContent(
      'This map uses the boundary data Countries December 2017 Ultra Generalised Clipped Boundaries in UK',
    );
  });

  test('does not render auto-generated boundary level footnote when boundary level not provided', async () => {
    render(
      <ChartRenderer
        id="id"
        {...produce(testMapChartRenderer, draft => {
          draft.boundaryLevel = undefined;
        })}
      />,
    );
    expect(screen.queryByTestId('footnotes')).not.toBeInTheDocument();
  });
});
