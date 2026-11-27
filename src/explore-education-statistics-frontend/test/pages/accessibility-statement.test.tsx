import render from '@common-test/render';
import testTenant, { minimalTestTenant } from '@common-test/testTenant';
import AccessibilityStatementPage from '@frontend/pages/accessibility-statement';
import { screen, within } from '@testing-library/react';
import React from 'react';

// Lives outside `src/pages`, as Next.js would treat a test file there as a page.
describe('AccessibilityStatementPage', () => {
  test('renders the EES organisation by default', () => {
    render(<AccessibilityStatementPage />);

    expect(
      screen.getByRole('link', {
        name: 'the Department for Education (DfE) (opens in new tab)',
      }),
    ).toHaveAttribute(
      'href',
      'https://www.gov.uk/government/organisations/department-for-education',
    );
  });

  test('renders the organisation and acronym from the tenant', () => {
    render(<AccessibilityStatementPage />, { tenant: testTenant });

    expect(
      screen.getByRole('link', {
        name: 'the Test Organisation (TO) (opens in new tab)',
      }),
    ).toHaveAttribute('href', 'https://test-organisation.example.com');
    expect(
      screen.getByText(
        /does not cover any other services run by the Test Organisation \(TO\) or GOV\.UK/,
      ),
    ).toBeInTheDocument();
  });

  test('leaves out the acronym when the tenant has none', () => {
    render(<AccessibilityStatementPage />, { tenant: minimalTestTenant });

    expect(
      within(screen.getByRole('main')).getByRole('link', {
        name: 'the Test Organisation (opens in new tab)',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        /does not cover any other services run by the Test Organisation or GOV\.UK/,
      ),
    ).toBeInTheDocument();
    expect(screen.queryByText(/\(TO\)/)).not.toBeInTheDocument();
  });
});
