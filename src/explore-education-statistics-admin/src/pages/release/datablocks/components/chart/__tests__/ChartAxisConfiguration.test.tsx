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
import { render, screen, waitFor, within } from '@testing-library/react';

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
    minor: {
      id: 'chartBuilder-minor',
      isValid: true,
      submitCount: 0,
      title: 'Y Axis (minor axis)',
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
    expect(generalSection.getByLabelText('Size of axis (pixels)')).toHaveValue(
      50,
    );
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
    expect(labelsSection.getByLabelText('Width (pixels)')).toHaveValue(null);

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
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Enter tick spacing' }),
    ).toHaveAttribute('href', '#chartBuilder-major-tickSpacing');

    await userEvent.type(screen.getByLabelText('Every nth value'), '-1');
    userEvent.tab();

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Tick spacing must be positive' }),
    ).toHaveAttribute('href', '#chartBuilder-major-tickSpacing');
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

    await userEvent.type(screen.getByLabelText('Width (pixels)'), '-1');
    userEvent.tab();

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Label width must be positive' }),
    ).toHaveAttribute('href', '#chartBuilder-major-labelWidth');
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

    await userEvent.type(screen.getByLabelText('Size of axis (pixels)'), '-1');
    userEvent.tab();

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Size of axis must be positive' }),
    ).toHaveAttribute('href', '#chartBuilder-major-size');
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

    await userEvent.type(screen.getByLabelText('Width (pixels)'), '-1');

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

    userEvent.clear(screen.getByLabelText('Size of axis (pixels)'));
    await userEvent.type(screen.getByLabelText('Size of axis (pixels)'), '100');

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

  describe('group data by', () => {
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
          groupByFilter: '',
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

      userEvent.selectOptions(screen.getByLabelText('Select a filter'), [
        'school_type',
      ]);

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

  describe('reference lines', () => {
    test('renders correctly with existing lines when grouped by time periods', () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={{
              ...testAxisConfiguration,
              referenceLines: [{ position: '2014_AY', label: 'Test label 1' }],
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );

      const rows = referenceLines.getAllByRole('row');
      expect(rows).toHaveLength(3);
      expect(within(rows[1]).getByText('2014/15')).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label 1')).toBeInTheDocument();

      const position = within(rows[2]).getByLabelText('Position');
      const options = within(position).getAllByRole('option');

      expect(options).toHaveLength(2);
      expect(options[0]).toHaveTextContent('Choose position');
      expect(options[0]).toHaveAttribute('value', '');
      expect(options[1]).toHaveTextContent('2015/16');
      expect(options[1]).toHaveAttribute('value', '2015_AY');
    });

    test('renders correctly with existing lines when grouped by filters', () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={{
              ...testAxisConfiguration,
              groupBy: 'filters',
              referenceLines: [
                { position: 'ethnicity-major-chinese', label: 'Test label 1' },
                { position: 'state-funded-secondary', label: 'Test label 2' },
              ],
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );

      const rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(4);
      expect(
        within(rows[1]).getByText('Ethnicity Major Chinese'),
      ).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label 1')).toBeInTheDocument();
      expect(
        within(rows[2]).getByText('State-funded secondary'),
      ).toBeInTheDocument();
      expect(within(rows[2]).getByText('Test label 2')).toBeInTheDocument();

      const position = within(rows[3]).getByLabelText('Position');
      const options = within(position).getAllByRole('option');

      expect(options).toHaveLength(3);
      expect(options[0]).toHaveTextContent('Choose position');
      expect(options[0]).toHaveAttribute('value', '');
      expect(options[1]).toHaveTextContent('Ethnicity Major Black Total');
      expect(options[1]).toHaveAttribute(
        'value',
        'ethnicity-major-black-total',
      );
      expect(options[2]).toHaveTextContent('State-funded primary');
      expect(options[2]).toHaveAttribute('value', 'state-funded-primary');
    });

    test('renders correctly with existing lines when grouped by locations', () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={{
              ...testAxisConfiguration,
              groupBy: 'locations',
              referenceLines: [{ position: 'barnet', label: 'Test label 1' }],
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );

      const rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(3);
      expect(within(rows[1]).getByText('Barnet')).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label 1')).toBeInTheDocument();

      const position = within(rows[2]).getByLabelText('Position');
      const options = within(position).getAllByRole('option');

      expect(options).toHaveLength(2);
      expect(options[0]).toHaveTextContent('Choose position');
      expect(options[0]).toHaveAttribute('value', '');
      expect(options[1]).toHaveTextContent('Barnsley');
      expect(options[1]).toHaveAttribute('value', 'barnsley');
    });

    test('renders correctly with existing lines when grouped by indicators', () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={{
              ...testAxisConfiguration,
              groupBy: 'indicators',
              referenceLines: [
                { position: 'overall-absence-sessions', label: 'Test label 1' },
              ],
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );

      const rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(3);
      expect(
        within(rows[1]).getByText('Number of overall absence sessions'),
      ).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label 1')).toBeInTheDocument();

      const position = within(rows[2]).getByLabelText('Position');
      const options = within(position).getAllByRole('option');

      expect(options).toHaveLength(2);
      expect(options[0]).toHaveTextContent('Choose position');
      expect(options[0]).toHaveAttribute('value', '');
      expect(options[1]).toHaveTextContent(
        'Number of authorised absence sessions',
      );
      expect(options[1]).toHaveAttribute(
        'value',
        'authorised-absence-sessions',
      );
    });

    test('renders correctly with existing lines for minor axis', () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-minor"
            type="minor"
            configuration={{
              ...testAxisConfiguration,
              type: 'minor',
              groupBy: undefined,
              referenceLines: [
                { position: 2000, label: 'Test label 1' },
                { position: 4000, label: 'Test label 2' },
              ],
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );

      const rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(4);
      expect(within(rows[1]).getByText('2000')).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label 1')).toBeInTheDocument();
      expect(within(rows[2]).getByText('4000')).toBeInTheDocument();
      expect(within(rows[2]).getByText('Test label 2')).toBeInTheDocument();
    });

    test('adding reference lines when grouped by time periods', async () => {
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

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );
      expect(referenceLines.getAllByRole('row')).toHaveLength(2);

      userEvent.selectOptions(referenceLines.getByLabelText('Position'), [
        '2014_AY',
      ]);

      await userEvent.type(
        referenceLines.getByLabelText('Label'),
        'Test label',
      );

      userEvent.click(screen.getByRole('button', { name: 'Add line' }));

      await waitFor(() => {
        expect(referenceLines.getByText('Test label')).toBeInTheDocument();
      });

      const rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(3);
      expect(within(rows[1]).getByText('2014/15')).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label')).toBeInTheDocument();
      expect(within(rows[1]).getByRole('button', { name: /Remove line/ }));

      // Added reference line should be removed from Position options
      const position = within(rows[2]).getByLabelText('Position');
      const options = within(position).getAllByRole('option');

      expect(options).toHaveLength(2);
      expect(options[0]).toHaveTextContent('Choose position');
      expect(options[0]).toHaveAttribute('value', '');
      expect(options[1]).toHaveTextContent('2015/16');
      expect(options[1]).toHaveAttribute('value', '2015_AY');
    });

    test('adding reference lines when grouped by filters', async () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={{
              ...testAxisConfiguration,
              groupBy: 'filters',
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );
      expect(referenceLines.getAllByRole('row')).toHaveLength(2);

      userEvent.selectOptions(referenceLines.getByLabelText('Position'), [
        'state-funded-primary',
      ]);

      await userEvent.type(
        referenceLines.getByLabelText('Label'),
        'Test label',
      );

      userEvent.click(screen.getByRole('button', { name: 'Add line' }));

      await waitFor(() => {
        expect(referenceLines.getByText('Test label')).toBeInTheDocument();
      });

      const rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(3);
      expect(
        within(rows[1]).getByText('State-funded primary'),
      ).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label')).toBeInTheDocument();
      expect(within(rows[1]).getByRole('button', { name: /Remove line/ }));

      // Added reference line should be removed from Position options
      const position = within(rows[2]).getByLabelText('Position');
      const options = within(position).getAllByRole('option');

      expect(options).toHaveLength(4);
      expect(options[0]).toHaveTextContent('Choose position');
      expect(options[0]).toHaveAttribute('value', '');
      expect(options[1]).toHaveTextContent('Ethnicity Major Chinese');
      expect(options[1]).toHaveAttribute('value', 'ethnicity-major-chinese');
      expect(options[2]).toHaveTextContent('Ethnicity Major Black Total');
      expect(options[2]).toHaveAttribute(
        'value',
        'ethnicity-major-black-total',
      );
      expect(options[3]).toHaveTextContent('State-funded secondary');
      expect(options[3]).toHaveAttribute('value', 'state-funded-secondary');
    });

    test('adding reference lines when grouped by locations', async () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={{
              ...testAxisConfiguration,
              groupBy: 'locations',
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );
      expect(referenceLines.getAllByRole('row')).toHaveLength(2);

      userEvent.selectOptions(referenceLines.getByLabelText('Position'), [
        'barnsley',
      ]);

      await userEvent.type(
        referenceLines.getByLabelText('Label'),
        'Test label',
      );

      userEvent.click(screen.getByRole('button', { name: 'Add line' }));

      await waitFor(() => {
        expect(referenceLines.getByText('Test label')).toBeInTheDocument();
      });

      const rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(3);
      expect(within(rows[1]).getByText('Barnsley')).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label')).toBeInTheDocument();
      expect(within(rows[1]).getByRole('button', { name: /Remove line/ }));

      // Added reference line should be removed from Position options
      const position = within(rows[2]).getByLabelText('Position');
      const options = within(position).getAllByRole('option');

      expect(options).toHaveLength(2);
      expect(options[0]).toHaveTextContent('Choose position');
      expect(options[0]).toHaveAttribute('value', '');
      expect(options[1]).toHaveTextContent('Barnet');
      expect(options[1]).toHaveAttribute('value', 'barnet');
    });

    test('adding reference lines when grouped by indicators', async () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={{
              ...testAxisConfiguration,
              groupBy: 'indicators',
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );
      expect(referenceLines.getAllByRole('row')).toHaveLength(2);

      userEvent.selectOptions(referenceLines.getByLabelText('Position'), [
        'authorised-absence-sessions',
      ]);

      await userEvent.type(
        referenceLines.getByLabelText('Label'),
        'Test label',
      );

      userEvent.click(screen.getByRole('button', { name: 'Add line' }));

      await waitFor(() => {
        expect(referenceLines.getByText('Test label')).toBeInTheDocument();
      });

      const rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(3);
      expect(
        within(rows[1]).getByText('Number of authorised absence sessions'),
      ).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label')).toBeInTheDocument();
      expect(within(rows[1]).getByRole('button', { name: /Remove line/ }));

      // Added reference line should be removed from Position options
      const position = within(rows[2]).getByLabelText('Position');
      const options = within(position).getAllByRole('option');

      expect(options).toHaveLength(2);
      expect(options[0]).toHaveTextContent('Choose position');
      expect(options[0]).toHaveAttribute('value', '');
      expect(options[1]).toHaveTextContent(
        'Number of overall absence sessions',
      );
      expect(options[1]).toHaveAttribute('value', 'overall-absence-sessions');
    });

    test('adding reference lines for minor axis', async () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="minor"
            configuration={{
              ...testAxisConfiguration,
              type: 'minor',
              groupBy: undefined,
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );
      expect(referenceLines.getAllByRole('row')).toHaveLength(2);

      await userEvent.type(referenceLines.getByLabelText('Position'), '3000');

      await userEvent.type(
        referenceLines.getByLabelText('Label'),
        'Test label',
      );

      userEvent.click(screen.getByRole('button', { name: 'Add line' }));

      await waitFor(() => {
        expect(referenceLines.getByText('Test label')).toBeInTheDocument();
      });

      const rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(3);
      expect(within(rows[1]).getByText('3000')).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label')).toBeInTheDocument();
      expect(within(rows[1]).getByRole('button', { name: /Remove line/ }));
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

      userEvent.selectOptions(
        referenceLinesSection.getByLabelText('Position'),
        ['2014_AY'],
      );

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

    test('cannot add more reference lines if none available', () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={{
              ...testAxisConfiguration,
              referenceLines: [
                { position: '2014_AY', label: 'Test label 1' },
                { position: '2015_AY', label: 'Test label 2' },
              ],
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );

      const rows = referenceLines.getAllByRole('row');

      // Only 3 rows - the last row for adding new lines should not render
      expect(rows).toHaveLength(3);
      expect(within(rows[1]).getByText('2014/15')).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label 1')).toBeInTheDocument();
      expect(within(rows[2]).getByText('2015/16')).toBeInTheDocument();
      expect(within(rows[2]).getByText('Test label 2')).toBeInTheDocument();

      expect(
        referenceLines.queryByLabelText('Position'),
      ).not.toBeInTheDocument();
      expect(referenceLines.queryByLabelText('Label')).not.toBeInTheDocument();
    });

    test('removing reference lines', async () => {
      const handleSubmit = jest.fn();

      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={{
              ...testAxisConfiguration,
              referenceLines: [
                { position: '2014_AY', label: 'Test label 1' },
                { position: '2015_AY', label: 'Test label 2' },
              ],
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );

      let rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(3);

      userEvent.click(
        within(rows[2]).getByRole('button', { name: /Remove line/ }),
      );

      await waitFor(() => {
        expect(
          referenceLines.queryByText('Test label 2'),
        ).not.toBeInTheDocument();
      });

      rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(3);
      expect(within(rows[1]).getByText('2014/15')).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label 1')).toBeInTheDocument();

      expect(within(rows[2]).getByLabelText('Position')).toBeInTheDocument();
      expect(within(rows[2]).getByLabelText('Label')).toBeInTheDocument();
    });

    test('successfully submit with reference lines removed', async () => {
      const handleSubmit = jest.fn();

      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={{
              ...testAxisConfiguration,
              referenceLines: [
                { position: '2014_AY', label: 'Test label 1' },
                { position: '2015_AY', label: 'Test label 2' },
              ],
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );

      const rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(3);

      userEvent.click(
        within(rows[1]).getByRole('button', { name: /Remove line/ }),
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
          referenceLines: [{ position: '2015_AY', label: 'Test label 2' }],
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

    test('submitting filters out reference lines that no longer match the `groupBy` filters', async () => {
      const handleSubmit = jest.fn();

      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            configuration={{
              ...testAxisConfiguration,
              referenceLines: [
                { position: '2014_AY', label: 'Test label 1' },
                { position: '2015_AY', label: 'Test label 2' },
                { position: 'barnet', label: 'Test label 3' },
                {
                  position: 'authorised-absence-sessions',
                  label: 'Test label 3',
                },
                { position: 'ethnicity-major-chinese', label: 'Test label 4' },
              ],
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
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
          referenceLines: [
            // Only reference lines for time periods should remain
            { position: '2014_AY', label: 'Test label 1' },
            { position: '2015_AY', label: 'Test label 2' },
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

    test('submitting does not filter out reference lines when there is no `groupBy`', async () => {
      const handleSubmit = jest.fn();

      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-minor"
            type="minor"
            configuration={{
              ...testAxisConfiguration,
              groupBy: undefined,
              type: 'minor',
              referenceLines: [
                { position: 1000, label: 'Test label 1' },
                { position: 2000, label: 'Test label 2' },
              ],
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      userEvent.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        const formValues: AxisConfiguration = {
          dataSets: [],
          groupBy: undefined,
          min: 0,
          max: undefined,
          referenceLines: [
            { position: 1000, label: 'Test label 1' },
            { position: 2000, label: 'Test label 2' },
          ],
          showGrid: true,
          size: 50,
          sortAsc: true,
          sortBy: 'name',
          tickConfig: 'default',
          tickSpacing: 1,
          type: 'minor',
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
