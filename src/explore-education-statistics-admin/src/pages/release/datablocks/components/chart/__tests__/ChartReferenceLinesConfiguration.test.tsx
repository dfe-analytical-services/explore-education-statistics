import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartReferenceLinesConfiguration, {
  ChartReferenceLinesConfigurationProps,
} from '@admin/pages/release/datablocks/components/chart/ChartReferenceLinesConfiguration';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import {
  AxisConfiguration,
  ChartDefinitionAxis,
} from '@common/modules/charts/types/chart';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import { MinorAxisDomainValues } from '@common/modules/charts/util/domainTicks';
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

  const testMajorAxisDefinition: ChartDefinitionAxis = {
    axis: 'x',
    id: 'xaxis',
    title: 'X Axis (major axis)',
    type: 'major',
    capabilities: {
      canRotateLabel: false,
    },
  };
  const testMinorAxisDefinition: ChartDefinitionAxis = {
    axis: 'y',
    id: 'yaxis',
    title: 'Y Axis (minor axis)',
    type: 'minor',
    capabilities: {
      canRotateLabel: true,
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

  const testMinorAxisDomain: MinorAxisDomainValues = { min: 0, max: 50000 };

  const barnsleyId = LocationFilter.createId({
    level: 'localAuthority',
    value: 'barnsley',
  });
  const barnetId = LocationFilter.createId({
    level: 'localAuthority',
    value: 'barnet',
  });

  test('renders correctly with existing lines when grouped by time periods', () => {
    render(
      <ChartReferenceLinesConfiguration
        dataSetCategories={testTimePeriodDataSetCategories}
        axisDefinition={testMajorAxisDefinition}
        lines={[{ position: '2014_AY', label: 'Test label 1' }]}
        minorAxisDomain={testMinorAxisDomain}
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
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'filters',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        axisDefinition={testMajorAxisDefinition}
        lines={[
          { position: 'ethnicity-major-chinese', label: 'Test label 1' },
          { position: 'state-funded-secondary', label: 'Test label 2' },
        ]}
        minorAxisDomain={testMinorAxisDomain}
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
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'locations',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        axisDefinition={testMajorAxisDefinition}
        lines={[{ position: barnetId, label: 'Test label 1' }]}
        minorAxisDomain={testMinorAxisDomain}
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
    expect(options[1]).toHaveAttribute('value', barnsleyId);
  });

  test('renders correctly with existing lines when grouped by indicators', () => {
    render(
      <ChartReferenceLinesConfiguration
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'indicators',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        axisDefinition={testMajorAxisDefinition}
        lines={[
          { position: 'overall-absence-sessions', label: 'Test label 1' },
        ]}
        minorAxisDomain={testMinorAxisDomain}
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
        dataSetCategories={[]}
        axisDefinition={testMinorAxisDefinition}
        lines={[
          { position: 2000, label: 'Test label 1' },
          { position: 4000, label: 'Test label 2' },
        ]}
        minorAxisDomain={testMinorAxisDomain}
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
        dataSetCategories={testTimePeriodDataSetCategories}
        axisDefinition={testMajorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
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

  test('shows validation error for a minor axis line when `Position` is not valid', async () => {
    render(
      <ChartReferenceLinesConfiguration
        dataSetCategories={testTimePeriodDataSetCategories}
        axisDefinition={testMinorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
        onAddLine={noop}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );

    expect(
      referenceLines.queryByText(
        'Enter a position within the Y axis min/max range',
      ),
    ).not.toBeInTheDocument();

    userEvent.type(referenceLines.getByLabelText('Position'), '50500');
    userEvent.tab();

    await waitFor(() => {
      expect(
        referenceLines.getByText(
          'Enter a position within the Y axis min/max range',
        ),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error for a major axis line when `otherAxisPosition` is not valid', async () => {
    render(
      <ChartReferenceLinesConfiguration
        dataSetCategories={testTimePeriodDataSetCategories}
        axisDefinition={testMajorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
        onAddLine={noop}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );

    expect(
      referenceLines.queryByText(
        'Enter a position within the Y axis min/max range',
      ),
    ).not.toBeInTheDocument();

    userEvent.type(referenceLines.getByLabelText('Y axis position'), '50500');
    userEvent.tab();

    await waitFor(() => {
      expect(
        referenceLines.getByText(
          'Enter a position within the Y axis min/max range',
        ),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error for a minor axis line when `otherAxisPosition` is not valid', async () => {
    render(
      <ChartReferenceLinesConfiguration
        dataSetCategories={testTimePeriodDataSetCategories}
        axisDefinition={testMinorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
        onAddLine={noop}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );

    expect(
      referenceLines.queryByText('Enter a percentage between 0 and 100%'),
    ).not.toBeInTheDocument();

    userEvent.type(referenceLines.getByLabelText('X axis position'), '101');
    userEvent.tab();

    await waitFor(() => {
      expect(
        referenceLines.getByText('Enter a percentage between 0 and 100%'),
      ).toBeInTheDocument();
    });
  });

  test('shows validation error when `Label` is empty', async () => {
    render(
      <ChartReferenceLinesConfiguration
        dataSetCategories={testTimePeriodDataSetCategories}
        axisDefinition={testMajorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
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
        dataSetCategories={testTimePeriodDataSetCategories}
        axisDefinition={testMajorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
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
        dataSetCategories={testTimePeriodDataSetCategories}
        axisDefinition={testMajorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
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
        dataSetCategories={testTimePeriodDataSetCategories}
        axisDefinition={{
          ...testMajorAxisDefinition,
          referenceLineDefaults: {
            style: 'none',
          },
        }}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
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
        dataSetCategories={testTimePeriodDataSetCategories}
        axisDefinition={testMajorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
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

    userEvent.type(referenceLines.getByLabelText('Label'), 'Test label');

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
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'filters',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        axisDefinition={testMajorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
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

    userEvent.type(referenceLines.getByLabelText('Label'), 'Test label');

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
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'locations',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        axisDefinition={testMajorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
        onAddLine={handleAddLine}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    expect(referenceLines.getAllByRole('row')).toHaveLength(2);

    userEvent.selectOptions(
      referenceLines.getByLabelText('Position'),
      barnsleyId,
    );

    userEvent.type(referenceLines.getByLabelText('Label'), 'Test label');

    userEvent.click(screen.getByRole('button', { name: 'Add line' }));

    await waitFor(() => {
      expect(handleAddLine).toHaveBeenCalledTimes(1);
      expect(handleAddLine).toHaveBeenCalledWith<
        Parameters<ChartReferenceLinesConfigurationProps['onAddLine']>
      >({
        label: 'Test label',
        position: barnsleyId,
        style: 'dashed',
      });
    });
  });

  test('adding reference line when grouped by indicators', async () => {
    const handleAddLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'indicators',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        axisDefinition={testMajorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
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

    userEvent.type(referenceLines.getByLabelText('Label'), 'Test label');

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
        dataSetCategories={[]}
        axisDefinition={testMinorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
        onAddLine={handleAddLine}
        onRemoveLine={noop}
      />,
    );

    const referenceLines = within(
      screen.getByRole('table', { name: 'Reference lines' }),
    );
    expect(referenceLines.getAllByRole('row')).toHaveLength(2);

    userEvent.type(referenceLines.getByLabelText('Position'), '3000');
    userEvent.type(referenceLines.getByLabelText('Label'), 'Test label');

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
        dataSetCategories={testTimePeriodDataSetCategories}
        axisDefinition={testMajorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
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
    userEvent.type(referenceLines.getByLabelText('Label'), 'Test label');

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
        dataSetCategories={testTimePeriodDataSetCategories}
        axisDefinition={testMajorAxisDefinition}
        lines={[
          { position: '2014_AY', label: 'Test label 1' },
          { position: '2015_AY', label: 'Test label 1' },
        ]}
        minorAxisDomain={testMinorAxisDomain}
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
        dataSetCategories={testTimePeriodDataSetCategories}
        axisDefinition={testMajorAxisDefinition}
        lines={[
          { position: '2014_AY', label: 'Test label 1' },
          { position: '2015_AY', label: 'Test label 2' },
        ]}
        minorAxisDomain={testMinorAxisDomain}
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
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'filters',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        axisDefinition={testMajorAxisDefinition}
        lines={[
          { position: 'state-funded-primary', label: 'Test label 1' },
          { position: 'state-funded-secondary', label: 'Test label 2' },
          { position: 'ethnicity-major-chinese', label: 'Test label 3' },
        ]}
        minorAxisDomain={testMinorAxisDomain}
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
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'locations',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        axisDefinition={testMajorAxisDefinition}
        lines={[
          { position: barnetId, label: 'Test label 1' },
          { position: barnsleyId, label: 'Test label 2' },
        ]}
        minorAxisDomain={testMinorAxisDomain}
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
        position: barnsleyId,
      });
    });
  });

  test('removing reference line when grouped by indicators', async () => {
    const handleRemoveLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        dataSetCategories={createDataSetCategories(
          {
            ...testAxisConfiguration,
            groupBy: 'indicators',
          },
          testTable.results,
          testTable.subjectMeta,
        )}
        axisDefinition={testMajorAxisDefinition}
        lines={[
          { position: 'authorised-absence-sessions', label: 'Test label 1' },
          { position: 'overall-absence-sessions', label: 'Test label 2' },
        ]}
        minorAxisDomain={testMinorAxisDomain}
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
        dataSetCategories={[]}
        axisDefinition={testMinorAxisDefinition}
        lines={[
          { position: 1000, label: 'Test label 1' },
          { position: 2000, label: 'Test label 2' },
          { position: 3000, label: 'Test label 2' },
        ]}
        minorAxisDomain={testMinorAxisDomain}
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

  test('setting the other axis position on a major axis reference line', async () => {
    const handleAddLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        dataSetCategories={testTimePeriodDataSetCategories}
        axisDefinition={testMajorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
        onAddLine={handleAddLine}
        onRemoveLine={noop}
      />,
    );

    userEvent.selectOptions(screen.getByLabelText('Position'), '2015_AY');
    userEvent.type(screen.getByLabelText('Label'), 'Test label');
    userEvent.type(screen.getByLabelText('Y axis position'), '20000');

    expect(handleAddLine).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Add line' }));

    await waitFor(() => {
      expect(handleAddLine).toHaveBeenCalledTimes(1);
      expect(handleAddLine).toHaveBeenCalledWith<
        Parameters<ChartReferenceLinesConfigurationProps['onAddLine']>
      >({
        label: 'Test label',
        otherAxisPosition: 20000,
        position: '2015_AY',
        style: 'dashed',
      });
    });
  });

  test('setting the other axis position on a minor axis reference line', async () => {
    const handleAddLine = jest.fn();

    render(
      <ChartReferenceLinesConfiguration
        dataSetCategories={[]}
        axisDefinition={testMinorAxisDefinition}
        lines={[]}
        minorAxisDomain={testMinorAxisDomain}
        onAddLine={handleAddLine}
        onRemoveLine={noop}
      />,
    );

    userEvent.type(screen.getByLabelText('Position'), '40000');
    userEvent.type(screen.getByLabelText('Label'), 'Test label');
    userEvent.type(screen.getByLabelText('X axis position'), '75');

    expect(handleAddLine).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Add line' }));

    await waitFor(() => {
      expect(handleAddLine).toHaveBeenCalledTimes(1);
      expect(handleAddLine).toHaveBeenCalledWith<
        Parameters<ChartReferenceLinesConfigurationProps['onAddLine']>
      >({
        label: 'Test label',
        otherAxisPosition: 75,
        position: 40000,
        style: 'dashed',
      });
    });
  });
});
