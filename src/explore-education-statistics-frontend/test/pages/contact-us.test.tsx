import render from '@common-test/render';
import testTenant, {
  fullTestTenant,
  minimalTestTenant,
} from '@common-test/testTenant';
import ContactPage from '@frontend/pages/contact-us';
import { screen } from '@testing-library/react';
import React from 'react';

// Lives outside `src/pages`, as Next.js would treat a test file there as a page.
describe('ContactPage', () => {
  test('renders every EES section by default', () => {
    render(<ContactPage />);

    expect(
      screen.getByRole('heading', { name: 'Have a general enquiry?' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        'Have a general question about education or the Department for Education?',
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Statistical policy enquiries' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', {
        name: 'Freedom of information (FOI) requests',
      }),
    ).toBeInTheDocument();
  });

  test('renders the optional sections from the tenant', () => {
    render(<ContactPage />, { tenant: fullTestTenant });

    expect(
      screen.getByText('Have a general question about testing?'),
    ).toBeInTheDocument();
    expect(
      screen.getByText('Call the test enquiries line:'),
    ).toBeInTheDocument();
    expect(screen.getByText(/Phone: 01234 567890/)).toBeInTheDocument();
    expect(
      screen.getByText(/Opening hours: Monday to Friday from 9am to 5pm/),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Test Head of Profession for Statistics',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'test.hop@example.com' }),
    ).toHaveAttribute('href', 'mailto:test.hop@example.com');

    expect(
      screen.getByRole('link', {
        name: 'freedom of information (FOI) request (opens in new tab)',
      }),
    ).toHaveAttribute('href', 'https://test-organisation.example.com/foi');

    expect(
      screen.queryByText(/Department for Education/),
    ).not.toBeInTheDocument();
  });

  test('hides the statistical policy and FOI sections for a tenant without them', () => {
    render(<ContactPage />, { tenant: testTenant });

    expect(
      screen.queryByRole('heading', { name: 'Statistical policy enquiries' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', {
        name: 'Freedom of information (FOI) requests',
      }),
    ).not.toBeInTheDocument();

    // Sections that don't depend on optional tenant fields are unaffected.
    expect(
      screen.getByRole('link', { name: 'test.contact@example.com' }),
    ).toHaveAttribute('href', 'mailto:test.contact@example.com');
    expect(
      screen.getByRole('heading', { name: 'Provide us with feedback' }),
    ).toBeInTheDocument();
  });

  test('hides the general enquiry section for a tenant with no publicEnquiries', () => {
    render(<ContactPage />, { tenant: minimalTestTenant });

    expect(
      screen.queryByRole('heading', { name: 'Have a general enquiry?' }),
    ).not.toBeInTheDocument();
    expect(screen.queryByText(/Opening hours/)).not.toBeInTheDocument();
  });
});
