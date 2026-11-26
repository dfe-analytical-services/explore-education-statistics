import PageFooter from '@frontend/components/PageFooter';
import React from 'react';
import render from '@common-test/render';
import testTenant from '@common-test/testTenant';
import { screen } from '@testing-library/react';

describe('PageFooter', () => {
  test('renders the EES organisation by default', () => {
    render(<PageFooter />);

    expect(
      screen.getByRole('link', { name: /Department for Education/ }),
    ).toHaveAttribute(
      'href',
      'https://www.gov.uk/government/organisations/department-for-education',
    );
  });

  test('renders the organisation from a different tenant', () => {
    render(<PageFooter />, { tenant: testTenant });

    expect(
      screen.getByRole('link', { name: /Test Organisation/ }),
    ).toHaveAttribute('href', 'https://test-organisation.example.com');
    expect(
      screen.queryByText(/Department for Education/),
    ).not.toBeInTheDocument();
  });
});
