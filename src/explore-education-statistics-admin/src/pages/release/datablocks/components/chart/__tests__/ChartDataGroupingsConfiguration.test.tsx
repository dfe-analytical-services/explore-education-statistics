import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartDataGroupingsConfiguration from '@admin/pages/release/datablocks/components/chart/ChartDataGroupingsConfiguration';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import { MapDataSetConfig } from '@common/modules/charts/types/chart';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import {
  render as baseRender,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
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

  test('renders correctly when editing a grouping with equal intervals', async () => {
    const handleChange = jest.fn();

    render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        data={testTable.results}
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
                numberOfGroups: 7,
                type: 'EqualIntervals',
              },
            },
          ],
        }}
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
        onChange={handleChange}
      />,
    );

    const rows = screen.getAllByRole('row');
    userEvent.click(within(rows[1]).getByRole('button', { name: 'Edit' }));

    const modal = within(screen.getByRole('dialog'));

    expect(modal.getByLabelText('Equal intervals')).toBeChecked();
    expect(modal.getAllByLabelText('Number of data groups')[0]).toHaveValue(7);

    expect(modal.getByLabelText('Quantiles')).not.toBeChecked();
    expect(modal.getByLabelText('New custom groups')).not.toBeChecked();
  });

  test('renders correctly when editing a grouping with quantiles', async () => {
    const handleChange = jest.fn();

    render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        data={testTable.results}
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
        onChange={handleChange}
      />,
    );

    const rows = screen.getAllByRole('row');
    userEvent.click(within(rows[1]).getByRole('button', { name: 'Edit' }));

    const modal = within(screen.getByRole('dialog'));

    expect(modal.getByLabelText('Quantiles')).toBeChecked();
    expect(modal.getAllByLabelText('Number of data groups')[1]).toHaveValue(4);

    expect(modal.getByLabelText('Equal intervals')).not.toBeChecked();
    expect(modal.getByLabelText('New custom groups')).not.toBeChecked();
  });

  test('renders correctly when editing a grouping with custom groups', async () => {
    const handleChange = jest.fn();

    render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        data={testTable.results}
        map={{
          dataSetConfigs: [
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataGrouping: {
                customGroups: [
                  {
                    min: 0,
                    max: 10,
                  },
                  {
                    min: 11,
                    max: 20,
                  },
                ],
                type: 'Custom',
              },
            },
          ],
        }}
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
        onChange={handleChange}
      />,
    );

    const rows = screen.getAllByRole('row');
    userEvent.click(within(rows[1]).getByRole('button', { name: 'Edit' }));

    const modal = within(screen.getByRole('dialog'));

    expect(modal.getByLabelText('New custom groups')).toBeChecked();
    const customGroupsRows = modal.getAllByRole('row');
    expect(customGroupsRows).toHaveLength(4);
    const customGroupsRow1Cells = within(customGroupsRows[1]).getAllByRole(
      'cell',
    );
    expect(customGroupsRow1Cells[0]).toHaveTextContent('0');
    expect(customGroupsRow1Cells[1]).toHaveTextContent('10');
    expect(
      within(customGroupsRow1Cells[2]).getByRole('button', {
        name: 'Remove group',
      }),
    ).toBeInTheDocument();
    const customGroupsRow2Cells = within(customGroupsRows[2]).getAllByRole(
      'cell',
    );
    expect(customGroupsRow2Cells[0]).toHaveTextContent('11');
    expect(customGroupsRow2Cells[1]).toHaveTextContent('20');
    expect(
      within(customGroupsRow2Cells[2]).getByRole('button', {
        name: 'Remove group',
      }),
    ).toBeInTheDocument();

    expect(modal.getByLabelText('Equal intervals')).not.toBeChecked();
    expect(modal.getByLabelText('Quantiles')).not.toBeChecked();
  });

  test('changing a data grouping to quantiles', async () => {
    const handleChange = jest.fn();

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
        onChange={handleChange}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    userEvent.click(within(rows[1]).getByRole('button', { name: 'Edit' }));

    const modal = within(screen.getByRole('dialog'));
    userEvent.click(modal.getByLabelText('Quantiles'));
    userEvent.clear(modal.getAllByLabelText('Number of data groups')[1]);
    userEvent.type(modal.getAllByLabelText('Number of data groups')[1], '3');

    userEvent.click(modal.getByRole('button', { name: 'Done' }));

    const expectedValues: MapDataSetConfig[] = [
      {
        dataSet: {
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          indicator: 'authorised-absence-sessions',
          timePeriod: '2014_AY',
        },
        dataGrouping: {
          customGroups: [],
          numberOfGroups: 3,
          type: 'Quantiles',
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
          numberOfGroups: 5,
          type: 'EqualIntervals',
        },
      },
    ];

    await waitFor(() => {
      expect(handleChange).toHaveBeenCalledWith(expectedValues);
    });

    expect(within(rows[1]).getByText('3 quantiles')).toBeInTheDocument();
  });

  test('changing a data grouping to custom groups', async () => {
    const handleChange = jest.fn();

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
        onChange={handleChange}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    userEvent.click(within(rows[1]).getByRole('button', { name: 'Edit' }));

    const modal = within(screen.getByRole('dialog'));
    userEvent.click(modal.getByLabelText('New custom groups'));
    userEvent.type(modal.getByLabelText('Min'), '0');
    userEvent.type(modal.getByLabelText('Max'), '10');
    userEvent.click(modal.getByRole('button', { name: 'Add group' }));

    await waitFor(() => {
      expect(screen.getByText('Remove group')).toBeInTheDocument();
    });
    const customGroupsRows = modal.getAllByRole('row');
    expect(customGroupsRows).toHaveLength(3);
    const customGroupsRow1Cells = within(customGroupsRows[1]).getAllByRole(
      'cell',
    );
    expect(customGroupsRow1Cells[0]).toHaveTextContent('0');
    expect(customGroupsRow1Cells[1]).toHaveTextContent('10');
    expect(
      within(customGroupsRow1Cells[2]).getByRole('button', {
        name: 'Remove group',
      }),
    ).toBeInTheDocument();

    userEvent.click(modal.getByRole('button', { name: 'Done' }));

    const expectedValues: MapDataSetConfig[] = [
      {
        dataSet: {
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          indicator: 'authorised-absence-sessions',
          timePeriod: '2014_AY',
        },
        dataGrouping: {
          customGroups: [{ min: 0, max: 10 }],
          type: 'Custom',
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
          numberOfGroups: 5,
          type: 'EqualIntervals',
        },
      },
    ];

    await waitFor(() => {
      expect(handleChange).toHaveBeenCalledWith(expectedValues);
    });

    expect(within(rows[1]).getByText('Custom')).toBeInTheDocument();
  });

  test('editing existing custom groups', async () => {
    const handleChange = jest.fn();

    render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        data={testTable.results}
        map={{
          dataSetConfigs: [
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataGrouping: {
                customGroups: [
                  { min: 0, max: 50 },
                  { min: 51, max: 66 },
                ],
                type: 'Custom',
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
                numberOfGroups: 5,
                type: 'EqualIntervals',
              },
            },
          ],
        }}
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
        onChange={handleChange}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    userEvent.click(within(rows[1]).getByRole('button', { name: 'Edit' }));

    const modal = within(screen.getByRole('dialog'));

    const customGroupsRows = modal.getAllByRole('row');
    expect(customGroupsRows).toHaveLength(4);

    const customGroupsRow1Cells = within(customGroupsRows[1]).getAllByRole(
      'cell',
    );
    expect(customGroupsRow1Cells[0]).toHaveTextContent('0');
    expect(customGroupsRow1Cells[1]).toHaveTextContent('50');
    expect(
      within(customGroupsRow1Cells[2]).getByRole('button', {
        name: 'Remove group',
      }),
    ).toBeInTheDocument();

    const customGroupsRow2Cells = within(customGroupsRows[2]).getAllByRole(
      'cell',
    );
    expect(customGroupsRow2Cells[0]).toHaveTextContent('51');
    expect(customGroupsRow2Cells[1]).toHaveTextContent('66');
    expect(
      within(customGroupsRow2Cells[2]).getByRole('button', {
        name: 'Remove group',
      }),
    ).toBeInTheDocument();

    userEvent.click(
      within(customGroupsRows[2]).getByRole('button', { name: 'Remove group' }),
    );

    expect(modal.getAllByRole('row')).toHaveLength(3);
    expect(modal.queryByText('51')).not.toBeInTheDocument();
    expect(modal.queryByText('66')).not.toBeInTheDocument();

    userEvent.type(modal.getByLabelText('Min'), '51');
    userEvent.type(modal.getByLabelText('Max'), '100');
    userEvent.click(modal.getByRole('button', { name: 'Add group' }));

    userEvent.click(modal.getByRole('button', { name: 'Done' }));

    const expectedValues: MapDataSetConfig[] = [
      {
        dataSet: {
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          indicator: 'authorised-absence-sessions',
          timePeriod: '2014_AY',
        },
        dataGrouping: {
          customGroups: [
            { min: 0, max: 50 },
            { min: 51, max: 100 },
          ],

          type: 'Custom',
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
          numberOfGroups: 5,
          type: 'EqualIntervals',
        },
      },
    ];

    await waitFor(() => {
      expect(handleChange).toHaveBeenCalledWith(expectedValues);
    });

    expect(within(rows[1]).getByText('Custom')).toBeInTheDocument();
  });

  test('copying custom groups from another data set', async () => {
    const handleChange = jest.fn();

    render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        data={testTable.results}
        map={{
          dataSetConfigs: [
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataGrouping: {
                customGroups: [
                  { min: 0, max: 50 },
                  { min: 51, max: 66 },
                ],
                type: 'Custom',
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
                numberOfGroups: 5,
                type: 'EqualIntervals',
              },
            },
          ],
        }}
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
        onChange={handleChange}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    userEvent.click(within(rows[2]).getByRole('button', { name: 'Edit' }));

    const modal = within(screen.getByRole('dialog'));

    userEvent.click(modal.getByLabelText('Copy custom groups'));

    userEvent.selectOptions(
      modal.getByLabelText('Copy custom groups from another data set'),
      generateDataSetKey({
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        indicator: 'authorised-absence-sessions',
        timePeriod: '2014_AY',
      }),
    );

    userEvent.click(modal.getByRole('button', { name: 'Done' }));

    const expectedValues: MapDataSetConfig[] = [
      {
        dataSet: {
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          indicator: 'authorised-absence-sessions',
          timePeriod: '2014_AY',
        },
        dataGrouping: {
          customGroups: [
            { min: 0, max: 50 },
            { min: 51, max: 66 },
          ],
          type: 'Custom',
        },
      },
      {
        dataSet: {
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          indicator: 'authorised-absence-sessions',
          timePeriod: '2015_AY',
        },
        dataGrouping: {
          customGroups: [
            { min: 0, max: 50 },
            { min: 51, max: 66 },
          ],
          type: 'Custom',
        },
      },
    ];

    await waitFor(() => {
      expect(handleChange).toHaveBeenCalledWith(expectedValues);
    });

    expect(within(rows[2]).getByText('Custom')).toBeInTheDocument();
  });

  test('shows a validation error if no number of groups when using equal intervals', async () => {
    const handleChange = jest.fn();

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
        onChange={handleChange}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    userEvent.click(within(rows[1]).getByRole('button', { name: 'Edit' }));

    const modal = within(screen.getByRole('dialog'));

    userEvent.clear(modal.getAllByLabelText('Number of data groups')[0]);

    userEvent.click(modal.getByRole('button', { name: 'Done' }));

    await waitFor(() => {
      expect(modal.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Enter a number of data groups' }),
    ).toHaveAttribute('href', '#chartDataGroupingForm-numberOfGroups');
  });

  test('shows a validation error if no number of groups when using quantiles', async () => {
    const handleChange = jest.fn();

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
        onChange={handleChange}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    userEvent.click(within(rows[1]).getByRole('button', { name: 'Edit' }));

    const modal = within(screen.getByRole('dialog'));

    userEvent.click(modal.getByLabelText('Quantiles'));

    userEvent.clear(modal.getAllByLabelText('Number of data groups')[1]);

    userEvent.click(modal.getByRole('button', { name: 'Done' }));

    await waitFor(() => {
      expect(modal.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Enter a number of data groups' }),
    ).toHaveAttribute('href', '#chartDataGroupingForm-numberOfGroupsQuantiles');
  });

  test('shows a validation error if no custom groups set when using custom grouping', async () => {
    const handleChange = jest.fn();

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
        onChange={handleChange}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    userEvent.click(within(rows[1]).getByRole('button', { name: 'Edit' }));

    const modal = within(screen.getByRole('dialog'));

    userEvent.click(modal.getByLabelText('New custom groups'));

    userEvent.click(modal.getByRole('button', { name: 'Done' }));

    await waitFor(() => {
      expect(modal.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Add one or more custom groups' }),
    ).toHaveAttribute('href', '#chartDataGroupingForm-customGroups');
  });

  test('shows a validation error when no data set selected to copy custom groups from', async () => {
    const handleChange = jest.fn();

    render(
      <ChartDataGroupingsConfiguration
        meta={testTable.subjectMeta}
        data={testTable.results}
        map={{
          dataSetConfigs: [
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataGrouping: {
                customGroups: [
                  { min: 0, max: 50 },
                  { min: 51, max: 66 },
                ],
                type: 'Custom',
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
                numberOfGroups: 5,
                type: 'EqualIntervals',
              },
            },
          ],
        }}
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
        onChange={handleChange}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    userEvent.click(within(rows[2]).getByRole('button', { name: 'Edit' }));

    const modal = within(screen.getByRole('dialog'));

    userEvent.click(modal.getByLabelText('Copy custom groups'));

    userEvent.click(modal.getByRole('button', { name: 'Done' }));

    await waitFor(() => {
      expect(modal.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', {
        name: 'Select a data set to copy custom groups from',
      }),
    ).toHaveAttribute('href', '#chartDataGroupingForm-copyCustomGroups');
  });

  test('submitting fails if another form is invalid', async () => {
    const handleSubmit = jest.fn();

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
        onSubmit={handleSubmit}
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

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      expect(screen.getByText('Cannot save chart')).toBeInTheDocument();
      expect(screen.getByText('Options tab is invalid')).toBeInTheDocument();

      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('successfully submits with updated values', async () => {
    const handleSubmit = jest.fn();
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
        onSubmit={handleSubmit}
        onChange={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    userEvent.click(within(rows[1]).getByRole('button', { name: 'Edit' }));

    const modal = within(screen.getByRole('dialog'));
    userEvent.click(modal.getByLabelText('Quantiles'));
    userEvent.clear(modal.getAllByLabelText('Number of data groups')[1]);
    userEvent.type(modal.getAllByLabelText('Number of data groups')[1], '3');

    userEvent.click(modal.getByRole('button', { name: 'Done' }));

    await waitFor(() => {
      expect(within(rows[1]).getByText('3 quantiles')).toBeInTheDocument();
    });

    expect(within(rows[1]).getByText('3 quantiles')).toBeInTheDocument();

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    const expectedValues: MapDataSetConfig[] = [
      {
        dataSet: {
          filters: ['ethnicity-major-chinese', 'state-funded-primary'],
          indicator: 'authorised-absence-sessions',
          timePeriod: '2014_AY',
        },
        dataGrouping: {
          customGroups: [],
          numberOfGroups: 3,
          type: 'Quantiles',
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
          numberOfGroups: 5,
          type: 'EqualIntervals',
        },
      },
    ];

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
    });
  });
});
