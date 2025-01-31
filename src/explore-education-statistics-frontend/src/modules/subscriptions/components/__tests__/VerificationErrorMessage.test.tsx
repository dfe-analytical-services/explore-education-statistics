import { render, screen } from '@testing-library/react';
import React from 'react';
import VerificationErrorMessage from '@frontend/modules/subscriptions/components/VerificationErrorMessage';

const testPublicationSlug = 'test-publication-slug';

describe('VerificationErrorMessage', () => {
  test('renders', () => {
    render(<VerificationErrorMessage slug={testPublicationSlug} />);

    expect(
      screen.getByText(/Your subscription verification token has expired/),
    ).toBeInTheDocument();
  });

  test('provides the correct mailbox link for help', () => {
    render(<VerificationErrorMessage slug={testPublicationSlug} />);

    expect(
      screen.getByRole('link', {
        name: 'explore.statistics@education.gov.uk',
      }),
    ).toHaveAttribute('href', 'mailto:explore.statistics@education.gov.uk');
  });

  test('provides the correct mailbox link for help', () => {
    render(<VerificationErrorMessage slug={testPublicationSlug} />);

    expect(
      screen.getByRole('link', {
        name: `publication's main screen.`,
      }),
    ).toHaveAttribute('href', '/find-statistics/test-publication-slug');
  });
});
