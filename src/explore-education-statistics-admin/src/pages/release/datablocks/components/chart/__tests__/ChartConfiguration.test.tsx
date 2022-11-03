import ChartConfiguration from '@admin/pages/release/datablocks/components/chart/ChartConfiguration';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import {
  ChartCapabilities,
  ChartDefinition,
} from '@common/modules/charts/types/chart';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('ChartConfiguration', () => {
  const testLineChartCapabilities: ChartCapabilities = {
    canPositionLegendInline: true,
    canSize: true,
    canSort: true,
    hasGridLines: true,
    hasLegend: true,
    hasLegendPosition: true,
    hasLineStyle: true,
    hasReferenceLines: true,
    hasSymbols: true,
    requiresGeoJson: false,
    stackable: false,
  };
  const testBarChartCapabilities: ChartCapabilities = {
    canPositionLegendInline: false,
    canSize: true,
    canSort: true,
    hasGridLines: true,
    hasLegend: true,
    hasLegendPosition: true,
    hasLineStyle: false,
    hasReferenceLines: true,
    hasSymbols: false,
    requiresGeoJson: false,
    stackable: true,
  };

  const testLineChartDefinition: ChartDefinition = {
    type: 'line',
    name: 'Line',
    capabilities: testLineChartCapabilities,
    options: {
      defaults: {
        height: 300,
      },
    },
    legend: {
      defaults: {
        position: 'bottom',
      },
    },
    axes: {
      major: {
        id: 'xaxis',
        title: 'X Axis (major axis)',
        type: 'major',
        capabilities: {
          canRotateLabel: false,
        },
        defaults: {
          groupBy: 'timePeriod',
          min: 0,
          showGrid: true,
          size: 50,
          sortAsc: true,
          sortBy: 'name',
          tickConfig: 'default',
          tickSpacing: 1,
          unit: '',
        },
      },
      minor: {
        id: 'yaxis',
        title: 'Y Axis (minor axis)',
        type: 'minor',
        capabilities: {
          canRotateLabel: true,
        },
        defaults: {
          min: 0,
          showGrid: true,
          tickConfig: 'default',
          tickSpacing: 1,
          unit: '',
          label: {
            width: 100,
          },
        },
      },
    },
  };

  const testHorizontalBarChartDefinition: ChartDefinition = {
    ...testLineChartDefinition,
    type: 'horizontalbar',
    name: 'Horizontal bar',
    capabilities: testBarChartCapabilities,
  };

  const testVerticalBarChartDefinition: ChartDefinition = {
    ...testLineChartDefinition,
    type: 'verticalbar',
    name: 'Vertical bar',
    capabilities: testBarChartCapabilities,
  };

  const testDefaultChartOptions: ChartOptions = {
    alt: '',
    height: 300,
    title: '',
    titleType: 'default',
  };
  const testChartOptionsWithAltText: ChartOptions = {
    ...testDefaultChartOptions,
    alt: 'This is the alt text',
  };

  const testInitialChartOptions: ChartOptions = {
    alt: 'This is the alt text',
    dataLabelPosition: 'above',
    height: 600,
    includeNonNumericData: true,
    showDataLabels: true,
    titleType: 'default',
    width: 400,
  };

  const testFormState: ChartBuilderForms = {
    options: {
      isValid: true,
      submitCount: 0,
      id: 'options',
      title: 'Options',
    },
    dataSets: {
      isValid: true,
      submitCount: 0,
      id: 'dataSets',
      title: 'Data sets',
    },
  };

  test('renders the options for line charts', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testDefaultChartOptions}
          definition={testLineChartDefinition}
          onChange={noop}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(
      screen.getByRole('group', { name: 'Chart title' }),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Alt text')).toBeInTheDocument();
    expect(screen.getByLabelText('Height (pixels)')).toBeInTheDocument();
    expect(screen.getByLabelText('Width (pixels)')).toBeInTheDocument();
    expect(
      screen.getByLabelText('Include data sets with non-numerical values'),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Show data labels')).toBeInTheDocument();
  });

  test('renders the options for horizontal bar charts', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testDefaultChartOptions}
          definition={testHorizontalBarChartDefinition}
          onChange={noop}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(
      screen.getByRole('group', { name: 'Chart title' }),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Alt text')).toBeInTheDocument();
    expect(screen.getByLabelText('Stacked bars')).toBeInTheDocument();
    expect(screen.getByLabelText('Height (pixels)')).toBeInTheDocument();
    expect(screen.getByLabelText('Width (pixels)')).toBeInTheDocument();
    expect(screen.getByLabelText('Bar thickness (pixels)')).toBeInTheDocument();
    expect(
      screen.getByLabelText('Include data sets with non-numerical values'),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Show data labels')).toBeInTheDocument();
  });

  test('renders the options for vertical bar charts', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testDefaultChartOptions}
          definition={testVerticalBarChartDefinition}
          onChange={noop}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(
      screen.getByRole('group', { name: 'Chart title' }),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Alt text')).toBeInTheDocument();
    expect(screen.getByLabelText('Stacked bars')).toBeInTheDocument();
    expect(screen.getByLabelText('Height (pixels)')).toBeInTheDocument();
    expect(screen.getByLabelText('Width (pixels)')).toBeInTheDocument();
    expect(screen.getByLabelText('Bar thickness (pixels)')).toBeInTheDocument();
    expect(
      screen.getByLabelText('Include data sets with non-numerical values'),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Show data labels')).toBeInTheDocument();
  });

  test('renders the options for maps', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testDefaultChartOptions}
          definition={testLineChartDefinition}
          onChange={noop}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(
      screen.getByRole('group', { name: 'Chart title' }),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Alt text')).toBeInTheDocument();
    expect(screen.getByLabelText('Height (pixels)')).toBeInTheDocument();
    expect(screen.getByLabelText('Width (pixels)')).toBeInTheDocument();
  });

  test('submitting fails with default options', async () => {
    const handleSubmit = jest.fn();
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testDefaultChartOptions}
          definition={testLineChartDefinition}
          onChange={noop}
          onSubmit={handleSubmit}
        />
      </ChartBuilderFormsContextProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('link', { name: 'Enter chart alt text' }),
    ).toHaveAttribute('href', '#chartConfigurationForm-alt');

    expect(handleSubmit).not.toHaveBeenCalled();
  });

  test('submitting succeeds with valid options', async () => {
    const handleSubmit = jest.fn();
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testDefaultChartOptions}
          definition={testLineChartDefinition}
          onChange={noop}
          onSubmit={handleSubmit}
        />
      </ChartBuilderFormsContextProvider>,
    );

    userEvent.type(screen.getByLabelText('Alt text'), 'This is the alt text');
    userEvent.type(screen.getByLabelText('Width (pixels)'), '500');

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        alt: 'This is the alt text',
        boundaryLevel: undefined,
        dataLabelPosition: 'above',
        height: 300,
        title: '',
        titleType: 'default',
        width: 500,
      });
    });
  });

  test('calls the `onChange` handler when form values change', async () => {
    const handleChange = jest.fn();
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testDefaultChartOptions}
          definition={testLineChartDefinition}
          onChange={handleChange}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    userEvent.type(screen.getByLabelText('Alt text'), 'This is the alt text');

    await waitFor(() => {
      expect(handleChange).toHaveBeenCalledWith({
        alt: 'This is the alt text',
        boundaryLevel: undefined,
        dataLabelPosition: 'above',
        height: 300,
        title: '',
        titleType: 'default',
      });
    });
  });

  test('renders with initial values', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testInitialChartOptions}
          definition={testLineChartDefinition}
          onChange={noop}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(screen.getByLabelText('Alt text')).toHaveValue(
      'This is the alt text',
    );
    expect(screen.getByLabelText('Height (pixels)')).toHaveValue(600);
    expect(screen.getByLabelText('Width (pixels)')).toHaveValue(400);
    expect(
      screen.getByLabelText('Include data sets with non-numerical values'),
    ).toBeChecked();
    expect(screen.getByLabelText('Show data labels')).toBeChecked();
    expect(screen.getByLabelText('Data label position')).toHaveValue('above');
  });

  test('setting an alternative title', async () => {
    const handleSubmit = jest.fn();
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testChartOptionsWithAltText}
          definition={testLineChartDefinition}
          onChange={noop}
          onSubmit={handleSubmit}
        />
      </ChartBuilderFormsContextProvider>,
    );

    userEvent.click(screen.getByLabelText('Set an alternative title'));
    userEvent.type(screen.getByLabelText('Enter chart title'), 'The title');

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        alt: 'This is the alt text',
        boundaryLevel: undefined,
        dataLabelPosition: 'above',
        height: 300,
        title: 'The title',
        titleType: 'alternative',
      });
    });
  });

  test('setting `showDataLabels`', async () => {
    const handleSubmit = jest.fn();
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testChartOptionsWithAltText}
          definition={testLineChartDefinition}
          onChange={noop}
          onSubmit={handleSubmit}
        />
      </ChartBuilderFormsContextProvider>,
    );

    userEvent.click(screen.getByLabelText('Show data labels'));

    userEvent.selectOptions(screen.getByLabelText('Data label position'), [
      'below',
    ]);

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        alt: 'This is the alt text',
        boundaryLevel: undefined,
        dataLabelPosition: 'below',
        height: 300,
        showDataLabels: true,
        title: '',
        titleType: 'default',
      });
    });
  });

  test('shows a validation error when `showDataLabels` is true and legend position is inline', async () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testChartOptionsWithAltText}
          definition={testLineChartDefinition}
          legendPosition="inline"
          onChange={noop}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    userEvent.click(screen.getByLabelText('Show data labels'));

    userEvent.selectOptions(screen.getByLabelText('Data label position'), [
      'below',
    ]);

    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByRole('link', {
          name: 'Data labels cannot be used with inline legends',
        }),
      ).toHaveAttribute('href', '#chartConfigurationForm-showDataLabels');
    });
  });
});
