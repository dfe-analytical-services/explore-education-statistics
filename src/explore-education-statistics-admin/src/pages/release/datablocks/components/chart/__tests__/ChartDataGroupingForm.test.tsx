import ChartDataGroupingForm from '@admin/pages/release/datablocks/components/chart/ChartDataGroupingForm';
import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import render from '@common-test/render';
import { MapDataSetConfig } from '@common/modules/charts/types/chart';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('ChartDataGroupingForm', () => {
  const testTable = testFullTable;

  const testDataSetConfigs: MapDataSetConfig[] = [
    {
      dataSet: {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        indicator: 'authorised-absence-sessions',
        timePeriod: '2014_AY',
      },
      dataSetKey: 'dataSetKey1',
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
      dataSetKey: 'dataSetKey2',
      dataGrouping: {
        customGroups: [],
        numberOfGroups: 4,
        type: 'Quantiles',
      },
    },
    {
      dataSet: {
        filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
        indicator: 'authorised-absence-sessions',
        timePeriod: '2014_AY',
      },
      dataSetKey: 'dataSetKey3',
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
  ];

  test('renders correctly when editing a grouping with equal intervals', async () => {
    render(
      <ChartDataGroupingForm
        dataSetConfig={testDataSetConfigs[0]}
        dataSetConfigs={testDataSetConfigs}
        meta={testTable.subjectMeta}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Equal intervals')).toBeChecked();
    expect(
      screen.getAllByLabelText('Number of data groups')[0],
    ).toHaveNumericValue(5);

    expect(screen.getByLabelText('Quantiles')).not.toBeChecked();
    expect(screen.getByLabelText('New custom groups')).not.toBeChecked();

    expect(screen.getByRole('button', { name: 'Done' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('renders correctly when editing a grouping with quantiles', async () => {
    render(
      <ChartDataGroupingForm
        dataSetConfig={testDataSetConfigs[1]}
        dataSetConfigs={testDataSetConfigs}
        meta={testTable.subjectMeta}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Quantiles')).toBeChecked();
    expect(
      screen.getAllByLabelText('Number of data groups')[1],
    ).toHaveNumericValue(4);

    expect(screen.getByLabelText('Equal intervals')).not.toBeChecked();
    expect(screen.getByLabelText('New custom groups')).not.toBeChecked();

    expect(screen.getByRole('button', { name: 'Done' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('renders correctly when editing a grouping with custom groups', async () => {
    render(
      <ChartDataGroupingForm
        dataSetConfig={testDataSetConfigs[2]}
        dataSetConfigs={testDataSetConfigs}
        meta={testTable.subjectMeta}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('New custom groups')).toBeChecked();
    const customGroupsRows = screen.getAllByRole('row');
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

    expect(screen.getByLabelText('Equal intervals')).not.toBeChecked();
    expect(screen.getByLabelText('Quantiles')).not.toBeChecked();

    expect(screen.getByRole('button', { name: 'Done' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument();
  });

  test('changing a data grouping to quantiles', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ChartDataGroupingForm
        dataSetConfig={testDataSetConfigs[0]}
        dataSetConfigs={testDataSetConfigs}
        meta={testTable.subjectMeta}
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.click(screen.getByLabelText('Quantiles'));
    await user.clear(screen.getAllByLabelText('Number of data groups')[1]);
    await user.type(screen.getAllByLabelText('Number of data groups')[1], '3');

    await user.click(screen.getByRole('button', { name: 'Done' }));

    const expectedValues: MapDataSetConfig = {
      dataSet: {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        indicator: 'authorised-absence-sessions',
        timePeriod: '2014_AY',
      },
      dataSetKey: 'dataSetKey1',
      dataGrouping: {
        customGroups: [],
        numberOfGroups: 3,
        type: 'Quantiles',
      },
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
    });
  });

  test('changing a data grouping to custom groups', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ChartDataGroupingForm
        dataSetConfig={testDataSetConfigs[0]}
        dataSetConfigs={testDataSetConfigs}
        meta={testTable.subjectMeta}
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.click(screen.getByLabelText('New custom groups'));
    await user.type(screen.getByLabelText('Min'), '0');
    await user.type(screen.getByLabelText('Max'), '10');
    await user.click(screen.getByRole('button', { name: 'Add group' }));

    await waitFor(() => {
      expect(screen.getByText('Remove group')).toBeInTheDocument();
    });
    const customGroupsRows = screen.getAllByRole('row');
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

    await user.click(screen.getByRole('button', { name: 'Done' }));

    const expectedValues: MapDataSetConfig = {
      dataSet: {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        indicator: 'authorised-absence-sessions',
        timePeriod: '2014_AY',
      },
      dataSetKey: 'dataSetKey1',
      dataGrouping: {
        customGroups: [{ min: 0, max: 10 }],
        type: 'Custom',
      },
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
    });
  });

  test('editing existing custom groups', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ChartDataGroupingForm
        dataSetConfig={testDataSetConfigs[2]}
        dataSetConfigs={testDataSetConfigs}
        meta={testTable.subjectMeta}
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    const customGroupsRows = screen.getAllByRole('row');
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

    await user.click(
      within(customGroupsRows[2]).getByRole('button', { name: 'Remove group' }),
    );

    expect(screen.getAllByRole('row')).toHaveLength(3);
    expect(screen.queryByText('11')).not.toBeInTheDocument();
    expect(screen.queryByText('20')).not.toBeInTheDocument();

    await user.type(screen.getByLabelText('Min'), '11');
    await user.type(screen.getByLabelText('Max'), '100');
    await user.click(screen.getByRole('button', { name: 'Add group' }));

    await user.click(screen.getByRole('button', { name: 'Done' }));

    const expectedValues: MapDataSetConfig = {
      dataSet: {
        filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
        indicator: 'authorised-absence-sessions',
        timePeriod: '2014_AY',
      },
      dataSetKey: 'dataSetKey1',
      dataGrouping: {
        customGroups: [
          { min: 0, max: 10 },
          { min: 11, max: 100 },
        ],

        type: 'Custom',
      },
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
    });
  });

  test('copying custom groups from another data set', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ChartDataGroupingForm
        dataSetConfig={testDataSetConfigs[0]}
        dataSetConfigs={testDataSetConfigs}
        meta={testTable.subjectMeta}
        onCancel={noop}
        onSubmit={handleSubmit}
      />,
    );

    await user.click(screen.getByLabelText('Copy custom groups'));

    await user.selectOptions(
      screen.getByLabelText('Copy custom groups from another data set'),
      generateDataSetKey({
        filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
        indicator: 'authorised-absence-sessions',
        timePeriod: '2014_AY',
      }),
    );

    await user.click(screen.getByRole('button', { name: 'Done' }));

    const expectedValues: MapDataSetConfig = {
      dataSet: {
        filters: ['ethnicity-major-chinese', 'state-funded-primary'],
        indicator: 'authorised-absence-sessions',
        timePeriod: '2014_AY',
      },
      dataSetKey: 'dataSetKey1',
      dataGrouping: {
        customGroups: [
          { min: 0, max: 10 },
          { min: 11, max: 20 },
        ],
        type: 'Custom',
      },
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
    });
  });

  test('shows a validation error if no number of groups when using equal intervals', async () => {
    const { user } = render(
      <ChartDataGroupingForm
        dataSetConfig={testDataSetConfigs[0]}
        dataSetConfigs={testDataSetConfigs}
        meta={testTable.subjectMeta}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    await user.clear(screen.getAllByLabelText('Number of data groups')[0]);

    await user.click(screen.getByRole('button', { name: 'Done' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Enter a number of data groups' }),
    ).toHaveAttribute('href', '#chartDataGroupingForm-numberOfGroups');
  });

  test('shows a validation error if too many groups when using equal intervals', async () => {
    const { user } = render(
      <ChartDataGroupingForm
        dataSetConfig={testDataSetConfigs[0]}
        dataSetConfigs={testDataSetConfigs}
        meta={testTable.subjectMeta}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    await user.clear(screen.getAllByLabelText('Number of data groups')[0]);
    await user.type(screen.getAllByLabelText('Number of data groups')[0], '6');

    await user.click(screen.getByRole('button', { name: 'Done' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Maximum 5 data groups' }),
    ).toHaveAttribute('href', '#chartDataGroupingForm-numberOfGroups');
  });

  test('shows a validation error if no number of groups when using quantiles', async () => {
    const { user } = render(
      <ChartDataGroupingForm
        dataSetConfig={testDataSetConfigs[1]}
        dataSetConfigs={testDataSetConfigs}
        meta={testTable.subjectMeta}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    await user.click(screen.getByLabelText('Quantiles'));

    await user.clear(screen.getAllByLabelText('Number of data groups')[1]);

    await user.click(screen.getByRole('button', { name: 'Done' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Enter a number of data groups' }),
    ).toHaveAttribute('href', '#chartDataGroupingForm-numberOfGroupsQuantiles');
  });

  test('shows a validation error if too many groups when using quantiles', async () => {
    const { user } = render(
      <ChartDataGroupingForm
        dataSetConfig={testDataSetConfigs[1]}
        dataSetConfigs={testDataSetConfigs}
        meta={testTable.subjectMeta}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    await user.click(screen.getByLabelText('Quantiles'));

    await user.clear(screen.getAllByLabelText('Number of data groups')[1]);

    await user.type(screen.getAllByLabelText('Number of data groups')[1], '6');

    await user.click(screen.getByRole('button', { name: 'Done' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Maximum 5 data groups' }),
    ).toHaveAttribute('href', '#chartDataGroupingForm-numberOfGroupsQuantiles');
  });

  test('shows a validation error if no custom groups set when using custom grouping', async () => {
    const { user } = render(
      <ChartDataGroupingForm
        dataSetConfig={testDataSetConfigs[0]}
        dataSetConfigs={testDataSetConfigs}
        meta={testTable.subjectMeta}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    await user.click(screen.getByLabelText('New custom groups'));

    await user.click(screen.getByRole('button', { name: 'Done' }));

    await user.tab();

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Add one or more custom groups' }),
    ).toHaveAttribute('href', '#chartDataGroupingForm-customGroups');
  });

  test('shows a validation error when no data set selected to copy custom groups from', async () => {
    const { user } = render(
      <ChartDataGroupingForm
        dataSetConfig={testDataSetConfigs[0]}
        dataSetConfigs={testDataSetConfigs}
        meta={testTable.subjectMeta}
        onCancel={noop}
        onSubmit={noop}
      />,
    );

    await user.click(screen.getByLabelText('Copy custom groups'));

    await user.click(screen.getByRole('button', { name: 'Done' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', {
        name: 'Select a data set to copy custom groups from',
      }),
    ).toHaveAttribute('href', '#chartDataGroupingForm-copyCustomGroups');
  });

  describe('custom groups', () => {
    test('shows validation errors without min and max values', async () => {
      const { user } = render(
        <ChartDataGroupingForm
          dataSetConfig={testDataSetConfigs[0]}
          dataSetConfigs={testDataSetConfigs}
          meta={testTable.subjectMeta}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.click(screen.getByLabelText('New custom groups'));
      await user.click(screen.getByLabelText('Min'));
      await user.tab();
      await user.click(screen.getByLabelText('Max'));
      await user.tab();

      await waitFor(() => {
        expect(screen.getByText('Enter a minimum value')).toBeInTheDocument();
        expect(screen.getByText('Enter a maximum value')).toBeInTheDocument();
      });
    });

    test('shows a validation error when the max is not greater than the min', async () => {
      const { user } = render(
        <ChartDataGroupingForm
          dataSetConfig={testDataSetConfigs[0]}
          dataSetConfigs={testDataSetConfigs}
          meta={testTable.subjectMeta}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.click(screen.getByLabelText('New custom groups'));
      await user.type(screen.getByLabelText('Min'), '10');
      await user.type(screen.getByLabelText('Max'), '9');
      await user.tab();

      expect(
        await within(
          screen.getByTestId('chartDataGroupingForm-max-error'),
        ).findByText('Must be greater than min'),
      ).toBeInTheDocument();
    });

    test('shows a validation error when the minimum value is in an existing group', async () => {
      const { user } = render(
        <ChartDataGroupingForm
          dataSetConfig={testDataSetConfigs[2]}
          dataSetConfigs={testDataSetConfigs}
          meta={testTable.subjectMeta}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.type(screen.getByLabelText('Min'), '10');
      await user.type(screen.getByLabelText('Max'), '200');
      await user.tab();

      expect(
        await within(
          screen.getByTestId('chartDataGroupingForm-min-error'),
        ).findByText('Min cannot overlap another group'),
      ).toBeInTheDocument();
    });

    test('shows a validation error when the maximum value is in an existing group', async () => {
      const { user } = render(
        <ChartDataGroupingForm
          dataSetConfig={testDataSetConfigs[2]}
          dataSetConfigs={testDataSetConfigs}
          meta={testTable.subjectMeta}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.click(screen.getByLabelText('New custom groups'));
      await user.type(screen.getByLabelText('Min'), '-10');
      await user.type(screen.getByLabelText('Max'), '15');
      await user.tab();

      expect(
        await within(
          screen.getByTestId('chartDataGroupingForm-max-error'),
        ).findByText('Max cannot overlap another group'),
      ).toBeInTheDocument();
    });

    test('shows validation errors when both values are in an existing group', async () => {
      const { user } = render(
        <ChartDataGroupingForm
          dataSetConfig={testDataSetConfigs[2]}
          dataSetConfigs={testDataSetConfigs}
          meta={testTable.subjectMeta}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.click(screen.getByLabelText('New custom groups'));
      await user.type(screen.getByLabelText('Min'), '10');
      await user.type(screen.getByLabelText('Max'), '15');
      await user.tab();

      expect(
        await within(
          screen.getByTestId('chartDataGroupingForm-min-error'),
        ).findByText('Min cannot overlap another group'),
      ).toBeInTheDocument();

      expect(
        await within(
          screen.getByTestId('chartDataGroupingForm-max-error'),
        ).findByText('Max cannot overlap another group'),
      ).toBeInTheDocument();
    });

    test('shows validation errors when the new group contains an existing group', async () => {
      const { user } = render(
        <ChartDataGroupingForm
          dataSetConfig={testDataSetConfigs[2]}
          dataSetConfigs={testDataSetConfigs}
          meta={testTable.subjectMeta}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.type(screen.getByLabelText('Min'), '-10');
      await user.type(screen.getByLabelText('Max'), '150');
      await user.click(screen.getByLabelText('Min'));
      await user.tab();

      expect(
        await within(
          screen.getByTestId('chartDataGroupingForm-min-error'),
        ).findByText('Min cannot overlap another group'),
      ).toBeInTheDocument();

      expect(
        await within(
          screen.getByTestId('chartDataGroupingForm-max-error'),
        ).findByText('Max cannot overlap another group'),
      ).toBeInTheDocument();
    });

    test('shows a validation error when groups with negative values overlap', async () => {
      const { user } = render(
        <ChartDataGroupingForm
          dataSetConfig={{
            ...testDataSetConfigs[2],
            dataGrouping: {
              ...testDataSetConfigs[2].dataGrouping,
              customGroups: [{ min: -2, max: 2 }],
            },
          }}
          dataSetConfigs={testDataSetConfigs}
          meta={testTable.subjectMeta}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      await user.type(screen.getByLabelText('Min'), '-3');
      await user.type(screen.getByLabelText('Max'), '3');
      await user.click(screen.getByLabelText('Min'));
      await user.tab();

      expect(
        await within(
          screen.getByTestId('chartDataGroupingForm-min-error'),
        ).findByText('Min cannot overlap another group'),
      ).toBeInTheDocument();

      expect(
        await within(
          screen.getByTestId('chartDataGroupingForm-max-error'),
        ).findByText('Max cannot overlap another group'),
      ).toBeInTheDocument();
    });

    test('does not allow adding more than the maximum custom groups', async () => {
      render(
        <ChartDataGroupingForm
          dataSetConfig={{
            ...testDataSetConfigs[2],
            dataGrouping: {
              ...testDataSetConfigs[2].dataGrouping,
              customGroups: [
                ...testDataSetConfigs[2].dataGrouping.customGroups,
                { min: 21, max: 30 },
                { min: 31, max: 40 },
                { min: 41, max: 50 },
              ],
            },
          }}
          dataSetConfigs={testDataSetConfigs}
          meta={testTable.subjectMeta}
          onCancel={noop}
          onSubmit={noop}
        />,
      );

      expect(screen.queryByLabelText('Min')).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Max')).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Add group' }),
      ).not.toBeInTheDocument();
    });
  });
});
