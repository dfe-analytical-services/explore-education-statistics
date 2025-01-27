import { render, screen } from '@testing-library/react';
import React from 'react';
import DataSymbolsModal from '../DataSymbolsModal';

describe('ReleaseTypesModal', () => {
  test('renders with default trigger button', () => {
    render(<DataSymbolsModal />);

    expect(
      screen.getByRole('button', { name: 'Data symbols Data symbols guide' }),
    ).toBeInTheDocument();
  });

  test('renders data symbols table text', () => {
    render(<DataSymbolsModal />);
    expect(
      screen.queryByText('Suppressed to protect confidential information'),
    ).not.toBeInTheDocument();
  });
});
