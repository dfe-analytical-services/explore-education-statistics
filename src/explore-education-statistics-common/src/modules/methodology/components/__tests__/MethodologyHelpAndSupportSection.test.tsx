import React from 'react';
import { render, screen } from '@testing-library/react';
import MethodologyHelpAndSupportSection from '@common/modules/methodology/components/MethodologyHelpAndSupportSection';
import { PublicationSummary } from '@common/services/publicationService';
import mockContact from '@common/modules/find-statistics/components/__tests__/__data__/testContact';

const testPublicationSummary: PublicationSummary = {
  id: 'Mock Publication Id',
  slug: 'Mock Publication Slug',
  latestReleaseSlug: 'Mock Latest Release Slug',
  title: 'Mock Publication Title',
  owner: false,
  contact: mockContact,
};

describe('MethodologyHelpAndSupportSection', () => {
  beforeEach(() => {
    render(
      <MethodologyHelpAndSupportSection
        owningPublication={testPublicationSummary}
      />,
    );
  });

  test('renders', () => {
    expect(
      screen.getByRole('heading', { name: /Help and support/ }),
    ).toBeInTheDocument();

    expect(screen.getByText(/Mock Contact Email/)).toBeInTheDocument();
    expect(screen.getByText(/Mock Contact Name/)).toBeInTheDocument();
  });
});
