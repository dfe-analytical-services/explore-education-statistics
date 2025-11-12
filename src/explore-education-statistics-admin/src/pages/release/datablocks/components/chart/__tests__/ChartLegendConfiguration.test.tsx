import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartLegendConfiguration from '@admin/pages/release/datablocks/components/chart/ChartLegendConfiguration';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import render from '@common-test/render';
import { lineChartBlockDefinition } from '@common/modules/charts/components/LineChartBlock';
import { mapBlockDefinition } from '@common/modules/charts/components/MapBlock';
import {
  DataSet,
  DataSetConfiguration,
} from '@common/modules/charts/types/dataSet';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import { fireEvent, screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import {
  testCategoricalMeta,
  testCategoricalData,
  testMapDataSetConfigs,
} from './__data__/testCategoricalData';

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
          onReorderCategories={noop}
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
          onReorderCategories={noop}
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
          onReorderCategories={noop}
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
          onReorderCategories={noop}
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
          onReorderCategories={noop}
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
          onReorderCategories={noop}
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
          onReorderCategories={noop}
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
          onReorderCategories={noop}
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
          onReorderCategories={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(1);

    const legendItem1 = within(legendItems[0]);
    expect(legendItem1.queryByLabelText('Style')).not.toBeInTheDocument();
  });

  test('renders the item position, label colour and y offset fields when the legend position is `inline`', () => {
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
          onReorderCategories={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(screen.getByLabelText('Legend position')).toHaveValue('inline');

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(1);

    const legendItem1 = within(legendItems[0]);

    expect(legendItem1.getByLabelText('Label')).toHaveValue('Legend item 1');
    expect(legendItem1.getByLabelText('Colour')).toHaveValue('#ff0000');
    expect(legendItem1.getByLabelText('Label Colour')).toHaveValue('black');
    expect(legendItem1.getByLabelText('Symbol')).toHaveValue('none');
    expect(legendItem1.getByLabelText('Style')).toHaveValue('solid');
    expect(legendItem1.getByLabelText('Position')).toHaveValue('right');
    expect(legendItem1.queryByLabelText('Y Offset')).toBeInTheDocument();
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

    const { user } = render(
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
          onReorderCategories={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await user.selectOptions(
      screen.getByLabelText('Legend position'),
      'inline',
    );

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(1);

    const legendItem1 = within(legendItems[0]);

    await user.clear(legendItem1.getByLabelText('Label'));
    await user.type(
      legendItem1.getByLabelText('Label'),
      'Updated legend item 1',
    );

    await user.clear(legendItem1.getByLabelText('Y Offset'));
    await user.type(legendItem1.getByLabelText('Y Offset'), '20');

    await user.selectOptions(legendItem1.getByLabelText('Position'), 'below');

    expect(handleChange).toHaveBeenCalledWith<[LegendConfiguration]>({
      position: 'inline',
      items: [
        {
          dataSet,
          label: 'Updated legend item 1',
          colour: '#12436D',
          labelColour: 'black',
          lineStyle: 'solid',
          symbol: 'none',
          inlinePosition: 'below',
          inlinePositionOffset: 20,
        },
      ],
    });
  });

  test('shows validation errors if missing legend item label', async () => {
    const { user } = render(
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
          onReorderCategories={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(2);

    const legendItem1 = within(legendItems[0]);
    const legendItem2 = within(legendItems[1]);

    await user.clear(legendItem2.getByLabelText('Label'));
    await user.tab();

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
        '#chartLegendConfigurationForm-items-1-label',
      );
    });
  });

  test('shows validation error if position is inline and showDataLabels is true', async () => {
    const { user } = render(
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
          onReorderCategories={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await user.selectOptions(
      screen.getByLabelText('Legend position'),
      'inline',
    );
    await user.tab();

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

    const { user } = render(
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
          onReorderCategories={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(1);

    const legendItem1 = within(legendItems[0]);

    await user.clear(legendItem1.getByLabelText('Label'));

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('submitting fails if another form is invalid', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
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
          onReorderCategories={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    expect(await screen.findByText('Cannot save chart')).toBeInTheDocument();
    expect(screen.getByText('Options tab is invalid')).toBeInTheDocument();
  });

  test('successfully submits with updated values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
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
          onReorderCategories={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await user.selectOptions(
      screen.getByLabelText('Legend position'),
      'inline',
    );

    const legendItems = screen.getAllByRole('group');
    expect(legendItems).toHaveLength(1);

    const legendItem1 = within(legendItems[0]);

    await user.clear(legendItem1.getByLabelText('Label'));
    await user.type(
      legendItem1.getByLabelText('Label'),
      'Updated legend item 1',
    );

    fireEvent.change(legendItem1.getByLabelText('Colour'), {
      target: {
        value: '#d53880',
      },
    });

    await user.selectOptions(legendItem1.getByLabelText('Symbol'), 'diamond');
    await user.selectOptions(legendItem1.getByLabelText('Style'), 'dotted');
    await user.selectOptions(legendItem1.getByLabelText('Position'), 'below');

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

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
            labelColour: 'black',
            lineStyle: 'dotted',
            symbol: 'diamond',
            inlinePosition: 'below',
            inlinePositionOffset: undefined,
          },
        ],
      };

      expect(handleSubmit).toHaveBeenCalledWith(values);
    });
  });

  describe('reorder categories in map key', () => {
    test('shows the reorder button for data sets with categorical data in maps', () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartLegendConfiguration
            definition={mapBlockDefinition}
            meta={testCategoricalMeta}
            data={testCategoricalData}
            axisMajor={{
              dataSets: [
                {
                  order: 0,
                  indicator: 'indicator-1',
                  filters: [],
                  timePeriod: '2024_CY',
                },
                {
                  order: 1,
                  indicator: 'indicator-2',
                  filters: [],
                  timePeriod: '2024_CY',
                },
              ],
              groupBy: 'locations',
              referenceLines: [],
              type: 'major',
            }}
            legend={{
              position: 'bottom',
              items: [],
            }}
            mapDataSetConfigs={testMapDataSetConfigs}
            onSubmit={noop}
            onChange={noop}
            onReorderCategories={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const legendItems = screen.getAllByRole('group');
      expect(legendItems).toHaveLength(2);
      expect(
        within(legendItems[0]).getByRole('button', {
          name: /Reorder categories/,
        }),
      ).toBeInTheDocument();
      expect(
        within(legendItems[1]).getByRole('button', {
          name: /Reorder categories/,
        }),
      ).toBeInTheDocument();
    });

    test('does not show the reorder button for data sets with numerical data in maps', () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartLegendConfiguration
            definition={mapBlockDefinition}
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
              groupBy: 'locations',
              referenceLines: [],
              type: 'major',
              visible: true,
            }}
            legend={{
              position: 'bottom',
              items: [],
            }}
            mapDataSetConfigs={testMapDataSetConfigs}
            onSubmit={noop}
            onChange={noop}
            onReorderCategories={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const legendItems = screen.getAllByRole('group');
      expect(legendItems).toHaveLength(2);
      expect(
        within(legendItems[0]).queryByRole('button', {
          name: /Reorder categories/,
        }),
      ).not.toBeInTheDocument();
      expect(
        within(legendItems[1]).queryByRole('button', {
          name: /Reorder categories/,
        }),
      ).not.toBeInTheDocument();
    });

    test('does not show the reorder button for charts', () => {
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
            onSubmit={noop}
            onChange={noop}
            onReorderCategories={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const legendItems = screen.getAllByRole('group');
      expect(legendItems).toHaveLength(1);
      expect(
        within(legendItems[0]).queryByRole('button', {
          name: /Reorder categories/,
        }),
      ).not.toBeInTheDocument();
    });

    test('shows the categories for the data set when the modal is opened', async () => {
      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartLegendConfiguration
            definition={mapBlockDefinition}
            meta={testCategoricalMeta}
            data={testCategoricalData}
            axisMajor={{
              dataSets: [
                {
                  order: 0,
                  indicator: 'indicator-1',
                  filters: [],
                  timePeriod: '2024_CY',
                },
                {
                  order: 1,
                  indicator: 'indicator-2',
                  filters: [],
                  timePeriod: '2024_CY',
                },
              ],
              groupBy: 'locations',
              referenceLines: [],
              type: 'major',
            }}
            legend={{
              position: 'bottom',
              items: [],
            }}
            mapDataSetConfigs={testMapDataSetConfigs}
            onSubmit={noop}
            onChange={noop}
            onReorderCategories={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const legendItems = screen.getAllByRole('group');

      await user.click(
        within(legendItems[0]).getByRole('button', {
          name: /Reorder categories/,
        }),
      );

      const modal = within(screen.getByRole('dialog'));
      const listItems = modal.getAllByRole('listitem');
      expect(listItems).toHaveLength(2);
      expect(listItems[0]).toHaveTextContent('high');
      expect(listItems[1]).toHaveTextContent('low');

      await user.click(screen.getByRole('button', { name: 'Cancel' }));

      await user.click(
        within(legendItems[1]).getByRole('button', {
          name: /Reorder categories/,
        }),
      );

      const modal2 = within(screen.getByRole('dialog'));
      const listItems2 = modal2.getAllByRole('listitem');
      expect(listItems2).toHaveLength(3);
      expect(listItems2[0]).toHaveTextContent('large');
      expect(listItems2[1]).toHaveTextContent('medium');
      expect(listItems2[2]).toHaveTextContent('small');
    });

    test('calls onReorderCategories with the correct order when confirm', async () => {
      const handleReorder = jest.fn();
      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartLegendConfiguration
            definition={mapBlockDefinition}
            meta={testCategoricalMeta}
            data={testCategoricalData}
            axisMajor={{
              dataSets: [
                {
                  order: 0,
                  indicator: 'indicator-1',
                  filters: [],
                  timePeriod: '2024_CY',
                },
                {
                  order: 1,
                  indicator: 'indicator-2',
                  filters: [],
                  timePeriod: '2024_CY',
                },
              ],
              groupBy: 'locations',
              referenceLines: [],
              type: 'major',
            }}
            legend={{
              position: 'bottom',
              items: [],
            }}
            mapDataSetConfigs={testMapDataSetConfigs}
            onSubmit={noop}
            onChange={noop}
            onReorderCategories={handleReorder}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const legendItems = screen.getAllByRole('group');

      await user.click(
        within(legendItems[0]).getByRole('button', {
          name: /Reorder categories/,
        }),
      );

      const modal = within(screen.getByRole('dialog'));
      const listItems = modal.getAllByRole('listitem');
      expect(listItems).toHaveLength(2);

      await user.click(modal.getByRole('button', { name: 'Move low up' }));

      expect(handleReorder).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Confirm' }));

      expect(handleReorder).toHaveBeenCalledTimes(1);
      expect(handleReorder).toHaveBeenCalledWith({
        ...testMapDataSetConfigs[0],
        categoricalDataConfig: [
          {
            value: 'low',
            colour: '#12436D',
          },
          {
            value: 'high',
            colour: '#28A197',
          },
        ],
      });
    });
  });
});
