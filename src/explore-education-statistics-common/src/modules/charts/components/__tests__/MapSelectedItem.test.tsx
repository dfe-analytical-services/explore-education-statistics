import MapSelectedItem from '@common/modules/charts/components/MapSelectedItem';
import { render, screen } from '@testing-library/react';
import React from 'react';

describe('MapSelectedItem', () => {
  test('renders the selected item', () => {
    render(
      <MapSelectedItem
        decimalPlaces={1}
        heading="Test heading"
        title="Test title"
        unit="%"
        value={100.26}
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Test heading',
      }),
    ).toBeInTheDocument();
    expect(screen.getByText('Test title')).toBeInTheDocument();
    expect(screen.getByText('100.3%')).toBeInTheDocument();
  });
});
