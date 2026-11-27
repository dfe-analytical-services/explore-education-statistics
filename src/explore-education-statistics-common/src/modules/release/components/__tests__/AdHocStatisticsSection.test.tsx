import render from '@common-test/render';
import testTenant, {
  fullTestTenant,
  minimalTestTenant,
} from '@common-test/testTenant';
import AdHocStatisticsSection from '@common/modules/release/components/AdHocStatisticsSection';
import { screen } from '@testing-library/react';
import React from 'react';

describe('AdHocStatisticsSection', () => {
  test('renders the EES standards link by default', () => {
    render(<AdHocStatisticsSection publishingOrganisations={[]} />);

    expect(
      screen.getByRole('link', {
        name: 'Standards for official statistics published by DfE guidance (opens in new tab)',
      }),
    ).toHaveAttribute(
      'href',
      'https://www.gov.uk/government/publications/standards-for-official-statistics-published-by-the-department-for-education',
    );
  });

  test('renders the standards link from the tenant', () => {
    render(<AdHocStatisticsSection publishingOrganisations={[]} />, {
      tenant: fullTestTenant,
    });

    expect(
      screen.getByRole('link', {
        name: 'Standards for official statistics published by TO guidance (opens in new tab)',
      }),
    ).toHaveAttribute(
      'href',
      'https://test-organisation.example.com/standards',
    );
  });

  test('uses the organisation name in the standards link when the tenant has no acronym', () => {
    render(<AdHocStatisticsSection publishingOrganisations={[]} />, {
      tenant: {
        ...fullTestTenant,
        organisation: minimalTestTenant.organisation,
      },
    });

    expect(
      screen.getByRole('link', {
        name: 'Standards for official statistics published by the Test Organisation guidance (opens in new tab)',
      }),
    ).toBeInTheDocument();
  });

  test('hides the standards link when the tenant has no officialStatisticsStandardsUrl', () => {
    render(<AdHocStatisticsSection publishingOrganisations={[]} />, {
      tenant: testTenant,
    });

    expect(
      screen.queryByRole('link', {
        name: /Standards for official statistics/,
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText(/Find out more about the standards we follow/),
    ).not.toBeInTheDocument();
  });
});
