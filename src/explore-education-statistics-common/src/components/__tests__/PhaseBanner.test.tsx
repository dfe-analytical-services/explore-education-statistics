import PhaseBanner from '@common/components/PhaseBanner';
import React from 'react';
import { render, screen } from '@testing-library/react';

describe('PhaseBanner', () => {
  test('renders with the url', () => {
    render(<PhaseBanner url="http://test.com" />);

    expect(screen.getByText('Beta')).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'feedback (opens in new tab)' }),
    ).toHaveAttribute('href', 'http://test.com');
  });
});
