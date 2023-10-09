import React from 'react';
import { Matcher, render, screen } from '@testing-library/react';
import ContactUsSection from '../ContactUsSection';
import mockContact from './__data__/mock-data';

describe('Contact Us Section', () => {
  it('Renders', () => {
    render(
      <ContactUsSection
        publicationContact={mockContact}
        publicationTitle="Mock Publication Title"
      />,
    );

    expect(screen.getByRole('heading', { name: 'Contact us' })).toBeVisible();
  });

  it.each([
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
    'Constructs sensible prompt text',
    (publicationTitle: string, expectedText: Matcher) => {
      render(
        <ContactUsSection
          publicationContact={mockContact}
          publicationTitle={publicationTitle}
        />,
      );

      expect(screen.getByText(expectedText, { exact: false })).toBeVisible();
    },
  );

  it('Contains an appropriate href to the contact email', () => {
    render(
      <ContactUsSection
        publicationContact={mockContact}
        publicationTitle="Mock Publication Title"
      />,
    );

    expect(
      screen.getByRole('link', { name: 'Mock Contact Email' }),
    ).toBeVisible();

    expect(
      screen.getByRole('link', { name: 'Mock Contact Email' }),
    ).toHaveAttribute('href', 'mailto:Mock Contact Email');
  });

  it('Displays the telephone number if one is supplied', () => {
    render(
      <ContactUsSection
        publicationContact={mockContact}
        publicationTitle="Mock Publication Title"
      />,
    );

    expect(
      screen.getByRole('link', { name: 'Mock Contact Tel No' }),
    ).toBeVisible();
    expect(
      screen.getByRole('link', { name: 'Mock Contact Tel No' }),
    ).toHaveAttribute('href', 'tel:Mock Contact Tel No');
  });

  it('Hides the telephone number section if one is not supplied', () => {
    render(
      <ContactUsSection
        publicationContact={{ ...mockContact, contactTelNo: undefined }}
        publicationTitle="Mock Publication Title"
      />,
    );

    expect(
      screen.queryByRole('link', { name: 'Mock Contact Tel No' }),
    ).toBeNull();
  });
});
