import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartAxisConfiguration from '@admin/pages/release/datablocks/components/chart/ChartAxisConfiguration';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import render from '@common-test/render';
import { horizontalBarBlockDefinition } from '@common/modules/charts/components/HorizontalBarBlock';
import { verticalBarBlockDefinition } from '@common/modules/charts/components/VerticalBarBlock';
import {
  AxesConfiguration,
  AxisConfiguration,
} from '@common/modules/charts/types/chart';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

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

  const testMajorAxisConfiguration: AxisConfiguration = {
    dataSets: [
      {
        indicator: 'authorised-absence-sessions',
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      },
      {
        indicator: 'authorised-absence-sessions',
        filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
      },
      {
        indicator: 'authorised-absence-sessions',
        filters: ['ethnicity-major-black-total', 'state-funded-primary'],
      },
      {
        indicator: 'authorised-absence-sessions',
        filters: ['ethnicity-major-black-total', 'state-funded-secondary'],
      },
      {
        indicator: 'overall-absence-sessions',
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      },
      {
        indicator: 'overall-absence-sessions',
        filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
      },
      {
        indicator: 'overall-absence-sessions',
        filters: ['ethnicity-major-black-total', 'state-funded-primary'],
      },
      {
        indicator: 'overall-absence-sessions',
        filters: ['ethnicity-major-black-total', 'state-funded-secondary'],
      },
    ],
    groupBy: 'timePeriod',
    groupByFilter: '',
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
  const testMinorAxisConfiguration: AxisConfiguration = {
    dataSets: [],
    groupByFilterGroups: false,
    referenceLines: [],
    showGrid: true,
    sortAsc: true,
    size: 50,
    tickConfig: 'default',
    tickSpacing: 1,
    type: 'minor',
    unit: '',
    decimalPlaces: undefined,
    visible: true,
    label: { text: '', width: 100, rotated: false },
    max: undefined,
    min: undefined,
  };

  const testAxesConfiguration: AxesConfiguration = {
    major: testMajorAxisConfiguration,
    minor: testMinorAxisConfiguration,
  };

  const testTable = testFullTable;

  test('renders correctly with initial values', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartAxisConfiguration
          id="chartBuilder-major"
          type="major"
          axesConfiguration={testAxesConfiguration}
          definition={horizontalBarBlockDefinition}
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
    expect(
      generalSection.getByLabelText('Size of axis (pixels)'),
    ).toHaveNumericValue(50);
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
    expect(labelsSection.getByLabelText('Width (pixels)')).toHaveValue('');

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
    expect(axisSection.getByLabelText('Minimum')).toHaveValue('0');
    expect(axisSection.getByLabelText('Maximum')).toHaveValue('1');

    expect(
      screen.getByRole('heading', { name: 'Reference lines' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Add new line' }),
    ).toBeInTheDocument();
  });

  test('calls `onChange` when form values change', async () => {
    const handleChange = jest.fn();

    const { user } = render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartAxisConfiguration
          id="chartBuilder-major"
          type="major"
          axesConfiguration={testAxesConfiguration}
          definition={verticalBarBlockDefinition}
          data={testTable.results}
          meta={testTable.subjectMeta}
          onChange={handleChange}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    const sizeInput = screen.getByLabelText('Size of axis (pixels)');

    expect(handleChange).not.toHaveBeenCalled();

    await user.clear(sizeInput);
    await user.type(sizeInput, '20');

    expect(handleChange).toHaveBeenCalledWith<[AxisConfiguration]>({
      ...testMajorAxisConfiguration,
      groupByFilterGroups: undefined,
      label: { text: '', width: undefined },
      max: 1,
      size: 20,
    });
  });

  test('shows validation error if invalid custom tick spacing given', async () => {
    const { user } = render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartAxisConfiguration
          id="chartBuilder-major"
          type="major"
          axesConfiguration={testAxesConfiguration}
          definition={verticalBarBlockDefinition}
          data={testTable.results}
          meta={testTable.subjectMeta}
          onChange={noop}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await user.click(screen.getByLabelText('Custom'));
    await user.clear(screen.getByLabelText('Every nth value'));
    await user.type(screen.getByLabelText('Every nth value'), 'x');
    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Enter tick spacing' }),
    ).toHaveAttribute('href', '#chartBuilder-major-tickSpacing');

    await user.clear(screen.getByLabelText('Every nth value'));
    await user.type(screen.getByLabelText('Every nth value'), '-1');
    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );
    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Tick spacing must be positive' }),
    ).toHaveAttribute('href', '#chartBuilder-major-tickSpacing');
  });

  test('shows validation error if invalid label width given', async () => {
    const { user } = render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartAxisConfiguration
          id="chartBuilder-major"
          type="major"
          axesConfiguration={testAxesConfiguration}
          definition={verticalBarBlockDefinition}
          data={testTable.results}
          meta={testTable.subjectMeta}
          onChange={noop}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await user.type(screen.getByLabelText('Width (pixels)'), '-1');
    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Label width must be positive' }),
    ).toHaveAttribute('href', '#chartBuilder-major-labelWidth');
  });

  test('shows validation error if invalid axis width given', async () => {
    const { user } = render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartAxisConfiguration
          id="chartBuilder-major"
          type="major"
          axesConfiguration={testAxesConfiguration}
          definition={verticalBarBlockDefinition}
          data={testTable.results}
          meta={testTable.subjectMeta}
          onChange={noop}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );
    await user.clear(screen.getByLabelText('Size of axis (pixels)'));
    await user.type(screen.getByLabelText('Size of axis (pixels)'), '-1');
    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    expect(await screen.findByText('There is a problem')).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Size of axis must be positive' }),
    ).toHaveAttribute('href', '#chartBuilder-major-size');
  });

  test('submitting fails with invalid values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartAxisConfiguration
          id="chartBuilder-major"
          type="major"
          axesConfiguration={testAxesConfiguration}
          definition={verticalBarBlockDefinition}
          data={testTable.results}
          meta={testTable.subjectMeta}
          onChange={noop}
          onSubmit={handleSubmit}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await user.type(screen.getByLabelText('Width (pixels)'), '-1');

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('submitting succeeds with valid values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartAxisConfiguration
          id="chartBuilder-major"
          type="major"
          axesConfiguration={testAxesConfiguration}
          definition={verticalBarBlockDefinition}
          data={testTable.results}
          meta={testTable.subjectMeta}
          onChange={noop}
          onSubmit={handleSubmit}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await user.clear(screen.getByLabelText('Size of axis (pixels)'));
    await user.type(screen.getByLabelText('Size of axis (pixels)'), '100');

    await user.click(screen.getByRole('checkbox', { name: 'Sort ascending' }));

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    await waitFor(() => {
      const formValues: AxisConfiguration = {
        dataSets: testMajorAxisConfiguration.dataSets,
        groupBy: 'timePeriod',
        groupByFilter: '',
        groupByFilterGroups: undefined,
        min: 0,
        max: 1,
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
          width: undefined,
        },
      };
      expect(handleSubmit).toHaveBeenCalledWith(formValues);
    });
  });

  describe('displayed decimal places', () => {
    test('shows the displayed decimal places field for the minor axis', async () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-minor"
            type="minor"
            axesConfiguration={testAxesConfiguration}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );
      expect(
        screen.getByLabelText('Displayed decimal places'),
      ).toBeInTheDocument();
    });

    test('does not show the displayed decimal places field for the major axis', async () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            axesConfiguration={testAxesConfiguration}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );
      expect(
        screen.queryByLabelText('Displayed decimal places'),
      ).not.toBeInTheDocument();
    });

    test('shows validation error if invalid decimal places given', async () => {
      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-minor"
            type="minor"
            axesConfiguration={testAxesConfiguration}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );
      await user.clear(screen.getByLabelText('Displayed decimal places'));
      await user.type(screen.getByLabelText('Displayed decimal places'), '-1');
      await user.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      expect(await screen.findByText('There is a problem')).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'Displayed decimal places must be positive',
        }),
      ).toHaveAttribute('href', '#chartBuilder-minor-decimalPlaces');
    });

    test('allows decimal places to be set to 0', async () => {
      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-minor"
            type="minor"
            axesConfiguration={testAxesConfiguration}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );
      await user.clear(screen.getByLabelText('Displayed decimal places'));
      await user.type(screen.getByLabelText('Displayed decimal places'), '0');
      await user.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      expect(screen.queryByText('There is a problem')).not.toBeInTheDocument();
    });
  });

  describe('group data by', () => {
    test('submitting succeeds with the group data by option changed', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            axesConfiguration={testAxesConfiguration}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      await user.click(screen.getByRole('radio', { name: 'Locations' }));

      await user.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        const formValues: AxisConfiguration = {
          dataSets: testMajorAxisConfiguration.dataSets,
          groupBy: 'locations',
          groupByFilter: '',
          min: 0,
          max: 1,
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
            width: undefined,
          },
        };
        expect(handleSubmit).toHaveBeenCalledWith(formValues);
      });
    });

    test('submitting succeeds with group by a specific filter selected', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            axesConfiguration={testAxesConfiguration}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      await user.click(screen.getByRole('radio', { name: 'Filters' }));

      await user.selectOptions(screen.getByLabelText('Select a filter'), [
        'school_type',
      ]);

      await user.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        const formValues: AxisConfiguration = {
          dataSets: testMajorAxisConfiguration.dataSets,
          groupBy: 'filters',
          groupByFilter: 'school_type',
          groupByFilterGroups: undefined,
          min: 0,
          max: 1,
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
            width: undefined,
          },
        };
        expect(handleSubmit).toHaveBeenCalledWith(formValues);
      });
    });

    test('does not show the `Group by filter groups` option when data is not grouped by a filter with filter groups and bars are not stacked', async () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            axesConfiguration={{
              ...testAxesConfiguration,
              major: {
                ...testMajorAxisConfiguration,
                groupBy: 'filters',
                groupByFilter: 'school_type',
              },
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      expect(
        screen.queryByLabelText('Group by filter groups'),
      ).not.toBeInTheDocument();
    });

    test('does not show the `Group by filter groups` option when data is grouped by a filter with filter groups but bars are not stacked', () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            axesConfiguration={{
              ...testAxesConfiguration,
              major: {
                ...testMajorAxisConfiguration,
                groupBy: 'filters',
                groupByFilter: 'characteristic',
              },
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      expect(
        screen.queryByLabelText('Group by filter groups'),
      ).not.toBeInTheDocument();
    });

    test('shows the `Group by filter groups` option when data is grouped by a filter with filter groups and bars are stacked', async () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            axesConfiguration={{
              ...testAxesConfiguration,
              major: {
                ...testMajorAxisConfiguration,
                groupBy: 'filters',
                groupByFilter: 'characteristic',
              },
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            stacked
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      expect(
        screen.getByLabelText('Group by filter groups'),
      ).toBeInTheDocument();
    });

    test('setting `Group by filter groups`', async () => {
      const handleSubmit = jest.fn();
      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            axesConfiguration={{
              ...testAxesConfiguration,
              major: {
                ...testMajorAxisConfiguration,
                groupBy: 'filters',
                groupByFilter: 'characteristic',
              },
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            stacked
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      await user.click(screen.getByLabelText('Group by filter groups'));

      await user.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          dataSets: testMajorAxisConfiguration.dataSets,
          groupBy: 'filters',
          groupByFilter: 'characteristic',
          groupByFilterGroups: true,
          min: 0,
          max: 1,
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

            widht: undefined,
          },
        });
      });
    });
  });

  describe('reference lines', () => {
    test('adding a reference line for major axis', async () => {
      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            axesConfiguration={testAxesConfiguration}
            definition={horizontalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      await user.click(screen.getByRole('button', { name: 'Add new line' }));

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );
      expect(referenceLines.getAllByRole('row')).toHaveLength(2);

      await user.selectOptions(referenceLines.getByLabelText('Position'), [
        '2014_AY',
      ]);

      await user.type(referenceLines.getByLabelText('Label'), 'Test label');

      await user.click(screen.getByRole('button', { name: 'Add' }));

      await waitFor(() => {
        expect(referenceLines.getByText('Test label')).toBeInTheDocument();
      });

      const rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(2);
      expect(within(rows[1]).getByText('2014/15')).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label')).toBeInTheDocument();
      expect(within(rows[1]).getByRole('button', { name: /Remove line/ }));

      await user.click(screen.getByRole('button', { name: 'Add new line' }));

      // Added reference line should be removed from Position options
      const position = screen.getByLabelText('Position');
      const options = within(position).getAllByRole('option');

      expect(options).toHaveLength(2);
      expect(options[0]).toHaveTextContent('Choose position');
      expect(options[0]).toHaveAttribute('value', '');
      expect(options[1]).toHaveTextContent('2015/16');
      expect(options[1]).toHaveAttribute('value', '2015_AY');
    });

    test('adding a reference line for minor axis', async () => {
      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-minor"
            type="minor"
            axesConfiguration={testAxesConfiguration}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={noop}
          />
        </ChartBuilderFormsContextProvider>,
      );

      await user.click(screen.getByRole('button', { name: 'Add new line' }));

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );
      expect(referenceLines.getAllByRole('row')).toHaveLength(2);

      await user.type(referenceLines.getByLabelText('Position'), '3000');

      await user.type(referenceLines.getByLabelText('Label'), 'Test label');

      await user.click(screen.getByRole('button', { name: 'Add' }));

      await waitFor(() => {
        expect(referenceLines.getByText('Test label')).toBeInTheDocument();
      });

      const rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(2);
      expect(within(rows[1]).getByText('3000')).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label')).toBeInTheDocument();
      expect(within(rows[1]).getByRole('button', { name: /Remove line/ }));
    });

    test('successfully submitting with a new reference line', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            axesConfiguration={testAxesConfiguration}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      await user.click(screen.getByRole('button', { name: 'Add new line' }));

      const referenceLinesSection = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );
      expect(referenceLinesSection.getAllByRole('row')).toHaveLength(2);

      await user.selectOptions(
        referenceLinesSection.getByLabelText('Position'),
        ['2014_AY'],
      );

      await user.type(
        referenceLinesSection.getByLabelText('Label'),
        'I am label',
      );

      await user.selectOptions(referenceLinesSection.getByLabelText('Style'), [
        'dashed',
      ]);

      await user.click(screen.getByRole('button', { name: 'Add' }));

      await user.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        const formValues: AxisConfiguration = {
          dataSets: testMajorAxisConfiguration.dataSets,
          groupBy: 'timePeriod',
          groupByFilterGroups: undefined,
          groupByFilter: '',
          min: 0,
          max: 1,
          referenceLines: [
            {
              label: 'I am label',

              position: '2014_AY',
              style: 'dashed',
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
            width: undefined,
          },
        };
        expect(handleSubmit).toHaveBeenCalledWith(formValues);
      });
    });

    test('removing reference lines', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            axesConfiguration={{
              major: {
                ...testMajorAxisConfiguration,
                referenceLines: [
                  { position: '2014_AY', label: 'Test label 1' },
                  { position: '2015_AY', label: 'Test label 2' },
                ],
              },
              minor: testMinorAxisConfiguration,
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

      await user.click(
        within(rows[2]).getByRole('button', { name: /Remove line/ }),
      );

      await waitFor(() => {
        expect(
          referenceLines.queryByText('Test label 2'),
        ).not.toBeInTheDocument();
      });

      rows = referenceLines.getAllByRole('row');

      expect(rows).toHaveLength(2);
      expect(within(rows[1]).getByText('2014/15')).toBeInTheDocument();
      expect(within(rows[1]).getByText('Test label 1')).toBeInTheDocument();
    });

    test('successfully submitting with a removed reference line', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            axesConfiguration={{
              major: {
                ...testMajorAxisConfiguration,
                referenceLines: [
                  { position: '2014_AY', label: 'Test label 1' },
                  { position: '2015_AY', label: 'Test label 2' },
                ],
              },
              minor: testMinorAxisConfiguration,
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

      await user.click(
        within(rows[1]).getByRole('button', { name: /Remove line/ }),
      );
      await user.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        const formValues: AxisConfiguration = {
          dataSets: testMajorAxisConfiguration.dataSets,
          groupBy: 'timePeriod',
          groupByFilterGroups: undefined,
          groupByFilter: '',
          min: 0,
          max: 1,
          referenceLines: [
            { position: '2015_AY', label: 'Test label 2', style: 'none' },
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
            width: undefined,
          },
        };
        expect(handleSubmit).toHaveBeenCalledWith(formValues);
      });
    });

    test('editing a reference line', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            axesConfiguration={{
              major: {
                ...testMajorAxisConfiguration,
                referenceLines: [
                  { position: '2014_AY', label: 'Test label 1', style: 'none' },
                  { position: '2015_AY', label: 'Test label 2', style: 'none' },
                ],
              },
              minor: testMinorAxisConfiguration,
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

      expect(within(rows[1]).getByText('Test label 1')).toBeInTheDocument();

      await user.click(
        within(rows[1]).getByRole('button', { name: 'Edit line' }),
      );

      await user.type(within(rows[1]).getByLabelText('Label'), ' edited');

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(within(rows[1]).getByRole('button', { name: 'Save' }));

      await user.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledTimes(1);
      });

      const formValues: AxisConfiguration = {
        dataSets: testMajorAxisConfiguration.dataSets,
        groupBy: 'timePeriod',
        groupByFilter: '',
        min: 0,
        max: 1,
        referenceLines: [
          {
            label: 'Test label 1 edited',
            position: '2014_AY',
            style: 'none',
          },
          {
            position: '2015_AY',
            label: 'Test label 2',
            style: 'none',
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

          width: undefined,
        },
      };
      expect(handleSubmit).toHaveBeenCalledWith(formValues);
    });

    test('submitting filters out reference lines that no longer match the `groupBy`', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
            type="major"
            axesConfiguration={{
              major: {
                ...testMajorAxisConfiguration,
                referenceLines: [
                  { position: '2014_AY', label: 'Test label 1' },
                  { position: '2015_AY', label: 'Test label 2' },
                  { position: 'barnet', label: 'Test label 3' },
                  {
                    position: 'authorised-absence-sessions',
                    label: 'Test label 3',
                  },
                  {
                    position: 'ethnicity-major-chinese',
                    label: 'Test label 4',
                  },
                ],
              },
              minor: testMinorAxisConfiguration,
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );
      await waitFor(() => {
        const formValues: AxisConfiguration = {
          dataSets: testMajorAxisConfiguration.dataSets,
          groupBy: 'timePeriod',
          groupByFilter: '',
          groupByFilterGroups: undefined,
          min: 0,
          max: 1,
          referenceLines: [
            // Only reference lines for time periods should remain
            { position: '2014_AY', label: 'Test label 1', style: 'none' },
            { position: '2015_AY', label: 'Test label 2', style: 'none' },
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
            width: undefined,
          },
        };
        expect(handleSubmit).toHaveBeenCalledWith(formValues);
      });
    });

    test('submitting does not filter out reference lines when there is no `groupBy`', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-minor"
            type="minor"
            axesConfiguration={{
              major: testMajorAxisConfiguration,
              minor: {
                ...testMinorAxisConfiguration,
                referenceLines: [
                  { position: 1000, label: 'Test label 1' },
                  { position: 2000, label: 'Test label 2' },
                ],
              },
            }}
            definition={verticalBarBlockDefinition}
            data={testTable.results}
            meta={testTable.subjectMeta}
            onChange={noop}
            onSubmit={handleSubmit}
          />
        </ChartBuilderFormsContextProvider>,
      );

      await user.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        const formValues: AxisConfiguration = {
          ...testMinorAxisConfiguration,
          referenceLines: [
            { position: 1000, label: 'Test label 1', style: 'none' },
            { position: 2000, label: 'Test label 2', style: 'none' },
          ],
        };
        expect(handleSubmit).toHaveBeenCalledWith(formValues);
      });
    });

    describe('validation', () => {
      test('shows validation error when `Position` is empty', async () => {
        const { user } = render(
          <ChartBuilderFormsContextProvider initialForms={testFormState}>
            <ChartAxisConfiguration
              id="chartBuilder-major"
              type="major"
              axesConfiguration={{
                major: testMajorAxisConfiguration,
                minor: testMinorAxisConfiguration,
              }}
              definition={verticalBarBlockDefinition}
              data={testTable.results}
              meta={testTable.subjectMeta}
              onChange={noop}
              onSubmit={noop}
            />
          </ChartBuilderFormsContextProvider>,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );

        expect(
          referenceLines.queryByText('Enter position'),
        ).not.toBeInTheDocument();

        await user.click(referenceLines.getByLabelText('Position'));
        await user.tab();

        expect(
          await referenceLines.findByText('Enter position'),
        ).toBeInTheDocument();
      });

      test('shows validation error for a major axis line when `otherAxisPosition` is not valid', async () => {
        const { user } = render(
          <ChartBuilderFormsContextProvider initialForms={testFormState}>
            <ChartAxisConfiguration
              id="chartBuilder-major"
              type="major"
              axesConfiguration={{
                major: testMajorAxisConfiguration,
                minor: testMinorAxisConfiguration,
              }}
              definition={verticalBarBlockDefinition}
              data={testTable.results}
              meta={testTable.subjectMeta}
              onChange={noop}
              onSubmit={noop}
            />
          </ChartBuilderFormsContextProvider>,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );

        expect(
          referenceLines.queryByText(
            'Enter a position within the Y axis min/max range',
          ),
        ).not.toBeInTheDocument();

        await user.type(
          referenceLines.getByLabelText('Y axis position'),
          '50500',
        );
        await user.tab();

        expect(
          await referenceLines.findByText(
            'Enter a position within the Y axis min/max range',
          ),
        ).toBeInTheDocument();
      });

      test('shows validation error when `Label` is empty', async () => {
        const { user } = render(
          <ChartBuilderFormsContextProvider initialForms={testFormState}>
            <ChartAxisConfiguration
              id="chartBuilder-major"
              type="major"
              axesConfiguration={{
                major: testMajorAxisConfiguration,
                minor: testMinorAxisConfiguration,
              }}
              definition={verticalBarBlockDefinition}
              data={testTable.results}
              meta={testTable.subjectMeta}
              onChange={noop}
              onSubmit={noop}
            />
          </ChartBuilderFormsContextProvider>,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );

        expect(
          referenceLines.queryByText('Enter label'),
        ).not.toBeInTheDocument();

        await user.click(referenceLines.getByLabelText('Label'));
        await user.tab();

        expect(
          await referenceLines.findByText('Enter label'),
        ).toBeInTheDocument();
      });

      test('disables the add button when adding a reference line with invalid values', async () => {
        const { user } = render(
          <ChartBuilderFormsContextProvider initialForms={testFormState}>
            <ChartAxisConfiguration
              id="chartBuilder-major"
              type="major"
              axesConfiguration={{
                major: testMajorAxisConfiguration,
                minor: testMinorAxisConfiguration,
              }}
              definition={verticalBarBlockDefinition}
              data={testTable.results}
              meta={testTable.subjectMeta}
              onChange={noop}
              onSubmit={noop}
            />
          </ChartBuilderFormsContextProvider>,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );

        expect(
          referenceLines.getByRole('tooltip', { hidden: true }),
        ).toHaveTextContent('Cannot add invalid reference line');
      });

      test('shows validation error for a minor axis line when `Position` is not valid', async () => {
        const { user } = render(
          <ChartBuilderFormsContextProvider initialForms={testFormState}>
            <ChartAxisConfiguration
              id="chartBuilder-minor"
              type="minor"
              axesConfiguration={{
                major: testMajorAxisConfiguration,
                minor: testMinorAxisConfiguration,
              }}
              definition={verticalBarBlockDefinition}
              data={testTable.results}
              meta={testTable.subjectMeta}
              onChange={noop}
              onSubmit={noop}
            />
          </ChartBuilderFormsContextProvider>,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );

        expect(
          referenceLines.queryByText(
            'Enter a position within the Y axis min/max range',
          ),
        ).not.toBeInTheDocument();

        await user.type(referenceLines.getByLabelText('Position'), '50500');
        await user.tab();

        await waitFor(() => {
          expect(
            referenceLines.getByText(
              'Enter a position within the Y axis min/max range',
            ),
          ).toBeInTheDocument();
        });
      });

      test('shows validation error for a minor axis line when `otherAxisPosition` is not valid', async () => {
        const { user } = render(
          <ChartBuilderFormsContextProvider initialForms={testFormState}>
            <ChartAxisConfiguration
              id="chartBuilder-minor"
              type="minor"
              axesConfiguration={{
                major: testMajorAxisConfiguration,
                minor: testMinorAxisConfiguration,
              }}
              definition={verticalBarBlockDefinition}
              data={testTable.results}
              meta={testTable.subjectMeta}
              onChange={noop}
              onSubmit={noop}
            />
          </ChartBuilderFormsContextProvider>,
        );
        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );

        expect(
          referenceLines.queryByText('Enter a percentage between 0 and 100%'),
        ).not.toBeInTheDocument();

        await user.selectOptions(
          referenceLines.getByLabelText('X axis position'),
          ['custom'],
        );

        await user.type(referenceLines.getByLabelText(/Percent/), '101');
        await user.tab();

        await waitFor(() => {
          expect(
            referenceLines.getByText('Enter a percentage between 0 and 100%'),
          ).toBeInTheDocument();
        });
      });

      test('shows validation error when trying to add a duplicate line on the minor axis', async () => {
        const { user } = render(
          <ChartBuilderFormsContextProvider initialForms={testFormState}>
            <ChartAxisConfiguration
              id="chartBuilder-minor"
              type="minor"
              axesConfiguration={{
                major: testMajorAxisConfiguration,
                minor: testMinorAxisConfiguration,
              }}
              definition={verticalBarBlockDefinition}
              data={testTable.results}
              meta={testTable.subjectMeta}
              onChange={noop}
              onSubmit={noop}
            />
          </ChartBuilderFormsContextProvider>,
        );
        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );

        await user.type(referenceLines.getByLabelText('Position'), '10');
        await user.type(referenceLines.getByLabelText('Label'), 'Test label');
        await user.click(referenceLines.getByRole('button', { name: 'Add' }));

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        expect(
          referenceLines.queryByText(
            'A line with these settings has already been added',
          ),
        ).not.toBeInTheDocument();

        await user.type(referenceLines.getByLabelText('Position'), '10');
        await user.type(referenceLines.getByLabelText('Label'), 'Test label');
        await user.click(referenceLines.getByRole('button', { name: 'Add' }));

        await waitFor(() => {
          expect(
            referenceLines.getByText(
              'A line with these settings has already been added',
            ),
          ).toBeInTheDocument();
        });
      });

      describe('reference lines between data points on the major axis', () => {
        test('shows validation error when start and end point are empty', async () => {
          const { user } = render(
            <ChartBuilderFormsContextProvider initialForms={testFormState}>
              <ChartAxisConfiguration
                id="chartBuilder-major"
                type="major"
                axesConfiguration={{
                  major: testMajorAxisConfiguration,
                  minor: testMinorAxisConfiguration,
                }}
                definition={verticalBarBlockDefinition}
                data={testTable.results}
                meta={testTable.subjectMeta}
                onChange={noop}
                onSubmit={noop}
              />
            </ChartBuilderFormsContextProvider>,
          );

          await user.click(
            screen.getByRole('button', { name: 'Add new line' }),
          );

          const referenceLines = within(
            screen.getByRole('table', { name: 'Reference lines' }),
          );

          await user.selectOptions(referenceLines.getByLabelText('Position'), [
            'between-data-points',
          ]);

          expect(
            referenceLines.queryByText('Enter start point'),
          ).not.toBeInTheDocument();
          expect(
            referenceLines.queryByText('Enter end point'),
          ).not.toBeInTheDocument();

          await user.click(referenceLines.getByLabelText('Start point'));
          await user.tab();

          await waitFor(() => {
            expect(
              referenceLines.getByText('Enter start point'),
            ).toBeInTheDocument();
          });

          await user.click(referenceLines.getByLabelText('End point'));
          await user.tab();

          await waitFor(() => {
            expect(
              referenceLines.getByText('Enter end point'),
            ).toBeInTheDocument();
          });
        });

        test('shows validation error when start and end point are the same', async () => {
          const { user } = render(
            <ChartBuilderFormsContextProvider initialForms={testFormState}>
              <ChartAxisConfiguration
                id="chartBuilder-major"
                type="major"
                axesConfiguration={{
                  major: testMajorAxisConfiguration,
                  minor: testMinorAxisConfiguration,
                }}
                definition={verticalBarBlockDefinition}
                data={testTable.results}
                meta={testTable.subjectMeta}
                onChange={noop}
                onSubmit={noop}
              />
            </ChartBuilderFormsContextProvider>,
          );

          await user.click(
            screen.getByRole('button', { name: 'Add new line' }),
          );

          const referenceLines = within(
            screen.getByRole('table', { name: 'Reference lines' }),
          );

          await user.selectOptions(referenceLines.getByLabelText('Position'), [
            'between-data-points',
          ]);

          expect(
            referenceLines.queryByText('End point cannot match start point'),
          ).not.toBeInTheDocument();

          await user.selectOptions(
            referenceLines.getByLabelText('Start point'),
            ['2014_AY'],
          );

          await user.selectOptions(referenceLines.getByLabelText('End point'), [
            '2014_AY',
          ]);
          await user.tab();

          await waitFor(() => {
            expect(
              referenceLines.getByText('End point cannot match start point'),
            ).toBeInTheDocument();
          });
        });

        test('shows validation error when other axis position is empty', async () => {
          const { user } = render(
            <ChartBuilderFormsContextProvider initialForms={testFormState}>
              <ChartAxisConfiguration
                id="chartBuilder-major"
                type="major"
                axesConfiguration={{
                  major: testMajorAxisConfiguration,
                  minor: testMinorAxisConfiguration,
                }}
                definition={verticalBarBlockDefinition}
                data={testTable.results}
                meta={testTable.subjectMeta}
                onChange={noop}
                onSubmit={noop}
              />
            </ChartBuilderFormsContextProvider>,
          );

          await user.click(
            screen.getByRole('button', { name: 'Add new line' }),
          );

          const referenceLines = within(
            screen.getByRole('table', { name: 'Reference lines' }),
          );

          await user.selectOptions(referenceLines.getByLabelText('Position'), [
            'between-data-points',
          ]);

          expect(
            referenceLines.queryByText('Enter a Y axis position'),
          ).not.toBeInTheDocument();

          await user.click(referenceLines.getByLabelText('Y axis position'));
          await user.tab();

          await waitFor(() => {
            expect(
              referenceLines.getByText('Enter a Y axis position'),
            ).toBeInTheDocument();
          });
        });
      });

      describe('reference lines between data points on the minor axis', () => {
        test('shows validation error when start and end point are empty', async () => {
          const { user } = render(
            <ChartBuilderFormsContextProvider initialForms={testFormState}>
              <ChartAxisConfiguration
                id="chartBuilder-minor"
                type="minor"
                axesConfiguration={{
                  major: testMajorAxisConfiguration,
                  minor: testMinorAxisConfiguration,
                }}
                definition={verticalBarBlockDefinition}
                data={testTable.results}
                meta={testTable.subjectMeta}
                onChange={noop}
                onSubmit={noop}
              />
            </ChartBuilderFormsContextProvider>,
          );

          await user.click(
            screen.getByRole('button', { name: 'Add new line' }),
          );

          const referenceLines = within(
            screen.getByRole('table', { name: 'Reference lines' }),
          );

          await user.selectOptions(
            referenceLines.getByLabelText('X axis position'),
            ['between-data-points'],
          );

          expect(
            referenceLines.queryByText('Enter start point'),
          ).not.toBeInTheDocument();
          expect(
            referenceLines.queryByText('Enter end point'),
          ).not.toBeInTheDocument();

          await user.click(referenceLines.getByLabelText('Start point'));
          await user.tab();

          await waitFor(() => {
            expect(
              referenceLines.getByText('Enter start point'),
            ).toBeInTheDocument();
          });

          await user.click(referenceLines.getByLabelText('End point'));
          await user.tab();

          await waitFor(() => {
            expect(
              referenceLines.getByText('Enter end point'),
            ).toBeInTheDocument();
          });
        });

        test('shows validation error when start and end point are the same', async () => {
          const { user } = render(
            <ChartBuilderFormsContextProvider initialForms={testFormState}>
              <ChartAxisConfiguration
                id="chartBuilder-minor"
                type="minor"
                axesConfiguration={{
                  major: testMajorAxisConfiguration,
                  minor: testMinorAxisConfiguration,
                }}
                definition={verticalBarBlockDefinition}
                data={testTable.results}
                meta={testTable.subjectMeta}
                onChange={noop}
                onSubmit={noop}
              />
            </ChartBuilderFormsContextProvider>,
          );
          await user.click(
            screen.getByRole('button', { name: 'Add new line' }),
          );

          const referenceLines = within(
            screen.getByRole('table', { name: 'Reference lines' }),
          );

          await user.selectOptions(
            referenceLines.getByLabelText('X axis position'),
            ['between-data-points'],
          );

          expect(
            referenceLines.queryByText('End point cannot match start point'),
          ).not.toBeInTheDocument();

          await user.selectOptions(
            referenceLines.getByLabelText('Start point'),
            ['2014_AY'],
          );

          await user.selectOptions(referenceLines.getByLabelText('End point'), [
            '2014_AY',
          ]);
          await user.tab();

          await waitFor(() => {
            expect(
              referenceLines.getByText('End point cannot match start point'),
            ).toBeInTheDocument();
          });
        });
      });
    });
  });
});
