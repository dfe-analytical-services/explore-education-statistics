import { render, screen } from '@testing-library/react';
import React from 'react';
import SubscriptionStatusMessage from '@frontend/modules/subscriptions/components/SubscriptionStatusMessage';
import { PublicationTitle } from '@common/services/publicationService';

const testPublicationData: PublicationTitle = {
  title: 'Test Publication Title',
  id: '123abc',
};
const testPublicationSlug = 'test-publication-slug';

describe('SubscriptionStatusMessage', () => {
  test('renders', async () => {
    render(
      <SubscriptionStatusMessage
        title={testPublicationData.title}
        message="You have subscribed!"
      />,
    );

    expect(screen.getByRole('heading')).toHaveTextContent(
      testPublicationData.title,
    );

    expect(screen.getByText('You have subscribed!')).toBeInTheDocument();
  });

  test('does not display link to publication if slug is not provided', async () => {
    render(
      <SubscriptionStatusMessage
        title={testPublicationData.title}
        message="You have subscribed!"
      />,
    );

    expect(
      screen.queryByText(/View Test Publication Title/),
    ).not.toBeInTheDocument();
  });

  test('displays link to publication if slug is provided', async () => {
    render(
      <SubscriptionStatusMessage
        title={testPublicationData.title}
        message="You have subscribed!"
        slug={testPublicationSlug}
      />,
    );

    expect(screen.getByText(/View Test Publication Title/)).toBeInTheDocument();
  });
});
