import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartLegendConfiguration from '@admin/pages/release/datablocks/components/chart/ChartLegendConfiguration';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { lineChartBlockDefinition } from '@common/modules/charts/components/LineChartBlock';
import {
  DataSet,
  DataSetConfiguration,
} from '@common/modules/charts/types/dataSet';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('ChartLegendConfiguration', () => {
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
  };

  test('renders correctly with no legend items', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={lineChartBlockDefinition}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'top',
            items: [],
          }}
          onSubmit={noop}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(
      screen.getByText('No legend items to customize.'),
    ).toBeInTheDocument();
  });

  test('renders correctly with single legend item from new data set', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={lineChartBlockDefinition}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [
              {
                location: {
                  value: 'barnet',
                  level: 'localAuthority',
                },
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
              },
            ],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'top',
            items: [],
          }}
          onSubmit={noop}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(screen.getByLabelText('Legend position')).toHaveValue('top');

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(1);

    const legendItem1 = within(legendItems[0]);

    expect(legendItem1.getByLabelText('Label')).toHaveValue(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, Barnet)',
    );
    expect(legendItem1.getByLabelText('Colour')).toHaveValue('#12436D');
    expect(legendItem1.getByLabelText('Symbol')).toHaveValue('none');
    expect(legendItem1.getByLabelText('Style')).toHaveValue('solid');
  });

  test('renders correctly with multiple legend items from new data set', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={lineChartBlockDefinition}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [
              {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
              },
            ],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'top',
            items: [],
          }}
          onSubmit={noop}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(screen.getByLabelText('Legend position')).toHaveValue('top');

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(2);

    const legendItem1 = within(legendItems[0]);

    expect(legendItem1.getByLabelText('Label')).toHaveValue(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, Barnet)',
    );
    expect(legendItem1.getByLabelText('Colour')).toHaveValue('#12436D');
    expect(legendItem1.getByLabelText('Symbol')).toHaveValue('none');
    expect(legendItem1.getByLabelText('Style')).toHaveValue('solid');

    const legendItem2 = within(legendItems[1]);

    expect(legendItem2.getByLabelText('Label')).toHaveValue(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, Barnsley)',
    );
    expect(legendItem2.getByLabelText('Colour')).toHaveValue('#F46A25');
    expect(legendItem2.getByLabelText('Symbol')).toHaveValue('none');
    expect(legendItem2.getByLabelText('Style')).toHaveValue('solid');
  });

  test('renders correctly with single legend item from deprecated data sets', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={lineChartBlockDefinition}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [
              {
                location: {
                  value: 'barnet',
                  level: 'localAuthority',
                },
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                config: {
                  label: 'Legend item 1',
                  colour: '#ff0000',
                  lineStyle: 'dashed',
                  symbol: 'diamond',
                },
              },
            ] as DataSetConfiguration[],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'top',
            items: [],
          }}
          onSubmit={noop}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(screen.getByLabelText('Legend position')).toHaveValue('top');

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(1);

    const legendItem1 = within(legendItems[0]);

    expect(legendItem1.getByLabelText('Label')).toHaveValue('Legend item 1');
    expect(legendItem1.getByLabelText('Colour')).toHaveValue('#ff0000');
    expect(legendItem1.getByLabelText('Symbol')).toHaveValue('diamond');
    expect(legendItem1.getByLabelText('Style')).toHaveValue('dashed');
  });

  test('renders correctly with multiple legend items from deprecated data sets', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={lineChartBlockDefinition}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [
              {
                location: {
                  value: 'barnsley',
                  level: 'localAuthority',
                },
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                config: {
                  label: 'Legend item 1',
                  colour: '#00ff00',
                  lineStyle: 'dotted',
                  symbol: 'square',
                },
              },
              {
                location: {
                  value: 'barnet',
                  level: 'localAuthority',
                },
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                config: {
                  label: 'Legend item 2',
                  colour: '#ff0000',
                  lineStyle: 'dashed',
                  symbol: 'diamond',
                },
              },
            ] as DataSetConfiguration[],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'top',
            items: [],
          }}
          onSubmit={noop}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(screen.getByLabelText('Legend position')).toHaveValue('top');

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(2);

    const legendItem1 = within(legendItems[0]);

    expect(legendItem1.getByLabelText('Label')).toHaveValue('Legend item 1');
    expect(legendItem1.getByLabelText('Colour')).toHaveValue('#00ff00');
    expect(legendItem1.getByLabelText('Symbol')).toHaveValue('square');
    expect(legendItem1.getByLabelText('Style')).toHaveValue('dotted');

    const legendItem2 = within(legendItems[1]);

    expect(legendItem2.getByLabelText('Label')).toHaveValue('Legend item 2');
    expect(legendItem2.getByLabelText('Colour')).toHaveValue('#ff0000');
    expect(legendItem2.getByLabelText('Symbol')).toHaveValue('diamond');
    expect(legendItem2.getByLabelText('Style')).toHaveValue('dashed');
  });

  test('renders correctly with single existing legend item', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={lineChartBlockDefinition}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [
              {
                location: {
                  value: 'barnet',
                  level: 'localAuthority',
                },
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
              },
            ],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'top',
            items: [
              {
                dataSet: {
                  location: {
                    value: 'barnet',
                    level: 'localAuthority',
                  },
                  filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                  indicator: 'authorised-absence-sessions',
                },
                label: 'Legend item 1',
                colour: '#00ff00',
                lineStyle: 'dashed',
                symbol: 'star',
              },
            ],
          }}
          onSubmit={noop}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(screen.getByLabelText('Legend position')).toHaveValue('top');

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(1);

    const legendItem1 = within(legendItems[0]);

    expect(legendItem1.getByLabelText('Label')).toHaveValue('Legend item 1');
    expect(legendItem1.getByLabelText('Colour')).toHaveValue('#00ff00');
    expect(legendItem1.getByLabelText('Symbol')).toHaveValue('star');
    expect(legendItem1.getByLabelText('Style')).toHaveValue('dashed');
  });

  test('renders correctly with multiple existing legend items', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={lineChartBlockDefinition}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [
              {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
              },
            ],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'top',
            items: [
              {
                dataSet: {
                  location: {
                    value: 'barnsley',
                    level: 'localAuthority',
                  },
                  filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                  indicator: 'authorised-absence-sessions',
                },
                label: 'Legend item 1',
                colour: '#ff0000',
                lineStyle: 'dotted',
                symbol: 'square',
              },
              {
                dataSet: {
                  location: {
                    value: 'barnet',
                    level: 'localAuthority',
                  },
                  filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                  indicator: 'authorised-absence-sessions',
                },
                label: 'Legend item 2',
                colour: '#00ff00',
                lineStyle: 'dashed',
                symbol: 'star',
              },
            ],
          }}
          onSubmit={noop}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(screen.getByLabelText('Legend position')).toHaveValue('top');

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(2);

    const legendItem1 = within(legendItems[0]);

    expect(legendItem1.getByLabelText('Label')).toHaveValue('Legend item 2');
    expect(legendItem1.getByLabelText('Colour')).toHaveValue('#00ff00');
    expect(legendItem1.getByLabelText('Symbol')).toHaveValue('star');
    expect(legendItem1.getByLabelText('Style')).toHaveValue('dashed');

    const legendItem2 = within(legendItems[1]);

    expect(legendItem2.getByLabelText('Label')).toHaveValue('Legend item 1');
    expect(legendItem2.getByLabelText('Colour')).toHaveValue('#ff0000');
    expect(legendItem2.getByLabelText('Symbol')).toHaveValue('square');
    expect(legendItem2.getByLabelText('Style')).toHaveValue('dotted');
  });

  test('does not render Symbol field if chart does not have capability', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={{
            ...lineChartBlockDefinition,
            capabilities: {
              ...lineChartBlockDefinition.capabilities,
              hasSymbols: false,
            },
          }}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [
              {
                location: {
                  value: 'barnet',
                  level: 'localAuthority',
                },
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
              },
            ],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'top',
            items: [
              {
                dataSet: {
                  location: {
                    value: 'barnet',
                    level: 'localAuthority',
                  },
                  filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                  indicator: 'authorised-absence-sessions',
                },
                label: 'Legend item 1',
                colour: '#ff0000',
              },
            ],
          }}
          onSubmit={noop}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(1);

    const legendItem1 = within(legendItems[0]);
    expect(legendItem1.queryByLabelText('Symbol')).not.toBeInTheDocument();
  });

  test('does not render Style field if chart does not have capability', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={{
            ...lineChartBlockDefinition,
            capabilities: {
              ...lineChartBlockDefinition.capabilities,
              hasLineStyle: false,
            },
          }}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [
              {
                location: {
                  value: 'barnet',
                  level: 'localAuthority',
                },
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
              },
            ],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'top',
            items: [
              {
                dataSet: {
                  location: {
                    value: 'barnet',
                    level: 'localAuthority',
                  },
                  filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                  indicator: 'authorised-absence-sessions',
                },
                label: 'Legend item 1',
                colour: '#ff0000',
              },
            ],
          }}
          onSubmit={noop}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(1);

    const legendItem1 = within(legendItems[0]);
    expect(legendItem1.queryByLabelText('Style')).not.toBeInTheDocument();
  });

  test('renders the item position field when the legend position is `inline`', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={lineChartBlockDefinition}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [
              {
                location: {
                  value: 'barnet',
                  level: 'localAuthority',
                },
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
              },
            ],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'inline',
            items: [
              {
                dataSet: {
                  location: {
                    value: 'barnet',
                    level: 'localAuthority',
                  },
                  filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                  indicator: 'authorised-absence-sessions',
                },
                label: 'Legend item 1',
                colour: '#ff0000',
              },
            ],
          }}
          onSubmit={noop}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(screen.getByLabelText('Legend position')).toHaveValue('inline');

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(1);

    const legendItem1 = within(legendItems[0]);

    expect(legendItem1.getByLabelText('Label')).toHaveValue('Legend item 1');
    expect(legendItem1.getByLabelText('Colour')).toHaveValue('#ff0000');
    expect(legendItem1.getByLabelText('Symbol')).toHaveValue('none');
    expect(legendItem1.getByLabelText('Style')).toHaveValue('solid');
    expect(legendItem1.getByLabelText('Position')).toHaveValue('above');
  });

  test('calls `onChange` handler if form values change', async () => {
    const handleChange = jest.fn();

    const dataSet: DataSet = {
      location: {
        value: 'barnet',
        level: 'localAuthority',
      },
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      indicator: 'authorised-absence-sessions',
    };

    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={lineChartBlockDefinition}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [dataSet],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'top',
            items: [],
          }}
          onSubmit={noop}
          onChange={handleChange}
        />
      </ChartBuilderFormsContextProvider>,
    );

    userEvent.selectOptions(screen.getByLabelText('Legend position'), 'inline');

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(1);

    const legendItem1 = within(legendItems[0]);

    userEvent.clear(legendItem1.getByLabelText('Label'));
    userEvent.type(
      legendItem1.getByLabelText('Label'),
      'Updated legend item 1',
    );

    userEvent.selectOptions(legendItem1.getByLabelText('Position'), 'below');

    expect(handleChange).toHaveBeenCalledWith<[LegendConfiguration]>({
      position: 'inline',
      items: [
        {
          dataSet,
          label: 'Updated legend item 1',
          colour: '#12436D',
          lineStyle: 'solid',
          symbol: 'none',
          inlinePosition: 'below',
        },
      ],
    });
  });

  test('shows validation errors if missing legend item label', async () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={lineChartBlockDefinition}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [
              {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
              },
            ],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'top',
            items: [],
          }}
          onSubmit={noop}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(2);

    const legendItem1 = within(legendItems[0]);
    const legendItem2 = within(legendItems[1]);

    userEvent.clear(legendItem2.getByLabelText('Label'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        legendItem1.queryByText(/Enter label for legend item/),
      ).not.toBeInTheDocument();
      expect(
        legendItem2.getByText('Enter label for legend item 2'),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('link', { name: 'Enter label for legend item 2' }),
      ).toHaveAttribute(
        'href',
        // Item ids are zero indexed
        '#chartLegendConfigurationForm-items1Label',
      );
    });
  });

  test('shows validation error if position is inline and showDataLabels is true', async () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={lineChartBlockDefinition}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [
              {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
              },
            ],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'top',
            items: [],
          }}
          showDataLabels
          onSubmit={noop}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    userEvent.selectOptions(screen.getByLabelText('Legend position'), 'inline');
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByRole('link', {
          name: 'Inline legends cannot be used with data labels',
        }),
      ).toHaveAttribute('href', '#chartLegendConfigurationForm-position');
    });
  });

  test('submitting fails with invalid values', async () => {
    const handleSubmit = jest.fn();

    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={lineChartBlockDefinition}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [
              {
                location: {
                  level: 'localAuthority',
                  value: 'barnet',
                },
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
              },
            ],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'top',
            items: [],
          }}
          onSubmit={handleSubmit}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(1);

    const legendItem1 = within(legendItems[0]);

    userEvent.clear(legendItem1.getByLabelText('Label'));

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('submitting fails if another form is invalid', async () => {
    const handleSubmit = jest.fn();

    render(
      <ChartBuilderFormsContextProvider
        initialForms={{
          ...testFormState,
          options: {
            ...testFormState.options,
            isValid: false,
          },
        }}
      >
        <ChartLegendConfiguration
          definition={lineChartBlockDefinition}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [
              {
                location: {
                  level: 'localAuthority',
                  value: 'barnet',
                },
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
              },
            ],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'top',
            items: [],
          }}
          onSubmit={handleSubmit}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
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
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartLegendConfiguration
          definition={lineChartBlockDefinition}
          meta={testTable.subjectMeta}
          data={testTable.results}
          axisMajor={{
            dataSets: [
              {
                location: {
                  level: 'localAuthority',
                  value: 'barnet',
                },
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
              },
            ],
            groupBy: 'timePeriod',
            referenceLines: [],
            type: 'major',
            visible: true,
          }}
          legend={{
            position: 'bottom',
            items: [],
          }}
          onSubmit={handleSubmit}
          onChange={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    userEvent.selectOptions(screen.getByLabelText('Legend position'), 'inline');

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(1);

    const legendItem1 = within(legendItems[0]);

    userEvent.clear(legendItem1.getByLabelText('Label'));
    userEvent.type(
      legendItem1.getByLabelText('Label'),
      'Updated legend item 1',
    );

    fireEvent.change(legendItem1.getByLabelText('Colour'), {
      target: {
        value: '#d53880',
      },
    });

    userEvent.selectOptions(legendItem1.getByLabelText('Symbol'), 'diamond');
    userEvent.selectOptions(legendItem1.getByLabelText('Style'), 'dotted');
    userEvent.selectOptions(legendItem1.getByLabelText('Position'), 'below');

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      const values: LegendConfiguration = {
        position: 'inline',
        items: [
          {
            dataSet: {
              location: {
                level: 'localAuthority',
                value: 'barnet',
              },
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
            },
            label: 'Updated legend item 1',
            colour: '#d53880',
            lineStyle: 'dotted',
            symbol: 'diamond',
            inlinePosition: 'below',
          },
        ],
      };

      expect(handleSubmit).toHaveBeenCalledWith(values);
    });
  });
});
