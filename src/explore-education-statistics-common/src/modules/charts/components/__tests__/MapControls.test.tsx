import { testGeoJsonFeature } from '@common/modules/charts/components/__tests__/__data__/testMapBlockData';
import MapControls from '@common/modules/charts/components/MapControls';
import { MapDataSetCategory } from '@common/modules/charts/components/utils/createMapDataSetCategories';
import {
  Indicator,
  LocationFilter,
} from '@common/modules/table-tool/types/filters';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { DataSet } from '@common/modules/charts/types/dataSet';
import { SelectOption } from '@common/components/form/FormSelect';
import { within } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import noop from 'lodash/noop';

describe('MapControls', () => {
  const testIndicator1 = new Indicator({
    label: 'Indicator 1',
    name: 'indicator-1-name',
    unit: '',
    value: 'indicator-1',
  });

  const testIndicator2 = new Indicator({
    label: 'Indicator 2',
    name: 'indicator-2-name',
    unit: '',
    value: 'indicator-2',
  });

  const testLocation1 = new LocationFilter({
    id: 'location-1-id',
    value: 'location-1-value',
    label: 'Location 1',
    group: 'LA group 1',
    level: 'localAuthority',
    geoJson: testGeoJsonFeature,
  });
  const testLocation2 = new LocationFilter({
    id: 'location-2-id',
    value: 'location-2-value',
    label: 'Location 2',
    group: 'LA group 1',
    level: 'localAuthority',
    geoJson: testGeoJsonFeature,
  });
  const testLocation3 = new LocationFilter({
    id: 'location-3-id',
    value: 'location-3-value',
    label: 'Location 3',
    group: 'Region group 1',
    level: 'region',
    geoJson: testGeoJsonFeature,
  });
  const testLocation4 = new LocationFilter({
    id: 'location-4-id',
    value: 'location-4-value',
    label: 'Location 4',
    group: 'Region group 1',
    level: 'region',
    geoJson: testGeoJsonFeature,
  });
  const testLocation5 = new LocationFilter({
    id: 'location-5-id',
    value: 'location-5-value',
    label: 'Location 5',
    group: 'Country group',
    level: 'country',
    geoJson: testGeoJsonFeature,
  });
  const testLocation6 = new LocationFilter({
    id: 'location-6-id',
    value: 'location-6-value',
    label: 'Location 6',
    group: 'LAD group 1',
    level: 'localAuthorityDistrict',
    geoJson: testGeoJsonFeature,
  });
  const testLocation7 = new LocationFilter({
    id: 'location-7-id',
    value: 'location-7-value',
    label: 'Location 7',
    group: 'LAD group 1',
    level: 'localAuthorityDistrict',
    geoJson: testGeoJsonFeature,
  });

  const testDataSet1: DataSet = {
    filters: ['filter-id'],
    indicator: testIndicator1.id,
    timePeriod: 'time-period-id',
    location: {
      level: testLocation1.level,
      value: testLocation1.value,
    },
  };

  const testDataSet2: DataSet = {
    filters: ['filter-id'],
    indicator: testIndicator2.id,
    timePeriod: 'time-period-id',
    location: {
      level: testLocation1.level,
      value: testLocation1.value,
    },
  };
  const testDataSet3: DataSet = {
    filters: ['filter-id'],
    indicator: testIndicator1.id,
    timePeriod: 'time-period-id',
    location: {
      level: testLocation3.level,
      value: testLocation3.value,
    },
  };
  const testDataSet4: DataSet = {
    filters: ['filter-id'],
    indicator: testIndicator1.id,
    timePeriod: 'time-period-id',
    location: {
      level: testLocation4.level,
      value: testLocation4.value,
    },
  };
  const testDataSet5: DataSet = {
    filters: ['filter-id'],
    indicator: testIndicator1.id,
    timePeriod: 'time-period-id',
    location: {
      level: testLocation5.level,
      value: testLocation5.value,
    },
  };
  const testDataSet6: DataSet = {
    filters: ['filter-id'],
    indicator: testIndicator1.id,
    timePeriod: 'time-period-id',
    location: {
      level: testLocation6.level,
      value: testLocation6.value,
    },
  };
  const testDataSet7: DataSet = {
    filters: ['filter-id'],
    indicator: testIndicator1.id,
    timePeriod: 'time-period-id',
    location: {
      level: testLocation7.level,
      value: testLocation7.value,
    },
  };
  const testDataSetKey1 = generateDataSetKey(testDataSet1, testIndicator1);
  const testDataSetKey2 = generateDataSetKey(testDataSet2, testIndicator2);
  const testDataSetKey3 = generateDataSetKey(testDataSet3, testIndicator1);
  const testDataSetKey4 = generateDataSetKey(testDataSet4, testIndicator1);
  const testDataSetKey5 = generateDataSetKey(testDataSet5, testIndicator1);
  const testDataSetKey6 = generateDataSetKey(testDataSet6, testIndicator1);
  const testDataSetKey7 = generateDataSetKey(testDataSet7, testIndicator1);

  const testDataSetCategories: MapDataSetCategory[] = [
    {
      dataSets: {
        [testDataSetKey1]: {
          dataSet: testDataSet1,
          value: 30,
        },
      },
      filter: testLocation1,
      geoJson: testGeoJsonFeature,
    },
    {
      dataSets: {
        [testDataSetKey2]: {
          dataSet: testDataSet2,
          value: 30,
        },
      },
      filter: testLocation2,
      geoJson: testGeoJsonFeature,
    },
  ];

  const testDataSetOptions: SelectOption[] = [
    { label: 'Data set 1', value: 'data-set-1' },
    { label: 'Data set 2', value: 'data-set-2' },
  ];

  test('renders the controls', () => {
    render(
      <MapControls
        dataSetCategories={testDataSetCategories}
        dataSetOptions={testDataSetOptions}
        id="test-id"
        selectedDataSetKey={testDataSetKey1}
        onChangeDataSet={noop}
        onChangeLocation={noop}
      />,
    );

    const dataSetsSelect = screen.getByLabelText('1. Select data to view');
    const dataSetOptions = within(dataSetsSelect).getAllByRole('option');
    expect(dataSetOptions).toHaveLength(2);
    expect(dataSetOptions[0]).toHaveTextContent('Data set 1');
    expect(dataSetOptions[1]).toHaveTextContent('Data set 2');

    const locationsSelect = screen.getByLabelText(
      '2. Select a Local Authority',
    );
    const locationOptions = within(locationsSelect).getAllByRole('option');
    expect(locationOptions).toHaveLength(3);
    expect(locationOptions[0]).toHaveTextContent('None selected');
    expect(locationOptions[1]).toHaveTextContent('Location 1');
    expect(locationOptions[2]).toHaveTextContent('Location 2');
  });

  test('calls `onChangeDataSet` when select a data set', async () => {
    const handleChangeDataSet = jest.fn();
    render(
      <MapControls
        dataSetCategories={testDataSetCategories}
        dataSetOptions={testDataSetOptions}
        id="test-id"
        selectedDataSetKey={testDataSetKey1}
        onChangeDataSet={handleChangeDataSet}
        onChangeLocation={noop}
      />,
    );

    expect(handleChangeDataSet).not.toHaveBeenCalled();
    await userEvent.selectOptions(
      screen.getByLabelText('1. Select data to view'),
      'Data set 2',
    );
    expect(handleChangeDataSet).toHaveBeenCalledWith('data-set-2');
  });

  test('calls `onChangeLocation` when select a data set', async () => {
    const handleChangeLocation = jest.fn();
    render(
      <MapControls
        dataSetCategories={testDataSetCategories}
        dataSetOptions={testDataSetOptions}
        id="test-id"
        selectedDataSetKey={testDataSetKey1}
        onChangeDataSet={noop}
        onChangeLocation={handleChangeLocation}
      />,
    );

    expect(handleChangeLocation).not.toHaveBeenCalled();
    await userEvent.selectOptions(
      screen.getByLabelText('2. Select a Local Authority'),
      'Location 2',
    );
    expect(handleChangeLocation).toHaveBeenCalledWith(
      JSON.stringify({ level: 'localAuthority', value: 'location-2-id' }),
    );
  });

  test('selects the location from the initial value', async () => {
    render(
      <MapControls
        dataSetCategories={testDataSetCategories}
        dataSetOptions={testDataSetOptions}
        id="test-id"
        selectedDataSetKey={testDataSetKey1}
        selectedLocation={JSON.stringify({
          level: 'localAuthority',
          value: 'location-2-id',
        })}
        onChangeDataSet={noop}
        onChangeLocation={noop}
      />,
    );

    const locationsSelect = screen.getByLabelText(
      '2. Select a Local Authority',
    );

    const locationOptions = within(locationsSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];
    expect(locationOptions).toHaveLength(3);
    expect(locationOptions[0]).toHaveTextContent('None selected');
    expect(locationOptions[1]).toHaveTextContent('Location 1');
    expect(locationOptions[2]).toHaveTextContent('Location 2');
    expect(locationOptions[2].selected).toBe(true);
  });

  test('shows the data set location type in the label', () => {
    const testDataSetCategoriesWithRegions: MapDataSetCategory[] = [
      {
        dataSets: {
          [testDataSetKey3]: {
            dataSet: testDataSet3,
            value: 30,
          },
        },
        filter: testLocation3,
        geoJson: testGeoJsonFeature,
      },
    ];

    const testDataSetOptions2: SelectOption[] = [
      { label: 'Data set 3', value: 'data-set-3' },
    ];

    render(
      <MapControls
        dataSetCategories={testDataSetCategoriesWithRegions}
        dataSetOptions={testDataSetOptions2}
        id="test-id"
        selectedDataSetKey={testDataSetKey3}
        onChangeDataSet={noop}
        onChangeLocation={noop}
      />,
    );

    expect(screen.getByLabelText('2. Select a Region')).toBeInTheDocument();
  });

  test('shows the default label if the data set contains multiple types', () => {
    const testDataSetCategoriesWithMixedLocations: MapDataSetCategory[] = [
      {
        dataSets: {
          [testDataSetKey1]: {
            dataSet: testDataSet1,
            value: 30,
          },
        },
        filter: testLocation1,
        geoJson: testGeoJsonFeature,
      },
      {
        dataSets: {
          [testDataSetKey3]: {
            dataSet: testDataSet3,
            value: 30,
          },
        },
        filter: testLocation3,
        geoJson: testGeoJsonFeature,
      },
    ];

    const testDataSetOptions2: SelectOption[] = [
      { label: 'Data set 3', value: 'data-set-3' },
    ];
    render(
      <MapControls
        dataSetCategories={testDataSetCategoriesWithMixedLocations}
        dataSetOptions={testDataSetOptions2}
        id="test-id"
        selectedDataSetKey={testDataSetKey3}
        onChangeDataSet={noop}
        onChangeLocation={noop}
      />,
    );

    expect(screen.getByLabelText('2. Select a location')).toBeInTheDocument();
  });

  test('shows ungrouped location options if no local authorities or LADs', () => {
    const testDataSetCategoriesWithRegions: MapDataSetCategory[] = [
      {
        dataSets: {
          [testDataSetKey3]: {
            dataSet: testDataSet3,
            value: 30,
          },
        },
        filter: testLocation3,
        geoJson: testGeoJsonFeature,
      },
      {
        dataSets: {
          [testDataSetKey4]: {
            dataSet: testDataSet4,
            value: 30,
          },
        },
        filter: testLocation4,
        geoJson: testGeoJsonFeature,
      },
    ];

    render(
      <MapControls
        dataSetCategories={testDataSetCategoriesWithRegions}
        dataSetOptions={testDataSetOptions}
        id="test-id"
        selectedDataSetKey={testDataSetKey1}
        onChangeDataSet={noop}
        onChangeLocation={noop}
      />,
    );

    const locationsSelect = screen.getByLabelText('2. Select a Region');
    const locationOptions = within(locationsSelect).getAllByRole('option');
    expect(locationOptions).toHaveLength(3);
    expect(locationOptions[0]).toHaveTextContent('None selected');
    expect(locationOptions[1]).toHaveTextContent('Location 3');
    expect(locationOptions[2]).toHaveTextContent('Location 4');

    expect(
      within(locationsSelect).queryByRole('group'),
    ).not.toBeInTheDocument();
  });

  test('shows grouped location options if there are local authorities', () => {
    const testDataSetCategoriesWithLAs: MapDataSetCategory[] = [
      {
        dataSets: {
          [testDataSetKey1]: {
            dataSet: testDataSet1,
            value: 30,
          },
        },
        filter: testLocation1,
        geoJson: testGeoJsonFeature,
      },
      {
        dataSets: {
          [testDataSetKey2]: {
            dataSet: testDataSet2,
            value: 30,
          },
        },
        filter: testLocation2,
        geoJson: testGeoJsonFeature,
      },
      {
        dataSets: {
          [testDataSetKey3]: {
            dataSet: testDataSet3,
            value: 30,
          },
        },
        filter: testLocation3,
        geoJson: testGeoJsonFeature,
      },
      {
        dataSets: {
          [testDataSetKey4]: {
            dataSet: testDataSet4,
            value: 30,
          },
        },
        filter: testLocation4,
        geoJson: testGeoJsonFeature,
      },
      {
        dataSets: {
          [testDataSetKey5]: {
            dataSet: testDataSet5,
            value: 30,
          },
        },
        filter: testLocation5,
        geoJson: testGeoJsonFeature,
      },
    ];

    render(
      <MapControls
        dataSetCategories={testDataSetCategoriesWithLAs}
        dataSetOptions={testDataSetOptions}
        id="test-id"
        selectedDataSetKey={testDataSetKey1}
        onChangeDataSet={noop}
        onChangeLocation={noop}
      />,
    );

    const locationsSelect = screen.getByLabelText('2. Select a location');
    const locationOptions = within(locationsSelect).getAllByRole('option');
    expect(locationOptions).toHaveLength(6);
    expect(locationOptions[0]).toHaveTextContent('None selected');

    const groups = within(locationsSelect).queryAllByRole('group');

    expect(groups).toHaveLength(3);
    expect(groups[0]).toHaveProperty('label', 'Country');
    const group1Options = within(groups[0]).getAllByRole('option');
    expect(group1Options).toHaveLength(1);
    expect(group1Options[0]).toHaveTextContent('Location 5');

    expect(groups[1]).toHaveProperty('label', 'LA group 1');
    const group2Options = within(groups[1]).getAllByRole('option');
    expect(group2Options).toHaveLength(2);
    expect(group2Options[0]).toHaveTextContent('Location 1');
    expect(group2Options[1]).toHaveTextContent('Location 2');

    expect(groups[2]).toHaveProperty('label', 'Region');
    const group3Options = within(groups[2]).getAllByRole('option');
    expect(group3Options).toHaveLength(2);
    expect(group3Options[0]).toHaveTextContent('Location 3');
    expect(group3Options[1]).toHaveTextContent('Location 4');
  });

  test('shows grouped location options if there are local authority districts', () => {
    const testDataSetCategoriesWithLADs: MapDataSetCategory[] = [
      {
        dataSets: {
          [testDataSetKey6]: {
            dataSet: testDataSet6,
            value: 30,
          },
        },
        filter: testLocation6,
        geoJson: testGeoJsonFeature,
      },
      {
        dataSets: {
          [testDataSetKey7]: {
            dataSet: testDataSet7,
            value: 30,
          },
        },
        filter: testLocation7,
        geoJson: testGeoJsonFeature,
      },
    ];

    render(
      <MapControls
        dataSetCategories={testDataSetCategoriesWithLADs}
        dataSetOptions={testDataSetOptions}
        id="test-id"
        selectedDataSetKey={testDataSetKey1}
        onChangeDataSet={noop}
        onChangeLocation={noop}
      />,
    );

    const locationsSelect = screen.getByLabelText(
      '2. Select a Local Authority District',
    );
    const locationOptions = within(locationsSelect).getAllByRole('option');
    expect(locationOptions).toHaveLength(3);
    expect(locationOptions[0]).toHaveTextContent('None selected');

    const groups = within(locationsSelect).queryAllByRole('group');

    expect(groups).toHaveLength(1);
    expect(groups[0]).toHaveProperty('label', 'LAD group 1');
    const group1Options = within(groups[0]).getAllByRole('option');
    expect(group1Options).toHaveLength(2);
    expect(group1Options[0]).toHaveTextContent('Location 6');
    expect(group1Options[1]).toHaveTextContent('Location 7');
  });
});
