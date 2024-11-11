import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartBoundaryLevelsConfiguration from '@admin/pages/release/datablocks/components/chart/ChartBoundaryLevelsConfiguration';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import baseRender from '@common-test/render';
import { MapConfig } from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';
import { defaultDataGrouping } from '@common/modules/charts/util/getMapDataSetCategoryConfigs';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import { ReactElement } from 'react';
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
  const testDefaultMap: MapConfig = {
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
  };

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

  test('renders without data sets table', () => {
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
    expect(screen.getByLabelText('Default boundary level')).toBeInTheDocument();
    expect(
      screen.queryByText('Set boundary levels per data set'),
    ).not.toBeInTheDocument();
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  test('renders data sets without initial boundary levels', () => {
    render(
      <ChartBoundaryLevelsConfiguration
        map={testDefaultMap}
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={noop}
        onSubmit={noop}
        hasDataSetBoundaryLevels
      />,
    );

    expect(screen.getByLabelText('Default boundary level')).not.toHaveValue();
    const [defaultBoundaryLevelNullOption, ...defaultBoundaryLevelOptions] =
      within(screen.getByLabelText('Default boundary level')).getAllByRole(
        'option',
      );

    expect(defaultBoundaryLevelOptions).toHaveLength(
      testMeta.boundaryLevels.length,
    );

    expect(defaultBoundaryLevelNullOption).toHaveTextContent('Please select');
    expect(defaultBoundaryLevelNullOption).toHaveValue('');

    defaultBoundaryLevelOptions.forEach((option, index) => {
      const { id, label } = testMeta.boundaryLevels[index];
      expect(option).toHaveTextContent(label);
      expect(option).toHaveValue(String(id));
    });

    expect(
      screen.getByText('Set boundary levels per data set'),
    ).toBeInTheDocument();
    expect(screen.queryByRole('table')).toBeInTheDocument();

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const [headerRow, ...dataRows] = screen.getAllByRole('row');
    expect(dataRows).toHaveLength(2);

    const row1Cells = within(dataRows[0]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2014/15)',
    );
    expect(within(row1Cells[1]).getByRole('combobox')).toHaveValue('');

    const row2Cells = within(dataRows[1]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent(
      'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2015/16)',
    );
    expect(within(row2Cells[1]).getByRole('combobox')).toHaveValue('');

    dataRows.forEach(row => {
      const cells = within(row).getAllByRole('cell');
      const [useDefaultOption, ...boundaryLevelOptions] = within(
        cells[1],
      ).getAllByRole('option');
      expect(useDefaultOption).toHaveTextContent('Use default');
      expect(useDefaultOption).toHaveValue('');

      boundaryLevelOptions.forEach((option, optionIndex) => {
        const { id, label } = testMeta.boundaryLevels[optionIndex];
        expect(option).toHaveTextContent(label);
        expect(option).toHaveValue(String(id));
      });
    });
  });

  test('renders data sets with initial boundary levels', () => {
    render(
      <ChartBoundaryLevelsConfiguration
        map={{
          dataSetConfigs: [
            {
              dataGrouping: defaultDataGrouping,
              dataSet: testDataSets[0],
              boundaryLevel: testMeta.boundaryLevels[0].id,
            },
            {
              dataGrouping: defaultDataGrouping,
              dataSet: testDataSets[1],
              boundaryLevel: testMeta.boundaryLevels[1].id,
            },
          ],
        }}
        meta={testMeta}
        options={{
          ...testDefaultChartOptions,
          boundaryLevel: testMeta.boundaryLevels[2].id,
        }}
        onChange={noop}
        onSubmit={noop}
        hasDataSetBoundaryLevels
      />,
    );

    expect(screen.getByLabelText('Default boundary level')).toHaveValue(
      String(testMeta.boundaryLevels[2].id),
    );
    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(3);

    expect(within(rows[1]).getByRole('combobox')).toHaveValue(
      String(testMeta.boundaryLevels[0].id),
    );
    expect(within(rows[2]).getByRole('combobox')).toHaveValue(
      String(testMeta.boundaryLevels[1].id),
    );
  });

  test('calls `onChange` handler when form values change', async () => {
    const handleChange = jest.fn();

    const { user } = render(
      <ChartBoundaryLevelsConfiguration
        map={testDefaultMap}
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
        map={testDefaultMap}
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
        map={testDefaultMap}
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
        map={testDefaultMap}
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
