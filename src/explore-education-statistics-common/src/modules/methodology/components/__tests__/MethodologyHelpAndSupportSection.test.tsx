import React from 'react';
import { render, screen } from '@testing-library/react';
import * as ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import MethodologyHelpAndSupportSection from '@common/modules/methodology/components/MethodologyHelpAndSupportSection';
import { PublicationSummary } from '@common/services/publicationService';
import mockContact from '@common/modules/find-statistics/components/__tests__/__data__/testContact';

jest.mock('@common/modules/find-statistics/components/ContactUsSection');

const testPublicationSummary: PublicationSummary = {
  id: 'Mock Publication Id',
  slug: 'Mock Publication Slug',
  title: 'Mock Publication Title',
  owner: false,
  contact: mockContact,
};

describe('MethodologyHelpAndSupportSection', () => {
  beforeEach(() => {
    jest
      .spyOn(ContactUsSection, 'default')
      .mockReturnValue(<div>This is a mocked ContactUsSection</div>);

    render(
      <MethodologyHelpAndSupportSection
        owningPublication={testPublicationSummary}
      />,
    );
  });

  test('renders', () => {
    expect(
      screen.getByRole('heading', { name: /Help and support/ }),
    ).toBeVisible();
  });

  test('renders a Contact Us Section', () => {
    expect(
      screen.getByText('This is a mocked ContactUsSection'),
    ).toBeInTheDocument();
  });
});
