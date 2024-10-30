import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartBoundaryLevelsConfiguration from '@admin/pages/release/datablocks/components/chart/ChartBoundaryLevelsConfiguration';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import baseRender from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React, { ReactElement } from 'react';
import { DataSet } from '@common/modules/charts/types/dataSet';
import { defaultDataGrouping } from '@common/modules/charts/util/getMapDataSetCategoryConfigs';
import { MapBoundaryLevelConfig } from '../types/mapConfig';

describe('ChartBoundaryLevelsConfiguration', () => {
  const testDefaultChartOptions: ChartOptions = {
    alt: '',
    height: 600,
    titleType: 'default',
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
    boundaryLevels: {
      isValid: true,
      submitCount: 0,
      id: 'map',
      title: 'Boundary levels configuration',
    },
  };
  const testDataSets: DataSet[] = [
    {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      indicator: 'authorised-absence-sessions',
      timePeriod: '2014_AY',
    },
    {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      indicator: 'authorised-absence-sessions',
      timePeriod: '2015_AY',
    },
  ];

  function render(element: ReactElement) {
    return baseRender(
      <ChartBuilderFormsContextProvider
        initialForms={{
          ...testFormState,
        }}
      >
        {element}
      </ChartBuilderFormsContextProvider>,
    );
  }

  test('renders without table one or less dataset is included', () => {
    render(
      <ChartBoundaryLevelsConfiguration
        map={{ dataSetConfigs: [] }}
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={noop}
        onSubmit={noop}
        hasDataSetBoundaryLevels
      />,
    );
    expect(
      screen.queryByText('Set boundary levels per data set'),
    ).not.toBeInTheDocument();
  });

  test('renders correctly without initial values', () => {
    render(
      <ChartBoundaryLevelsConfiguration
        map={{
          dataSetConfigs: [
            {
              dataGrouping: defaultDataGrouping,
              dataSet: testDataSets[0],
              boundaryLevel: undefined,
            },
            {
              dataGrouping: defaultDataGrouping,
              dataSet: testDataSets[1],
              boundaryLevel: undefined,
            },
          ],
        }}
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={noop}
        onSubmit={noop}
        hasDataSetBoundaryLevels
      />,
    );

    expect(screen.getByLabelText('Default boundary level')).not.toHaveValue();
    const boundaryLevels = within(
      screen.getByLabelText('Default boundary level'),
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

    expect(
      screen.getByText('Set boundary levels per data set'),
    ).toBeInTheDocument();

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2014/15)',
    );
    expect(row1Cells[1]).toHaveTextContent('Use default');
  });

  test('renders correctly with initial values', () => {
    render(
      <ChartBoundaryLevelsConfiguration
        map={{
          dataSetConfigs: [
            {
              dataGrouping: defaultDataGrouping,
              dataSet: testDataSets[0],
              boundaryLevel: 3,
            },
            {
              dataGrouping: defaultDataGrouping,
              dataSet: testDataSets[1],
              boundaryLevel: 3,
            },
          ],
        }}
        meta={testMeta}
        options={{
          ...testDefaultChartOptions,
          boundaryLevel: 2,
        }}
        onChange={noop}
        onSubmit={noop}
        hasDataSetBoundaryLevels
      />,
    );

    expect(screen.getByLabelText('Default boundary level')).toHaveValue('2');
    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    expect(within(rows[1]).getByRole('combobox')).toHaveValue('3');
    expect(within(rows[2]).getByRole('combobox')).toHaveValue('3');
  });

  test('calls `onChange` handler when form values change', async () => {
    const handleChange = jest.fn();

    const { user } = render(
      <ChartBoundaryLevelsConfiguration
        map={{
          dataSetConfigs: [
            {
              dataGrouping: defaultDataGrouping,
              dataSet: testDataSets[0],
            },
            {
              dataGrouping: defaultDataGrouping,
              dataSet: testDataSets[1],
            },
          ],
        }}
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={handleChange}
        onSubmit={noop}
        hasDataSetBoundaryLevels
      />,
    );

    await user.selectOptions(screen.getByLabelText('Default boundary level'), [
      '2',
    ]);

    expect(handleChange).toHaveBeenCalledWith<[MapBoundaryLevelConfig]>({
      boundaryLevel: 2,
      dataSetConfigs: testDataSets.map(dataSet => ({
        dataSet,
      })),
    });

    const rows = screen.getAllByRole('row');
    await user.selectOptions(within(rows[1]).getByRole('combobox'), ['2']);

    expect(handleChange).toHaveBeenCalledWith<[MapBoundaryLevelConfig]>({
      boundaryLevel: 2,
      dataSetConfigs: [
        { dataSet: testDataSets[0], boundaryLevel: 2 },
        { dataSet: testDataSets[1] },
      ],
    });
  });

  test('submitting fails with validation errors if no boundary level set', async () => {
    const { user } = render(
      <ChartBoundaryLevelsConfiguration
        map={{
          dataSetConfigs: [
            {
              dataGrouping: defaultDataGrouping,
              dataSet: testDataSets[0],
            },
            {
              dataGrouping: defaultDataGrouping,
              dataSet: testDataSets[1],
            },
          ],
        }}
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={noop}
        onSubmit={noop}
        hasDataSetBoundaryLevels
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Choose a boundary level' }),
    ).toHaveAttribute(
      'href',
      '#chartBoundaryLevelsConfigurationForm-boundaryLevel',
    );
  });

  test('submitting succeeds with form that has been filled out correctly', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ChartBoundaryLevelsConfiguration
        map={{
          dataSetConfigs: [
            {
              dataGrouping: defaultDataGrouping,
              dataSet: testDataSets[0],
            },
            {
              dataGrouping: defaultDataGrouping,
              dataSet: testDataSets[1],
            },
          ],
        }}
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={noop}
        onSubmit={handleSubmit}
        hasDataSetBoundaryLevels
      />,
    );

    await user.selectOptions(screen.getByLabelText('Default boundary level'), [
      '2',
    ]);

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith<[MapBoundaryLevelConfig]>({
        boundaryLevel: 2,
        dataSetConfigs: testDataSets.map(dataSet => ({ dataSet })),
      });
    });
  });

  test('submitting succeeds with valid initial values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(
      <ChartBoundaryLevelsConfiguration
        map={{
          dataSetConfigs: [
            {
              dataGrouping: defaultDataGrouping,
              dataSet: testDataSets[0],
            },
            {
              dataGrouping: defaultDataGrouping,
              dataSet: testDataSets[1],
            },
          ],
        }}
        meta={testMeta}
        options={{
          ...testDefaultChartOptions,
          boundaryLevel: 3,
        }}
        onChange={noop}
        onSubmit={handleSubmit}
        hasDataSetBoundaryLevels
      />,
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith<[MapBoundaryLevelConfig]>({
        boundaryLevel: 3,
        dataSetConfigs: testDataSets.map(dataSet => ({ dataSet })),
      });
    });
  });
});
