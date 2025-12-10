import React from 'react';
import { screen, waitFor } from '@testing-library/react';
import render from '@common-test/render';
import _publicationService from '@common/services/publicationService';
import _notificationService from '@frontend/services/notificationService';
import testPublicationTitle from '@frontend/modules/subscriptions/__tests__/__data__/testPublicationData';
import testActiveSubscription from '@frontend/modules/subscriptions/__tests__/__data__/testSubscriptionData';
import ConfirmSubscriptionPage from '@frontend/modules/subscriptions/ConfirmSubscriptionPage';

jest.mock('@common/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;

jest.mock('@frontend/services/notificationService');
const notificationService = _notificationService as jest.Mocked<
  typeof _notificationService
>;

describe('ConfirmSubscriptionPage', () => {
  beforeEach(() => {
    publicationService.getPublicationTitle.mockResolvedValue(
      testPublicationTitle,
    );
  });

  test('renders', async () => {
    render(
      <ConfirmSubscriptionPage
        publicationSlug="test-publication-slug"
        publicationTitle={testPublicationTitle.title}
        publicationId={testPublicationTitle.publicationId}
        token="test-token"
      />,
    );

    expect(
      screen.getByRole('heading', { name: testPublicationTitle.title }),
    ).toBeInTheDocument();
  });

  test('calls the service when Confirm is clicked', async () => {
    notificationService.confirmPendingSubscription.mockResolvedValue(
      testActiveSubscription,
    );

    const { user } = render(
      <ConfirmSubscriptionPage
        publicationSlug="test-publication-slug"
        publicationTitle={testPublicationTitle.title}
        publicationId={testPublicationTitle.publicationId}
        token="test-token"
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(
        notificationService.confirmPendingSubscription,
      ).toHaveBeenCalledWith(testPublicationTitle.publicationId, 'test-token');
    });
  });

  test('shows the confirmation message upon successful API response', async () => {
    notificationService.confirmPendingSubscription.mockResolvedValue(
      testActiveSubscription,
    );

    const { user } = render(
      <ConfirmSubscriptionPage
        publicationSlug="test-publication-slug"
        publicationTitle={testPublicationTitle.title}
        publicationId={testPublicationTitle.publicationId}
        token="test-token"
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(
        notificationService.confirmPendingSubscription,
      ).toHaveBeenCalledWith(testPublicationTitle.publicationId, 'test-token');
    });

    expect(
      screen.queryByRole('button', { name: 'Confirm' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByText('You have successfully subscribed to these updates.'),
    ).toBeInTheDocument();
  });

  test('shows the verification failure message upon unsuccessful API response', async () => {
    notificationService.confirmPendingSubscription.mockResolvedValue(undefined);

    const { user } = render(
      <ConfirmSubscriptionPage
        publicationSlug="test-publication-slug"
        publicationTitle={testPublicationTitle.title}
        publicationId={testPublicationTitle.publicationId}
        token="test-token"
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(
        notificationService.confirmPendingSubscription,
      ).toHaveBeenCalledWith(testPublicationTitle.publicationId, 'test-token');
    });

    expect(
      screen.queryByRole('button', { name: 'Confirm' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText('You have successfully subscribed to these updates.'),
    ).not.toBeInTheDocument();

    expect(
      screen.getByText(/Your subscription verification token has expired./),
    ).toBeInTheDocument();
  });
});
