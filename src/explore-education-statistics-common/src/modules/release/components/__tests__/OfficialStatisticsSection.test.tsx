import React from 'react';
import render from '@common-test/render';
import testTenant, {
  fullTestTenant,
  minimalTestTenant,
} from '@common-test/testTenant';
import { screen } from '@testing-library/react';
import OfficialStatisticsSection from '@common/modules/release/components/OfficialStatisticsSection';

describe('OfficialStatisticsSection', () => {
  test('renders', () => {
    render(<OfficialStatisticsSection publishingOrganisations={[]} />);

    expect(
      screen.getByRole('link', {
        name: 'Code of Practice for Official Statistics (opens in new tab)',
      }),
    ).toBeInTheDocument();
  });

  test('shows the heading if showHeading is true', () => {
    render(
      <OfficialStatisticsSection publishingOrganisations={[]} showHeading />,
    );

    expect(
      screen.getByRole('heading', { name: 'Official statistics' }),
    ).toBeInTheDocument();
  });

  test('hides the heading if showHeading is false', () => {
    render(
      <OfficialStatisticsSection
        publishingOrganisations={[]}
        showHeading={false}
      />,
    );

    expect(
      screen.queryByRole('heading', { name: 'Official statistics' }),
    ).not.toBeInTheDocument();
  });

  test('includes the OSR guidance text', () => {
    // Introduced because of changes to the National Statistics Designation Review,
    // Documented in https://dfedigital.atlassian.net/browse/EES-4621

    render(<OfficialStatisticsSection publishingOrganisations={[]} />);

    expect(
      screen.getByText(
        'Our statistical practice is regulated by the Office for Statistics Regulation (OSR).',
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', {
        name: 'Code of Practice for Statistics (opens in new tab)',
      }),
    ).toHaveAttribute(
      'href',
      'https://code.statisticsauthority.gov.uk/the-code/',
    );

    expect(
      screen.getByRole('link', { name: 'regulation@statistics.gov.uk' }),
    ).toHaveAttribute('href', 'mailto:regulation@statistics.gov.uk');

    expect(
      screen.getByRole('link', { name: 'OSR website (opens in new tab)' }),
    ).toHaveAttribute('href', 'https://osr.statisticsauthority.gov.uk/');
  });

  test('renders the EES standards link by default', () => {
    render(<OfficialStatisticsSection publishingOrganisations={[]} />);

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
    render(<OfficialStatisticsSection publishingOrganisations={[]} />, {
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
    render(<OfficialStatisticsSection publishingOrganisations={[]} />, {
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
    render(<OfficialStatisticsSection publishingOrganisations={[]} />, {
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

    // The rest of the section is unaffected.
    expect(
      screen.getByText(
        'Our statistical practice is regulated by the Office for Statistics Regulation (OSR).',
      ),
    ).toBeInTheDocument();
  });
});
