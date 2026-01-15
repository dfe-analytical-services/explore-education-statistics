import render from '@common-test/render';
import React from 'react';
import {
  testCategoricalMapConfiguration,
  testGeoJsonFeature,
  testMapConfiguration,
  testMapTableData,
} from '@common/modules/charts/components/__tests__/__data__/testMapBlockData';
import {
  testMixedMapConfiguration,
  testMixedData,
  testMixedMeta,
} from '@common/modules/charts/components/__tests__/__data__/testMixedMapData';
import MapBlock, {
  MapBlockProps,
} from '@common/modules/charts/components/MapBlock';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { within } from '@testing-library/dom';
import { screen, waitFor } from '@testing-library/react';
import { produce } from 'immer';
import {
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';

describe('MapBlock', () => {
  const testFullTable = mapFullTable(testMapTableData);
  const onBoundaryLevelChange = jest.fn();
  const testBlockProps: MapBlockProps = {
    ...testMapConfiguration,
    boundaryLevel: 1,
    id: 'testMap',
    axes: testMapConfiguration.axes as MapBlockProps['axes'],
    legend: testMapConfiguration.legend as LegendConfiguration,
    meta: testFullTable.subjectMeta,
    data: testFullTable.results,
    height: 600,
    onBoundaryLevelChange,
  };

  test('renders legends and polygons correctly', async () => {
    const { container } = render(<MapBlock {...testBlockProps} />);

    await waitFor(() => {
      const paths = container.querySelectorAll<HTMLElement>(
        '.leaflet-container svg:not(.leaflet-attribution-flag) path',
      );

      expect(paths).toHaveLength(4);

      // Location polygons
      expect(paths[0]).toHaveAttribute('fill', 'rgba(145, 161, 201, 1)');
      expect(paths[1]).toHaveAttribute('fill', 'rgba(218, 224, 237, 1)');
      expect(paths[2]).toHaveAttribute('fill', 'rgba(71, 99, 165, 1)');
      // UK polygon
      expect(paths[3]).toHaveAttribute('fill', '#003078');
    });

    expect(
      await screen.findByLabelText('1. Select data to view'),
    ).toBeInTheDocument();

    const legendItems = screen.getAllByTestId('mapBlock-legend-item');

    expect(legendItems).toHaveLength(5);
    expect(legendItems[0]).toHaveTextContent('3.0% to 3.1%');
    expect(legendItems[1]).toHaveTextContent('3.2% to 3.3%');
    expect(legendItems[2]).toHaveTextContent('3.4% to 3.5%');
    expect(legendItems[3]).toHaveTextContent('3.6% to 3.7%');
    expect(legendItems[4]).toHaveTextContent('3.8% to 4.0%');

    const legendColours = screen.getAllByTestId('mapBlock-legend-colour');

    expect(legendColours).toHaveLength(5);
    expect(legendColours[0].style.backgroundColor).toBe('rgb(218, 224, 237)');
    expect(legendColours[1].style.backgroundColor).toBe('rgb(181, 193, 219)');
    expect(legendColours[2].style.backgroundColor).toBe('rgb(145, 161, 201)');
    expect(legendColours[3].style.backgroundColor).toBe('rgb(108, 130, 183)');
    expect(legendColours[4].style.backgroundColor).toBe('rgb(71, 99, 165)');
  });

  test('renders legends and polygons correctly for categorical data', async () => {
    const testCategoricalMeta: FullTableMeta = {
      filters: {},
      footnotes: [],
      indicators: [
        new Indicator({
          label: 'Indicator 1',
          name: 'indicator-1-name',
          unit: '',
          value: 'indicator-1',
        }),
        new Indicator({
          label: 'Indicator 2',
          name: 'indicator-2-name',
          unit: '',
          value: 'indicator-2',
        }),
      ],
      locations: [
        new LocationFilter({
          value: 'location-1-value',
          label: 'Location 1',
          level: 'region',
          geoJson: testGeoJsonFeature,
        }),
        new LocationFilter({
          value: 'location-2-value',
          label: 'Location 2',
          level: 'region',
          geoJson: testGeoJsonFeature,
        }),
        new LocationFilter({
          value: 'location-3-value',
          label: 'Location 3',
          level: 'region',
          geoJson: testGeoJsonFeature,
        }),
      ],
      boundaryLevels: [
        {
          id: 15,
          label: 'Regions England BUC 2022/12',
        },
      ],
      publicationName: 'Publication 1',
      subjectName: 'Subject 1',
      timePeriodRange: [
        new TimePeriodFilter({
          year: 2024,
          code: 'CY',
          label: '2024',
          order: 0,
        }),
      ],
      geoJsonAvailable: true,
      isCroppedTable: false,
    };

    const testCategoricalData: TableDataResult[] = [
      {
        filters: [],
        geographicLevel: 'region',
        locationId: 'location-1-value',
        measures: {
          'indicator-1': 'small',
        },
        timePeriod: '2024_CY',
      },
      {
        filters: [],
        geographicLevel: 'region',
        locationId: 'location-2-value',
        measures: {
          'indicator-1': 'medium',
        },
        timePeriod: '2024_CY',
      },

      {
        filters: [],
        geographicLevel: 'region',
        locationId: 'location-3-value',
        measures: {
          'indicator-1': 'large',
        },
        timePeriod: '2024_CY',
      },
    ];

    const testCategoricalMapProps: MapBlockProps = {
      ...testCategoricalMapConfiguration,
      boundaryLevel: 1,
      id: 'testMap',
      axes: testCategoricalMapConfiguration.axes as MapBlockProps['axes'],
      legend: testCategoricalMapConfiguration.legend as LegendConfiguration,
      meta: testCategoricalMeta,
      data: testCategoricalData,
      height: 600,
      onBoundaryLevelChange,
    };
    const { container } = render(<MapBlock {...testCategoricalMapProps} />);

    await waitFor(() => {
      const paths = container.querySelectorAll<HTMLElement>(
        '.leaflet-container svg:not(.leaflet-attribution-flag) path',
      );

      expect(paths).toHaveLength(4);

      // Location polygons
      expect(paths[0]).toHaveAttribute('fill', '#12436D');
      expect(paths[1]).toHaveAttribute('fill', '#801650');
      expect(paths[2]).toHaveAttribute('fill', '#28A197');
      // UK polygon
      expect(paths[3]).toHaveAttribute('fill', '#003078');
    });

    expect(
      await screen.findByLabelText('1. Select data to view'),
    ).toBeInTheDocument();

    const legendItems = screen.getAllByTestId('mapBlock-legend-item');

    expect(legendItems).toHaveLength(3);
    expect(legendItems[0]).toHaveTextContent('small');
    expect(legendItems[1]).toHaveTextContent('medium');
    expect(legendItems[2]).toHaveTextContent('large');

    const legendColours = screen.getAllByTestId('mapBlock-legend-colour');

    expect(legendColours).toHaveLength(3);
    expect(legendColours[0].style.backgroundColor).toBe('rgb(18, 67, 109)');
    expect(legendColours[1].style.backgroundColor).toBe('rgb(128, 22, 80)');
    expect(legendColours[2].style.backgroundColor).toBe('rgb(40, 161, 151)');
  });

  test('renders legend groups correctly with custom 1 d.p decimal places', async () => {
    const fullTable = mapFullTable(
      produce(testMapTableData, draft => {
        draft.results[0].measures['authorised-absence-rate'] = '3.5123';
        draft.results[1].measures['authorised-absence-rate'] = '3.012';
        draft.results[2].measures['authorised-absence-rate'] = '4.009';
        draft.subjectMeta.indicators[0].decimalPlaces = 1;
      }),
    );

    render(
      <MapBlock
        {...testBlockProps}
        meta={fullTable.subjectMeta}
        data={fullTable.results}
      />,
    );

    await waitFor(() => {
      const legendItems = screen.getAllByTestId('mapBlock-legend-item');

      expect(legendItems).toHaveLength(5);
      expect(legendItems[0]).toHaveTextContent('3.0% to 3.1%');
      expect(legendItems[1]).toHaveTextContent('3.2% to 3.3%');
      expect(legendItems[2]).toHaveTextContent('3.4% to 3.5%');
      expect(legendItems[3]).toHaveTextContent('3.6% to 3.7%');
      expect(legendItems[4]).toHaveTextContent('3.8% to 4.0%');
    });
  });

  test('renders legend groups correctly with custom 3 d.p decimal places', async () => {
    const fullTable = mapFullTable(
      produce(testMapTableData, draft => {
        draft.results[0].measures['authorised-absence-rate'] = '3.5123';
        draft.results[1].measures['authorised-absence-rate'] = '3.012';
        draft.results[2].measures['authorised-absence-rate'] = '4.009';
        draft.subjectMeta.indicators[0].decimalPlaces = 3;
      }),
    );

    render(
      <MapBlock
        {...testBlockProps}
        meta={fullTable.subjectMeta}
        data={fullTable.results}
      />,
    );

    await waitFor(() => {
      const legendItems = screen.getAllByTestId('mapBlock-legend-item');

      expect(legendItems).toHaveLength(5);
      expect(legendItems[0]).toHaveTextContent('3.012% to 3.211%');
      expect(legendItems[1]).toHaveTextContent('3.212% to 3.411%');
      expect(legendItems[2]).toHaveTextContent('3.412% to 3.611%');
      expect(legendItems[3]).toHaveTextContent('3.612% to 3.811%');
      expect(legendItems[4]).toHaveTextContent('3.812% to 4.009%');
    });
  });

  test('changing selected data set changes legends', async () => {
    const { user } = render(<MapBlock {...testBlockProps} />);

    const select = screen.getByLabelText('1. Select data to view');
    expect(select.children[1]).toHaveTextContent(
      'Overall absence rate (2016/17)',
    );
    await user.selectOptions(select, select.children[1] as HTMLElement);

    const legendItems = screen.getAllByTestId('mapBlock-legend-item');

    expect(legendItems[0]).toHaveTextContent('4.70% to 4.77%');
    expect(legendItems[1]).toHaveTextContent('4.78% to 4.85%');
    expect(legendItems[2]).toHaveTextContent('4.86% to 4.93%');
    expect(legendItems[3]).toHaveTextContent('4.94% to 5.01%');
    expect(legendItems[4]).toHaveTextContent('5.02% to 5.10%');

    const legendColours = screen.getAllByTestId('mapBlock-legend-colour');

    expect(legendColours[0].style.backgroundColor).toBe('rgb(253, 237, 220)');
    expect(legendColours[1].style.backgroundColor).toBe('rgb(251, 219, 185)');
    expect(legendColours[2].style.backgroundColor).toBe('rgb(249, 200, 150)');
    expect(legendColours[3].style.backgroundColor).toBe('rgb(247, 182, 115)');
    expect(legendColours[4].style.backgroundColor).toBe('rgb(245, 164, 80)');
  });

  test('selecting data set with different boundary level calls onBoundaryLevelChange', async () => {
    const { user } = render(<MapBlock {...testBlockProps} />);

    expect(onBoundaryLevelChange).not.toHaveBeenCalled();

    const select = screen.getByLabelText('1. Select data to view');
    const options = within(select).getAllByRole('option');
    expect(options).toHaveLength(2);

    // Selecting another data set with different boundary level
    await user.selectOptions(select, options[1]);
    expect(onBoundaryLevelChange).toHaveBeenCalledWith(2);
  });

  test('changing selected location focuses the correct polygon', async () => {
    const { container, user } = render(<MapBlock {...testBlockProps} />);

    await waitFor(() => {
      expect(
        screen.getByLabelText('2. Select a Local Authority District'),
      ).toBeInTheDocument();
    });

    const select = screen.getByLabelText(
      '2. Select a Local Authority District',
    );

    const groups = within(select).getAllByRole('group');
    const group1Options = within(groups[0]).getAllByRole('option');
    expect(group1Options[0]).toHaveTextContent('Leeds');

    await user.selectOptions(select, group1Options[0]);

    const paths = container.querySelectorAll<HTMLElement>(
      '.leaflet-container svg:not(.leaflet-attribution-flag) path.leaflet-interactive',
    );

    expect(paths).toHaveLength(3);

    // Location polygons
    // selected polygon has a wider border.
    expect(paths[0]).toHaveAttribute('stroke-width', '1');
    expect(paths[1]).toHaveAttribute('stroke-width', '1');
    expect(paths[2]).toHaveAttribute('stroke-width', '3');
  });

  test('changing selected location renders its indicator tile', async () => {
    const { user } = render(<MapBlock {...testBlockProps} />);

    await waitFor(() => {
      expect(
        screen.getByLabelText('2. Select a Local Authority District'),
      ).toBeInTheDocument();
    });

    const select = screen.getByLabelText(
      '2. Select a Local Authority District',
    );
    const groups = within(select).getAllByRole('group');
    const group1Options = within(groups[0]).getAllByRole('option');

    await user.selectOptions(select, group1Options[0]);

    const indicator = screen.getByTestId('mapBlock-indicator');
    const tile = within(indicator);

    expect(tile.getByTestId('mapBlock-indicatorTile-title')).toHaveTextContent(
      'Authorised absence rate (2016/17)',
    );
    expect(
      tile.getByTestId('mapBlock-indicatorTile-statistic'),
    ).toHaveTextContent('3.5%');
  });

  test('renders location indicator tiles correctly with custom decimal places', async () => {
    const fullTable = mapFullTable(
      produce(testMapTableData, draft => {
        draft.results[0].measures['authorised-absence-rate'] = '3.5123';
        draft.results[1].measures['authorised-absence-rate'] = '3.012';
        draft.results[2].measures['authorised-absence-rate'] = '4.009';
        draft.subjectMeta.indicators[0].decimalPlaces = 2;
      }),
    );

    const { user } = render(
      <MapBlock
        {...testBlockProps}
        meta={fullTable.subjectMeta}
        data={fullTable.results}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByLabelText('2. Select a Local Authority District'),
      ).toBeInTheDocument();
    });

    const select = screen.getByLabelText(
      '2. Select a Local Authority District',
    );
    const groups = within(select).getAllByRole('group');
    const group1Options = within(groups[0]).getAllByRole('option');

    await user.selectOptions(select, group1Options[0]);

    const tile1 = within(screen.getByTestId('mapBlock-indicator'));
    expect(tile1.getByTestId('mapBlock-indicatorTile-title')).toHaveTextContent(
      'Authorised absence rate (2016/17)',
    );
    expect(
      tile1.getByTestId('mapBlock-indicatorTile-statistic'),
    ).toHaveTextContent('3.51%');

    await user.selectOptions(select, group1Options[1]);

    const tile2 = within(screen.getByTestId('mapBlock-indicator'));
    expect(tile2.getByTestId('mapBlock-indicatorTile-title')).toHaveTextContent(
      'Authorised absence rate (2016/17)',
    );
    expect(
      tile2.getByTestId('mapBlock-indicatorTile-statistic'),
    ).toHaveTextContent('3.01%');

    await user.selectOptions(select, group1Options[2]);

    const tile3 = within(screen.getByTestId('mapBlock-indicator'));
    expect(tile3.getByTestId('mapBlock-indicatorTile-title')).toHaveTextContent(
      'Authorised absence rate (2016/17)',
    );
    expect(
      tile3.getByTestId('mapBlock-indicatorTile-statistic'),
    ).toHaveTextContent('4.01%');
  });

  test('resetting the map when no location selected', async () => {
    const { container, user } = render(<MapBlock {...testBlockProps} />);

    await waitFor(() => {
      expect(
        screen.getByLabelText('2. Select a Local Authority District'),
      ).toBeInTheDocument();
    });

    const select = screen.getByLabelText(
      '2. Select a Local Authority District',
    );
    const groups = within(select).getAllByRole('group');
    const group1Options = within(groups[0]).getAllByRole('option');

    await user.selectOptions(select, group1Options[0]);

    const paths = container.querySelectorAll<HTMLElement>(
      '.leaflet-container svg:not(.leaflet-attribution-flag) path.leaflet-interactive',
    );

    expect(paths).toHaveLength(3);
    expect(paths[0]).toHaveAttribute('stroke-width', '1');
    expect(paths[1]).toHaveAttribute('stroke-width', '1');
    expect(paths[2]).toHaveAttribute('stroke-width', '3');

    await user.selectOptions(select, within(select).getAllByRole('option')[0]);

    expect(paths[0]).toHaveAttribute('stroke-width', '1');
    expect(paths[1]).toHaveAttribute('stroke-width', '1');
    // Stroke width is reset
    expect(paths[2]).toHaveAttribute('stroke-width', '1');
  });

  test('ensure values with decimal places go are assigned the correct colour when the legend values are set to 0 decimal places', async () => {
    const fullTable = mapFullTable(
      produce(testMapTableData, draft => {
        draft.results[0].measures['authorised-absence-rate'] =
          '16388.4329758565';
        draft.results[1].measures['authorised-absence-rate'] =
          '-1395.33948144574';
        draft.results[2].measures['authorised-absence-rate'] =
          '6059.30533950346';
        draft.subjectMeta.indicators[0].decimalPlaces = 0;
        draft.subjectMeta.indicators[0].unit = '£';
      }),
    );

    const { container } = render(
      <MapBlock
        {...testBlockProps}
        meta={fullTable.subjectMeta}
        data={fullTable.results}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByLabelText('1. Select data to view'),
      ).toBeInTheDocument();
    });

    const paths = container.querySelectorAll<HTMLElement>(
      '.leaflet-container svg:not(.leaflet-attribution-flag) path',
    );

    expect(paths).toHaveLength(3);
    // Location polygon
    expect(paths[0]).toHaveAttribute('fill', 'rgba(71, 99, 165, 1)');
    expect(paths[1]).toHaveAttribute('fill', 'rgba(218, 224, 237, 1)');
    expect(paths[2]).toHaveAttribute('fill', 'rgba(145, 161, 201, 1)');

    const legendItems = screen.getAllByTestId('mapBlock-legend-item');

    expect(legendItems[0]).toHaveTextContent('-£1,395 to £2,161');
    expect(legendItems[1]).toHaveTextContent('£2,162 to £5,718');
    expect(legendItems[2]).toHaveTextContent('£5,719 to £9,275');
    expect(legendItems[3]).toHaveTextContent('£9,276 to £12,832');
    expect(legendItems[4]).toHaveTextContent('£12,833 to £16,388');

    const legendColours = screen.getAllByTestId('mapBlock-legend-colour');

    expect(legendColours[0].style.backgroundColor).toBe('rgb(218, 224, 237)');
    expect(legendColours[1].style.backgroundColor).toBe('rgb(181, 193, 219)');
    expect(legendColours[2].style.backgroundColor).toBe('rgb(145, 161, 201)');
    expect(legendColours[3].style.backgroundColor).toBe('rgb(108, 130, 183)');
    expect(legendColours[4].style.backgroundColor).toBe('rgb(71, 99, 165)');
  });

  test('renders correctly with mixed data sets', async () => {
    const testProps: MapBlockProps = {
      ...testMixedMapConfiguration,
      boundaryLevel: 1,
      id: 'testMap',
      axes: testMixedMapConfiguration.axes as MapBlockProps['axes'],
      legend: testMixedMapConfiguration.legend as LegendConfiguration,
      meta: testMixedMeta,
      data: testMixedData,
      height: 600,
      onBoundaryLevelChange,
    };
    const { container, user } = render(<MapBlock {...testProps} />);

    await waitFor(() => {
      const paths = container.querySelectorAll<HTMLElement>(
        '.leaflet-container svg:not(.leaflet-attribution-flag) path',
      );

      expect(paths).toHaveLength(3);

      // Location polygons
      expect(paths[0]).toHaveAttribute('fill', 'rgba(208, 217, 226, 1)');
      expect(paths[1]).toHaveAttribute('fill', 'rgba(18, 67, 109, 1)');
      // UK polygon
      expect(paths[2]).toHaveAttribute('fill', '#003078');
    });

    const legendItems = screen.getAllByTestId('mapBlock-legend-item');
    expect(legendItems).toHaveLength(5);
    expect(legendItems[0]).toHaveTextContent('400 to 419');
    expect(legendItems[1]).toHaveTextContent('420 to 439');
    expect(legendItems[2]).toHaveTextContent('440 to 459');
    expect(legendItems[3]).toHaveTextContent('460 to 479');
    expect(legendItems[4]).toHaveTextContent('480 to 500');

    const legendColours = screen.getAllByTestId('mapBlock-legend-colour');
    expect(legendColours).toHaveLength(5);
    expect(legendColours[0].style.backgroundColor).toBe('rgb(208, 217, 226)');
    expect(legendColours[1].style.backgroundColor).toBe('rgb(160, 180, 197)');
    expect(legendColours[2].style.backgroundColor).toBe('rgb(113, 142, 167)');
    expect(legendColours[3].style.backgroundColor).toBe('rgb(65, 105, 138)');
    expect(legendColours[4].style.backgroundColor).toBe('rgb(18, 67, 109)');

    const dataSetSelect = screen.getByLabelText('1. Select data to view');
    expect(dataSetSelect.children).toHaveLength(2);
    expect(dataSetSelect.children[0]).toHaveTextContent(
      'Numerical indicator (2024)',
    );
    expect(dataSetSelect.children[1]).toHaveTextContent(
      'Categorical indicator (2024)',
    );

    const locationSelect = screen.getByLabelText('2. Select a Region');
    expect(locationSelect.children).toHaveLength(3);
    expect(locationSelect.children[0]).toHaveTextContent('None selected');
    expect(locationSelect.children[1]).toHaveTextContent('Location 1');
    expect(locationSelect.children[2]).toHaveTextContent('Location 2');

    // Change selected data set
    await user.selectOptions(dataSetSelect, ['Categorical indicator (2024)']);

    await waitFor(() => {
      const updatedPaths = container.querySelectorAll<HTMLElement>(
        '.leaflet-container svg:not(.leaflet-attribution-flag) path',
      );

      expect(updatedPaths).toHaveLength(2);

      // Location polygons
      expect(updatedPaths[0]).toHaveAttribute('fill', '#003078');
      // UK polygon
      expect(updatedPaths[1]).toHaveAttribute('fill', '#12436D');
    });

    const updatedLegendItems = screen.getAllByTestId('mapBlock-legend-item');
    expect(updatedLegendItems).toHaveLength(1);
    expect(updatedLegendItems[0]).toHaveTextContent('large');

    const updatedLegendColours = screen.getAllByTestId(
      'mapBlock-legend-colour',
    );
    expect(updatedLegendColours).toHaveLength(1);
    expect(updatedLegendColours[0].style.backgroundColor).toBe(
      'rgb(18, 67, 109)',
    );

    const updatedLocationSelect = screen.getByLabelText('2. Select a Region');
    expect(updatedLocationSelect.children).toHaveLength(2);
    expect(updatedLocationSelect.children[0]).toHaveTextContent(
      'None selected',
    );
    expect(updatedLocationSelect.children[1]).toHaveTextContent('Location 3');
  });

  describe('MapBlock (aria-label)', () => {
    const baseProps: MapBlockProps = {
      ...testMapConfiguration,
      boundaryLevel: 1,
      id: 'testMapAria',
      axes: testMapConfiguration.axes as MapBlockProps['axes'],
      legend: testMapConfiguration.legend as LegendConfiguration,
      meta: testFullTable.subjectMeta,
      data: testFullTable.results,
      height: 600,
      onBoundaryLevelChange,
    };

    test('sets aria-label to "Interactive map showing education statistics by area" on the map container when alt is not provided', async () => {
      const { container } = render(<MapBlock {...baseProps} />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('1. Select data to view'),
        ).toBeInTheDocument();
      });

      const mapEl = container.querySelector<HTMLElement>('.leaflet-container');
      expect(mapEl).not.toBeNull();

      await waitFor(() => {
        expect(mapEl).toHaveAttribute(
          'aria-label',
          'Interactive map showing education statistics by area, for alternative see table tab',
        );
      });
    });

    test('sets aria-label on both wrapper and map container when alt is provided', async () => {
      const altText = 'Pupils absence map';
      const { container } = render(<MapBlock {...baseProps} alt={altText} />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('1. Select data to view'),
        ).toBeInTheDocument();
      });

      const mapEl = container.querySelector<HTMLElement>('.leaflet-container');
      expect(mapEl).not.toBeNull();

      await waitFor(() => {
        expect(mapEl).toHaveAttribute(
          'aria-label',
          `${altText}, for alternative see table tab`,
        );
      });
    });
  });
});
