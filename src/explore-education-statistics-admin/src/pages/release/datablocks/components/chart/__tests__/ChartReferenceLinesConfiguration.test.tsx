import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartReferenceLinesConfiguration, {
  ChartReferenceLinesConfigurationProps,
} from '@admin/pages/release/datablocks/components/chart/ChartReferenceLinesConfiguration';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import { lineChartBlockDefinition } from '@common/modules/charts/components/LineChartBlock';
import {
  AxisConfiguration,
  ChartDefinitionAxis,
} from '@common/modules/charts/types/chart';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('ChartReferenceLinesConfiguration', () => {
  const testAxisConfiguration: AxisConfiguration = {
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

  const testTable = testFullTable;
  const testTimePeriodDataSetCategories = createDataSetCategories(
    {
      ...testAxisConfiguration,
      groupBy: 'timePeriod',
    },
    testTable.results,
    testTable.subjectMeta,
  );

  test('renders correctly with existing lines when grouped by time periods', () => {
    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={testTimePeriodDataSetCategories}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[{ position: '2014_AY', label: 'Test label 1' }]}
        onAddLine={noop}
        onRemoveLine={noop}
      />,
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
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'filters',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[
          { position: 'ethnicity-major-chinese', label: 'Test label 1' },
          { position: 'state-funded-secondary', label: 'Test label 2' },
        ]}
        onAddLine={noop}
        onRemoveLine={noop}
      />,
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
    expect(options[1]).toHaveAttribute('value', 'ethnicity-major-black-total');
    expect(options[2]).toHaveTextContent('State-funded primary');
    expect(options[2]).toHaveAttribute('value', 'state-funded-primary');
  });

  test('renders correctly with existing lines when grouped by locations', () => {
    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'locations',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[{ position: 'barnet', label: 'Test label 1' }]}
        onAddLine={noop}
        onRemoveLine={noop}
      />,
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
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'indicators',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[
          { position: 'overall-absence-sessions', label: 'Test label 1' },
        ]}
        onAddLine={noop}
        onRemoveLine={noop}
      />,
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
    expect(options[1]).toHaveAttribute('value', 'authorised-absence-sessions');
  });

  test('renders correctly with existing lines for minor axis', () => {
    render(
      <ChartReferenceLinesConfiguration
        axisType="minor"
        dataSetCategories={[]}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[
          { position: 2000, label: 'Test label 1' },
          { position: 4000, label: 'Test label 2' },
        ]}
        onAddLine={noop}
        onRemoveLine={noop}
      />,
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

  test('shows validation error when `Position` is empty', async () => {
    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={testTimePeriodDataSetCategories}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[]}
        onAddLine={noop}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );

    expect(
      referenceLines.queryByText('Enter position'),
    ).not.toBeInTheDocument();

    userEvent.click(referenceLines.getByLabelText('Position'));
    userEvent.tab();

    await waitFor(() => {
      expect(referenceLines.getByText('Enter position')).toBeInTheDocument();
    });
  });

  test('shows validation error when `Label` is empty', async () => {
    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={testTimePeriodDataSetCategories}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[]}
        onAddLine={noop}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );

    expect(referenceLines.queryByText('Enter label')).not.toBeInTheDocument();

    userEvent.click(referenceLines.getByLabelText('Label'));
    userEvent.tab();

    await waitFor(() => {
      expect(referenceLines.getByText('Enter label')).toBeInTheDocument();
    });
  });

  test('shows error messages when adding reference line with invalid values', async () => {
    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={testTimePeriodDataSetCategories}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[]}
        onAddLine={noop}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );

    expect(
      referenceLines.queryByText('Enter position'),
    ).not.toBeInTheDocument();
    expect(referenceLines.queryByText('Enter label')).not.toBeInTheDocument();

    userEvent.click(referenceLines.getByRole('button', { name: 'Add line' }));

    await waitFor(() => {
      expect(referenceLines.getByText('Enter position')).toBeInTheDocument();
      expect(referenceLines.getByText('Enter label')).toBeInTheDocument();
    });

    expect(
      referenceLines.getByRole('tooltip', { hidden: true }),
    ).toHaveTextContent('Cannot add invalid reference line');
  });

  test('default value for `Style` field is dashed', async () => {
    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={testTimePeriodDataSetCategories}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[]}
        onAddLine={noop}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    expect(referenceLines.getAllByRole('row')).toHaveLength(2);

    expect(referenceLines.getByLabelText('Style')).toHaveValue('dashed');
  });

  test('can set default value for reference line `Style` field via chart definition', async () => {
    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={testTimePeriodDataSetCategories}
        definition={{
          ...lineChartBlockDefinition,
          axes: {
            major: {
              ...lineChartBlockDefinition.axes?.major,
              referenceLineDefaults: {
                style: 'none',
              },
            } as ChartDefinitionAxis,
            minor: lineChartBlockDefinition.axes.minor,
          },
        }}
        id="test-form"
        lines={[]}
        onAddLine={noop}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    expect(referenceLines.getAllByRole('row')).toHaveLength(2);

    expect(referenceLines.getByLabelText('Style')).toHaveValue('none');
  });

  test('adding reference line when grouped by time periods', async () => {
    const handleAddLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={testTimePeriodDataSetCategories}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[]}
        onAddLine={handleAddLine}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    expect(referenceLines.getAllByRole('row')).toHaveLength(2);

    userEvent.selectOptions(referenceLines.getByLabelText('Position'), [
      '2014_AY',
    ]);

    await userEvent.type(referenceLines.getByLabelText('Label'), 'Test label');

    expect(handleAddLine).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Add line' }));

    await waitFor(() => {
      expect(handleAddLine).toHaveBeenCalledTimes(1);
      expect(handleAddLine).toHaveBeenCalledWith<
        Parameters<ChartReferenceLinesConfigurationProps['onAddLine']>
      >({
        label: 'Test label',
        position: '2014_AY',
        style: 'dashed',
      });
    });
  });

  test('adding reference line when grouped by filters', async () => {
    const handleAddLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'filters',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[]}
        onAddLine={handleAddLine}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    expect(referenceLines.getAllByRole('row')).toHaveLength(2);

    userEvent.selectOptions(referenceLines.getByLabelText('Position'), [
      'state-funded-primary',
    ]);

    await userEvent.type(referenceLines.getByLabelText('Label'), 'Test label');

    userEvent.click(screen.getByRole('button', { name: 'Add line' }));

    await waitFor(() => {
      expect(handleAddLine).toHaveBeenCalledTimes(1);
      expect(handleAddLine).toHaveBeenCalledWith<
        Parameters<ChartReferenceLinesConfigurationProps['onAddLine']>
      >({
        label: 'Test label',
        position: 'state-funded-primary',
        style: 'dashed',
      });
    });
  });

  test('adding reference line when grouped by locations', async () => {
    const handleAddLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'locations',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[]}
        onAddLine={handleAddLine}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    expect(referenceLines.getAllByRole('row')).toHaveLength(2);

    userEvent.selectOptions(referenceLines.getByLabelText('Position'), [
      'barnsley',
    ]);

    await userEvent.type(referenceLines.getByLabelText('Label'), 'Test label');

    userEvent.click(screen.getByRole('button', { name: 'Add line' }));

    await waitFor(() => {
      expect(handleAddLine).toHaveBeenCalledTimes(1);
      expect(handleAddLine).toHaveBeenCalledWith<
        Parameters<ChartReferenceLinesConfigurationProps['onAddLine']>
      >({
        label: 'Test label',
        position: 'barnsley',
        style: 'dashed',
      });
    });
  });

  test('adding reference line when grouped by indicators', async () => {
    const handleAddLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'indicators',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[]}
        onAddLine={handleAddLine}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    expect(referenceLines.getAllByRole('row')).toHaveLength(2);

    userEvent.selectOptions(referenceLines.getByLabelText('Position'), [
      'authorised-absence-sessions',
    ]);

    await userEvent.type(referenceLines.getByLabelText('Label'), 'Test label');

    userEvent.click(screen.getByRole('button', { name: 'Add line' }));

    await waitFor(() => {
      expect(handleAddLine).toHaveBeenCalledTimes(1);
      expect(handleAddLine).toHaveBeenCalledWith<
        Parameters<ChartReferenceLinesConfigurationProps['onAddLine']>
      >({
        label: 'Test label',
        position: 'authorised-absence-sessions',
        style: 'dashed',
      });
    });
  });

  test('adding reference line for minor axis', async () => {
    const handleAddLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        axisType="minor"
        dataSetCategories={[]}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[]}
        onAddLine={handleAddLine}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    expect(referenceLines.getAllByRole('row')).toHaveLength(2);

    await userEvent.type(referenceLines.getByLabelText('Position'), '3000');
    await userEvent.type(referenceLines.getByLabelText('Label'), 'Test label');

    userEvent.click(screen.getByRole('button', { name: 'Add line' }));

    await waitFor(() => {
      expect(handleAddLine).toHaveBeenCalledTimes(1);
      expect(handleAddLine).toHaveBeenCalledWith<
        Parameters<ChartReferenceLinesConfigurationProps['onAddLine']>
      >({
        label: 'Test label',
        position: 3000,
        style: 'dashed',
      });
    });
  });

  test('adding reference line with non-default style', async () => {
    const handleAddLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={testTimePeriodDataSetCategories}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[]}
        onAddLine={handleAddLine}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    expect(referenceLines.getAllByRole('row')).toHaveLength(2);

    userEvent.selectOptions(referenceLines.getByLabelText('Position'), [
      '2014_AY',
    ]);
    await userEvent.type(referenceLines.getByLabelText('Label'), 'Test label');

    expect(referenceLines.getByLabelText('Style')).toHaveValue('dashed');

    userEvent.selectOptions(referenceLines.getByLabelText('Style'), ['none']);

    expect(handleAddLine).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Add line' }));

    await waitFor(() => {
      expect(handleAddLine).toHaveBeenCalledTimes(1);
      expect(handleAddLine).toHaveBeenCalledWith<
        Parameters<ChartReferenceLinesConfigurationProps['onAddLine']>
      >({
        label: 'Test label',
        position: '2014_AY',
        style: 'none',
      });
    });
  });

  test('cannot add reference line when no more options', async () => {
    const handleAddLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={testTimePeriodDataSetCategories}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[
          { position: '2014_AY', label: 'Test label 1' },
          { position: '2015_AY', label: 'Test label 1' },
        ]}
        onAddLine={handleAddLine}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    expect(referenceLines.getAllByRole('row')).toHaveLength(3);

    expect(referenceLines.queryByLabelText('Position')).not.toBeInTheDocument();
    expect(referenceLines.queryByLabelText('Label')).not.toBeInTheDocument();
    expect(
      referenceLines.queryByRole('button', { name: 'Add line' }),
    ).not.toBeInTheDocument();
  });

  test('removing reference line when grouped by time periods', async () => {
    const handleRemoveLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={testTimePeriodDataSetCategories}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[
          { position: '2014_AY', label: 'Test label 1' },
          { position: '2015_AY', label: 'Test label 2' },
        ]}
        onAddLine={noop}
        onRemoveLine={handleRemoveLine}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    const rows = referenceLines.getAllByRole('row');
    expect(rows).toHaveLength(3);

    userEvent.click(
      within(rows[1]).getByRole('button', { name: 'Remove line' }),
    );

    await waitFor(() => {
      expect(handleRemoveLine).toHaveBeenCalledTimes(1);
      expect(handleRemoveLine).toHaveBeenCalledWith<
        Parameters<ChartReferenceLinesConfigurationProps['onRemoveLine']>
      >({
        label: 'Test label 1',
        position: '2014_AY',
      });
    });
  });

  test('removing reference line when grouped by filters', async () => {
    const handleRemoveLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'filters',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[
          { position: 'state-funded-primary', label: 'Test label 1' },
          { position: 'state-funded-secondary', label: 'Test label 2' },
          { position: 'ethnicity-major-chinese', label: 'Test label 3' },
        ]}
        onAddLine={noop}
        onRemoveLine={handleRemoveLine}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    const rows = referenceLines.getAllByRole('row');
    expect(rows).toHaveLength(5);

    userEvent.click(
      within(rows[2]).getByRole('button', { name: 'Remove line' }),
    );

    await waitFor(() => {
      expect(handleRemoveLine).toHaveBeenCalledTimes(1);
      expect(handleRemoveLine).toHaveBeenCalledWith<
        Parameters<ChartReferenceLinesConfigurationProps['onRemoveLine']>
      >({
        label: 'Test label 2',
        position: 'state-funded-secondary',
      });
    });
  });

  test('removing reference line when grouped by locations', async () => {
    const handleRemoveLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'locations',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[
          { position: 'barnet', label: 'Test label 1' },
          { position: 'barnsley', label: 'Test label 2' },
        ]}
        onAddLine={noop}
        onRemoveLine={handleRemoveLine}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    const rows = referenceLines.getAllByRole('row');
    expect(rows).toHaveLength(3);

    userEvent.click(
      within(rows[2]).getByRole('button', { name: 'Remove line' }),
    );

    await waitFor(() => {
      expect(handleRemoveLine).toHaveBeenCalledTimes(1);
      expect(handleRemoveLine).toHaveBeenCalledWith<
        Parameters<ChartReferenceLinesConfigurationProps['onRemoveLine']>
      >({
        label: 'Test label 2',
        position: 'barnsley',
      });
    });
  });

  test('removing reference line when grouped by indicators', async () => {
    const handleRemoveLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        axisType="major"
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'indicators',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[
          { position: 'authorised-absence-sessions', label: 'Test label 1' },
          { position: 'overall-absence-sessions', label: 'Test label 2' },
        ]}
        onAddLine={noop}
        onRemoveLine={handleRemoveLine}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    const rows = referenceLines.getAllByRole('row');
    expect(rows).toHaveLength(3);

    userEvent.click(
      within(rows[2]).getByRole('button', { name: 'Remove line' }),
    );

    await waitFor(() => {
      expect(handleRemoveLine).toHaveBeenCalledTimes(1);
      expect(handleRemoveLine).toHaveBeenCalledWith<
        Parameters<ChartReferenceLinesConfigurationProps['onRemoveLine']>
      >({
        label: 'Test label 2',
        position: 'overall-absence-sessions',
      });
    });
  });

  test('removing reference line for minor axis', async () => {
    const handleRemoveLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        axisType="minor"
        dataSetCategories={[]}
        definition={lineChartBlockDefinition}
        id="test-form"
        lines={[
          { position: 1000, label: 'Test label 1' },
          { position: 2000, label: 'Test label 2' },
          { position: 3000, label: 'Test label 2' },
        ]}
        onAddLine={noop}
        onRemoveLine={handleRemoveLine}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    const rows = referenceLines.getAllByRole('row');
    expect(rows).toHaveLength(5);

    userEvent.click(
      within(rows[2]).getByRole('button', { name: 'Remove line' }),
    );

    await waitFor(() => {
      expect(handleRemoveLine).toHaveBeenCalledTimes(1);
      expect(handleRemoveLine).toHaveBeenCalledWith<
        Parameters<ChartReferenceLinesConfigurationProps['onRemoveLine']>
      >({
        label: 'Test label 2',
        position: 2000,
      });
    });
  });
});
