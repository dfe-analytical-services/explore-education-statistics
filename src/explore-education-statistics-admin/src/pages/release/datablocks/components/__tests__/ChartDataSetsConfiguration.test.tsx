import ChartDataSetsConfiguration from '@admin/pages/release/datablocks/components/ChartDataSetsConfiguration';
import { ChartBuilderForms } from '@admin/pages/release/datablocks/components/types/chartBuilderForms';
import { DataSet } from '@common/modules/charts/types/dataSet';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import mapFullTableMeta from '@common/modules/table-tool/utils/mapFullTableMeta';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('ChartDataSetsConfiguration', () => {
  const testSubjectMeta: FullTableMeta = mapFullTableMeta({
    publicationName: '',
    subjectName: '',
    geoJsonAvailable: false,
    footnotes: [],
    boundaryLevels: [],
    locations: [
      {
        label: 'Barnet',
        value: 'barnet',
        level: 'localAuthority',
      },
      {
        label: 'Barnsley',
        value: 'barnsley',
        level: 'localAuthority',
      },
    ],
    timePeriodRange: [
      {
        label: '2019/20',
        year: 2019,
        code: 'AY',
      },
      {
        label: '2020/21',
        year: 2020,
        code: 'AY',
      },
    ],
    indicators: [
      {
        value: 'authorised-absence-sessions',
        label: 'Number of authorised absence sessions',
        unit: '',
        name: 'sess_authorised',
        decimalPlaces: 2,
      },
      {
        value: 'unauthorised-absence-sessions',
        label: 'Number of unauthorised absence sessions',
        unit: '',
        name: 'sess_unauthorised',
        decimalPlaces: 2,
      },
    ],
    filters: {
      Characteristic: {
        legend: 'Characteristic',
        name: 'characteristic',
        options: {
          gender: {
            label: 'Gender',
            options: [
              {
                value: 'male',
                label: 'Male',
              },
              {
                value: 'female',
                label: 'Female',
              },
            ],
          },
        },
      },
    },
  });

  const testFormState: ChartBuilderForms = {
    options: {
      isValid: true,
      submitCount: 0,
      id: 'options',
      title: 'Options',
    },
    legend: {
      isValid: true,
      submitCount: 0,
      id: 'legend',
      title: 'Legend',
    },
    data: {
      isValid: true,
      submitCount: 0,
      id: 'data',
      title: 'Data',
    },
  };

  test('renders data set labels correctly', () => {
    render(
      <ChartDataSetsConfiguration
        meta={testSubjectMeta}
        forms={testFormState}
        dataSets={[
          {
            indicator: 'authorised-absence-sessions',
            filters: ['male'],
          },
          {
            indicator: 'authorised-absence-sessions',
            filters: ['male'],
            location: {
              level: 'localAuthority',
              value: 'barnet',
            },
          },
          {
            indicator: 'authorised-absence-sessions',
            filters: ['male'],
            location: {
              level: 'localAuthority',
              value: 'barnet',
            },
            timePeriod: '2019_AY',
          },
        ]}
        onSubmit={noop}
      />,
    );

    const rows = screen.getAllByRole('row');
    expect(rows).toHaveLength(4);

    expect(rows[0].children[0]).toHaveTextContent('Data set');
    expect(rows[1].children[0]).toHaveTextContent(
      'Number of authorised absence sessions (Male, All locations, All time periods)',
    );
    expect(rows[2].children[0]).toHaveTextContent(
      'Number of authorised absence sessions (Male, Barnet, All time periods)',
    );
    expect(rows[3].children[0]).toHaveTextContent(
      'Number of authorised absence sessions (Male, Barnet, 2019/20)',
    );
  });

  test('submitting fails if another form is invalid', async () => {
    const handleSubmit = jest.fn();

    render(
      <ChartDataSetsConfiguration
        meta={testSubjectMeta}
        forms={{
          ...testFormState,
          data: {
            ...testFormState.data,
            submitCount: 1,
          },
          options: {
            ...testFormState.options,
            isValid: false,
          },
        }}
        dataSets={[
          {
            indicator: 'authorised-absence-sessions',
            filters: ['male'],
          },
        ]}
        onSubmit={handleSubmit}
      />,
    );

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save chart options' }));

    await waitFor(() => {
      expect(screen.getByText('Cannot save chart')).toBeInTheDocument();
      expect(screen.getByText('Options tab is invalid')).toBeInTheDocument();

      expect(handleSubmit).toHaveBeenCalled();
    });
  });

  describe('add data set form', () => {
    test('renders correctly with multiple options per select', () => {
      render(
        <ChartDataSetsConfiguration
          meta={testSubjectMeta}
          forms={testFormState}
          onSubmit={noop}
        />,
      );

      const characteristic = screen.getByLabelText('Characteristic');
      expect(characteristic).toBeInTheDocument();

      const characteristicOptions = within(characteristic).getAllByRole(
        'option',
      );
      expect(characteristicOptions).toHaveLength(3);
      expect(characteristicOptions[0]).toHaveTextContent(
        'Select characteristic',
      );
      expect(characteristicOptions[1]).toHaveTextContent('Female');
      expect(characteristicOptions[2]).toHaveTextContent('Male');

      const indicator = screen.getByLabelText('Indicator');
      expect(indicator).toBeInTheDocument();

      const indicatorOptions = within(indicator).getAllByRole('option');
      expect(indicatorOptions).toHaveLength(3);
      expect(indicatorOptions[0]).toHaveTextContent('Select indicator');
      expect(indicatorOptions[1]).toHaveTextContent(
        'Number of authorised absence sessions',
      );
      expect(indicatorOptions[2]).toHaveTextContent(
        'Number of unauthorised absence sessions',
      );

      const location = screen.getByLabelText('Location');
      expect(location).toBeInTheDocument();

      const locationOptions = within(location).getAllByRole('option');
      expect(locationOptions).toHaveLength(3);
      expect(locationOptions[0]).toHaveTextContent('All locations');
      expect(locationOptions[1]).toHaveTextContent('Barnet');
      expect(locationOptions[2]).toHaveTextContent('Barnsley');

      const timePeriod = screen.getByLabelText('Time period');
      expect(timePeriod).toBeInTheDocument();

      const timePeriodOptions = within(timePeriod).getAllByRole('option');
      expect(timePeriodOptions).toHaveLength(3);
      expect(timePeriodOptions[0]).toHaveTextContent('All time periods');
      expect(timePeriodOptions[1]).toHaveTextContent('2019/20');
      expect(timePeriodOptions[2]).toHaveTextContent('2020/21');
    });

    test('does not render indicator when there is no option', () => {
      render(
        <ChartDataSetsConfiguration
          meta={{
            ...testSubjectMeta,
            indicators: [],
          }}
          forms={testFormState}
          onSubmit={noop}
        />,
      );

      expect(screen.queryByLabelText('Indicator')).not.toBeInTheDocument();
    });

    test('does not render indicator when there is only one option', () => {
      render(
        <ChartDataSetsConfiguration
          meta={{
            ...testSubjectMeta,
            indicators: [testSubjectMeta.indicators[0]],
          }}
          forms={testFormState}
          onSubmit={noop}
        />,
      );

      expect(screen.queryByLabelText('Indicator')).not.toBeInTheDocument();
    });

    test('does not render location when there is no option', () => {
      render(
        <ChartDataSetsConfiguration
          meta={{
            ...testSubjectMeta,
            locations: [],
          }}
          forms={testFormState}
          onSubmit={noop}
        />,
      );

      expect(screen.queryByLabelText('Location')).not.toBeInTheDocument();
    });

    test('does not render location when there is only one option', () => {
      render(
        <ChartDataSetsConfiguration
          meta={{
            ...testSubjectMeta,
            locations: [testSubjectMeta.locations[0]],
          }}
          forms={testFormState}
          onSubmit={noop}
        />,
      );

      expect(screen.queryByLabelText('Location')).not.toBeInTheDocument();
    });

    test('does not render time period when there is no option', () => {
      render(
        <ChartDataSetsConfiguration
          meta={{
            ...testSubjectMeta,
            timePeriodRange: [],
          }}
          forms={testFormState}
          onSubmit={noop}
        />,
      );

      expect(screen.queryByLabelText('Time period')).not.toBeInTheDocument();
    });

    test('does not render time period when there is only one option', () => {
      render(
        <ChartDataSetsConfiguration
          meta={{
            ...testSubjectMeta,
            timePeriodRange: [testSubjectMeta.timePeriodRange[0]],
          }}
          forms={testFormState}
          onSubmit={noop}
        />,
      );

      expect(screen.queryByLabelText('Time period')).not.toBeInTheDocument();
    });

    test('shows validation errors when no values have been selected', async () => {
      const handleDataAdded = jest.fn();

      render(
        <ChartDataSetsConfiguration
          meta={testSubjectMeta}
          forms={testFormState}
          onDataAdded={handleDataAdded}
          onSubmit={noop}
        />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Add data set' }));

      expect(handleDataAdded).not.toHaveBeenCalled();

      await waitFor(() => {
        expect(
          screen.getByRole('link', { name: 'Choose indicator' }),
        ).toHaveAttribute('href', '#chartDataSetsConfigurationForm-indicator');

        expect(
          screen.getByRole('link', { name: 'Choose characteristic' }),
        ).toHaveAttribute(
          'href',
          '#chartDataSetsConfigurationForm-filtersCharacteristic',
        );

        expect(handleDataAdded).not.toHaveBeenCalled();
      });
    });

    test('shows submit error if data set already exists', async () => {
      const handleDataAdded = jest.fn();

      render(
        <ChartDataSetsConfiguration
          meta={testSubjectMeta}
          dataSets={[
            {
              filters: ['male'],
              indicator: 'unauthorised-absence-sessions',
            },
          ]}
          forms={testFormState}
          onDataAdded={handleDataAdded}
          onSubmit={noop}
        />,
      );

      userEvent.selectOptions(screen.getByLabelText('Characteristic'), 'male');
      userEvent.selectOptions(
        screen.getByLabelText('Indicator'),
        'unauthorised-absence-sessions',
      );

      expect(handleDataAdded).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Add data set' }));

      await waitFor(() => {
        expect(
          screen.getByRole('link', {
            name: 'The selected options have already been added to the chart',
          }),
        ).toHaveAttribute('href', '#chartDataSetsConfigurationForm-submit');
      });

      expect(handleDataAdded).not.toHaveBeenCalled();
    });

    test('successfully submits by selecting only indicator and filter', async () => {
      const handleDataAdded = jest.fn();

      render(
        <ChartDataSetsConfiguration
          meta={testSubjectMeta}
          forms={testFormState}
          onDataAdded={handleDataAdded}
          onSubmit={noop}
        />,
      );

      userEvent.selectOptions(screen.getByLabelText('Characteristic'), 'male');
      userEvent.selectOptions(
        screen.getByLabelText('Indicator'),
        'unauthorised-absence-sessions',
      );

      expect(handleDataAdded).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Add data set' }));

      await waitFor(() => {
        const expectedValues: DataSet = {
          filters: ['male'],
          indicator: 'unauthorised-absence-sessions',
        };

        expect(handleDataAdded).toHaveBeenCalledWith(expectedValues);
      });
    });

    test('successfully submits by selecting all options', async () => {
      const handleDataAdded = jest.fn();

      render(
        <ChartDataSetsConfiguration
          meta={testSubjectMeta}
          forms={testFormState}
          onDataAdded={handleDataAdded}
          onSubmit={noop}
        />,
      );

      userEvent.selectOptions(screen.getByLabelText('Characteristic'), 'male');
      userEvent.selectOptions(
        screen.getByLabelText('Indicator'),
        'unauthorised-absence-sessions',
      );
      userEvent.selectOptions(
        screen.getByLabelText('Location'),
        LocationFilter.createId({
          value: 'barnsley',
          level: 'localAuthority',
        }),
      );
      userEvent.selectOptions(screen.getByLabelText('Time period'), '2020_AY');

      expect(handleDataAdded).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Add data set' }));

      await waitFor(() => {
        const expectedValues: DataSet = {
          filters: ['male'],
          indicator: 'unauthorised-absence-sessions',
          location: {
            value: 'barnsley',
            level: 'localAuthority',
          },
          timePeriod: '2020_AY',
        };

        expect(handleDataAdded).toHaveBeenCalledWith(expectedValues);
      });
    });

    test('successfully submits by selecting only indicator when there are no filters', async () => {
      const handleDataAdded = jest.fn();

      render(
        <ChartDataSetsConfiguration
          meta={{
            ...testSubjectMeta,
            filters: {},
          }}
          forms={testFormState}
          onDataAdded={handleDataAdded}
          onSubmit={noop}
        />,
      );

      userEvent.selectOptions(
        screen.getByLabelText('Indicator'),
        'unauthorised-absence-sessions',
      );

      expect(handleDataAdded).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Add data set' }));

      await waitFor(() => {
        const expectedValues: DataSet = {
          filters: [],
          indicator: 'unauthorised-absence-sessions',
        };

        expect(handleDataAdded).toHaveBeenCalledWith(expectedValues);
      });
    });
  });
});
