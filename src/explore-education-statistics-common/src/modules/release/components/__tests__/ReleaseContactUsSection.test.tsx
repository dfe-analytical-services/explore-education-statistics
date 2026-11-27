import render from '@common-test/render';
import testTenant, {
  fullTestTenant,
  minimalTestTenant,
} from '@common-test/testTenant';
import testContact from '@common/modules/find-statistics/components/__tests__/__data__/testContact';
import testOrganisation from '@common/modules/find-statistics/components/__tests__/__data__/testOrganisation';
import ReleaseContactUsSection from '@common/modules/release/components/ReleaseContactUsSection';
import { Matcher, screen } from '@testing-library/react';
import React from 'react';

describe('ReleaseContactUsSection', () => {
  test('renders', () => {
    render(
      <ReleaseContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[testOrganisation]}
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
        <ReleaseContactUsSection
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
      <ReleaseContactUsSection
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
      <ReleaseContactUsSection
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
      <ReleaseContactUsSection
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
      <ReleaseContactUsSection
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

  test('renders the EES press office and public enquiries details by default', () => {
    render(
      <ReleaseContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[testOrganisation]}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Press office' }),
    ).toBeInTheDocument();
    expect(screen.getByText('Telephone: 020 7783 8300')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Public enquiries' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        'If you have a general enquiry about the Department for Education (DfE) or education:',
      ),
    ).toBeInTheDocument();
    expect(
      screen.queryByText('Call the DfE public enquiries line:'),
    ).not.toBeInTheDocument();
    expect(screen.getByText('Phone: 0370 000 2288')).toBeInTheDocument();
  });

  test('renders the press office and public enquiries details from the tenant', () => {
    render(
      <ReleaseContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[testOrganisation]}
      />,
      { tenant: fullTestTenant },
    );

    expect(
      screen.getByRole('heading', { name: 'Press office' }),
    ).toBeInTheDocument();
    expect(screen.getByText('Telephone: 09876 543210')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Public enquiries' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText('If you have a general enquiry about testing:'),
    ).toBeInTheDocument();
    expect(screen.getByText('Phone: 01234 567890')).toBeInTheDocument();
    expect(
      screen.getByText('Monday to Friday from 9am to 5pm'),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(/Department for Education/),
    ).not.toBeInTheDocument();
    expect(screen.queryByText(/DfE/)).not.toBeInTheDocument();
  });

  test('hides the press office section for a tenant with no pressOffice', () => {
    render(
      <ReleaseContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[testOrganisation]}
      />,
      { tenant: testTenant },
    );

    expect(
      screen.queryByRole('heading', { name: 'Press office' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText('If you have a media enquiry:'),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Public enquiries' }),
    ).toBeInTheDocument();
  });

  test('hides the public enquiries section for a tenant with no publicEnquiries', () => {
    render(
      <ReleaseContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
        publishingOrganisations={[testOrganisation]}
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
      <ReleaseContactUsSection
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
    expect(
      screen.getAllByRole('link', { name: 'skills.england@education.gov.uk' }),
    ).toHaveLength(2);
  });
});
