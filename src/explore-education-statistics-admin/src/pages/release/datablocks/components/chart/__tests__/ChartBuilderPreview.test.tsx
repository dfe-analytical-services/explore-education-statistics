import ChartBuilderPreview from '@admin/pages/release/datablocks/components/chart/ChartBuilderPreview';
import { testChartTableData } from '@common/modules/charts/components/__tests__/__data__/testChartData';
import { MapBlockProps } from '@common/modules/charts/components/MapBlock';
import {
  AxisConfiguration,
  DraftFullChart,
  InfographicConfig,
  MapChartConfig,
} from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { render, screen } from '@testing-library/react';
import { produce } from 'immer';

describe('ChartBuilderPreview', () => {
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

  const testInfographicChart: DraftFullChart = {
    data: [],
    meta: testFullTableMeta.subjectMeta,
    chartConfig: {
      type: 'infographic',
      fileId: '1',
      alt: '',
      height: 100,
    },
  };

  const testLineChart: DraftFullChart = {
    data: [],
    meta: testFullTableMeta.subjectMeta,
    chartConfig: {
      type: 'line',
      alt: '',
      height: 100,
      axes: {
        major: testAxisConfiguration,
        minor: testAxisConfiguration,
      },
      legend: {
        items: [],
      },
    },
  };

  const testVerticalBarChart: DraftFullChart = {
    data: [],
    meta: testFullTableMeta.subjectMeta,
    chartConfig: {
      type: 'verticalbar',
      alt: '',
      height: 100,
      axes: {
        major: testAxisConfiguration,
        minor: testAxisConfiguration,
      },
      legend: {
        items: [],
      },
    },
  };

  const testHorizontalBarChart: DraftFullChart = {
    data: [],
    meta: testFullTableMeta.subjectMeta,
    chartConfig: {
      type: 'horizontalbar',
      alt: '',
      height: 100,
      axes: {
        major: testAxisConfiguration,
        minor: testAxisConfiguration,
      },
      legend: {
        items: [],
      },
    },
  };

  const testMapChart: DraftFullChart<Omit<MapBlockProps, 'id'>> = {
    onBoundaryLevelChange: async () => {},
    data: [],
    meta: testFullTableMeta.subjectMeta,
    chartConfig: {
      type: 'map',
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
    },
  };

  test('renders the loading spinner when the loading flag is indicating that it is loading', () => {
    render(<ChartBuilderPreview fullChart={testLineChart} loading />);
    const detailsSection = screen.queryByTestId('chartBuilderPreviewContainer');
    expect(detailsSection).toBeInTheDocument();
    expect(detailsSection).toHaveTextContent('Loading chart data');
  });

  [
    testInfographicChart,
    testLineChart,
    testVerticalBarChart,
    testHorizontalBarChart,
    testMapChart,
  ].forEach(fullChart => {
    test(`renders chart of type '${fullChart.chartConfig.type}' when all mandatory fields are provided`, () => {
      render(<ChartBuilderPreview fullChart={fullChart} loading={false} />);
      const detailsSection = screen.queryByTestId(
        'chartBuilderPreviewContainer',
      );
      expect(detailsSection).toBeInTheDocument();

      // We don't need to test a fully rendering chart here, just that the component is attempting to render it.
      expect(detailsSection).toHaveTextContent(
        'Unable to render chart, invalid data configured',
      );
    });
  });

  [
    testLineChart,
    testVerticalBarChart,
    testHorizontalBarChart,
    testMapChart,
  ].forEach(fullChart => {
    test(`renders preview help text for chart of type '${fullChart.chartConfig.type}' when no data sets are yet added`, () => {
      render(
        <ChartBuilderPreview
          fullChart={produce(fullChart, draft => {
            draft.chartConfig.axes = {
              ...draft.chartConfig.axes,
              major: {
                dataSets: [],
                type: 'major',
                referenceLines: [],
              },
            };
          })}
          loading={false}
        />,
      );
      const detailsSection = screen.queryByTestId(
        'chartBuilderPreviewContainer',
      );
      expect(detailsSection).toBeInTheDocument();
      const expectedHelpText =
        fullChart.chartConfig.type === 'map'
          ? 'Add data and choose a version of geographic data to view a preview'
          : 'Configure the chart and add data to view a preview';
      expect(detailsSection).toHaveTextContent(expectedHelpText);
    });
  });

  test(`renders preview help text for chart of type 'infographic' when no fileId is selected`, () => {
    render(
      <ChartBuilderPreview
        fullChart={produce(testInfographicChart, draft => {
          (draft.chartConfig as InfographicConfig).fileId = '';
        })}
        loading={false}
      />,
    );
    const detailsSection = screen.queryByTestId('chartBuilderPreviewContainer');
    expect(detailsSection).toBeInTheDocument();
    expect(detailsSection).toHaveTextContent(
      'Choose an infographic file to view a preview',
    );
  });

  test(`renders preview help text for chart of type 'map' when no boundaryLevel is selected`, () => {
    render(
      <ChartBuilderPreview
        fullChart={produce(testMapChart, draft => {
          (draft.chartConfig as MapChartConfig).boundaryLevel = 0;
        })}
        loading={false}
      />,
    );
    const detailsSection = screen.queryByTestId('chartBuilderPreviewContainer');
    expect(detailsSection).toBeInTheDocument();
    expect(detailsSection).toHaveTextContent(
      'Add data and choose a version of geographic data to view a preview',
    );
  });
});
