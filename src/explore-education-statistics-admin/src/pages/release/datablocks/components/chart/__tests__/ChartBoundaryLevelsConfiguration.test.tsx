import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartBoundaryLevelsConfiguration from '@admin/pages/release/datablocks/components/chart/ChartBoundaryLevelsConfiguration';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import {
  render as baseRender,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React, { ReactElement } from 'react';

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

  test('renders correctly without initial values', () => {
    render(
      <ChartBoundaryLevelsConfiguration
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={noop}
        onSubmit={noop}
      />,
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
  });

  test('renders correctly with initial values', () => {
    render(
      <ChartBoundaryLevelsConfiguration
        meta={testMeta}
        options={{
          ...testDefaultChartOptions,
          boundaryLevel: 2,
        }}
        onChange={noop}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Boundary level')).toHaveValue('2');
  });

  test('calls `onChange` handler when form values change', () => {
    const handleChange = jest.fn();

    render(
      <ChartBoundaryLevelsConfiguration
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={handleChange}
        onSubmit={noop}
      />,
    );

    expect(handleChange).not.toHaveBeenCalled();

    userEvent.selectOptions(screen.getByLabelText('Boundary level'), ['2']);

    expect(handleChange).toHaveBeenCalledWith<[ChartOptions]>({
      ...testDefaultChartOptions,
      boundaryLevel: 2,
    });
  });

  test('submitting fails with validation errors if no boundary level set', async () => {
    render(
      <ChartBoundaryLevelsConfiguration
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={noop}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

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

    render(
      <ChartBoundaryLevelsConfiguration
        meta={testMeta}
        options={testDefaultChartOptions}
        onChange={noop}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.selectOptions(screen.getByLabelText('Boundary level'), ['2']);

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith<[ChartOptions]>({
        ...testDefaultChartOptions,
        boundaryLevel: 2,
      });
    });
  });

  test('submitting succeeds with valid initial values', async () => {
    const handleSubmit = jest.fn();

    render(
      <ChartBoundaryLevelsConfiguration
        meta={testMeta}
        options={{
          ...testDefaultChartOptions,
          boundaryLevel: 3,
        }}
        onChange={noop}
        onSubmit={handleSubmit}
      />,
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith<[ChartOptions]>({
        ...testDefaultChartOptions,
        boundaryLevel: 3,
      });
    });
  });
});
