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
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React, { ReactNode } from 'react';
import FormProvider from '@common/components/form/FormProvider';
import baseRender from '@common-test/render';

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
  };
  const testMinorAxisDefinition: ChartDefinitionAxis = {
    axis: 'y',
    id: 'yaxis',
    title: 'Y Axis (minor axis)',
    type: 'minor',
  };

  const testTable = testFullTable;
  const testTimePeriodDataSetCategories = createDataSetCategories({
    axisConfiguration: {
      ...testAxisConfiguration,
      groupBy: 'timePeriod',
    },
    data: testTable.results,
    meta: testTable.subjectMeta,
  });

  const barnsleyId = LocationFilter.createId({
    level: 'localAuthority',
    value: 'barnsley',
  });
  const barnetId = LocationFilter.createId({
    level: 'localAuthority',
    value: 'barnet',
  });

  describe('major axis', () => {
    describe('initial reference lines', () => {
      test('renders correctly with initial reference lines', () => {
        render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[
              { position: '2014_AY', label: 'Test label 1' },
              { position: '2015_AY', label: 'Test label 2' },
            ]}
            onSubmit={noop}
          />,
        );

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );
        const rows = referenceLines.getAllByRole('row');
        expect(rows).toHaveLength(3);

        expect(within(rows[1]).getByText('2014/15')).toBeInTheDocument();
        expect(within(rows[1]).getByText('Test label 1')).toBeInTheDocument();
        expect(
          within(rows[1]).getByRole('button', { name: 'Edit line' }),
        ).toBeInTheDocument();
        expect(
          within(rows[1]).getByRole('button', { name: 'Remove line' }),
        ).toBeInTheDocument();

        expect(within(rows[2]).getByText('2015/16')).toBeInTheDocument();
        expect(within(rows[2]).getByText('Test label 2')).toBeInTheDocument();
        expect(
          within(rows[2]).getByRole('button', { name: 'Edit line' }),
        ).toBeInTheDocument();
        expect(
          within(rows[2]).getByRole('button', { name: 'Remove line' }),
        ).toBeInTheDocument();
      });

      test('filters out reference lines from a different axis grouping', () => {
        render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[
              { position: '2014_AY', label: 'Test label 1' },
              { position: 'ethnicity-major-chinese', label: 'Test label 1' },
              { position: '2015_AY', label: 'Test label 2' },
            ]}
            onSubmit={noop}
          />,
        );

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );
        const rows = referenceLines.getAllByRole('row');
        expect(rows).toHaveLength(3);

        expect(within(rows[1]).getByText('2014/15')).toBeInTheDocument();
        expect(within(rows[1]).getByText('Test label 1')).toBeInTheDocument();
        expect(
          within(rows[1]).getByRole('button', { name: 'Edit line' }),
        ).toBeInTheDocument();
        expect(
          within(rows[1]).getByRole('button', { name: 'Remove line' }),
        ).toBeInTheDocument();

        expect(within(rows[2]).getByText('2015/16')).toBeInTheDocument();
        expect(within(rows[2]).getByText('Test label 2')).toBeInTheDocument();
        expect(
          within(rows[2]).getByRole('button', { name: 'Edit line' }),
        ).toBeInTheDocument();
        expect(
          within(rows[2]).getByRole('button', { name: 'Remove line' }),
        ).toBeInTheDocument();
      });
    });

    describe('adding reference lines', () => {
      test('renders the correct position options when grouped by time periods', async () => {
        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[{ position: '2014_AY', label: 'Test label 1' }]}
            onSubmit={noop}
          />,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const position = screen.getByLabelText('Position');
        const options = within(position).getAllByRole('option');

        expect(options).toHaveLength(2);
        expect(options[0]).toHaveTextContent('Choose position');
        expect(options[0]).toHaveAttribute('value', '');
        expect(options[1]).toHaveTextContent('2015/16');
        expect(options[1]).toHaveAttribute('value', '2015_AY');
      });

      test('renders the correct position options when grouped by filters', async () => {
        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={createDataSetCategories({
              axisConfiguration: {
                ...testAxisConfiguration,
                groupBy: 'filters',
              },
              data: testTable.results,
              meta: testTable.subjectMeta,
            })}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[
              { position: 'ethnicity-major-chinese', label: 'Test label 1' },
              { position: 'state-funded-secondary', label: 'Test label 2' },
            ]}
            onSubmit={noop}
          />,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const position = screen.getByLabelText('Position');
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

      test('renders the correct position options when grouped by locations', async () => {
        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={createDataSetCategories({
              axisConfiguration: {
                ...testAxisConfiguration,
                groupBy: 'locations',
              },
              data: testTable.results,
              meta: testTable.subjectMeta,
            })}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[{ position: barnetId, label: 'Test label 1' }]}
            onSubmit={noop}
          />,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const position = screen.getByLabelText('Position');
        const options = within(position).getAllByRole('option');

        expect(options).toHaveLength(2);
        expect(options[0]).toHaveTextContent('Choose position');
        expect(options[0]).toHaveAttribute('value', '');
        expect(options[1]).toHaveTextContent('Barnsley');
        expect(options[1]).toHaveAttribute('value', barnsleyId);
      });

      test('renders the correct position options when grouped by indicators', async () => {
        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={createDataSetCategories({
              axisConfiguration: {
                ...testAxisConfiguration,
                groupBy: 'indicators',
              },
              data: testTable.results,
              meta: testTable.subjectMeta,
            })}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[
              { position: 'overall-absence-sessions', label: 'Test label 1' },
            ]}
            onSubmit={noop}
          />,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const position = screen.getByLabelText('Position');
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

      test('adding a reference line when grouped by time periods', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[]}
            onSubmit={handleSubmit}
          />,
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

        expect(handleSubmit).not.toHaveBeenCalled();

        await user.click(screen.getByRole('button', { name: 'Add' }));

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
          expect(handleSubmit).toHaveBeenCalledWith<
            Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
          >([
            {
              label: 'Test label',
              position: '2014_AY',
              style: 'dashed',
            },
          ]);
        });
      });

      test('adding a reference line when grouped by filters', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={createDataSetCategories({
              axisConfiguration: {
                ...testAxisConfiguration,
                groupBy: 'filters',
              },
              data: testTable.results,
              meta: testTable.subjectMeta,
            })}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[]}
            onSubmit={handleSubmit}
          />,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );
        expect(referenceLines.getAllByRole('row')).toHaveLength(2);

        await user.selectOptions(referenceLines.getByLabelText('Position'), [
          'state-funded-primary',
        ]);

        await user.type(referenceLines.getByLabelText('Label'), 'Test label');

        await user.click(screen.getByRole('button', { name: 'Add' }));

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
          expect(handleSubmit).toHaveBeenCalledWith<
            Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
          >([
            {
              label: 'Test label',
              position: 'state-funded-primary',
              style: 'dashed',
            },
          ]);
        });
      });

      test('adding a reference line when grouped by locations', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={createDataSetCategories({
              axisConfiguration: {
                ...testAxisConfiguration,
                groupBy: 'locations',
              },
              data: testTable.results,
              meta: testTable.subjectMeta,
            })}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[]}
            onSubmit={handleSubmit}
          />,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );
        expect(referenceLines.getAllByRole('row')).toHaveLength(2);

        await user.selectOptions(
          referenceLines.getByLabelText('Position'),
          barnsleyId,
        );

        await user.type(referenceLines.getByLabelText('Label'), 'Test label');

        await user.click(screen.getByRole('button', { name: 'Add' }));

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
          expect(handleSubmit).toHaveBeenCalledWith<
            Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
          >([
            {
              label: 'Test label',
              position: barnsleyId,
              style: 'dashed',
            },
          ]);
        });
      });

      test('adding a reference line when grouped by indicators', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={createDataSetCategories({
              axisConfiguration: {
                ...testAxisConfiguration,
                groupBy: 'indicators',
              },
              data: testTable.results,
              meta: testTable.subjectMeta,
            })}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[]}
            onSubmit={handleSubmit}
          />,
        );
        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );
        expect(referenceLines.getAllByRole('row')).toHaveLength(2);

        await user.selectOptions(referenceLines.getByLabelText('Position'), [
          'authorised-absence-sessions',
        ]);

        await user.type(referenceLines.getByLabelText('Label'), 'Test label');

        await user.click(screen.getByRole('button', { name: 'Add' }));

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
          expect(handleSubmit).toHaveBeenCalledWith<
            Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
          >([
            {
              label: 'Test label',
              position: 'authorised-absence-sessions',
              style: 'dashed',
            },
          ]);
        });
      });

      test('adding a reference line with non-default style', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[]}
            onSubmit={handleSubmit}
          />,
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

        expect(referenceLines.getByLabelText('Style')).toHaveValue('dashed');

        await user.selectOptions(referenceLines.getByLabelText('Style'), [
          'none',
        ]);

        expect(handleSubmit).not.toHaveBeenCalled();

        await user.click(screen.getByRole('button', { name: 'Add' }));

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
          expect(handleSubmit).toHaveBeenCalledWith<
            Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
          >([
            {
              label: 'Test label',
              position: '2014_AY',
              style: 'none',
            },
          ]);
        });
      });

      test('adding a reference line with a label width', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[]}
            onSubmit={handleSubmit}
          />,
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

        await user.type(referenceLines.getByLabelText('Label width'), '99');

        expect(handleSubmit).not.toHaveBeenCalled();

        await user.click(screen.getByRole('button', { name: 'Add' }));

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
          expect(handleSubmit).toHaveBeenCalledWith<
            Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
          >([
            {
              label: 'Test label',
              labelWidth: 99,
              position: '2014_AY',
              style: 'dashed',
            },
          ]);
        });
      });

      test('setting the other axis position on a major axis reference line', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[]}
            onSubmit={handleSubmit}
          />,
        );
        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        await user.selectOptions(screen.getByLabelText('Position'), '2015_AY');
        await user.type(screen.getByLabelText('Label'), 'Test label');
        await user.type(screen.getByLabelText('Y axis position'), '20000');
        await user.selectOptions(screen.getByLabelText('Style'), 'dashed');

        expect(handleSubmit).not.toHaveBeenCalled();

        await user.click(screen.getByRole('button', { name: 'Add' }));

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
          expect(handleSubmit).toHaveBeenCalledWith<
            Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
          >([
            {
              label: 'Test label',
              labelWidth: undefined,
              otherAxisPosition: 20000,
              position: '2015_AY',
              style: 'dashed',
            },
          ]);
        });
      });
    });

    describe('reference lines between data points on the major axis', () => {
      test('does not show the between data points option on horizontal bar charts', async () => {
        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="horizontalbar"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[]}
            onSubmit={noop}
          />,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const positionOptions = within(
          screen.getByLabelText('Position'),
        ).getAllByRole('option');
        expect(positionOptions).toHaveLength(3);
        expect(positionOptions[0]).toHaveTextContent('Choose position');
        expect(positionOptions[0]).toHaveAttribute('value', '');
        expect(positionOptions[1]).toHaveTextContent('2014/15');
        expect(positionOptions[1]).toHaveAttribute('value', '2014_AY');
        expect(positionOptions[2]).toHaveTextContent('2015/16');
        expect(positionOptions[2]).toHaveAttribute('value', '2015_AY');
      });

      test('does not show the between data points option on line bar charts', async () => {
        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[]}
            onSubmit={noop}
          />,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const positionOptions = within(
          screen.getByLabelText('Position'),
        ).getAllByRole('option');
        expect(positionOptions).toHaveLength(3);
        expect(positionOptions[0]).toHaveTextContent('Choose position');
        expect(positionOptions[0]).toHaveAttribute('value', '');
        expect(positionOptions[1]).toHaveTextContent('2014/15');
        expect(positionOptions[1]).toHaveAttribute('value', '2014_AY');
        expect(positionOptions[2]).toHaveTextContent('2015/16');
        expect(positionOptions[2]).toHaveAttribute('value', '2015_AY');
      });

      test('shows the between data points option on vertical bar charts', async () => {
        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="verticalbar"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[]}
            onSubmit={noop}
          />,
        );
        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const positionOptions = within(
          screen.getByLabelText('Position'),
        ).getAllByRole('option');
        expect(positionOptions).toHaveLength(4);
        expect(positionOptions[0]).toHaveTextContent('Choose position');
        expect(positionOptions[0]).toHaveAttribute('value', '');
        expect(positionOptions[1]).toHaveTextContent('2014/15');
        expect(positionOptions[1]).toHaveAttribute('value', '2014_AY');
        expect(positionOptions[2]).toHaveTextContent('2015/16');
        expect(positionOptions[2]).toHaveAttribute('value', '2015_AY');
        expect(positionOptions[3]).toHaveTextContent('Between data points');
        expect(positionOptions[3]).toHaveAttribute(
          'value',
          'between-data-points',
        );
      });

      test('adding a reference line between two data points', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="verticalbar"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[]}
            onSubmit={handleSubmit}
          />,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );
        expect(referenceLines.getAllByRole('row')).toHaveLength(2);

        await user.selectOptions(referenceLines.getByLabelText('Position'), [
          'between-data-points',
        ]);

        await user.selectOptions(referenceLines.getByLabelText('Start point'), [
          '2014_AY',
        ]);

        await user.selectOptions(referenceLines.getByLabelText('End point'), [
          '2015_AY',
        ]);

        await user.type(
          referenceLines.getByLabelText('Y axis position'),
          '3000',
        );

        await user.type(referenceLines.getByLabelText('Label'), 'Test label');

        expect(handleSubmit).not.toHaveBeenCalled();

        await user.click(screen.getByRole('button', { name: 'Add' }));

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
          expect(handleSubmit).toHaveBeenCalledWith<
            Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
          >([
            {
              label: 'Test label',
              labelWidth: undefined,
              otherAxisEnd: '2015_AY',
              otherAxisStart: '2014_AY',
              otherAxisPosition: 3000,
              position: 'between-data-points',
              style: 'dashed',
            },
          ]);
        });
      });

      test('renders correctly with existing lines between two data points', () => {
        render(
          <ChartReferenceLinesConfiguration
            chartType="verticalbar"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[
              {
                otherAxisEnd: '2015_AY',
                label: 'Test label 1',
                position: 'between-data-points',
                otherAxisStart: '2014_AY',
                otherAxisPosition: 3000,
              },
            ]}
            onSubmit={noop}
          />,
        );

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );

        const rows = referenceLines.getAllByRole('row');
        expect(rows).toHaveLength(2);
        expect(
          within(rows[1]).getByText('2014/15 - 2015/16'),
        ).toBeInTheDocument();
        expect(within(rows[1]).getByText('3000')).toBeInTheDocument();
        expect(within(rows[1]).getByText('Test label 1')).toBeInTheDocument();
      });
    });

    describe('removing reference lines', () => {
      test('removing a reference line when grouped by time periods', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[
              { position: '2014_AY', label: 'Test label 1' },
              { position: '2015_AY', label: 'Test label 2' },
            ]}
            onSubmit={handleSubmit}
          />,
        );

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );
        const rows = referenceLines.getAllByRole('row');
        expect(rows).toHaveLength(3);

        expect(handleSubmit).not.toHaveBeenCalled();

        await user.click(
          within(rows[1]).getByRole('button', { name: 'Remove line' }),
        );

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
        });

        expect(handleSubmit).toHaveBeenCalledWith<
          Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
        >([{ position: '2015_AY', label: 'Test label 2' }]);

        expect(referenceLines.getAllByRole('row')).toHaveLength(2);
      });

      test('removing a reference line when grouped by filters', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={createDataSetCategories({
              axisConfiguration: {
                ...testAxisConfiguration,
                groupBy: 'filters',
              },
              data: testTable.results,
              meta: testTable.subjectMeta,
            })}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[
              { position: 'state-funded-primary', label: 'Test label 1' },
              { position: 'state-funded-secondary', label: 'Test label 2' },
              { position: 'ethnicity-major-chinese', label: 'Test label 3' },
            ]}
            onSubmit={handleSubmit}
          />,
        );

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );
        const rows = referenceLines.getAllByRole('row');
        expect(rows).toHaveLength(4);

        expect(handleSubmit).not.toHaveBeenCalled();

        await user.click(
          within(rows[2]).getByRole('button', { name: 'Remove line' }),
        );

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
        });

        expect(handleSubmit).toHaveBeenCalledWith<
          Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
        >([
          { position: 'state-funded-primary', label: 'Test label 1' },
          { position: 'ethnicity-major-chinese', label: 'Test label 3' },
        ]);

        expect(referenceLines.getAllByRole('row')).toHaveLength(3);
      });

      test('removing a reference line when grouped by locations', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={createDataSetCategories({
              axisConfiguration: {
                ...testAxisConfiguration,
                groupBy: 'locations',
              },
              data: testTable.results,
              meta: testTable.subjectMeta,
            })}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[
              { position: barnetId, label: 'Test label 1' },
              { position: barnsleyId, label: 'Test label 2' },
            ]}
            onSubmit={handleSubmit}
          />,
        );

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );
        const rows = referenceLines.getAllByRole('row');
        expect(rows).toHaveLength(3);

        expect(handleSubmit).not.toHaveBeenCalled();

        await user.click(
          within(rows[2]).getByRole('button', { name: 'Remove line' }),
        );

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
        });

        expect(handleSubmit).toHaveBeenCalledWith<
          Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
        >([{ position: barnetId, label: 'Test label 1' }]);

        expect(referenceLines.getAllByRole('row')).toHaveLength(2);
      });

      test('removing a reference line when grouped by indicators', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={createDataSetCategories({
              axisConfiguration: {
                ...testAxisConfiguration,
                groupBy: 'indicators',
              },
              data: testTable.results,
              meta: testTable.subjectMeta,
            })}
            axisDefinition={testMajorAxisDefinition}
            referenceLines={[
              {
                position: 'authorised-absence-sessions',
                label: 'Test label 1',
              },
              { position: 'overall-absence-sessions', label: 'Test label 2' },
            ]}
            onSubmit={handleSubmit}
          />,
        );

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );
        const rows = referenceLines.getAllByRole('row');
        expect(rows).toHaveLength(3);

        expect(handleSubmit).not.toHaveBeenCalled();

        await user.click(
          within(rows[2]).getByRole('button', { name: 'Remove line' }),
        );

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
        });

        expect(handleSubmit).toHaveBeenCalledWith<
          Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
        >([{ position: 'authorised-absence-sessions', label: 'Test label 1' }]);

        expect(referenceLines.getAllByRole('row')).toHaveLength(2);
      });
    });

    test('default value for `Style` field is dashed', async () => {
      const { user } = render(
        <ChartReferenceLinesConfiguration
          chartType="line"
          dataSetCategories={testTimePeriodDataSetCategories}
          axisDefinition={testMajorAxisDefinition}
          referenceLines={[]}
          onSubmit={noop}
        />,
      );

      await user.click(screen.getByRole('button', { name: 'Add new line' }));

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );
      expect(referenceLines.getAllByRole('row')).toHaveLength(2);

      expect(referenceLines.getByLabelText('Style')).toHaveValue('dashed');
    });

    test('can set default value for reference line `Style` field via chart definition', async () => {
      const { user } = render(
        <ChartReferenceLinesConfiguration
          chartType="line"
          dataSetCategories={testTimePeriodDataSetCategories}
          axisDefinition={{
            ...testMajorAxisDefinition,
            referenceLineDefaults: {
              style: 'none',
            },
          }}
          referenceLines={[]}
          onSubmit={noop}
        />,
      );

      await user.click(screen.getByRole('button', { name: 'Add new line' }));

      const referenceLines = within(
        screen.getByRole('table', { name: 'Reference lines' }),
      );
      expect(referenceLines.getAllByRole('row')).toHaveLength(2);

      expect(referenceLines.getByLabelText('Style')).toHaveValue('none');
    });
  });

  describe('minor axis', () => {
    describe('initial reference lines', () => {
      test('renders correctly with initial reference lines', () => {
        render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={[]}
            axisDefinition={testMinorAxisDefinition}
            referenceLines={[
              { position: 2000, label: 'Test label 1' },
              { position: 4000, label: 'Test label 2' },
            ]}
            onSubmit={noop}
          />,
        );

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );

        const rows = referenceLines.getAllByRole('row');

        expect(rows).toHaveLength(3);
        expect(within(rows[1]).getByText('2000')).toBeInTheDocument();
        expect(within(rows[1]).getByText('Test label 1')).toBeInTheDocument();
        expect(within(rows[2]).getByText('4000')).toBeInTheDocument();
        expect(within(rows[2]).getByText('Test label 2')).toBeInTheDocument();
      });
    });

    describe('adding reference lines', () => {
      test('adding reference line for minor axis', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={[]}
            axisDefinition={testMinorAxisDefinition}
            referenceLines={[]}
            onSubmit={handleSubmit}
          />,
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
          expect(handleSubmit).toHaveBeenCalledTimes(1);
          expect(handleSubmit).toHaveBeenCalledWith<
            Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
          >([
            {
              label: 'Test label',
              position: 3000,
              style: 'dashed',
            },
          ]);
        });
      });

      test('setting a custom other axis position on a minor axis reference line', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={[]}
            axisDefinition={testMinorAxisDefinition}
            referenceLines={[]}
            onSubmit={handleSubmit}
          />,
        );
        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        await user.type(screen.getByLabelText('Position'), '40000');
        await user.type(screen.getByLabelText('Label'), 'Test label');
        await user.selectOptions(screen.getByLabelText('X axis position'), [
          'custom',
        ]);

        await user.type(screen.getByLabelText(/Percent/), '75');

        expect(handleSubmit).not.toHaveBeenCalled();

        await user.click(screen.getByRole('button', { name: 'Add' }));

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
          expect(handleSubmit).toHaveBeenCalledWith<
            Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
          >([
            {
              label: 'Test label',
              otherAxisPosition: 75,
              position: 40000,
              style: 'dashed',
            },
          ]);
        });
      });
    });

    describe('reference lines between data points on the minor axis', () => {
      test('adding a reference line between two data points', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMinorAxisDefinition}
            referenceLines={[]}
            onSubmit={handleSubmit}
          />,
        );

        await user.click(screen.getByRole('button', { name: 'Add new line' }));

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );

        await user.type(referenceLines.getByLabelText('Position'), '3000');

        await user.selectOptions(
          referenceLines.getByLabelText('X axis position'),
          ['between-data-points'],
        );

        await user.selectOptions(referenceLines.getByLabelText('Start point'), [
          '2014_AY',
        ]);

        await user.selectOptions(referenceLines.getByLabelText('End point'), [
          '2015_AY',
        ]);

        await user.type(referenceLines.getByLabelText('Label'), 'Test label');

        expect(handleSubmit).not.toHaveBeenCalled();

        await user.click(screen.getByRole('button', { name: 'Add' }));

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
          expect(handleSubmit).toHaveBeenCalledWith<
            Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
          >([
            {
              label: 'Test label',
              otherAxisEnd: '2015_AY',
              otherAxisStart: '2014_AY',
              position: 3000,
              style: 'dashed',
            },
          ]);
        });
      });

      test('renders correctly with existing lines between two data points', () => {
        render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={testTimePeriodDataSetCategories}
            axisDefinition={testMinorAxisDefinition}
            referenceLines={[
              {
                otherAxisEnd: '2015_AY',
                label: 'Test label 1',
                position: '50',
                otherAxisStart: '2014_AY',
              },
            ]}
            onSubmit={noop}
          />,
        );

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );

        const rows = referenceLines.getAllByRole('row');
        expect(rows).toHaveLength(2);
        expect(within(rows[1]).getByText('50')).toBeInTheDocument();
        expect(
          within(rows[1]).getByText('2014/15 - 2015/16'),
        ).toBeInTheDocument();
        expect(within(rows[1]).getByText('Test label 1')).toBeInTheDocument();
      });
    });

    describe('removing reference lines', () => {
      test('removing a reference line for minor axis', async () => {
        const handleSubmit = jest.fn();

        const { user } = render(
          <ChartReferenceLinesConfiguration
            chartType="line"
            dataSetCategories={[]}
            axisDefinition={testMinorAxisDefinition}
            referenceLines={[
              { position: 1000, label: 'Test label 1' },
              { position: 2000, label: 'Test label 2' },
              { position: 3000, label: 'Test label 3' },
            ]}
            onSubmit={handleSubmit}
          />,
        );

        const referenceLines = within(
          screen.getByRole('table', { name: 'Reference lines' }),
        );
        const rows = referenceLines.getAllByRole('row');
        expect(rows).toHaveLength(4);

        expect(handleSubmit).not.toHaveBeenCalled();

        await user.click(
          within(rows[2]).getByRole('button', { name: 'Remove line' }),
        );

        await waitFor(() => {
          expect(handleSubmit).toHaveBeenCalledTimes(1);
        });

        expect(handleSubmit).toHaveBeenCalledWith<
          Parameters<ChartReferenceLinesConfigurationProps['onSubmit']>
        >([
          { position: 1000, label: 'Test label 1' },
          { position: 3000, label: 'Test label 3' },
        ]);

        expect(referenceLines.getAllByRole('row')).toHaveLength(3);
      });
    });
  });

  function render(children: ReactNode) {
    return baseRender(<FormProvider>{children}</FormProvider>);
  }
});
