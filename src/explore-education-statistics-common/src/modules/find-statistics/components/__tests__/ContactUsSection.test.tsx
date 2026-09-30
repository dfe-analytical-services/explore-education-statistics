import React from 'react';
import { Matcher, render, screen } from '@testing-library/react';
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
});
