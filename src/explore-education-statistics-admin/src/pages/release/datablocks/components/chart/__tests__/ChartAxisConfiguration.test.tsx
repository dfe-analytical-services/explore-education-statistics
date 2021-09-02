import ChartAxisConfiguration from '@admin/pages/release/datablocks/components/chart/ChartAxisConfiguration';
import { testTableData } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { verticalBarBlockDefinition } from '@common/modules/charts/components/VerticalBarBlock';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { AxisConfiguration } from '@common/modules/charts/types/chart';
import userEvent from '@testing-library/user-event';
import React from 'react';
import noop from 'lodash/noop';
import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react';

describe('ChartAxisConfiguration', () => {
  const testFormState: ChartBuilderForms = {
    options: {
      isValid: true,
      submitCount: 0,
      id: 'options',
      title: 'Options',
    },
    major: {
      id: 'chartBuilder-major',
      isValid: true,
      submitCount: 0,
      title: 'X Axis (major axis)',
    },
  };

  const testAxisConfiguration: AxisConfiguration = {
    dataSets: [],
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

  const testAxisConfigurationWithReferenceLines = {
    ...testAxisConfiguration,
    referenceLines: [
      {
        label: 'I am label',
        position: '2014_AY',
      },
    ],
  };

  const testTable = mapFullTable(testTableData);

  test('renders correctly with initial values', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartAxisConfiguration
          id="chartBuilder-major"
          type="major"
          configuration={testAxisConfiguration}
          definition={verticalBarBlockDefinition}
          data={testTable.results}
          meta={testTable.subjectMeta}
          onChange={noop}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    const generalSection = within(
      screen.getByRole('group', { name: 'General' }),
    );
    expect(generalSection.getByLabelText('Size of axis (px)')).toHaveValue(50);
    expect(generalSection.getByLabelText('Show grid lines')).toBeChecked();
    expect(generalSection.getByLabelText('Show axis')).toBeChecked();
    expect(generalSection.getByLabelText('Displayed unit')).toHaveValue('');

    const groupBySection = within(
      screen.getByRole('group', { name: 'Group data by' }),
    );
    const groupByOptions = groupBySection.getAllByRole('radio');
    expect(groupByOptions).toHaveLength(4);
    expect(groupByOptions[0]).toHaveAttribute('value', 'filters');
    expect(groupByOptions[0]).toBeEnabled();
    expect(groupByOptions[0]).not.toBeChecked();
    expect(groupByOptions[0]).toEqual(groupBySection.getByLabelText('Filters'));
    expect(groupByOptions[1]).toHaveAttribute('value', 'indicators');
    expect(groupByOptions[1]).toBeEnabled();
    expect(groupByOptions[1]).not.toBeChecked();
    expect(groupByOptions[1]).toEqual(
      groupBySection.getByLabelText('Indicators'),
    );
    expect(groupByOptions[2]).toHaveAttribute('value', 'locations');
    expect(groupByOptions[2]).toBeEnabled();
    expect(groupByOptions[2]).not.toBeChecked();
    expect(groupByOptions[2]).toEqual(
      groupBySection.getByLabelText('Locations'),
    );
    expect(groupByOptions[3]).toHaveAttribute('value', 'timePeriod');
    expect(groupByOptions[3]).toBeEnabled();
    expect(groupByOptions[3]).toBeChecked();
    expect(groupByOptions[3]).toEqual(
      groupBySection.getByLabelText('Time periods'),
    );

    const labelsSection = within(screen.getByRole('group', { name: 'Labels' }));
    expect(labelsSection.getByLabelText('Label')).toHaveValue('');
    expect(labelsSection.getByLabelText('Width (px)')).toHaveValue(null);

    const sortingSection = within(
      screen.getByRole('group', { name: 'Sorting' }),
    );
    expect(sortingSection.getByLabelText('Sort ascending')).toBeChecked();

    const tickSection = within(
      screen.getByRole('group', { name: 'Tick display type' }),
    );
    const tickOptions = tickSection.getAllByRole('radio');
    expect(tickOptions).toHaveLength(3);
    expect(tickOptions[0]).toHaveAttribute('value', 'default');
    expect(tickOptions[0]).toBeEnabled();
    expect(tickOptions[0]).toBeChecked();
    expect(tickOptions[0]).toEqual(tickSection.getByLabelText('Automatic'));
    expect(tickOptions[1]).toHaveAttribute('value', 'startEnd');
    expect(tickOptions[1]).toBeEnabled();
    expect(tickOptions[1]).not.toBeChecked();
    expect(tickOptions[1]).toEqual(
      tickSection.getByLabelText('Start and end only'),
    );
    expect(tickOptions[2]).toHaveAttribute('value', 'custom');
    expect(tickOptions[2]).toBeEnabled();
    expect(tickOptions[2]).not.toBeChecked();
    expect(tickOptions[2]).toEqual(tickSection.getByLabelText('Custom'));

    const axisSection = within(
      screen.getByRole('group', { name: 'Axis range' }),
    );
    expect(axisSection.getByLabelText('Minimum')).toHaveValue('');
    expect(axisSection.getByLabelText('Maximum')).toHaveValue('');

    const referenceLinesSection = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    expect(referenceLinesSection.getAllByRole('row')).toHaveLength(2);
    expect(referenceLinesSection.getByLabelText('Position')).toHaveValue('');
    expect(referenceLinesSection.getByLabelText('Label')).toHaveValue('');
  });

  test('shows validation error if invalid custom tick spacing given', async () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartAxisConfiguration
          id="chartBuilder-major"
          type="major"
          configuration={testAxisConfiguration}
          definition={verticalBarBlockDefinition}
          data={testTable.results}
          meta={testTable.subjectMeta}
          onChange={noop}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    userEvent.click(screen.getByLabelText('Custom'));
    await userEvent.type(screen.getByLabelText('Every nth value'), 'x');
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByRole('link', { name: 'Enter tick spacing' }),
      ).toHaveAttribute('href', '#chartBuilder-major-tickSpacing');
    });

    await userEvent.type(screen.getByLabelText('Every nth value'), '-1');
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByRole('link', { name: 'Tick spacing must be positive' }),
      ).toHaveAttribute('href', '#chartBuilder-major-tickSpacing');
    });
  });

  test('shows validation error if invalid label width given', async () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartAxisConfiguration
          id="chartBuilder-major"
          type="major"
          configuration={testAxisConfiguration}
          definition={verticalBarBlockDefinition}
          data={testTable.results}
          meta={testTable.subjectMeta}
          onChange={noop}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await userEvent.type(screen.getByLabelText('Width (px)'), '-1');
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByRole('link', { name: 'Label width must be positive' }),
      ).toHaveAttribute('href', '#chartBuilder-major-labelWidth');
    });
  });

  test('shows validation error if invalid axis width given', async () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartAxisConfiguration
          id="chartBuilder-major"
          type="major"
          configuration={testAxisConfiguration}
          definition={verticalBarBlockDefinition}
          data={testTable.results}
          meta={testTable.subjectMeta}
          onChange={noop}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await userEvent.type(screen.getByLabelText('Size of axis (px)'), '-1');
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByRole('link', { name: 'Size of axis must be positive' }),
      ).toHaveAttribute('href', '#chartBuilder-major-size');
    });
  });

  test('submitting fails with invalid values', async () => {
    const handleSubmit = jest.fn();

    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartAxisConfiguration
          id="chartBuilder-major"
          type="major"
          configuration={testAxisConfiguration}
          definition={verticalBarBlockDefinition}
          data={testTable.results}
          meta={testTable.subjectMeta}
          onChange={noop}
          onSubmit={handleSubmit}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await userEvent.type(screen.getByLabelText('Width (px)'), '-1');

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('submitting succeeds with valid values', async () => {
    const handleSubmit = jest.fn();

    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartAxisConfiguration
          id="chartBuilder-major"
          type="major"
          configuration={testAxisConfiguration}
          definition={verticalBarBlockDefinition}
          data={testTable.results}
          meta={testTable.subjectMeta}
          onChange={noop}
          onSubmit={handleSubmit}
        />
      </ChartBuilderFormsContextProvider>,
    );

    userEvent.clear(screen.getByLabelText('Size of axis (px)'));
    await userEvent.type(screen.getByLabelText('Size of axis (px)'), '100');

    userEvent.click(screen.getByRole('checkbox', { name: 'Sort ascending' }));

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      const formValues: AxisConfiguration = {
        dataSets: [],
        groupBy: 'timePeriod',
        min: 0,
        max: undefined,
        referenceLines: [],
        showGrid: true,
        size: 100,
        sortAsc: false,
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
      expect(handleSubmit).toHaveBeenCalledWith(formValues);
    });
  });

  describe('Group data by', () => {
    test('submitting succeeds with the group data by option changed', async () => {
      const handleSubmit = jest.fn();

      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={testAxisConfiguration}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      userEvent.click(screen.getByRole('radio', { name: 'Locations' }));

      userEvent.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        const formValues: AxisConfiguration = {
          dataSets: [],
          groupBy: 'locations',
          min: 0,
          max: undefined,
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
        expect(handleSubmit).toHaveBeenCalledWith(formValues);
      });
    });

    test('submitting succeeds with group by a specific filter selected', async () => {
      const handleSubmit = jest.fn();

      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={testAxisConfiguration}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      userEvent.click(screen.getByRole('radio', { name: 'Filters' }));

      fireEvent.change(screen.getByLabelText('Select a filter'), {
        target: { value: 'school_type' },
      });

      userEvent.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        const formValues: AxisConfiguration = {
          dataSets: [],
          groupBy: 'filters',
          groupByFilter: 'school_type',
          min: 0,
          max: undefined,
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
        expect(handleSubmit).toHaveBeenCalledWith(formValues);
      });
    });
  });

  describe('Reference lines', () => {
    test('adding reference lines', async () => {
      const handleSubmit = jest.fn();

      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={testAxisConfiguration}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLinesSection = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );
      expect(referenceLinesSection.getAllByRole('row')).toHaveLength(2);

      fireEvent.change(referenceLinesSection.getByLabelText('Position'), {
        target: { value: '2014_AY' },
      });

      await userEvent.type(
        referenceLinesSection.getByLabelText('Label'),
        'I am label',
      );

      userEvent.click(screen.getByRole('button', { name: 'Add line' }));

      await waitFor(() => {
        const rows = referenceLinesSection.getAllByRole('row');
        expect(rows).toHaveLength(3);
        expect(rows[1]).toHaveTextContent('2014_AY');
        expect(rows[1]).toHaveTextContent('I am label');
        expect(within(rows[1]).getByRole('button', { name: 'Remove' }));
      });
    });

    test('successfully submitting with reference lines', async () => {
      const handleSubmit = jest.fn();

      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={testAxisConfiguration}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLinesSection = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );
      expect(referenceLinesSection.getAllByRole('row')).toHaveLength(2);

      fireEvent.change(referenceLinesSection.getByLabelText('Position'), {
        target: { value: '2014_AY' },
      });

      await userEvent.type(
        referenceLinesSection.getByLabelText('Label'),
        'I am label',
      );

      userEvent.click(screen.getByRole('button', { name: 'Add line' }));

      userEvent.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        const formValues: AxisConfiguration = {
          dataSets: [],
          groupBy: 'timePeriod',
          min: 0,
          max: undefined,
          referenceLines: [
            {
              label: 'I am label',
              position: '2014_AY',
            },
          ],
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
        expect(handleSubmit).toHaveBeenCalledWith(formValues);
      });
    });

    test('removing reference lines', async () => {
      const handleSubmit = jest.fn();

      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={testAxisConfigurationWithReferenceLines}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLinesSection = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );
      expect(referenceLinesSection.getAllByRole('row')).toHaveLength(3);

      userEvent.click(
        referenceLinesSection.getByRole('button', { name: 'Remove' }),
      );

      await waitFor(() => {
        const rows = referenceLinesSection.getAllByRole('row');
        expect(rows).toHaveLength(2);
      });
    });

    test('successfully submit with reference lines removed', async () => {
      const handleSubmit = jest.fn();

      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={testAxisConfigurationWithReferenceLines}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLinesSection = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );
      expect(referenceLinesSection.getAllByRole('row')).toHaveLength(3);

      userEvent.click(
        referenceLinesSection.getByRole('button', { name: 'Remove' }),
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        const formValues: AxisConfiguration = {
          dataSets: [],
          groupBy: 'timePeriod',
          min: 0,
          max: undefined,
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
        expect(handleSubmit).toHaveBeenCalledWith(formValues);
      });
    });
  });
});
