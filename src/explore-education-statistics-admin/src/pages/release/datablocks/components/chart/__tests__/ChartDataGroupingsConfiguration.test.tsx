import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartDataGroupingsConfiguration from '@admin/pages/release/datablocks/components/chart/ChartDataGroupingsConfiguration';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import baseRender from '@common-test/render';
import { screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React, { ReactElement } from 'react';

describe('ChartDataGroupingsConfiguration', () => {
  const testTable = testFullTable;

  const testFormState: ChartBuilderForms = {
    options: {
      isValid: true,
      submitCount: 0,
      id: 'options',
      title: 'Options',
    },
    legend: {
      isValid: true,
      submitCount: 0,
      id: 'legend',
      title: 'Legend',
    },
    dataGroupings: {
      isValid: true,
      submitCount: 0,
      id: 'dataGroupings',
      title: 'Data groupings',
    },
  };

  const testDefaultChartOptions: ChartOptions = {
    alt: '',
    height: 600,
    titleType: 'default',
  };

  function render(
    element: ReactElement,
    initialForms: ChartBuilderForms = testFormState,
  ) {
    return baseRender(
      <ChartBuilderFormsContextProvider initialForms={initialForms}>
        {element}
      </ChartBuilderFormsContextProvider>,
    );
  }

  test('renders correctly with no data groupings', () => {
    render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        data={testTable.results}
        axisMajor={{
          dataSets: [],
          groupBy: 'locations',
          referenceLines: [],
          type: 'major',
          visible: true,
        }}
        legend={{
          position: 'top',
          items: [],
        }}
        options={testDefaultChartOptions}
        onSubmit={noop}
        onChange={noop}
      />,
    );

    expect(screen.getByText('No data groupings to edit.')).toBeInTheDocument();
  });

  test('renders correctly with single data grouping from new data set', () => {
    render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        data={testTable.results}
        axisMajor={{
          dataSets: [
            {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2014_AY',
            },
          ],
          groupBy: 'locations',
          referenceLines: [],
          type: 'major',
          visible: true,
        }}
        legend={{
          position: 'top',
          items: [],
        }}
        options={testDefaultChartOptions}
        onSubmit={noop}
        onChange={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(2);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2014/15)',
    );
    expect(row1Cells[1]).toHaveTextContent('5 equal intervals');
    expect(
      within(row1Cells[2]).getByRole('button', { name: 'Edit' }),
    ).toBeInTheDocument();
  });

  test('renders correctly with multiple data groupings from new data sets', () => {
    render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        data={testTable.results}
        axisMajor={{
          dataSets: [
            {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2014_AY',
            },
            {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
          ],
          groupBy: 'locations',
          referenceLines: [],
          type: 'major',
          visible: true,
        }}
        legend={{
          position: 'top',
          items: [],
        }}
        options={testDefaultChartOptions}
        onSubmit={noop}
        onChange={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2014/15)',
    );
    expect(row1Cells[1]).toHaveTextContent('5 equal intervals');
    expect(
      within(row1Cells[2]).getByRole('button', { name: 'Edit' }),
    ).toBeInTheDocument();

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2015/16)',
    );
    expect(row2Cells[1]).toHaveTextContent('5 equal intervals');
    expect(
      within(row2Cells[2]).getByRole('button', { name: 'Edit' }),
    ).toBeInTheDocument();
  });

  test('renders correctly with a single existing data group', () => {
    render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        data={testTable.results}
        axisMajor={{
          dataSets: [
            {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2014_AY',
            },
          ],
          groupBy: 'locations',
          referenceLines: [],
          type: 'major',
          visible: true,
        }}
        map={{
          dataSetConfigs: [
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataGrouping: {
                customGroups: [],
                numberOfGroups: 4,
                type: 'Quantiles',
              },
            },
          ],
        }}
        legend={{
          position: 'top',
          items: [],
        }}
        options={testDefaultChartOptions}
        onSubmit={noop}
        onChange={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(2);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2014/15)',
    );
    expect(row1Cells[1]).toHaveTextContent('4 quantiles');
    expect(
      within(row1Cells[2]).getByRole('button', { name: 'Edit' }),
    ).toBeInTheDocument();
  });

  test('renders correctly with multiple existing data groups', () => {
    render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        data={testTable.results}
        axisMajor={{
          dataSets: [
            {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2014_AY',
            },
            {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
            {
              filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
          ],
          groupBy: 'locations',
          referenceLines: [],
          type: 'major',
          visible: true,
        }}
        map={{
          dataSetConfigs: [
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataGrouping: {
                customGroups: [],
                numberOfGroups: 5,
                type: 'EqualIntervals',
              },
            },
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
              dataGrouping: {
                customGroups: [],
                numberOfGroups: 2,
                type: 'Quantiles',
              },
            },
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
              dataGrouping: {
                customGroups: [{ min: 0, max: 50 }],
                type: 'Custom',
              },
            },
          ],
        }}
        legend={{
          position: 'top',
          items: [],
        }}
        options={testDefaultChartOptions}
        onSubmit={noop}
        onChange={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(4);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2014/15)',
    );
    expect(row1Cells[1]).toHaveTextContent('5 equal intervals');
    expect(
      within(row1Cells[2]).getByRole('button', { name: 'Edit' }),
    ).toBeInTheDocument();

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2015/16)',
    );
    expect(row2Cells[1]).toHaveTextContent('2 quantiles');
    expect(
      within(row2Cells[2]).getByRole('button', { name: 'Edit' }),
    ).toBeInTheDocument();

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded secondary, All locations, 2015/16)',
    );
    expect(row3Cells[1]).toHaveTextContent('Custom');
    expect(
      within(row3Cells[2]).getByRole('button', { name: 'Edit' }),
    ).toBeInTheDocument();
  });

  test('renders correctly when data sets are removed', () => {
    render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        data={testTable.results}
        axisMajor={{
          dataSets: [
            {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
            {
              filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
          ],
          groupBy: 'locations',
          referenceLines: [],
          type: 'major',
          visible: true,
        }}
        map={{
          dataSetConfigs: [
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataGrouping: {
                customGroups: [],
                numberOfGroups: 6,
                type: 'EqualIntervals',
              },
            },
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
              dataGrouping: {
                customGroups: [],
                numberOfGroups: 2,
                type: 'Quantiles',
              },
            },
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
              dataGrouping: {
                customGroups: [{ min: 0, max: 50 }],
                type: 'Custom',
              },
            },
          ],
        }}
        legend={{
          position: 'top',
          items: [],
        }}
        options={testDefaultChartOptions}
        onSubmit={noop}
        onChange={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2015/16)',
    );
    expect(row1Cells[1]).toHaveTextContent('2 quantiles');
    expect(
      within(row1Cells[2]).getByRole('button', { name: 'Edit' }),
    ).toBeInTheDocument();

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded secondary, All locations, 2015/16)',
    );
    expect(row2Cells[1]).toHaveTextContent('Custom');
    expect(
      within(row2Cells[2]).getByRole('button', { name: 'Edit' }),
    ).toBeInTheDocument();
  });

  test('renders correctly when data sets are added', () => {
    render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        data={testTable.results}
        axisMajor={{
          dataSets: [
            {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2014_AY',
            },
            {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
            {
              filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
          ],
          groupBy: 'locations',
          referenceLines: [],
          type: 'major',
          visible: true,
        }}
        map={{
          dataSetConfigs: [
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
              dataGrouping: {
                customGroups: [],
                numberOfGroups: 2,
                type: 'Quantiles',
              },
            },
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
              dataGrouping: {
                customGroups: [{ min: 0, max: 50 }],
                type: 'Custom',
              },
            },
          ],
        }}
        legend={{
          position: 'top',
          items: [],
        }}
        options={testDefaultChartOptions}
        onSubmit={noop}
        onChange={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(4);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2014/15)',
    );
    expect(row1Cells[1]).toHaveTextContent('5 equal intervals');
    expect(
      within(row1Cells[2]).getByRole('button', { name: 'Edit' }),
    ).toBeInTheDocument();

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2015/16)',
    );
    expect(row2Cells[1]).toHaveTextContent('2 quantiles');
    expect(
      within(row2Cells[2]).getByRole('button', { name: 'Edit' }),
    ).toBeInTheDocument();

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded secondary, All locations, 2015/16)',
    );
    expect(row3Cells[1]).toHaveTextContent('Custom');
    expect(
      within(row3Cells[2]).getByRole('button', { name: 'Edit' }),
    ).toBeInTheDocument();
  });

  test('shows a validation error if another form is invalid', async () => {
    const { user } = render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        data={testTable.results}
        axisMajor={{
          dataSets: [
            {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2014_AY',
            },
          ],
          groupBy: 'locations',
          referenceLines: [],
          type: 'major',
          visible: true,
        }}
        legend={{
          position: 'top',
          items: [],
        }}
        options={testDefaultChartOptions}
        onSubmit={noop}
        onChange={noop}
      />,
      {
        ...testFormState,
        options: {
          ...testFormState.options,
          isValid: false,
        },
      },
    );

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    expect(await screen.findByText('Cannot save chart')).toBeInTheDocument();
    expect(screen.getByText('Options tab is invalid')).toBeInTheDocument();
  });
});
