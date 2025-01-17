import ChartBuilderPreview from '@admin/pages/release/datablocks/components/chart/ChartBuilderPreview';
import { testChartTableData } from '@common/modules/charts/components/__tests__/__data__/testChartData';
import { RenderrableChart } from '@common/modules/charts/components/ChartRenderer';
import { AxisConfiguration } from '@common/modules/charts/types/chart';
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

  const testInfographicChart: RenderrableChart = {
    type: 'infographic',
    fileId: '1',
    data: [],
    meta: testFullTableMeta.subjectMeta,
    alt: '',
    height: 100,
    axes: {},
  };

  const testLineChart: RenderrableChart = {
    type: 'line',
    data: [],
    meta: testFullTableMeta.subjectMeta,
    alt: '',
    height: 100,
    axes: {
      major: testAxisConfiguration,
      minor: testAxisConfiguration,
    },
    legend: {
      items: [],
    },
  };

  const testVerticalBarChart: RenderrableChart = {
    type: 'verticalbar',
    data: [],
    meta: testFullTableMeta.subjectMeta,
    alt: '',
    height: 100,
    axes: {
      major: testAxisConfiguration,
      minor: testAxisConfiguration,
    },
    legend: {
      items: [],
    },
  };

  const testHorizontalBarChart: RenderrableChart = {
    type: 'horizontalbar',
    data: [],
    meta: testFullTableMeta.subjectMeta,
    alt: '',
    height: 100,
    axes: {
      major: testAxisConfiguration,
      minor: testAxisConfiguration,
    },
    legend: {
      items: [],
    },
  };

  const testMapChart: RenderrableChart = {
    type: 'map',
    onBoundaryLevelChange: async () => {},
    data: [],
    meta: testFullTableMeta.subjectMeta,
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
  };

  test('renders the loading spinner when the loading flag is indicating that it is loading', () => {
    render(<ChartBuilderPreview chart={testLineChart} loading />);
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
  ].forEach(chart => {
    test(`renders chart of type '${chart.type}' when all mandatory fields are provided`, () => {
      render(<ChartBuilderPreview chart={chart} loading={false} />);
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
  ].forEach(chart => {
    test(`renders preview help text for chart of type '${chart.type}' when no data sets are yet added`, () => {
      render(
        <ChartBuilderPreview
          chart={produce(chart, draft => {
            draft.axes.major.dataSets = [];
          })}
          loading={false}
        />,
      );
      const detailsSection = screen.queryByTestId(
        'chartBuilderPreviewContainer',
      );
      expect(detailsSection).toBeInTheDocument();
      const expectedHelpText =
        chart.type === 'map'
          ? 'Add data and choose a version of geographic data to view a preview'
          : 'Configure the chart and add data to view a preview';
      expect(detailsSection).toHaveTextContent(expectedHelpText);
    });
  });

  test(`renders preview help text for chart of type 'infographic' when no fileId is selected`, () => {
    render(
      <ChartBuilderPreview
        chart={produce(testInfographicChart, draft => {
          draft.fileId = '';
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
        chart={produce(testMapChart, draft => {
          draft.boundaryLevel = 0;
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
