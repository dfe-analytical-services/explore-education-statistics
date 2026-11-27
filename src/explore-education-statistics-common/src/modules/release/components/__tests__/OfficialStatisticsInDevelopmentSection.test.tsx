import React from 'react';
import render from '@common-test/render';
import testTenant, {
  fullTestTenant,
  minimalTestTenant,
} from '@common-test/testTenant';
import { screen } from '@testing-library/react';
import OfficialStatisticsInDevelopmentSection from '@common/modules/release/components/OfficialStatisticsInDevelopmentSection';
import testOrganisation from '@common/modules/find-statistics/components/__tests__/__data__/testOrganisation';
import { Organisation } from '@common/services/types/organisation';

describe('OfficialStatisticsInDevelopmentSection', () => {
  const skillsEngland: Organisation = {
    id: 'test-id',
    title: 'Skills England',
    url: 'test-url',
    useGISLogo: true,
    logoFileName: 'logo.png',
  };

  test('renders correctly with default settings', () => {
    render(
      <OfficialStatisticsInDevelopmentSection
        publishingOrganisations={[testOrganisation]}
      />,
    );

    expect(
      screen.getByText(
        /They have been developed under the guidance of the Head of Profession for Statistics/,
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Standards for official statistics published by DfE guidance (opens in new tab)',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(
        /They have been developed by Skills England under the guidance of Skills England Lead Official for Statistics/,
      ),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('link', {
        name: 'Standards for official statistics published by Skills England guidance (opens in new tab)',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly when the publishing organisation is Skills England', () => {
    render(
      <OfficialStatisticsInDevelopmentSection
        publishingOrganisations={[skillsEngland]}
      />,
    );

    expect(
      screen.getByText(
        /They have been developed by Skills England under the guidance of Skills England Lead Official for Statistics/,
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Standards for official statistics published by Skills England guidance (opens in new tab)',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(
        /They have been developed under the guidance of the Head of Profession for Statistics/,
      ),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('link', {
        name: 'Standards for official statistics published by DfE guidance (opens in new tab)',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders the Skills England text when Skills England is one of several publishing organisations', () => {
    render(
      <OfficialStatisticsInDevelopmentSection
        publishingOrganisations={[testOrganisation, skillsEngland]}
      />,
    );

    expect(
      screen.getByText(
        /They have been developed by Skills England under the guidance of Skills England Lead Official for Statistics/,
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Standards for official statistics published by Skills England guidance (opens in new tab)',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(
        /They have been developed under the guidance of the Head of Profession for Statistics/,
      ),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('link', {
        name: 'Standards for official statistics published by DfE guidance (opens in new tab)',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders the default text when there are no publishing organisations', () => {
    render(
      <OfficialStatisticsInDevelopmentSection publishingOrganisations={[]} />,
    );

    expect(
      screen.getByText(
        /They have been developed under the guidance of the Head of Profession for Statistics/,
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', {
        name: 'Standards for official statistics published by DfE guidance (opens in new tab)',
      }),
    ).toBeInTheDocument();
  });

  test('shows the heading if showHeading is true', () => {
    render(
      <OfficialStatisticsInDevelopmentSection
        publishingOrganisations={[testOrganisation]}
        showHeading
      />,
    );

    expect(
      screen.getByRole('heading', {
        name: 'Official statistics in development',
      }),
    ).toBeInTheDocument();
  });

  test('hides the heading if showHeading is false', () => {
    render(
      <OfficialStatisticsInDevelopmentSection
        publishingOrganisations={[testOrganisation]}
        showHeading={false}
      />,
    );

    expect(
      screen.queryByRole('heading', {
        name: 'Official statistics in development',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders the standards link from the tenant', () => {
    render(
      <OfficialStatisticsInDevelopmentSection
        publishingOrganisations={[testOrganisation]}
      />,
      { tenant: fullTestTenant },
    );

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
    render(
      <OfficialStatisticsInDevelopmentSection
        publishingOrganisations={[testOrganisation]}
      />,
      {
        tenant: {
          ...fullTestTenant,
          organisation: minimalTestTenant.organisation,
        },
      },
    );

    expect(
      screen.getByRole('link', {
        name: 'Standards for official statistics published by the Test Organisation guidance (opens in new tab)',
      }),
    ).toBeInTheDocument();
  });

  test('hides the standards link when the tenant has no officialStatisticsStandardsUrl', () => {
    render(
      <OfficialStatisticsInDevelopmentSection
        publishingOrganisations={[testOrganisation]}
      />,
      { tenant: testTenant },
    );

    expect(
      screen.queryByRole('link', {
        name: /Standards for official statistics/,
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText(/Find out more about the standards we follow/),
    ).not.toBeInTheDocument();
  });

  test('renders the Skills England standards link even when the tenant has no officialStatisticsStandardsUrl', () => {
    render(
      <OfficialStatisticsInDevelopmentSection
        publishingOrganisations={[skillsEngland]}
      />,
      { tenant: testTenant },
    );

    expect(
      screen.getByRole('link', {
        name: 'Standards for official statistics published by Skills England guidance (opens in new tab)',
      }),
    ).toBeInTheDocument();
  });
});
