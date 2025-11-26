import ChartConfiguration from '@admin/pages/release/datablocks/components/chart/ChartConfiguration';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import render from '@common-test/render';
import {
  ChartCapabilities,
  ChartDefinition,
} from '@common/modules/charts/types/chart';
import { screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('ChartConfiguration', () => {
  const testLineChartCapabilities: ChartCapabilities = {
    canIncludeNonNumericData: true,
    canPositionLegendInline: true,
    canReorderDataCategories: false,
    canSetBarThickness: false,
    canSetDataLabelColour: true,
    canSetDataLabelPosition: true,
    canShowDataLabels: true,
    canShowAllMajorAxisTicks: false,
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
    canIncludeNonNumericData: true,
    canReorderDataCategories: false,
    canPositionLegendInline: true,
    canSetBarThickness: true,
    canSetDataLabelColour: false,
    canSetDataLabelPosition: true,
    canShowDataLabels: true,
    canShowAllMajorAxisTicks: false,
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
    subtitle: 'This is the subtitle',
    titleType: 'default',
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
    expect(screen.getByLabelText('Subtitle')).toBeInTheDocument();
    expect(screen.getByLabelText('Alt text')).toBeInTheDocument();
    expect(screen.getByLabelText('Height (pixels)')).toBeInTheDocument();
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
    expect(screen.getByLabelText('Subtitle')).toBeInTheDocument();
    expect(screen.getByLabelText('Alt text')).toBeInTheDocument();
    expect(screen.getByLabelText('Stacked bars')).toBeInTheDocument();
    expect(screen.getByLabelText('Height (pixels)')).toBeInTheDocument();
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
    expect(screen.getByLabelText('Subtitle')).toBeInTheDocument();
    expect(screen.getByLabelText('Alt text')).toBeInTheDocument();
    expect(screen.getByLabelText('Stacked bars')).toBeInTheDocument();
    expect(screen.getByLabelText('Height (pixels)')).toBeInTheDocument();
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
    expect(screen.getByLabelText('Subtitle')).toBeInTheDocument();
    expect(screen.getByLabelText('Alt text')).toBeInTheDocument();
    expect(screen.getByLabelText('Height (pixels)')).toBeInTheDocument();
  });

  test('submitting fails with default options', async () => {
    const handleSubmit = jest.fn();
    const { user } = render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testDefaultChartOptions}
          definition={testLineChartDefinition}
          onChange={noop}
          onSubmit={handleSubmit}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

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
    const { user } = render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testDefaultChartOptions}
          definition={testLineChartDefinition}
          onChange={noop}
          onSubmit={handleSubmit}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await user.type(screen.getByLabelText('Subtitle'), 'This is the subtitle');
    await user.type(screen.getByLabelText('Alt text'), 'This is the alt text');

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        alt: 'This is the alt text',
        boundaryLevel: undefined,
        dataLabelColour: 'black',
        dataLabelPosition: 'above',
        height: 300,
        includeNonNumericData: false,
        showDataLabels: false,
        subtitle: 'This is the subtitle',
        title: '',
        titleType: 'default',
      });
    });
  });

  test('calls the `onChange` handler when form values change', async () => {
    const handleChange = jest.fn();
    const { user } = render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testDefaultChartOptions}
          definition={testLineChartDefinition}
          onChange={handleChange}
          onSubmit={noop}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await user.type(screen.getByLabelText('Alt text'), 'This is the alt text');

    await waitFor(() => {
      expect(handleChange).toHaveBeenCalledWith({
        alt: 'This is the alt text',
        boundaryLevel: undefined,
        dataLabelColour: 'black',
        dataLabelPosition: 'above',
        height: 300,
        includeNonNumericData: false,
        showDataLabels: false,
        subtitle: '',
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

    expect(screen.getByLabelText('Subtitle')).toHaveValue(
      'This is the subtitle',
    );
    expect(screen.getByLabelText('Alt text')).toHaveValue(
      'This is the alt text',
    );
    expect(screen.getByLabelText('Height (pixels)')).toHaveNumericValue(600);
    expect(
      screen.getByLabelText('Include data sets with non-numerical values'),
    ).toBeChecked();
    expect(screen.getByLabelText('Show data labels')).toBeChecked();
    expect(screen.getByLabelText('Data label position')).toHaveValue('above');
  });

  test('setting an alternative title', async () => {
    const handleSubmit = jest.fn();
    const { user } = render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testChartOptionsWithAltText}
          definition={testLineChartDefinition}
          onChange={noop}
          onSubmit={handleSubmit}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await user.click(screen.getByLabelText('Set an alternative title'));
    await user.type(screen.getByLabelText('Enter chart title'), 'The title');

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        alt: 'This is the alt text',
        boundaryLevel: undefined,
        dataLabelColour: 'black',
        dataLabelPosition: 'above',
        height: 300,
        includeNonNumericData: false,
        showDataLabels: false,
        subtitle: '',
        title: 'The title',
        titleType: 'alternative',
      });
    });
  });

  test('setting `showDataLabels`', async () => {
    const handleSubmit = jest.fn();
    const { user } = render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartConfiguration
          chartOptions={testChartOptionsWithAltText}
          definition={testLineChartDefinition}
          onChange={noop}
          onSubmit={handleSubmit}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await user.click(screen.getByLabelText('Show data labels'));

    await user.selectOptions(screen.getByLabelText('Data label position'), [
      'below',
    ]);

    await user.selectOptions(screen.getByLabelText('Data label colour'), [
      'inherit',
    ]);

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        alt: 'This is the alt text',
        boundaryLevel: undefined,
        dataLabelColour: 'inherit',
        dataLabelPosition: 'below',
        height: 300,
        includeNonNumericData: false,
        showDataLabels: true,
        subtitle: '',
        title: '',
        titleType: 'default',
      });
    });
  });

  test('shows a validation error when `showDataLabels` is true and legend position is inline', async () => {
    const { user } = render(
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

    await user.click(screen.getByLabelText('Show data labels'));

    await user.selectOptions(screen.getByLabelText('Data label position'), [
      'below',
    ]);

    await user.click(
      screen.getByRole('button', { name: 'Save chart options' }),
    );

    await waitFor(() => {
      expect(
        screen.getByRole('link', {
          name: 'Data labels cannot be used with inline legends',
        }),
      ).toHaveAttribute('href', '#chartConfigurationForm-showDataLabels');
    });
  });
});
