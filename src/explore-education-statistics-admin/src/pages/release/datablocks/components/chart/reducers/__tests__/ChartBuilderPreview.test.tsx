import { verticalBarBlockDefinition } from '@common/modules/charts/components/VerticalBarBlock';
import { AxisConfiguration } from '@common/modules/charts/types/chart';
import React from 'react';
import { render, screen } from '@testing-library/react';
import ChartBuilderPreview from '@admin/pages/release/datablocks/components/chart/ChartBuilderPreview';
import { ChartRendererProps } from '@common/modules/charts/components/ChartRenderer';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import produce from 'immer';
import { DataSet } from '@common/modules/charts/types/dataSet';

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

  const testFullTableMeta: FullTableMeta = {
    geoJsonAvailable: false,
    publicationName: 'Pupil absence in schools in England',
    subjectName: 'Absence by characteristic',
    footnotes: [],
    boundaryLevels: [],
    filters: {
      Characteristic: {
        name: 'characteristic',
        options: [
          new CategoryFilter({
            value: 'total',
            label: 'Total',
            group: 'Gender',
            category: 'Characteristic',
          }),
        ],
      },
    },
    indicators: [
      new Indicator({
        label: 'Authorised absence rate',
        value: 'authAbsRate',
        unit: '%',
        name: 'sess_authorised_percent',
      }),
    ],
    locations: [
      new LocationFilter({
        value: 'england',
        label: 'England',
        level: 'country',
      }),
    ],
    timePeriodRange: [
      new TimePeriodFilter({
        code: 'AY',
        year: 2015,
        label: '2015/16',
        order: 0,
      }),
    ],
  };

  const testInfographicChartRenderer: ChartRendererProps = {
    type: 'infographic',
    fileId: '1',
    data: [],
    meta: testFullTableMeta,
    alt: '',
    height: 100,
    axes: {},
  };

  const testLineChartRenderer: ChartRendererProps = {
    type: 'line',
    data: [],
    meta: testFullTableMeta,
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

  const testVerticalBarChartRenderer: ChartRendererProps = {
    type: 'verticalbar',
    data: [],
    meta: testFullTableMeta,
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

  const testHorizontalBarChartRenderer: ChartRendererProps = {
    type: 'horizontalbar',
    data: [],
    meta: testFullTableMeta,
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

  const testMapChartRenderer: ChartRendererProps = {
    type: 'map',
    data: [],
    meta: testFullTableMeta,
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

  test('renders the loading spinner when the loading flag is indicating that it is loading', () => {
    render(
      <ChartBuilderPreview
        definition={verticalBarBlockDefinition}
        chart={testLineChartRenderer}
        loading
      />,
    );
    const detailsSection = screen.queryByTestId('chartBuilderPreviewContainer');
    expect(detailsSection).toBeInTheDocument();
    expect(detailsSection).toHaveTextContent('Loading chart data');
  });

  [
    testInfographicChartRenderer,
    testLineChartRenderer,
    testVerticalBarChartRenderer,
    testHorizontalBarChartRenderer,
    testMapChartRenderer,
  ].forEach(chartRenderer => {
    test(`renders chart of type '${chartRenderer.type}' when all mandatory fields are provided`, () => {
      render(
        <ChartBuilderPreview
          definition={verticalBarBlockDefinition}
          chart={chartRenderer}
          loading={false}
        />,
      );
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
    testLineChartRenderer,
    testVerticalBarChartRenderer,
    testHorizontalBarChartRenderer,
    testMapChartRenderer,
  ].forEach(chartRenderer => {
    test(`renders preview help text for chart of type '${chartRenderer.type}' when no data sets are yet added`, () => {
      render(
        <ChartBuilderPreview
          definition={verticalBarBlockDefinition}
          chart={produce(chartRenderer, draft => {
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
        chartRenderer.type === 'map'
          ? 'Add data and choose a version of geographic data to view a preview'
          : 'Configure the chart and add data to view a preview';
      expect(detailsSection).toHaveTextContent(expectedHelpText);
    });
  });

  test(`renders preview help text for chart of type 'infographic' when no fileId is selected`, () => {
    render(
      <ChartBuilderPreview
        definition={verticalBarBlockDefinition}
        chart={produce(testInfographicChartRenderer, draft => {
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
        definition={verticalBarBlockDefinition}
        chart={produce(testMapChartRenderer, draft => {
          draft.boundaryLevel = undefined;
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
