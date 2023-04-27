import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartMapConfiguration from '@admin/pages/release/datablocks/components/chart/ChartMapConfiguration';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React, { FC } from 'react';

describe('ChartMapConfiguration', () => {
  const testDefaultChartOptions: ChartOptions = {
    alt: '',
    height: 600,
    titleType: 'default',
    customDataGroups: [],
  };

  const testMeta: FullTableMeta = {
    ...testFullTable.subjectMeta,
    boundaryLevels: [
      {
        id: 1,
        label: 'Boundary level 1',
      },
      {
        id: 2,
        label: 'Boundary level 2',
      },
      {
        id: 3,
        label: 'Boundary level 3',
      },
    ],
  };

  const testFormState: ChartBuilderForms = {
    options: {
      isValid: true,
      submitCount: 0,
      id: 'options',
      title: 'Options',
    },
    map: {
      isValid: true,
      submitCount: 0,
      id: 'map',
      title: 'Map configuration',
    },
  };

  const wrapper: FC = ({ children }) => (
    <ChartBuilderFormsContextProvider
      initialForms={{
        ...testFormState,
      }}
    >
      {children}
    </ChartBuilderFormsContextProvider>
  );

  test('renders correctly without initial values', () => {
    render(
      <ChartMapConfiguration
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={noop}
        onSubmit={noop}
      />,
      { wrapper },
    );

    expect(screen.getByLabelText('Boundary level')).not.toHaveValue();
    const boundaryLevels = within(
      screen.getByLabelText('Boundary level'),
    ).getAllByRole('option');

    expect(boundaryLevels).toHaveLength(4);
    expect(boundaryLevels[0]).toHaveTextContent('Please select');
    expect(boundaryLevels[0]).toHaveValue('');
    expect(boundaryLevels[1]).toHaveTextContent('Boundary level 1');
    expect(boundaryLevels[1]).toHaveValue('1');
    expect(boundaryLevels[2]).toHaveTextContent('Boundary level 2');
    expect(boundaryLevels[2]).toHaveValue('2');
    expect(boundaryLevels[3]).toHaveTextContent('Boundary level 3');
    expect(boundaryLevels[3]).toHaveValue('3');

    // Data classification
    expect(screen.getByLabelText('Equal intervals')).not.toBeChecked();
    expect(screen.getByLabelText('Quantiles')).not.toBeChecked();

    expect(screen.getByLabelText('Number of data groups')).not.toHaveValue();
  });

  test('renders correctly with initial values', () => {
    render(
      <ChartMapConfiguration
        meta={testMeta}
        options={{
          ...testDefaultChartOptions,
          boundaryLevel: 2,
          dataClassification: 'EqualIntervals',
          dataGroups: 6,
        }}
        onChange={noop}
        onSubmit={noop}
      />,
      { wrapper },
    );

    expect(screen.getByLabelText('Boundary level')).toHaveValue('2');

    // Data classification
    expect(screen.getByLabelText('Equal intervals')).toBeChecked();
    expect(screen.getByLabelText('Quantiles')).not.toBeChecked();

    expect(screen.getByLabelText('Number of data groups')).toHaveValue(6);
  });

  test('renders without boundary level selector when none available', () => {
    render(
      <ChartMapConfiguration
        meta={{
          ...testMeta,
          boundaryLevels: [],
        }}
        options={testDefaultChartOptions}
        onChange={noop}
        onSubmit={noop}
      />,
      { wrapper },
    );

    expect(screen.queryByLabelText('Boundary level')).not.toBeInTheDocument();
  });

  test('calls `onChange` handler when form values change', () => {
    const handleChange = jest.fn();

    render(
      <ChartMapConfiguration
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={handleChange}
        onSubmit={noop}
      />,
      { wrapper },
    );

    expect(handleChange).not.toHaveBeenCalled();

    userEvent.selectOptions(screen.getByLabelText('Boundary level'), ['2']);

    expect(handleChange).toHaveBeenCalledWith<[ChartOptions]>({
      ...testDefaultChartOptions,
      boundaryLevel: 2,
      customDataGroups: [],
    });
  });

  test('shows a validation error when number of data groups is less than 1', async () => {
    render(
      <ChartMapConfiguration
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={noop}
        onSubmit={noop}
      />,
      { wrapper },
    );

    userEvent.type(screen.getByLabelText('Number of data groups'), '0');
    userEvent.tab();

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', {
        name: 'The number of data groups must be greater than 1',
      }),
    ).toHaveAttribute('href', '#chartMapConfigurationForm-dataGroups');
  });

  test('shows a validation error when number of data groups is more than 100', async () => {
    render(
      <ChartMapConfiguration
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={noop}
        onSubmit={noop}
      />,
      { wrapper },
    );

    userEvent.type(screen.getByLabelText('Number of data groups'), '101');
    userEvent.tab();

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', {
        name: 'The number of data groups cannot be greater than 100',
      }),
    ).toHaveAttribute('href', '#chartMapConfigurationForm-dataGroups');
  });

  test('submitting fails with validation errors for required fields', async () => {
    render(
      <ChartMapConfiguration
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={noop}
        onSubmit={noop}
      />,
      { wrapper },
    );

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Choose a boundary level' }),
    ).toHaveAttribute('href', '#chartMapConfigurationForm-boundaryLevel');

    expect(
      screen.getByRole('link', { name: 'Choose a data classification' }),
    ).toHaveAttribute('href', '#chartMapConfigurationForm-dataClassification');

    expect(
      screen.getByRole('link', { name: 'Enter the number of data groups' }),
    ).toHaveAttribute('href', '#chartMapConfigurationForm-dataGroups');
  });

  test('submitting succeeds with form that has been filled out correctly', async () => {
    const handleSubmit = jest.fn();

    render(
      <ChartMapConfiguration
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={noop}
        onSubmit={handleSubmit}
      />,
      { wrapper },
    );

    userEvent.selectOptions(screen.getByLabelText('Boundary level'), ['2']);
    userEvent.click(screen.getByLabelText('Quantiles'));
    userEvent.type(screen.getByLabelText('Number of data groups'), '7');

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith<[ChartOptions]>({
        ...testDefaultChartOptions,
        boundaryLevel: 2,
        customDataGroups: [],
        dataClassification: 'Quantiles',
        dataGroups: 7,
      });
    });
  });

  test('submitting succeeds with valid initial values', async () => {
    const handleSubmit = jest.fn();

    render(
      <ChartMapConfiguration
        meta={testMeta}
        options={{
          ...testDefaultChartOptions,
          boundaryLevel: 3,
          dataClassification: 'EqualIntervals',
          dataGroups: 5,
        }}
        onChange={noop}
        onSubmit={handleSubmit}
      />,
      { wrapper },
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith<[ChartOptions]>({
        ...testDefaultChartOptions,
        boundaryLevel: 3,
        customDataGroups: [],
        dataClassification: 'EqualIntervals',
        dataGroups: 5,
      });
    });
  });

  // EES-3858 - skipped until feature is reimplemented
  describe.skip('custom data groups', () => {
    test('adding groups', async () => {
      render(
        <ChartMapConfiguration
          meta={testMeta}
          options={testDefaultChartOptions}
          onChange={noop}
          onSubmit={noop}
        />,
        { wrapper },
      );

      userEvent.click(screen.getByLabelText('Custom'));

      const table = within(
        screen.getByRole('table', { name: 'Custom groups' }),
      );
      expect(table.getAllByRole('row')).toHaveLength(2);

      userEvent.type(screen.getByLabelText('Min'), '0');
      userEvent.type(screen.getByLabelText('Max'), '10');

      userEvent.click(screen.getByRole('button', { name: 'Add group' }));

      await waitFor(() => {
        expect(screen.getByText('Remove group')).toBeInTheDocument();
      });

      expect(table.getAllByRole('row')).toHaveLength(3);
      const newRow1 = within(table.getAllByRole('row')[1]);
      expect(newRow1.getAllByRole('cell')[0]).toHaveTextContent('0');
      expect(newRow1.getAllByRole('cell')[1]).toHaveTextContent('10');
      expect(
        newRow1.getByRole('button', {
          name: 'Remove group',
        }),
      );

      userEvent.type(screen.getByLabelText('Min'), '11');
      userEvent.type(screen.getByLabelText('Max'), '20');

      userEvent.click(screen.getByRole('button', { name: 'Add group' }));

      await waitFor(() => {
        expect(screen.getAllByText('Remove group')).toHaveLength(2);
      });

      expect(table.getAllByRole('row')).toHaveLength(4);
      const newRow2 = within(table.getAllByRole('row')[2]);
      expect(newRow2.getAllByRole('cell')[0]).toHaveTextContent('11');
      expect(newRow2.getAllByRole('cell')[1]).toHaveTextContent('20');
      expect(
        newRow2.getByRole('button', {
          name: 'Remove group',
        }),
      );
    });

    test('removing groups', async () => {
      render(
        <ChartMapConfiguration
          meta={testMeta}
          options={{
            ...testDefaultChartOptions,
            customDataGroups: [
              { min: 0, max: 10 },
              { min: 11, max: 20 },
              { min: 21, max: 30 },
            ],
          }}
          onChange={noop}
          onSubmit={noop}
        />,
        { wrapper },
      );

      userEvent.click(screen.getByLabelText('Custom'));

      const table = within(
        screen.getByRole('table', { name: 'Custom groups' }),
      );
      expect(table.getAllByRole('row')).toHaveLength(5);

      expect(table.getByText('11')).toBeInTheDocument();
      expect(table.getByText('20')).toBeInTheDocument();

      userEvent.click(
        within(table.getAllByRole('row')[2]).getByRole('button', {
          name: 'Remove group',
        }),
      );

      await waitFor(() => {
        expect(screen.getAllByText('Remove group')).toHaveLength(2);
      });

      expect(table.getAllByRole('row')).toHaveLength(4);
      expect(table.queryByText('11')).not.toBeInTheDocument();
      expect(table.queryByText('20')).not.toBeInTheDocument();
    });

    test('submitting fails with validation errors when custom grouping is selected and no groups are added', async () => {
      render(
        <ChartMapConfiguration
          meta={testMeta}
          options={testDefaultChartOptions}
          onChange={noop}
          onSubmit={noop}
        />,
        { wrapper },
      );

      userEvent.click(screen.getByLabelText('Custom'));

      userEvent.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('link', {
          name: 'There must be at least 1 data group',
        }),
      ).toHaveAttribute('href', '#chartMapConfigurationForm-customDataGroups');
    });

    test('submitting succeeds with custom data groups', async () => {
      const handleSubmit = jest.fn();

      render(
        <ChartMapConfiguration
          meta={testMeta}
          options={testDefaultChartOptions}
          onChange={noop}
          onSubmit={handleSubmit}
        />,
        { wrapper },
      );

      userEvent.selectOptions(screen.getByLabelText('Boundary level'), ['2']);

      userEvent.type(screen.getByLabelText('Number of data groups'), '5');

      userEvent.click(screen.getByLabelText('Custom'));

      userEvent.type(screen.getByLabelText('Min'), '0');
      userEvent.type(screen.getByLabelText('Max'), '10');

      userEvent.click(screen.getByRole('button', { name: 'Add group' }));

      await waitFor(() => {
        expect(screen.getByText('Remove group')).toBeInTheDocument();
      });

      userEvent.type(screen.getByLabelText('Min'), '11');
      userEvent.type(screen.getByLabelText('Max'), '20');

      userEvent.click(screen.getByRole('button', { name: 'Add group' }));

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith<[ChartOptions]>({
          ...testDefaultChartOptions,
          boundaryLevel: 2,
          customDataGroups: [
            { min: 0, max: 10 },
            { min: 11, max: 20 },
          ],
          dataClassification: 'Custom',
          dataGroups: 5,
        });
      });
    });
  });
});
