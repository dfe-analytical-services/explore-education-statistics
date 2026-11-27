import PageHeader from '@frontend/components/PageHeader';
import React from 'react';
import render from '@common-test/render';
import testTenant from '@common-test/testTenant';
import { screen } from '@testing-library/react';

describe('PageHeader', () => {
  test('renders the EES service name by default', () => {
    render(<PageHeader />);

    expect(
      screen.getByText('Explore education statistics'),
    ).toBeInTheDocument();
  });

  test('renders the service name from a different tenant', () => {
    render(<PageHeader />, { tenant: testTenant });

    expect(screen.getByText('Test statistics')).toBeInTheDocument();
    expect(
      screen.queryByText('Explore education statistics'),
    ).not.toBeInTheDocument();
  });
});
