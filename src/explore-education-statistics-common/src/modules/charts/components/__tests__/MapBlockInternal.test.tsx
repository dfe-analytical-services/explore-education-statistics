import {
  testMapConfiguration,
  testMapTableData,
  testMapTableDataRegion,
  testMapTableDataMixed,
  testsMixedLocationsFullTableMeta,
  testsMixedLocationsTableData,
  testsMixedLocationsTableDataWithLADs,
} from '@common/modules/charts/components/__tests__/__data__/testMapBlockData';
import { MapBlockProps } from '@common/modules/charts/components/MapBlock';
import MapBlockInternal from '@common/modules/charts/components/MapBlockInternal';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { within } from '@testing-library/dom';
import { render, screen, waitFor } from '@testing-library/react';
import produce from 'immer';
import React from 'react';
import userEvent from '@testing-library/user-event';

describe('MapBlockInternal', () => {
  const testFullTable = mapFullTable(testMapTableData);
  const testBlockProps: MapBlockProps = {
    ...testMapConfiguration,
    id: 'testMap',
    axes: testMapConfiguration.axes as MapBlockProps['axes'],
    legend: testMapConfiguration.legend as LegendConfiguration,
    meta: testFullTable.subjectMeta,
    data: testFullTable.results,
    height: 600,
    width: 900,
  };

  const testMixedLocationsAxes: MapBlockProps['axes'] = {
    major: {
      type: 'major',
      groupBy: 'locations',
      dataSets: [
        {
          indicator: 'authorised-absence-rate',
          filters: ['characteristic-total'],
          timePeriod: '2016_AY',
        },
      ],
      referenceLines: [],
      visible: true,
    },
  };

  test('renders legends and polygons correctly', async () => {
    const { container } = render(<MapBlockInternal {...testBlockProps} />);

    await waitFor(() => {
      const paths = container.querySelectorAll<HTMLElement>(
        '.leaflet-container svg:not(.leaflet-attribution-flag) path',
      );

      expect(paths).toHaveLength(4);

      // UK polygon
      expect(paths[0]).toHaveAttribute('fill', '#3388ff');
      // Location polygons
      expect(paths[1]).toHaveAttribute('fill', 'rgba(145, 161, 201, 1)');
      expect(paths[2]).toHaveAttribute('fill', 'rgba(218, 224, 237, 1)');
      expect(paths[3]).toHaveAttribute('fill', 'rgba(71, 99, 165, 1)');
    });

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
      <MapBlockInternal
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
      <MapBlockInternal
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

  test('includes all data sets in select', async () => {
    render(<MapBlockInternal {...testBlockProps} />);

    await waitFor(() => {
      const select = screen.getByLabelText('1. Select data to view');

      expect(select).toBeVisible();

      expect(select.children).toHaveLength(2);
      expect(select.children[0]).toHaveTextContent(
        'Authorised absence rate (2016/17)',
      );
      expect(select.children[1]).toHaveTextContent(
        'Overall absence rate (2016/17)',
      );
    });
  });

  test('changing selected data set changes legends and polygons', async () => {
    const { container } = render(<MapBlockInternal {...testBlockProps} />);

    await waitFor(() => {
      const select = screen.getByLabelText('1. Select data to view');

      expect(select.children[1]).toHaveTextContent(
        'Overall absence rate (2016/17)',
      );

      userEvent.selectOptions(select, select.children[1] as HTMLElement);
      const paths = container.querySelectorAll<HTMLElement>(
        '.leaflet-container svg:not(.leaflet-attribution-flag) path',
      );

      expect(paths).toHaveLength(4);
      // UK polygon
      expect(paths[0]).toHaveAttribute('fill', '#3388ff');
      // Location polygon
      expect(paths[1]).toHaveAttribute('fill', 'rgba(251, 219, 185, 1)');
      expect(paths[2]).toHaveAttribute('fill', 'rgba(253, 237, 220, 1)');
      expect(paths[3]).toHaveAttribute('fill', 'rgba(245, 164, 80, 1)');
    });

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

  test('changing selected location focuses the correct polygon', async () => {
    const { container } = render(<MapBlockInternal {...testBlockProps} />);

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

    userEvent.selectOptions(select, group1Options[0]);
    const paths = container.querySelectorAll<HTMLElement>(
      '.leaflet-container svg:not(.leaflet-attribution-flag) path',
    );

    expect(paths).toHaveLength(4);
    // UK polygon
    expect(paths[0]).not.toHaveClass('selected');
    // Location polygons
    expect(paths[1]).not.toHaveClass('selected');
    expect(paths[2]).not.toHaveClass('selected');
    expect(paths[3]).toHaveClass('selected');
  });

  test('changing selected location renders its indicator tile', async () => {
    render(<MapBlockInternal {...testBlockProps} />);

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

    userEvent.selectOptions(select, group1Options[0]);

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

    render(
      <MapBlockInternal
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

    userEvent.selectOptions(select, group1Options[0]);

    const tile1 = within(screen.getByTestId('mapBlock-indicator'));
    expect(tile1.getByTestId('mapBlock-indicatorTile-title')).toHaveTextContent(
      'Authorised absence rate (2016/17)',
    );
    expect(
      tile1.getByTestId('mapBlock-indicatorTile-statistic'),
    ).toHaveTextContent('3.51%');

    userEvent.selectOptions(select, group1Options[1]);

    const tile2 = within(screen.getByTestId('mapBlock-indicator'));
    expect(tile2.getByTestId('mapBlock-indicatorTile-title')).toHaveTextContent(
      'Authorised absence rate (2016/17)',
    );
    expect(
      tile2.getByTestId('mapBlock-indicatorTile-statistic'),
    ).toHaveTextContent('3.01%');

    userEvent.selectOptions(select, group1Options[2]);

    const tile3 = within(screen.getByTestId('mapBlock-indicator'));
    expect(tile3.getByTestId('mapBlock-indicatorTile-title')).toHaveTextContent(
      'Authorised absence rate (2016/17)',
    );
    expect(
      tile3.getByTestId('mapBlock-indicatorTile-statistic'),
    ).toHaveTextContent('4.01%');
  });

  test('resetting the map when select None Selected', async () => {
    const { container } = render(<MapBlockInternal {...testBlockProps} />);

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

    userEvent.selectOptions(select, group1Options[0]);

    const paths = container.querySelectorAll<HTMLElement>(
      '.leaflet-container svg:not(.leaflet-attribution-flag) path',
    );

    expect(paths[3]).toHaveClass('selected');

    userEvent.selectOptions(select, within(select).getAllByRole('option')[0]);
    expect(paths[3]).not.toHaveClass('selected');
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
      <MapBlockInternal
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

    expect(paths).toHaveLength(4);
    // UK polygon
    expect(paths[0]).toHaveAttribute('fill', '#3388ff');
    // Location polygon
    expect(paths[1]).toHaveAttribute('fill', 'rgba(71, 99, 165, 1)');
    expect(paths[2]).toHaveAttribute('fill', 'rgba(218, 224, 237, 1)');
    expect(paths[3]).toHaveAttribute('fill', 'rgba(145, 161, 201, 1)');

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

  describe('Location dropdown', () => {
    test('shows the data set location type in the label', async () => {
      const testFullTableRegion = mapFullTable(testMapTableDataRegion);

      const testBlockPropsRegion = produce(testBlockProps, draft => {
        draft.meta = testFullTableRegion.subjectMeta;
        draft.data = testFullTableRegion.results;
      });

      render(<MapBlockInternal {...testBlockPropsRegion} />);

      await waitFor(() => {
        expect(screen.getByLabelText('2. Select a Region')).toBeInTheDocument();
      });
    });

    test('shows the default label if the data set contains multiple types', async () => {
      const testFullTableMixed = mapFullTable(testMapTableDataMixed);
      const testBlockPropsMixed = produce(testBlockProps, draft => {
        draft.meta = testFullTableMixed.subjectMeta;
        draft.data = testFullTableMixed.results;
      });

      render(<MapBlockInternal {...testBlockPropsMixed} />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('2. Select a location'),
        ).toBeInTheDocument();
      });
    });

    test('shows ungrouped location options if no local authorities', async () => {
      render(<MapBlockInternal {...testBlockProps} />);

      await waitFor(() => {
        const select = screen.getByLabelText(
          '2. Select a Local Authority District',
        );

        expect(select).toBeVisible();
        const options = within(select).getAllByRole('option');
        expect(options).toHaveLength(4);
        expect(options[0]).toHaveTextContent('None selected');
        expect(options[1]).toHaveTextContent('Leeds');
        expect(options[2]).toHaveTextContent('Manchester');
        expect(options[3]).toHaveTextContent('Sheffield');
      });
    });

    test('shows grouped location options if there are local authorities', async () => {
      const testBlockProps2: MapBlockProps = {
        ...testMapConfiguration,
        id: 'testMap',
        axes: testMixedLocationsAxes,
        legend: testMapConfiguration.legend as LegendConfiguration,
        meta: testsMixedLocationsFullTableMeta,
        data: testsMixedLocationsTableData,
      };

      render(<MapBlockInternal {...testBlockProps2} />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('2. Select a location'),
        ).toBeInTheDocument();
      });
      const select = screen.getByLabelText('2. Select a location');

      expect(select.children[0]).toHaveTextContent('None selected');

      const groups = within(select).getAllByRole('group');
      expect(groups).toHaveLength(4);

      expect(groups[0]).toHaveProperty('label', 'Country');
      const group1Options = within(groups[0]).getAllByRole('option');
      expect(group1Options).toHaveLength(1);
      expect(group1Options[0]).toHaveTextContent('England');

      expect(groups[1]).toHaveProperty('label', 'North East');
      const group2Options = within(groups[1]).getAllByRole('option');
      expect(group2Options).toHaveLength(2);
      expect(group2Options[0]).toHaveTextContent('Darlington');
      expect(group2Options[1]).toHaveTextContent('Newcastle upon Tyne');

      expect(groups[2]).toHaveProperty('label', 'Region');
      const group3Options = within(groups[2]).getAllByRole('option');
      expect(group3Options).toHaveLength(2);
      expect(group3Options[0]).toHaveTextContent('North East');
      expect(group3Options[1]).toHaveTextContent('North West');

      expect(groups[3]).toHaveProperty('label', 'Yorkshire and the Humber');
      const group4Options = within(groups[3]).getAllByRole('option');
      expect(group4Options).toHaveLength(2);
      expect(group4Options[0]).toHaveTextContent('Rotherham');
      expect(group4Options[1]).toHaveTextContent('Sheffield');
    });

    test('shows grouped location options if there are local authority districts', async () => {
      const testBlockProps2: MapBlockProps = {
        ...testMapConfiguration,
        id: 'testMap',
        axes: testMixedLocationsAxes,
        legend: testMapConfiguration.legend as LegendConfiguration,
        meta: testsMixedLocationsFullTableMeta,
        data: testsMixedLocationsTableDataWithLADs,
      };

      render(<MapBlockInternal {...testBlockProps2} />);

      await waitFor(() => {
        expect(
          screen.getByLabelText('2. Select a Local Authority District'),
        ).toBeInTheDocument();
      });
      const select = screen.getByLabelText(
        '2. Select a Local Authority District',
      );

      expect(select.children[0]).toHaveTextContent('None selected');

      const groups = within(select).getAllByRole('group');
      expect(groups).toHaveLength(1);

      expect(groups[0]).toHaveProperty('label', 'Yorkshire and the Humber');
      const group1Options = within(groups[0]).getAllByRole('option');
      expect(group1Options).toHaveLength(2);
      expect(group1Options[0]).toHaveTextContent('Rotherham LAD');
      expect(group1Options[1]).toHaveTextContent('Sheffield LAD');
    });
  });
});
