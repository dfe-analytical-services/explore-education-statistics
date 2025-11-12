import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartDataGroupingsConfiguration from '@admin/pages/release/datablocks/components/chart/ChartDataGroupingsConfiguration';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import baseRender from '@common-test/render';
import { defaultDataGrouping } from '@common/modules/charts/util/getMapDataSetCategoryConfigs';
import { screen, waitFor, within } from '@testing-library/react';
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
        meta={testFullTable.subjectMeta}
        onChange={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByText('No data groupings to edit.')).toBeInTheDocument();
  });

  test.only('renders correctly with single data grouping from new data set', () => {
    render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        map={{
          dataSetConfigs: [
            {
              dataGrouping: defaultDataGrouping,
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataSetKey: 'dataSetKey1',
            },
          ],
        }}
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
        map={{
          dataSetConfigs: [
            {
              dataGrouping: defaultDataGrouping,
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataSetKey: 'dataSetKey1',
            },
            {
              dataGrouping: defaultDataGrouping,
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
              dataSetKey: 'dataSetKey2',
            },
          ],
        }}
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
        map={{
          dataSetConfigs: [
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataSetKey: 'dataSetKey1',
              dataGrouping: {
                customGroups: [],
                numberOfGroups: 4,
                type: 'Quantiles',
              },
            },
          ],
        }}
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
        map={{
          dataSetConfigs: [
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataSetKey: 'dataSetKey1',
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
              dataSetKey: 'dataSetKey2',
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
              dataSetKey: 'dataSetKey3',
              dataGrouping: {
                customGroups: [{ min: 0, max: 50 }],
                type: 'Custom',
              },
            },
          ],
        }}
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
        map={{
          dataSetConfigs: [
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataSetKey: 'dataSetKey1',
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
              dataSetKey: 'dataSetKey2',
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
              dataSetKey: 'dataSetKey3',
              dataGrouping: {
                customGroups: [{ min: 0, max: 50 }],
                type: 'Custom',
              },
            },
          ],
        }}
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

  test('making a data grouping change and submitting the form', async () => {
    const handleChange = jest.fn();
    const handleSubmit = jest.fn();

    const { user } = render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        map={{
          dataSetConfigs: [
            {
              dataGrouping: defaultDataGrouping,
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataSetKey: 'dataSetKey1',
            },
            {
              dataGrouping: defaultDataGrouping,
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
              dataSetKey: 'dataSetKey2',
            },
          ],
        }}
        onSubmit={handleSubmit}
        onChange={handleChange}
      />,
    );

    await user.click(screen.getAllByRole('button', { name: 'Edit' })[0]);

    expect(await screen.findByRole('dialog')).toBeInTheDocument();
    expect(screen.getByText('Edit groupings')).toBeInTheDocument();

    await user.click(screen.getByLabelText('Quantiles'));
    await user.click(screen.getByRole('button', { name: 'Done' }));

    await waitFor(() => {
      expect(handleChange).toHaveBeenCalledTimes(1);
      expect(handleChange).toHaveBeenCalledWith({
        dataSetConfigs: [
          {
            dataGrouping: { ...defaultDataGrouping, type: 'Quantiles' },
            dataSet: {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2014_AY',
            },
            dataSetKey: 'dataSetKey1',
          },
          {
            dataGrouping: defaultDataGrouping,
            dataSet: {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
            dataSetKey: 'dataSetKey2',
          },
        ],
      });
    });

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
    });
  });
});
