import render from '@common-test/render';
import testTenant, {
  fullTestTenant,
  minimalTestTenant,
} from '@common-test/testTenant';
import HomePage from '@frontend/modules/home/HomePage';
import { screen } from '@testing-library/react';
import React from 'react';

describe('HomePage', () => {
  test('renders the EES related services and general enquiry section by default', () => {
    render(<HomePage />);

    expect(
      screen.getByRole('heading', { name: 'Related services' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Statistics at DfE (opens in new tab)',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Have a general enquiry?' }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        'Have a general question about education or the Department for Education?',
      ),
    ).toBeInTheDocument();
  });

  test('renders the related services and public enquiries from the tenant', () => {
    render(<HomePage />, { tenant: fullTestTenant });

    expect(
      screen.getByRole('heading', { name: 'Related services' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        /other statistical services provided by the Test Organisation \(TO\)/,
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Test Related Service (opens in new tab)',
      }),
    ).toHaveAttribute('href', 'https://related-service.example.com');
    expect(
      screen.getByText('Find out more about testing.'),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('link', {
        name: 'Statistics at DfE (opens in new tab)',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText('Have a general question about testing?'),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        'The test statistics service is operated by the Test Organisation (TO).',
      ),
    ).toBeInTheDocument();
  });

  test('hides the related services section for a tenant with no relatedServices', () => {
    render(<HomePage />, { tenant: testTenant });

    expect(
      screen.queryByRole('heading', { name: 'Related services' }),
    ).not.toBeInTheDocument();
  });

  test('hides the related services section for a tenant with an empty relatedServices list', () => {
    render(<HomePage />, {
      tenant: { ...fullTestTenant, relatedServices: [] },
    });

    expect(
      screen.queryByRole('heading', { name: 'Related services' }),
    ).not.toBeInTheDocument();
  });

  test('hides the general enquiry section and acronym for a tenant without them', () => {
    render(<HomePage />, { tenant: minimalTestTenant });

    expect(
      screen.queryByRole('heading', { name: 'Have a general enquiry?' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByText(
        'The test statistics service is operated by the Test Organisation.',
      ),
    ).toBeInTheDocument();
  });
});
