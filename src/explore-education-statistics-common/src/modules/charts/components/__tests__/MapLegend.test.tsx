import MapLegend from '@common/modules/charts/components/MapLegend';
import { MapLegendItem } from '@common/modules/charts/types/chart';
import { render, screen, within } from '@testing-library/react';
import React from 'react';

describe('MapLegend', () => {
  const testMapLegendItems: MapLegendItem[] = [
    {
      colour: 'rgb(128, 128, 128)',
      value: 'Item 1',
    },
    {
      colour: 'rgb(0, 0, 0)',
      value: 'Item 2',
    },
  ];

  test('renders the legend', () => {
    render(
      <MapLegend heading="Test heading" legendItems={testMapLegendItems} />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Key to Test heading',
      }),
    ).toBeInTheDocument();

    const listItems = screen.getAllByRole('definition');
    expect(listItems).toHaveLength(2);
    expect(listItems[0]).toHaveTextContent('Item 1');
    expect(listItems[1]).toHaveTextContent('Item 2');

    const legendColours = screen.getAllByTestId('mapBlock-legend-colour');

    expect(legendColours).toHaveLength(2);
    expect(legendColours[0].style.backgroundColor).toBe('rgb(128, 128, 128)');
    expect(legendColours[1].style.backgroundColor).toBe('rgb(0, 0, 0)');
  });

  test('renders a span with hex and colour name for each legend item', () => {
    render(
      <MapLegend heading="Test heading" legendItems={testMapLegendItems} />,
    );

    const spans = screen.getAllByTestId('mapBlock-legend-item');
    expect(spans).toHaveLength(2);
    expect(within(spans[0]).getByRole('term')).toHaveTextContent(
      'Group 1 of 2',
    );
    expect(within(spans[1]).getByRole('term')).toHaveTextContent(
      'Group 2 of 2',
    );
  });
});
