import FeedbackSection from '@common/components/FeedbackSection';
import { render, screen } from '@testing-library/react';
import React from 'react';

describe('FeedbackSection', () => {
  test('renders the heading and feedback form link', () => {
    render(<FeedbackSection />);

    const link = screen.getByRole('link', {
      name: 'feedback form (opens in new tab)',
    });

    expect(link).toHaveAttribute(
      'href',
      'https://forms.office.com/Pages/ResponsePage.aspx?id=yXfS-grGoU2187O4s0qC-XMiKzsnr8xJoWM_DeGwIu9UNDJHOEJDRklTNVA1SDdLOFJITEwyWU1OQS4u',
    );
    expect(link).toHaveAttribute('target', '_blank');
    expect(link).toHaveAttribute('rel', 'noopener noreferrer nofollow');
  });
});
