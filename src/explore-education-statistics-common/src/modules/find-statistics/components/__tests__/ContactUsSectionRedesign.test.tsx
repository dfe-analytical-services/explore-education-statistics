import render from '@common-test/render';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import testContact from '@common/modules/find-statistics/components/__tests__/__data__/testContact';
import { Matcher, screen } from '@testing-library/react';
import React from 'react';

describe('ContactUsSectionRedesign', () => {
  test('renders', () => {
    render(
      <ContactUsSection
        publicationContact={testContact}
        publicationTitle="Mock Publication Title"
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
      />,
    );

    expect(
      screen.queryByRole('link', { name: 'Mock Contact Tel No' }),
    ).not.toBeInTheDocument();
  });
});
