import React from 'react';
import render from '@common-test/render';
import testTenant, {
  fullTestTenant,
  minimalTestTenant,
} from '@common-test/testTenant';
import { Matcher, screen } from '@testing-library/react';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import testContact from '@common/modules/find-statistics/components/__tests__/__data__/testContact';
import testOrganisation from '@common/modules/find-statistics/components/__tests__/__data__/testOrganisation';

describe('ContactUsSection', () => {
  test('renders', () => {
    render(
      <ContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[]}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Contact us' }),
    ).toBeInTheDocument();
  });

  test.each([
    [
      'Mock Title 1',
      'If you have a specific enquiry about Mock Title 1 statistics and data:',
    ],
    [
      'Mock Title 2',
      'If you have a specific enquiry about Mock Title 2 statistics and data:',
    ],
    ['', 'If you have a specific enquiry about statistics and data'],
  ])(
    'constructs a sensible prompt text',
    (publicationTitle: string, expectedText: Matcher) => {
      render(
        <ContactUsSection
          publicationContact={testContact}
          publicationTitle={publicationTitle}
          publishingOrganisations={[testOrganisation]}
        />,
      );

      expect(
        screen.getByText(expectedText, { exact: false }),
      ).toBeInTheDocument();
    },
  );

  test('contains an appropriate href to the contact email', () => {
    render(
      <ContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[testOrganisation]}
      />,
    );

    expect(
      screen.getByRole('link', { name: 'Mock Contact Email' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Mock Contact Email' }),
    ).toHaveAttribute('href', 'mailto:Mock Contact Email');
  });

  test('displays the telephone number if one is supplied', () => {
    render(
      <ContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[testOrganisation]}
      />,
    );

    expect(
      screen.getByText(/Telephone: Mock Contact Tel No/),
    ).toBeInTheDocument();
  });

  test('hides the telephone number section if one is not supplied', () => {
    render(
      <ContactUsSection
        publicationContact={{ ...testContact, contactTelNo: undefined }}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[testOrganisation]}
      />,
    );

    expect(
      screen.queryByRole('link', { name: 'Mock Contact Tel No' }),
    ).not.toBeInTheDocument();
  });

  test('renders correctly when the publishing organisation is Skills England', () => {
    render(
      <ContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[
          {
            id: 'test-id',
            title: 'Skills England',
            url: 'test-url',
            useGISLogo: true,
            logoFileName: 'logo.png',
          },
        ]}
      />,
    );

    expect(
      screen.getAllByRole('link', { name: 'skills.england@education.gov.uk' }),
    ).toHaveLength(2);
  });

  test('renders the public enquiries details for a different tenant', () => {
    render(
      <ContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[]}
      />,
      { tenant: testTenant },
    );

    expect(
      screen.getByText('If you have a general enquiry about testing:'),
    ).toBeInTheDocument();
    expect(screen.getByText('Phone: 01234 567890')).toBeInTheDocument();
  });

  test('hides the press office section for a tenant with no pressOffice', () => {
    render(
      <ContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[]}
      />,
      { tenant: testTenant },
    );

    expect(
      screen.queryByRole('heading', { name: 'Press office' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText('If you have a media enquiry:'),
    ).not.toBeInTheDocument();
  });

  test('renders the EES press office details by default', () => {
    render(
      <ContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[]}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Press office' }),
    ).toBeInTheDocument();
    expect(screen.getByText('Telephone: 020 7783 8300')).toBeInTheDocument();
  });

  test('renders the press office details from the tenant', () => {
    render(
      <ContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[]}
      />,
      { tenant: fullTestTenant },
    );

    expect(
      screen.getByRole('heading', { name: 'Press office' }),
    ).toBeInTheDocument();
    expect(screen.getByText('Telephone: 09876 543210')).toBeInTheDocument();
    expect(
      screen.queryByText('Telephone: 020 7783 8300'),
    ).not.toBeInTheDocument();
  });

  test('hides the public enquiries section for a tenant with no publicEnquiries', () => {
    render(
      <ContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[]}
      />,
      { tenant: minimalTestTenant },
    );

    expect(
      screen.queryByRole('heading', { name: 'Public enquiries' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Opening times' }),
    ).not.toBeInTheDocument();
  });

  test('renders the Skills England contact details regardless of the tenant', () => {
    render(
      <ContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[
          {
            id: 'test-id',
            title: 'Skills England',
            url: 'test-url',
            useGISLogo: true,
            logoFileName: 'logo.png',
          },
        ]}
      />,
      { tenant: minimalTestTenant },
    );

    expect(
      screen.getByRole('heading', { name: 'Press office' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Public enquiries' }),
    ).toBeInTheDocument();
  });
});
