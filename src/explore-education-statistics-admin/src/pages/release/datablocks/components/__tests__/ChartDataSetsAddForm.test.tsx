import ChartDataSetsAddForm, {
  ChartDataSetsAddFormValues,
} from '@admin/pages/release/datablocks/components/ChartDataSetsAddForm';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { TableDataResponse } from '@common/services/tableBuilderService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('ChartDataSetsAddForm', () => {
  const testTableData: TableDataResponse = {
    results: [],
    subjectMeta: {
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
    },
  };

  test('renders correctly with multiple options per select', () => {
    const { subjectMeta } = mapFullTable(testTableData);

    render(<ChartDataSetsAddForm meta={subjectMeta} onSubmit={noop} />);

    const characteristic = screen.getByLabelText('Characteristic');
    expect(characteristic).toBeInTheDocument();

    const characteristicOptions = within(characteristic).getAllByRole('option');
    expect(characteristicOptions).toHaveLength(3);
    expect(characteristicOptions[0]).toHaveTextContent('Select characteristic');
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
    expect(locationOptions[0]).toHaveTextContent('Any location');
    expect(locationOptions[1]).toHaveTextContent('Barnet');
    expect(locationOptions[2]).toHaveTextContent('Barnsley');

    const timePeriod = screen.getByLabelText('Time period');
    expect(timePeriod).toBeInTheDocument();

    const timePeriodOptions = within(timePeriod).getAllByRole('option');
    expect(timePeriodOptions).toHaveLength(3);
    expect(timePeriodOptions[0]).toHaveTextContent('Any time period');
    expect(timePeriodOptions[1]).toHaveTextContent('2019/20');
    expect(timePeriodOptions[2]).toHaveTextContent('2020/21');
  });

  test('does not render indicator when there is no option', () => {
    const { subjectMeta } = mapFullTable(testTableData);

    render(
      <ChartDataSetsAddForm
        meta={{
          ...subjectMeta,
          indicators: [],
        }}
        onSubmit={noop}
      />,
    );

    expect(screen.queryByLabelText('Indicator')).not.toBeInTheDocument();
  });

  test('does not render indicator when there is only one option', () => {
    const { subjectMeta } = mapFullTable(testTableData);

    render(
      <ChartDataSetsAddForm
        meta={{
          ...subjectMeta,
          indicators: [subjectMeta.indicators[0]],
        }}
        onSubmit={noop}
      />,
    );

    expect(screen.queryByLabelText('Indicator')).not.toBeInTheDocument();
  });

  test('does not render location when there is no option', () => {
    const { subjectMeta } = mapFullTable(testTableData);

    render(
      <ChartDataSetsAddForm
        meta={{
          ...subjectMeta,
          locations: [],
        }}
        onSubmit={noop}
      />,
    );

    expect(screen.queryByLabelText('Location')).not.toBeInTheDocument();
  });

  test('does not render location when there is only one option', () => {
    const { subjectMeta } = mapFullTable(testTableData);

    render(
      <ChartDataSetsAddForm
        meta={{
          ...subjectMeta,
          locations: [subjectMeta.locations[0]],
        }}
        onSubmit={noop}
      />,
    );

    expect(screen.queryByLabelText('Location')).not.toBeInTheDocument();
  });

  test('does not render time period when there is no option', () => {
    const { subjectMeta } = mapFullTable(testTableData);

    render(
      <ChartDataSetsAddForm
        meta={{
          ...subjectMeta,
          timePeriodRange: [],
        }}
        onSubmit={noop}
      />,
    );

    expect(screen.queryByLabelText('Time period')).not.toBeInTheDocument();
  });

  test('does not render time period when there is only one option', () => {
    const { subjectMeta } = mapFullTable(testTableData);

    render(
      <ChartDataSetsAddForm
        meta={{
          ...subjectMeta,
          timePeriodRange: [subjectMeta.timePeriodRange[0]],
        }}
        onSubmit={noop}
      />,
    );

    expect(screen.queryByLabelText('Time period')).not.toBeInTheDocument();
  });

  describe('submitting form', () => {
    test('shows correct error messages when no values have been selected', async () => {
      const { subjectMeta } = mapFullTable(testTableData);

      const handleSubmit = jest.fn();

      render(
        <ChartDataSetsAddForm meta={subjectMeta} onSubmit={handleSubmit} />,
      );

      userEvent.click(screen.getByRole('button', { name: 'Add data' }));

      expect(handleSubmit).not.toHaveBeenCalled();

      await waitFor(() => {
        expect(
          screen.getByText('Select an indicator', {
            selector: '#chartDataSetsAddForm-indicator-error',
          }),
        ).toBeInTheDocument();

        expect(
          screen.getByText('Select a characteristic', {
            selector: '#chartDataSetsAddForm-filtersCharacteristic-error',
          }),
        ).toBeInTheDocument();

        expect(handleSubmit).not.toHaveBeenCalled();
      });
    });

    test('successfully submits by selecting only indicator and filter', async () => {
      const { subjectMeta } = mapFullTable(testTableData);

      const handleSubmit = jest.fn();

      render(
        <ChartDataSetsAddForm meta={subjectMeta} onSubmit={handleSubmit} />,
      );

      userEvent.selectOptions(screen.getByLabelText('Characteristic'), 'male');
      userEvent.selectOptions(
        screen.getByLabelText('Indicator'),
        'unauthorised-absence-sessions',
      );

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Add data' }));

      await waitFor(() => {
        const expectedValues: ChartDataSetsAddFormValues = {
          filters: {
            Characteristic: 'male',
          },
          indicator: 'unauthorised-absence-sessions',
          location: '',
          timePeriod: '',
        };

        expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
      });
    });

    test('successfully submits by selecting all options', async () => {
      const { subjectMeta } = mapFullTable(testTableData);

      const handleSubmit = jest.fn();

      render(
        <ChartDataSetsAddForm meta={subjectMeta} onSubmit={handleSubmit} />,
      );

      const locationId = LocationFilter.createId({
        value: 'barnsley',
        level: 'localAuthority',
      });

      userEvent.selectOptions(screen.getByLabelText('Characteristic'), 'male');
      userEvent.selectOptions(
        screen.getByLabelText('Indicator'),
        'unauthorised-absence-sessions',
      );
      userEvent.selectOptions(screen.getByLabelText('Location'), locationId);
      userEvent.selectOptions(screen.getByLabelText('Time period'), '2020_AY');

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Add data' }));

      await waitFor(() => {
        const expectedValues: ChartDataSetsAddFormValues = {
          filters: {
            Characteristic: 'male',
          },
          indicator: 'unauthorised-absence-sessions',
          location: locationId,
          timePeriod: '2020_AY',
        };

        expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
      });
    });

    test('successfully submits by selecting only indicator when there are no filters', async () => {
      const { subjectMeta } = mapFullTable(testTableData);

      const handleSubmit = jest.fn();

      render(
        <ChartDataSetsAddForm
          meta={{
            ...subjectMeta,
            filters: {},
          }}
          onSubmit={handleSubmit}
        />,
      );

      userEvent.selectOptions(
        screen.getByLabelText('Indicator'),
        'unauthorised-absence-sessions',
      );

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Add data' }));

      await waitFor(() => {
        const expectedValues: ChartDataSetsAddFormValues = {
          filters: {},
          indicator: 'unauthorised-absence-sessions',
          location: '',
          timePeriod: '',
        };

        expect(handleSubmit).toHaveBeenCalledWith(expectedValues);
      });
    });
  });
});
