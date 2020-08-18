import {
  testMapConfiguration,
  testMapTableData,
} from '@common/modules/charts/components/__tests__/__data__/testMapBlockData';
import MapBlock, {
  MapBlockProps,
} from '@common/modules/charts/components/MapBlock';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { within } from '@testing-library/dom';
import { render, wait, fireEvent } from '@testing-library/react';
import React from 'react';

describe('MapBlock', () => {
  const fullTable = mapFullTable(testMapTableData);
  const props: MapBlockProps = {
    ...testMapConfiguration,
    id: 'testMap',
    axes: testMapConfiguration.axes as MapBlockProps['axes'],
    meta: fullTable.subjectMeta,
    data: fullTable.results,
    height: 600,
    width: 900,
  };

  test('renders legends and polygons correctly', async () => {
    const { container, getAllByTestId } = render(<MapBlock {...props} />);

    await wait();

    const paths = container.querySelectorAll<HTMLElement>(
      '.leaflet-container svg path',
    );

    expect(paths).toHaveLength(4);
    // UK polygon
    expect(paths[0]).toHaveAttribute('fill', '#3388ff');
    // Location polygons
    expect(paths[1]).toHaveAttribute('fill', '#86bcff');
    expect(paths[2]).toHaveAttribute('fill', '#86bcff');
    expect(paths[3]).toHaveAttribute('fill', '#1c2742');

    const legendItems = getAllByTestId('mapBlock-legend-item');

    expect(legendItems[0]).toHaveTextContent('3% to 3.03%');
    expect(legendItems[1]).toHaveTextContent('3.03% to 3.05%');
    expect(legendItems[2]).toHaveTextContent('3.05% to 3.08%');
    expect(legendItems[3]).toHaveTextContent('3.08% to 3.1%');
    expect(legendItems[4]).toHaveTextContent('3.1% to 3.13%');

    const legendColours = getAllByTestId('mapBlock-legend-colour');

    expect(legendColours[0].style.backgroundColor).toBe('rgb(134, 188, 255)');
    expect(legendColours[1].style.backgroundColor).toBe('rgb(108, 150, 251)');
    expect(legendColours[2].style.backgroundColor).toBe('rgb(81, 113, 189)');
    expect(legendColours[3].style.backgroundColor).toBe('rgb(55, 76, 127)');
    expect(legendColours[4].style.backgroundColor).toBe('rgb(28, 39, 66)');
  });

  test('includes all data sets in select', async () => {
    const { getByLabelText } = render(<MapBlock {...props} />);

    await wait();

    const select = getByLabelText('1. Select data to view');

    expect(select).toBeVisible();

    expect(select.children).toHaveLength(2);
    expect(select.children[0]).toHaveTextContent(
      'Authorised absence rate (2016/17)',
    );
    expect(select.children[1]).toHaveTextContent(
      'Overall absence rate (2016/17)',
    );
  });

  test('includes all locations in select', async () => {
    const { getByLabelText } = render(<MapBlock {...props} />);

    await wait();

    const select = getByLabelText('2. Select a location');

    expect(select).toBeVisible();

    expect(select.children).toHaveLength(4);
    expect(select.children[0]).toHaveTextContent('Select location');
    expect(select.children[1]).toHaveTextContent('Leeds');
    expect(select.children[2]).toHaveTextContent('Manchester');
    expect(select.children[3]).toHaveTextContent('Sheffield');
  });

  test('changing selected data set changes legends and polygons', async () => {
    const { container, getAllByTestId, getByLabelText } = render(
      <MapBlock {...props} />,
    );

    await wait();

    const select = getByLabelText('1. Select data to view');

    expect(select.children[1]).toHaveTextContent(
      'Overall absence rate (2016/17)',
    );

    fireEvent.change(select, {
      target: {
        value: select.children[1].getAttribute('value'),
      },
    });

    const paths = container.querySelectorAll<HTMLElement>(
      '.leaflet-container svg path',
    );

    expect(paths).toHaveLength(4);
    // UK polygon
    expect(paths[0]).toHaveAttribute('fill', '#3388ff');
    // Location polygon
    expect(paths[1]).toHaveAttribute('fill', '#ffff7f');
    expect(paths[2]).toHaveAttribute('fill', '#ffff98');
    expect(paths[3]).toHaveAttribute('fill', '#624120');

    const legendItems = getAllByTestId('mapBlock-legend-item');

    expect(legendItems[0]).toHaveTextContent('4.6% to 4.73%');
    expect(legendItems[1]).toHaveTextContent('4.73% to 4.85%');
    expect(legendItems[2]).toHaveTextContent('4.85% to 4.98%');
    expect(legendItems[3]).toHaveTextContent('4.98% to 5.1%');
    expect(legendItems[4]).toHaveTextContent('5.1% to 5.23%');

    const legendColours = getAllByTestId('mapBlock-legend-colour');

    expect(legendColours[0].style.backgroundColor).toBe('rgb(255, 255, 152)');
    expect(legendColours[1].style.backgroundColor).toBe('rgb(255, 250, 122)');
    expect(legendColours[2].style.backgroundColor).toBe('rgb(255, 188, 92)');
    expect(legendColours[3].style.backgroundColor).toBe('rgb(189, 127, 62)');
    expect(legendColours[4].style.backgroundColor).toBe('rgb(98, 65, 32)');
  });

  test('changing selected location focuses the correct polygon', async () => {
    const { getByLabelText, container } = render(<MapBlock {...props} />);

    await wait();

    const select = getByLabelText('2. Select a location');

    expect(select.children[1]).toHaveTextContent('Leeds');

    fireEvent.change(select, {
      target: {
        value: select.children[1].getAttribute('value'),
      },
    });

    const paths = container.querySelectorAll<HTMLElement>(
      '.leaflet-container svg path',
    );

    expect(paths).toHaveLength(4);
    // UK polygon
    expect(paths[0]).not.toHaveClass('selected');
    // Location polygons
    expect(paths[1]).not.toHaveClass('selected');
    expect(paths[2]).not.toHaveClass('selected');
    expect(paths[3]).toHaveClass('selected');
  });

  test('changing selected location shows its indicator tiles', async () => {
    const { getByLabelText, getAllByTestId } = render(<MapBlock {...props} />);

    await wait();

    const select = getByLabelText('2. Select a location');

    expect(select.children[1]).toHaveTextContent('Leeds');

    fireEvent.change(select, {
      target: {
        value: select.children[1].getAttribute('value'),
      },
    });

    const tiles = getAllByTestId('mapBlock-indicator');

    expect(tiles).toHaveLength(2);

    const tile1 = within(tiles[0]);

    expect(tile1.getByTestId('mapBlock-indicator-title')).toHaveTextContent(
      'Authorised absence rate (2016/17)',
    );
    expect(tile1.getByTestId('mapBlock-indicator-value')).toHaveTextContent(
      '3%',
    );

    const tile2 = within(tiles[1]);

    expect(tile2.getByTestId('mapBlock-indicator-title')).toHaveTextContent(
      'Overall absence rate (2016/17)',
    );
    expect(tile2.getByTestId('mapBlock-indicator-value')).toHaveTextContent(
      '4.7%',
    );
  });
});
