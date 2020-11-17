import {
  testMapConfiguration,
  testMapTableData,
} from '@common/modules/charts/components/__tests__/__data__/testMapBlockData';
import MapBlock, {
  MapBlockProps,
} from '@common/modules/charts/components/MapBlock';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { within } from '@testing-library/dom';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
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
    const { container } = render(<MapBlock {...props} />);

    await waitFor(() => {
      const paths = container.querySelectorAll<HTMLElement>(
        '.leaflet-container svg path',
      );

      expect(paths).toHaveLength(4);

      // UK polygon
      expect(paths[0]).toHaveAttribute('fill', '#3388ff');
      // Location polygons
      expect(paths[1]).toHaveAttribute('fill', '#5171bd');
      expect(paths[2]).toHaveAttribute('fill', '#86bcff');
      expect(paths[3]).toHaveAttribute('fill', '#1c2742');
    });

    const legendItems = screen.getAllByTestId('mapBlock-legend-item');

    expect(legendItems[0]).toHaveTextContent('3% to 3.2%');
    expect(legendItems[1]).toHaveTextContent('3.2% to 3.4%');
    expect(legendItems[2]).toHaveTextContent('3.4% to 3.6%');
    expect(legendItems[3]).toHaveTextContent('3.6% to 3.8%');
    expect(legendItems[4]).toHaveTextContent('3.8% to 4%');

    const legendColours = screen.getAllByTestId('mapBlock-legend-colour');

    expect(legendColours[0].style.backgroundColor).toBe('rgb(134, 188, 255)');
    expect(legendColours[1].style.backgroundColor).toBe('rgb(113, 158, 255)');
    expect(legendColours[2].style.backgroundColor).toBe('rgb(92, 128, 214)');
    expect(legendColours[3].style.backgroundColor).toBe('rgb(71, 99, 165)');
    expect(legendColours[4].style.backgroundColor).toBe('rgb(49, 69, 115)');
  });

  test('includes all data sets in select', async () => {
    render(<MapBlock {...props} />);

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

  test('includes all locations in select', async () => {
    render(<MapBlock {...props} />);

    await waitFor(() => {
      const select = screen.getByLabelText('2. Select a location');

      expect(select).toBeVisible();

      expect(select.children).toHaveLength(4);
      expect(select.children[0]).toHaveTextContent('Select location');
      expect(select.children[1]).toHaveTextContent('Leeds');
      expect(select.children[2]).toHaveTextContent('Manchester');
      expect(select.children[3]).toHaveTextContent('Sheffield');
    });
  });

  test('changing selected data set changes legends and polygons', async () => {
    const { container } = render(<MapBlock {...props} />);

    await waitFor(() => {
      const select = screen.getByLabelText('1. Select data to view');

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
      expect(paths[1]).toHaveAttribute('fill', '#fffa7a');
      expect(paths[2]).toHaveAttribute('fill', '#ffff98');
      expect(paths[3]).toHaveAttribute('fill', '#624120');
    });

    const legendItems = screen.getAllByTestId('mapBlock-legend-item');

    expect(legendItems[0]).toHaveTextContent('4.7% to 4.78%');
    expect(legendItems[1]).toHaveTextContent('4.78% to 4.86%');
    expect(legendItems[2]).toHaveTextContent('4.86% to 4.94%');
    expect(legendItems[3]).toHaveTextContent('4.94% to 5.02%');
    expect(legendItems[4]).toHaveTextContent('5.02% to 5.1%');

    const legendColours = screen.getAllByTestId('mapBlock-legend-colour');

    expect(legendColours[0].style.backgroundColor).toBe('rgb(255, 255, 152)');
    expect(legendColours[1].style.backgroundColor).toBe('rgb(255, 255, 128)');
    expect(legendColours[2].style.backgroundColor).toBe('rgb(255, 213, 104)');
    expect(legendColours[3].style.backgroundColor).toBe('rgb(245, 164, 80)');
    expect(legendColours[4].style.backgroundColor).toBe('rgb(171, 114, 56)');
  });

  test('changing selected location focuses the correct polygon', async () => {
    const { container } = render(<MapBlock {...props} />);

    await waitFor(() => {
      expect(screen.getByLabelText('2. Select a location')).toBeInTheDocument();
    });

    const select = screen.getByLabelText('2. Select a location');

    expect(select.children[1]).toHaveTextContent('Leeds');

    fireEvent.change(screen.getByLabelText('2. Select a location'), {
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
    render(<MapBlock {...props} />);

    await waitFor(() => {
      expect(screen.getByLabelText('2. Select a location')).toBeInTheDocument();
    });

    const select = screen.getByLabelText('2. Select a location');

    expect(select.children[1]).toHaveTextContent('Leeds');

    fireEvent.change(select, {
      target: {
        value: select.children[1].getAttribute('value'),
      },
    });

    const indicators = screen.getAllByTestId('mapBlock-indicator');

    expect(indicators).toHaveLength(2);

    const tile1 = within(indicators[0]);

    expect(tile1.getByTestId('mapBlock-indicatorTile-title')).toHaveTextContent(
      'Authorised absence rate (2016/17)',
    );
    expect(tile1.getByTestId('mapBlock-indicatorTile-value')).toHaveTextContent(
      '3.5%',
    );

    const tile2 = within(indicators[1]);

    expect(tile2.getByTestId('mapBlock-indicatorTile-title')).toHaveTextContent(
      'Overall absence rate (2016/17)',
    );
    expect(tile2.getByTestId('mapBlock-indicatorTile-value')).toHaveTextContent(
      '4.8%',
    );
  });
});
