import MapLegend from '@common/modules/charts/components/MapLegend';
import { LegendDataGroup } from '@common/modules/charts/components/utils/generateLegendDataGroups';
import { render, screen } from '@testing-library/react';
import React from 'react';

describe('MapLegend', () => {
  const testLegendDataGroups: LegendDataGroup[] = [
    {
      colour: 'rgb(128, 128, 128)',
      decimalPlaces: 0,
      max: '3',
      maxRaw: 3,
      min: '1',
      minRaw: 1,
    },
    {
      colour: 'rgb(0, 0, 0)',
      decimalPlaces: 0,
      max: '5',
      maxRaw: 5,
      min: '4',
      minRaw: 4,
    },
  ];

  test('renders the legend', () => {
    render(
      <MapLegend
        heading="Test heading"
        legendDataGroups={testLegendDataGroups}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Key to Test heading',
      }),
    ).toBeInTheDocument();

    const listItems = screen.getAllByRole('listitem');
    expect(listItems).toHaveLength(2);
    expect(listItems[0]).toHaveTextContent('1 to 3');
    expect(listItems[1]).toHaveTextContent('4 to 5');

    const legendColours = screen.getAllByTestId('mapBlock-legend-colour');

    expect(legendColours).toHaveLength(2);
    expect(legendColours[0].style.backgroundColor).toBe('rgb(128, 128, 128)');
    expect(legendColours[1].style.backgroundColor).toBe('rgb(0, 0, 0)');
  });
});
