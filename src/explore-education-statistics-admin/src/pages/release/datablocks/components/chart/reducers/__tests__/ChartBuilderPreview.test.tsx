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

  [testLineChartRenderer, testInfographicChartRenderer].forEach(
    chartRenderer => {
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
    },
  );

  test('renders preview help text when no data sets are yet added', () => {
    render(
      <ChartBuilderPreview
        definition={verticalBarBlockDefinition}
        chart={produce(testLineChartRenderer, draft => {
          draft.axes.major.dataSets = [];
        })}
        loading={false}
      />,
    );
    const detailsSection = screen.queryByTestId('chartBuilderPreviewContainer');
    expect(detailsSection).toBeInTheDocument();
    expect(detailsSection).toHaveTextContent(
      'Add data to view a preview of the chart',
    );
  });

  test('renders preview help text for infographics when no fileId is provided', () => {
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
      'Add data to view a preview of the chart',
    );
  });

  test('renders preview help text for infographics when no fileId is provided', () => {
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
      'Add data to view a preview of the chart',
    );
  });
});
