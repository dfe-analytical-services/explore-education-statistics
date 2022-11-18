import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartAxisConfiguration from '@admin/pages/release/datablocks/components/chart/ChartAxisConfiguration';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { verticalBarBlockDefinition } from '@common/modules/charts/components/VerticalBarBlock';
import {
  AxesConfiguration,
  AxisConfiguration,
} from '@common/modules/charts/types/chart';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
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
    type: 'minor',
    referenceLines: [],
    visible: true,
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
    expect(axisSection.getByLabelText('Minimum')).toHaveValue('0');
    expect(axisSection.getByLabelText('Maximum')).toHaveValue('');

    const referenceLinesSection = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    expect(referenceLinesSection.getAllByRole('row')).toHaveLength(2);
    expect(referenceLinesSection.getByLabelText('Position')).toHaveValue('');
    expect(referenceLinesSection.getByLabelText('Label')).toHaveValue('');
    expect(referenceLinesSection.getByLabelText('Style')).toHaveValue('none');
  });

  test('calls `onChange` when form values change', () => {
    const handleChange = jest.fn();

    render(
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

    userEvent.clear(sizeInput);
    userEvent.type(sizeInput, '20');

    expect(handleChange).toHaveBeenCalledWith<[AxisConfiguration]>({
      ...testMajorAxisConfiguration,
      size: 20,
    });
  });

  test('shows validation error if invalid custom tick spacing given', async () => {
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
          axesConfiguration={testAxesConfiguration}
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
          axesConfiguration={testAxesConfiguration}
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
          axesConfiguration={testAxesConfiguration}
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
          axesConfiguration={testAxesConfiguration}
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
        dataSets: testMajorAxisConfiguration.dataSets,
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
            axesConfiguration={testAxesConfiguration}
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
          dataSets: testMajorAxisConfiguration.dataSets,
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
            axesConfiguration={testAxesConfiguration}
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
          dataSets: testMajorAxisConfiguration.dataSets,
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
    test('adding reference line for major axis', async () => {
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

    test('adding reference line for minor axis', async () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartAxisConfiguration
            id="chartBuilder-major"
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

    test('successfully submitting with new reference line', async () => {
      const handleSubmit = jest.fn();

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

      userEvent.selectOptions(referenceLinesSection.getByLabelText('Style'), [
        'dashed',
      ]);

      userEvent.click(screen.getByRole('button', { name: 'Add line' }));

      userEvent.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        const formValues: AxisConfiguration = {
          dataSets: testMajorAxisConfiguration.dataSets,
          groupBy: 'timePeriod',
          min: 0,
          max: undefined,
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

    test('successfully submitting with removed reference line', async () => {
      const handleSubmit = jest.fn();

      render(
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

      userEvent.click(
        within(rows[1]).getByRole('button', { name: /Remove line/ }),
      );
      userEvent.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        const formValues: AxisConfiguration = {
          dataSets: testMajorAxisConfiguration.dataSets,
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

    test('submitting filters out reference lines that no longer match the `groupBy`', async () => {
      const handleSubmit = jest.fn();

      render(
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

      userEvent.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        const formValues: AxisConfiguration = {
          dataSets: testMajorAxisConfiguration.dataSets,
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

      userEvent.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        const formValues: AxisConfiguration = {
          ...testMinorAxisConfiguration,
          referenceLines: [
            { position: 1000, label: 'Test label 1' },
            { position: 2000, label: 'Test label 2' },
          ],
        };
        expect(handleSubmit).toHaveBeenCalledWith(formValues);
      });
    });
  });
});
