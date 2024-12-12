import render from '@common-test/render';
import React from 'react';
import {
  testMapConfiguration,
  testMapTableData,
} from '@common/modules/charts/components/__tests__/__data__/testMapBlockData';
import MapBlock, {
  MapBlockProps,
} from '@common/modules/charts/components/MapBlock';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { within } from '@testing-library/dom';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { produce } from 'immer';

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
    width: 900,
    map: {
      dataSetConfigs: [
        {
          dataGrouping: { customGroups: [], type: 'EqualIntervals' },
          dataSet: {
            filters: ['characteristic-total', 'school-type-total'],
            indicator: 'authorised-absence-rate',
            timePeriod: '2016_AY',
          },
        },
        {
          dataGrouping: { customGroups: [], type: 'EqualIntervals' },
          dataSet: {
            filters: ['characteristic-total', 'school-type-total'],
            indicator: 'overall-absence-rate',
            timePeriod: '2016_AY',
          },
          boundaryLevel: 2,
        },
      ],
    },
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
    render(<MapBlock {...testBlockProps} />);

    const select = screen.getByLabelText('1. Select data to view');
    expect(select.children[1]).toHaveTextContent(
      'Overall absence rate (2016/17)',
    );
    await userEvent.selectOptions(select, select.children[1] as HTMLElement);

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

  // EES-5718 name should be: 'selecting data set with different boundary level fetches and renders different boundary geo-JSON'
  test('selecting data set with different boundary level calls onBoundaryLevelChange', async () => {
    render(<MapBlock {...testBlockProps} />);

    expect(onBoundaryLevelChange).not.toHaveBeenCalled();

    const dataSetSelectInput = screen.getByLabelText('1. Select data to view');
    const dataSetOptions = within(dataSetSelectInput).getAllByRole('option');
    expect(dataSetOptions).toHaveLength(2);

    // Selecting another data set with different boundary level
    await userEvent.selectOptions(dataSetSelectInput, dataSetOptions[1]);

    waitFor(() => {
      // Fetching new geoJson
      expect(screen.getByTestId('loadingSpinner')).toBeInTheDocument();
      expect(onBoundaryLevelChange).toHaveBeenCalled();
    });

    // TODO: EES-5718
    // test that returned location geo Json is actually renderred on the screen when 'selecting data set with different boundary level fetches different boundary geo-JSON'
  });

  test('changing selected location focuses the correct polygon', async () => {
    const { container } = render(<MapBlock {...testBlockProps} />);

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

    await userEvent.selectOptions(select, group1Options[0]);

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
    render(<MapBlock {...testBlockProps} />);

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

    await userEvent.selectOptions(select, group1Options[0]);

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

    await userEvent.selectOptions(select, group1Options[0]);

    const tile1 = within(screen.getByTestId('mapBlock-indicator'));
    expect(tile1.getByTestId('mapBlock-indicatorTile-title')).toHaveTextContent(
      'Authorised absence rate (2016/17)',
    );
    expect(
      tile1.getByTestId('mapBlock-indicatorTile-statistic'),
    ).toHaveTextContent('3.51%');

    await userEvent.selectOptions(select, group1Options[1]);

    const tile2 = within(screen.getByTestId('mapBlock-indicator'));
    expect(tile2.getByTestId('mapBlock-indicatorTile-title')).toHaveTextContent(
      'Authorised absence rate (2016/17)',
    );
    expect(
      tile2.getByTestId('mapBlock-indicatorTile-statistic'),
    ).toHaveTextContent('3.01%');

    await userEvent.selectOptions(select, group1Options[2]);

    const tile3 = within(screen.getByTestId('mapBlock-indicator'));
    expect(tile3.getByTestId('mapBlock-indicatorTile-title')).toHaveTextContent(
      'Authorised absence rate (2016/17)',
    );
    expect(
      tile3.getByTestId('mapBlock-indicatorTile-statistic'),
    ).toHaveTextContent('4.01%');
  });

  test('resetting the map when no location selected', async () => {
    const { container } = render(<MapBlock {...testBlockProps} />);

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

    await userEvent.selectOptions(select, group1Options[0]);

    const paths = container.querySelectorAll<HTMLElement>(
      '.leaflet-container svg:not(.leaflet-attribution-flag) path.leaflet-interactive',
    );

    expect(paths).toHaveLength(3);
    expect(paths[0]).toHaveAttribute('stroke-width', '1');
    expect(paths[1]).toHaveAttribute('stroke-width', '1');
    expect(paths[2]).toHaveAttribute('stroke-width', '3');

    await userEvent.selectOptions(
      select,
      within(select).getAllByRole('option')[0],
    );

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

    render(
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

    // const paths = container.querySelectorAll<HTMLElement>(
    //   '.leaflet-container svg:not(.leaflet-attribution-flag) path',
    // );

    // expect(paths).toHaveLength(4);
    // // Location polygon
    // expect(paths[0]).toHaveAttribute('fill', 'rgba(71, 99, 165, 1)');
    // expect(paths[1]).toHaveAttribute('fill', 'rgba(218, 224, 237, 1)');
    // expect(paths[2]).toHaveAttribute('fill', 'rgba(145, 161, 201, 1)');
    // // UK polygon
    // expect(paths[3]).toHaveAttribute('fill', '#003078');

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
});
